﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GroupApp
{
    /// <summary>
    /// Interaction logic for OperatingSys_Course.xaml
    /// </summary>
    public partial class OperatingSys_Course : Window
    {
        private int courseId;
        private string studentEmail;
        public OperatingSys_Course(int courseId, string studentEmail)
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
            // Show the ActivityStack to display activities
            ActivityPopup.IsOpen = true;

            // Load the activities related to "Discussion"
            LoadCourseActivities(courseId, "Discussion");
        }

        private void CourseWorkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening Course Work...");
            // Show the ActivityStack to display activities
            ActivityPopup.IsOpen = true;

            // Load the activities
            LoadCourseActivities(courseId, "Course Work");
        }
        private void LoadCourseActivities(int courseId, string section)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            string query = @"
        SELECT ActivityName, DatePosted
        FROM Activities
        WHERE CourseID = @courseId AND Section = @section";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                cmd.Parameters.AddWithValue("@section", section); // Filter by 'Course Work' or 'Discussion'
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // Clear existing items in the ListBox
                    ActivityListBox.Items.Clear();

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

        private void CloseActivityPopup_Click(object sender, RoutedEventArgs e)
        {
            // Close the activity popup when "X" button inside the Popup is clicked
            ActivityPopup.IsOpen = false;
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
            // Show the ActivityStack to display activities
            ActivityPopup.IsOpen = true;

            // Load the activities
            LoadCourseActivities(courseId, "Quiz");
        }

        private void ViewAnnouncements_Click(object sender, RoutedEventArgs e)
        {
            LoadCourseAnnouncements();
            AnnouncementsPopup.IsOpen = true;
        }

        private void CloseAnnouncementsPopup_Click(object sender, RoutedEventArgs e)
        {
            AnnouncementsPopup.IsOpen = false;
        }

        private void LoadCourseAnnouncements()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = @"SELECT a.AnnouncementText, a.PostedDate, a.PostedBy 
                        FROM Announcements a
                        INNER JOIN StudentCourses sc ON a.CourseID = sc.CourseID
                        WHERE sc.StudentID = @studentId
                        AND a.CourseID = @courseId
                        ORDER BY a.PostedDate DESC";

                AnnouncementsListBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Get student ID from email
                    int studentId = GetStudentId(studentEmail);

                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AnnouncementsListBox.Items.Add(new
                            {
                                AnnouncementText = reader.GetString(0),
                                PostedDate = reader.GetDateTime(1),
                                PostedBy = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading announcements: {ex.Message}");
            }
        }

        private int GetStudentId(string email)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = "SELECT StudentID FROM LogIn WHERE Username = @email";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void GradebookButton_Click(object sender, RoutedEventArgs e)
        {
            LoadGradebookData();
            GradebookPopup.IsOpen = true;
        }

        private void CloseGradebookPopup_Click(object sender, RoutedEventArgs e)
        {
            GradebookPopup.IsOpen = false;
        }

        private void LoadGradebookData()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = @"SELECT a.ActivityName, a.ScoreLimit, sa.Grade
                FROM StudentActivities sa
                JOIN Activities a ON sa.ActivityID = a.ActivityID
                JOIN StudentCourses sc ON a.CourseID = sc.CourseID
                WHERE sc.StudentID = @studentId
                AND a.CourseID = @courseId
                ORDER BY a.DatePosted DESC";

                GradebookListView.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    int studentId = GetStudentId(studentEmail);

                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int scoreLimit = reader.GetInt32(1);
                            int grade = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            double percentage = scoreLimit == 0 ? 0 : Math.Round((double)grade / scoreLimit * 100, 2);

                            GradebookListView.Items.Add(new
                            {
                                ActivityName = reader.GetString(0),
                                ScoreLimit = scoreLimit,
                                Grade = grade,
                                Percentage = percentage + "%"  // Add percentage column
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading gradebook: {ex.Message}");
            }
        }



    }
}