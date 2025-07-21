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
        private string teacherEmail; // Store the email of the logged-in teacher
        private int courseId; // Store the selected courseId
        private int selectedCourseId = -1;  // Store the selected course ID
        private int studentId; // Store the student's ID for later use

        public string Username { get; }

        public Teacher_Main(string username)
        {
            InitializeComponent();
            LoadCourses();
            teacherEmail = username; // Initialize the teacher's email

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

        // This method will handle the display of the Activity Sidebar
        private void Activity_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the visibility of the Activity Sidebar
            ActivitySidebar.Visibility = ActivitySidebar.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            // Ensure the DeleteActivitySidebar is hidden when the Activity Sidebar is toggled
            DeleteActivitySidebar.Visibility = Visibility.Collapsed;
        }

        private void SubmitActivity_Click(object sender, RoutedEventArgs e)
        {
            string activity = ActivityTextBox.Text;
            int courseId = ((ComboBoxItem)CourseComboBox.SelectedItem)?.Tag as int? ?? -1;
            string section = ((ComboBoxItem)SectionComboBox.SelectedItem)?.Content.ToString(); // Get the selected section
            int scoreLimit;

            // Try to parse the Score Limit from the TextBox
            if (!int.TryParse(ScoreLimitTextBox.Text, out scoreLimit))
            {
                MessageBox.Show("Please enter a valid score limit.");
                return;
            }

            if (courseId != -1 && !string.IsNullOrEmpty(activity) && !string.IsNullOrEmpty(section))
            {
                // Insert activity into database with section info and user input for score limit
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = "INSERT INTO Activities (CourseID, ActivityName, Section, DatePosted, ScoreLimit) VALUES (@courseId, @activity, @section, @datePosted, @scoreLimit)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    cmd.Parameters.AddWithValue("@activity", activity);
                    cmd.Parameters.AddWithValue("@section", section); // Save the selected section as string (e.g., "Course Work", "Quiz")
                    cmd.Parameters.AddWithValue("@datePosted", DateTime.Now); // Save the current date and time
                    cmd.Parameters.AddWithValue("@scoreLimit", scoreLimit); // Use the user input for the score limit
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Activity added successfully!");
            }
            else
            {
                MessageBox.Show("Please select a course, section, and enter an activity.");
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
        
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }


        //////
        ///
         // Check if the logged-in user is a teacher
        private bool IsTeacher()
        {
            return teacherEmail.EndsWith("@mymail.ph", StringComparison.OrdinalIgnoreCase);
        }

        // Event handler for deleting an activity
        // Delete activity click handler
        // Event handler for deleting an activity
        private void ShowDeleteActivitySidebar(object sender, RoutedEventArgs e)
        {
            // Show the Delete Activity Sidebar
            DeleteActivitySidebar.Visibility = Visibility.Visible;

            // Fetch and display the activities in the ActivityDeleteListBox
            LoadActivitiesForDeletion();
        }

        // This method will load activities for the selected course into the DeleteActivitySidebar
        private void LoadActivitiesForDeletion()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            // Assuming you have the courseId stored somewhere, e.g., from the selected course in the UI
            int courseId = 1;  // Use the actual course ID from the selected course

            string query = @"
    SELECT ActivityID, ActivityName
    FROM Activities
    WHERE CourseID = @courseId";

            // Clear the ListBox before loading new items
            ActivityDeleteListBox.Items.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string activityName = reader.GetString(1);
                        int activityId = reader.GetInt32(0);

                        // Add each activity to the ListBox with a Delete button
                        ActivityDeleteListBox.Items.Add(new
                        {
                            ActivityName = activityName,
                            ActivityID = activityId
                        });
                    }
                }
            }
        }
        // Toggle to collapse or show the Delete Activity Sidebar
        private void ToggleDeleteActivitySidebar(object sender, RoutedEventArgs e)
        {
            // Toggle the visibility of the DeleteActivitySidebar
            DeleteActivitySidebar.Visibility = (DeleteActivitySidebar.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }
        // Event handler to delete the activity
        private void DeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            int activityId = (int)button.Tag;  // Get the ActivityID from the button's Tag

            // Confirm the deletion with the teacher
            var result = MessageBox.Show("Are you sure you want to delete this activity?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DeleteActivityFromDatabase(activityId);
            }
        }

        // This method deletes the activity from the database
        private void DeleteActivityFromDatabase(int activityId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = "DELETE FROM Activities WHERE ActivityID = @activityId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@activityId", activityId);
                conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Activity deleted successfully!");
                    LoadActivitiesForDeletion();  // Refresh the list of activities
                }
                else
                {
                    MessageBox.Show("Activity not found or could not be deleted.");
                }
            }
        }






        // This method loads the activities based on the courseId and section
        // Load activities from database
        private void LoadCourseActivities(int courseId, string section)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = @"
            SELECT ActivityID, ActivityName, DatePosted
            FROM Activities
            WHERE CourseID = @courseId AND Section = @section";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                cmd.Parameters.AddWithValue("@section", section);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ActivityDeleteListBox.Items.Clear(); // Clear existing items in the ListBox

                    while (reader.Read())
                    {
                        int activityId = reader.GetInt32(0);
                        string activityName = reader.IsDBNull(1) ? "No Activity Name" : reader.GetString(1);
                        DateTime datePosted = reader.GetDateTime(2); // Get the DatePosted

                        // Create a StackPanel for each activity
                        StackPanel activityPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal
                        };

                        TextBlock activityText = new TextBlock
                        {
                            Text = $"{activityName} - {datePosted:MM/dd/yyyy}",
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 14,
                            Margin = new Thickness(5)
                        };

                        // Delete button for each activity
                        Button deleteButton = new Button
                        {
                            Content = "Delete",
                            Width = 60,
                            Height = 30,
                            Margin = new Thickness(10, 0, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Tag = activityId // Store ActivityID in the Tag
                        };

                        deleteButton.Click += DeleteActivity_Click;

                        activityPanel.Children.Add(activityText);
                        activityPanel.Children.Add(deleteButton);

                        ActivityDeleteListBox.Items.Add(activityPanel);
                    }
                }
            }
        }

        // When the Grades button is clicked, show the ComboBox to select a course
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show the ComboBox to select a course
                CourseCombo.Visibility = Visibility.Visible;
                LoadCoursesForComboBox();  // Load the courses into the ComboBox
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Load available courses into the ComboBox
        private void LoadCoursesForComboBox()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = "SELECT CourseID, CourseName FROM Courses";  // Fetch courses for the teacher

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Clear the ComboBox before adding new courses
                    CourseCombo.Items.Clear();

                    // Populate ComboBox with courses
                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = reader.GetString(1), // Course name
                            Tag = reader.GetInt32(0) // CourseID
                        };
                        CourseCombo.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courses: {ex.Message}");
            }
        }

        // When a course is selected, load the students in that course
        private void CourseCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CourseCombo.SelectedItem != null)
                {
                    // Get the selected course
                    var selectedItem = (ComboBoxItem)CourseCombo.SelectedItem;
                    int selectedCourseId = (int)selectedItem.Tag;

                    // Load students for the selected course
                    LoadStudentsForCourse(selectedCourseId);

                    // Hide the ComboBox after selecting a course
                    CourseCombo.Visibility = Visibility.Collapsed;

                    // Show the student selection ComboBox
                    StudentCombo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting course: {ex.Message}");
            }
        }

        // Load students for the selected course
        // Load students for the selected course
        // Load students for the selected course
        private void LoadStudentsForCourse(int courseId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                // Updated query to ensure we only get students enrolled in this course
                string query = @"
        SELECT l.StudentID, l.FULLName
        FROM [LogIn] l
        INNER JOIN [StudentCourses] sc ON l.StudentID = sc.StudentID
        WHERE sc.CourseID = @courseId";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    StudentCombo.Items.Clear();

                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = reader.GetString(1), // FullName
                            Tag = reader.GetInt32(0) // StudentID
                        };
                        StudentCombo.Items.Add(item);
                    }

                    if (StudentCombo.Items.Count == 0)
                    {
                        MessageBox.Show("No students found for this course.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}");
            }
        }



        // When a student is selected, load the activities for grading
        private void StudentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (StudentCombo.SelectedItem != null)
                {
                    // Get the selected student
                    var selectedItem = (ComboBoxItem)StudentCombo.SelectedItem;
                    int selectedStudentId = (int)selectedItem.Tag;

                    // Load activities for the selected student
                    LoadActivitiesForStudent(selectedStudentId);

                    // Hide the ComboBox after selecting a student
                    StudentCombo.Visibility = Visibility.Collapsed;

                    // Show the grade entry sidebar
                    GradeEntrySidebar.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting student: {ex.Message}");
            }
        }

        // Load activities for the selected student
        private void LoadActivitiesForStudent(int studentId)
        {
            try
            {
                if (CourseCombo.SelectedItem == null)
                {
                    MessageBox.Show("Please select a course first.");
                    return;
                }

                int courseId = (int)((ComboBoxItem)CourseCombo.SelectedItem).Tag;
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                // Get all activities for the course, and include grade if it exists
                string query = @"
        SELECT 
            a.ActivityID, 
            a.ActivityName, 
            a.ScoreLimit,
            COALESCE(sa.Grade, -1) AS Grade
        FROM Activities a
        LEFT JOIN StudentActivities sa ON a.ActivityID = sa.ActivityID AND sa.StudentID = @studentId
        WHERE a.CourseID = @courseId
        ORDER BY a.DatePosted DESC";

                ActivityGradeListBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            MessageBox.Show("No activities available for this course yet.");
                            return;
                        }

                        while (reader.Read())
                        {
                            int activityId = reader.GetInt32(0);
                            string activityName = reader.GetString(1);
                            int scoreLimit = reader.GetInt32(2);
                            int grade = reader.GetInt32(3);

                            // Create UI elements for each activity
                            StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                            // Activity info
                            TextBlock activityText = new TextBlock
                            {
                                Text = $"{activityName} (Max: {scoreLimit})",
                                Width = 250,
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            // Grade input
                            TextBox gradeBox = new TextBox
                            {
                                Text = grade >= 0 ? grade.ToString() : "",
                                Width = 60,
                                Margin = new Thickness(10, 0, 10, 0)
                            };

                            // Submit button
                            Button submitButton = new Button
                            {
                                Content = grade >= 0 ? "Update" : "Submit",
                                Tag = activityId,
                                Width = 80,
                                Margin = new Thickness(0, 0, 10, 0)
                            };
                            submitButton.Click += SubmitGrade_Click;

                            panel.Children.Add(activityText);
                            panel.Children.Add(gradeBox);
                            panel.Children.Add(submitButton);

                            ActivityGradeListBox.Items.Add(panel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activities: {ex.Message}");
            }
        }

        // Submit grade for an activity
        private void SubmitGrade_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StudentCombo.SelectedItem == null)
                {
                    MessageBox.Show("Please select a student first.");
                    return;
                }

                var button = sender as Button;
                if (button == null) return;

                int activityId = (int)button.Tag;
                int studentId = (int)((ComboBoxItem)StudentCombo.SelectedItem).Tag;

                // Get the parent StackPanel
                var panel = button.Parent as StackPanel;
                if (panel == null) return;

                // Find the TextBox in the panel
                TextBox gradeBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                if (gradeBox == null || string.IsNullOrWhiteSpace(gradeBox.Text))
                {
                    MessageBox.Show("Please enter a grade.");
                    return;
                }

                if (!int.TryParse(gradeBox.Text, out int grade))
                {
                    MessageBox.Show("Please enter a valid numeric grade.");
                    return;
                }

                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // First check if the grade exists
                    string checkQuery = @"
            SELECT COUNT(*) 
            FROM StudentActivities 
            WHERE StudentID = @studentId AND ActivityID = @activityId";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@studentId", studentId);
                        checkCmd.Parameters.AddWithValue("@activityId", activityId);
                        bool gradeExists = (int)checkCmd.ExecuteScalar() > 0;

                        // Use appropriate query based on whether grade exists
                        string query = gradeExists ?
                            "UPDATE StudentActivities SET Grade = @grade WHERE StudentID = @studentId AND ActivityID = @activityId" :
                            "INSERT INTO StudentActivities (StudentID, ActivityID, Grade) VALUES (@studentId, @activityId, @grade)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@studentId", studentId);
                            cmd.Parameters.AddWithValue("@activityId", activityId);
                            cmd.Parameters.AddWithValue("@grade", grade);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Grade saved successfully!");
                                // Refresh the activities list
                                LoadActivitiesForStudent(studentId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving grade: {ex.Message}");
            }
        }

        // Save the grade to the database
        private void SaveGradeToDatabase(int activityId, int grade)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = @"
            UPDATE Activities
            SET Grade = @grade
            WHERE ActivityID = @activityId";  // Save grade for the selected activity

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@activityId", activityId);
                    cmd.Parameters.AddWithValue("@grade", grade);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving grade: {ex.Message}");
            }
        }








    }
}
