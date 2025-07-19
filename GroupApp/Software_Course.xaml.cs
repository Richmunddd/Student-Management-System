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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;

namespace GroupApp
{
    /// <summary>
    /// Interaction logic for Software_Course.xaml
    /// </summary>
    public partial class Software_Course : Window
    {
        private int courseId;
        private string studentEmail;
        public Software_Course(int courseId, string studentEmail)
        {
            InitializeComponent();
            this.courseId = courseId;
            LoadCourseDetails(courseId);
            this.studentEmail = studentEmail;
        }

        private void LoadCourseDetails(int courseId)
        {
            // Logic to load course details based on courseId
            // This could involve querying a database or other data source
            // and populating the UI elements with the course information.
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //xxxx
            Application.Current.Shutdown();

        }
        // Event handler for the "Content" button click
        private void ContentButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the visibility of the Course Content section
            if (CourseContentStack.Visibility == Visibility.Collapsed)
            {
                CourseContentStack.Visibility = Visibility.Visible;
            }
            else
            {
                CourseContentStack.Visibility = Visibility.Collapsed;
            }
        }
      

        private void DiscussionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening Discussions...");
        }

        private void CourseWorkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening Course Work...");
            // Show the ActivityStack to display activities
            ActivityStack.Visibility = Visibility.Visible;

            // Load the activities
            LoadCourseActivities(courseId);
        }
        private void LoadCourseActivities(int courseId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            string query = @"
        SELECT ActivityName, DatePosted
        FROM Activities
        WHERE CourseID = @courseId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        // If no rows are returned, show a message
                        MessageBox.Show("No activities found for this course.");
                        return; // Exit early if no activities are found
                    }

                    // Clear existing items in the ListBox
                    ActivityListBox.Items.Clear();

                    // Read and display the activities
                    while (reader.Read())
                    {
                        string activityName = reader.IsDBNull(0) ? "No Activity Name" : reader.GetString(0);
                        DateTime datePosted = reader.GetDateTime(1); // Get the DatePosted

                        // Add the activity to the ListBox
                        ActivityListBox.Items.Add($"{activityName} - {datePosted.ToString("MM/dd/yyyy")}");
                    }
                }
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Hide();
            System_Main systemMain = new System_Main(studentEmail);
            systemMain.Show();
            this.Close();
        }
        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, show a message box when the Quiz button is clicked
            MessageBox.Show("Opening Quiz...");
        }


    }
}
