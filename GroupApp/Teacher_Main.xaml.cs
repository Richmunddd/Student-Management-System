using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;

namespace GroupApp
{
    /// <summary>
    /// Interaction logic for Teacher_Main.xaml
    /// </summary>
    public partial class Teacher_Main : Window
    {
        public Teacher_Main()
        {
            InitializeComponent();
            LoadCourses();
        }
        private void LoadCourses()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = "SELECT CourseID, CourseName FROM Courses";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CourseComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = reader.GetString(1),
                        Tag = reader.GetInt32(0) // Store CourseID in the Tag
                    });
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //SOFTWARE 
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //ECEA
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //OPERATING SYSTEMS
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Student_List studentWindow = new Student_List();
            studentWindow.ShowDialog();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }

        private void Activity_Click(object sender, RoutedEventArgs e)
        {
            ActivitySidebar.Visibility = ActivitySidebar.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
        private void SubmitActivity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Retrieve the data from the UI
                string activityName = ActivityTextBox.Text;

                // Safely get the selected section from the ComboBox
                string section = SectionComboBox.SelectedItem?.ToString();

                // Get the course name by accessing the Content property of the selected ComboBoxItem
                string course = (CourseComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                string scoreLimit = ScoreLimitTextBox.Text; // Get the score limit from the input field

                // Validate the score limit to ensure it's a valid number
                if (!int.TryParse(scoreLimit, out int parsedScoreLimit))
                {
                    MessageBox.Show("Please enter a valid score limit.");
                    return;
                }

                // Ensure that the course, section, and activity are not empty
                if (!string.IsNullOrEmpty(activityName) && !string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(course))
                {
                    // Get the courseId based on the course name
                    int courseId = GetCourseIdFromCourseName(course);

                    if (courseId != -1)  // Make sure the courseId was found
                    {
                        // Insert activity into the database with section and score limit
                        string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                        string query = @"
                    INSERT INTO Activities (ActivityName, Section, CourseID, ScoreLimit, DatePosted)
                    VALUES (@ActivityName, @Section, @CourseID, @ScoreLimit, @DatePosted)";

                        using (SqlConnection conn = new SqlConnection(connectionString))
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ActivityName", activityName); // Use ActivityName as the parameter
                            cmd.Parameters.AddWithValue("@Section", section); // Save the selected section
                            cmd.Parameters.AddWithValue("@CourseID", courseId); // Save the courseId
                            cmd.Parameters.AddWithValue("@ScoreLimit", parsedScoreLimit); // Save the score limit
                            cmd.Parameters.AddWithValue("@DatePosted", DateTime.Now); // Save the current date and time
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Activity added successfully!");
                        LoadStudentsInCourse(courseId);  // Load students after adding the activity
                    }
                    else
                    {
                        MessageBox.Show("Course not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a course, section, and enter an activity.");
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and show the error message
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }




        // Method to get courseId based on course name (assuming this method exists)
        private int GetCourseIdFromCourseName(string courseName)
        {
            int courseId = -1; // Default to -1 if course is not found
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            // Update the query to specifically use the "LogInDatabase" and "Courses" table
            string query = "SELECT CourseID FROM [LogInDatabase].[dbo].[Courses] WHERE CourseName = @courseName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // Use the parameterized query to prevent SQL injection
                cmd.Parameters.AddWithValue("@courseName", courseName);
                conn.Open();
                var result = cmd.ExecuteScalar();

                // If course is found, assign courseId, otherwise it stays -1
                if (result != null)
                {
                    courseId = Convert.ToInt32(result);
                }

                // Debugging: Output the courseName being searched for and the result
                Console.WriteLine($"Searching for course: {courseName}");
                if (courseId == -1)
                {
                    Console.WriteLine($"Course '{courseName}' not found in the database.");
                    MessageBox.Show($"Course '{courseName}' not found in the database.");
                }
                else
                {
                    Console.WriteLine($"Found Course ID: {courseId}");
                }
            }

            return courseId;
        }


        // Load students enrolled in the selected course
        private void LoadStudentsInCourse(int courseId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            string query = @"
        SELECT Username, FullName, StudentID, CourseID 
        FROM dbo.LogIn
        WHERE CourseID = @courseId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string username = reader.GetString(0);
                        string fullName = reader.GetString(1);
                        int studentId = reader.GetInt32(2);
                        int courseIdFromDb = reader.GetInt32(3);

                        // Create a new Student object and add it to the ListBox
                        StudentListBox.Items.Add(new Student
                        {
                            Username = username,
                            FullName = fullName,
                            StudentId = studentId,
                            CourseId = courseIdFromDb
                        });
                    }
                }
            }
        }



        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }
    }
}
