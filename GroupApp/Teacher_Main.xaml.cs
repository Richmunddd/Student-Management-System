using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
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

namespace GroupApp
{
    /// <summary>
    /// Interaction logic for Teacher_Main.xaml
    /// </summary>
    /// 
    
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
        // Class level variable
        private List<Course> _availableCourses = new List<Course>();

        private void ShowDeleteActivitySidebar(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide other panels
                ActivitySidebar.Visibility = Visibility.Collapsed;
                GradeEntrySidebar.Visibility = Visibility.Collapsed;

                // Show delete sidebar
                DeleteActivitySidebar.Visibility = Visibility.Visible;

                // Initialize the course combo box
                InitializeDeleteActivityCourseCombo();

                // Hide activities list initially
                ActivityDeleteListBox.Visibility = Visibility.Collapsed;
                NoActivitiesText.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing delete sidebar: {ex.Message}");
            }
        }

        private void InitializeDeleteActivityCourseCombo()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = "SELECT CourseID, CourseName FROM Courses";

                _availableCourses.Clear();
                DeleteActivityCourseCombo.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _availableCourses.Add(new Course
                            {
                                CourseID = reader.GetInt32(0),
                                CourseName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                            });
                        }
                    }
                }

                DeleteActivityCourseCombo.ItemsSource = _availableCourses;

                if (_availableCourses.Count > 0)
                {
                    DeleteActivityCourseCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courses: {ex.Message}");
            }
        }

        private void DeleteActivityCourseCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeleteActivityCourseCombo.SelectedItem is Course selectedCourse)
            {
                LoadActivitiesForDeletion(selectedCourse.CourseID);
            }
        }

        private void LoadActivitiesForDeletion(int courseId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = @"
SELECT ActivityID, ActivityName, DatePosted
FROM Activities
WHERE CourseID = @courseId
ORDER BY DatePosted DESC";

                ActivityDeleteListBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool hasActivities = false;

                        while (reader.Read())
                        {
                            hasActivities = true;
                            ActivityDeleteListBox.Items.Add(new
                            {
                                ActivityID = reader.GetInt32(0),
                                ActivityName = reader.IsDBNull(1) ? "Unnamed Activity" : reader.GetString(1),
                                DatePosted = reader.GetDateTime(2)
                            });
                        }

                        // Show appropriate UI based on whether activities exist
                        ActivityDeleteListBox.Visibility = hasActivities ? Visibility.Visible : Visibility.Collapsed;
                        NoActivitiesText.Visibility = hasActivities ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activities: {ex.Message}");
            }
        }

        private int GetSelectedCourseId()
        {
            // Implement based on how you select courses in your UI
            // Example if you have a course selection button or combo box:
            if (CourseCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int courseId)
            {
                return courseId;
            }

            // Or if you select courses by clicking buttons:
            // return selectedCourseId; // (you'll need to set this when selecting a course)

            return -1; // Indicates no course selected
        }

        // Toggle to collapse or show the Delete Activity Sidebar
        private void ToggleDeleteActivitySidebar(object sender, RoutedEventArgs e)
        {
            // Toggle the visibility of the DeleteActivitySidebar
            DeleteActivitySidebar.Visibility = (DeleteActivitySidebar.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag == null || !(button.Tag is int activityId))
                {
                    MessageBox.Show("Invalid activity selected");
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to delete this activity?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteActivityFromDatabase(activityId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during deletion: {ex.Message}");
            }
        }

        private void DeleteActivityFromDatabase(int activityId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // First delete from StudentActivities (if needed)
                    string deleteGradesQuery = "DELETE FROM StudentActivities WHERE ActivityID = @activityId";
                    using (SqlCommand cmd = new SqlCommand(deleteGradesQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@activityId", activityId);
                        cmd.ExecuteNonQuery();
                    }

                    // Then delete the activity
                    string deleteActivityQuery = "DELETE FROM Activities WHERE ActivityID = @activityId";
                    using (SqlCommand cmd = new SqlCommand(deleteActivityQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@activityId", activityId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Activity deleted successfully!");

                            // Refresh the activities list for the current course
                            if (DeleteActivityCourseCombo.SelectedItem is Course selectedCourse)
                            {
                                LoadActivitiesForDeletion(selectedCourse.CourseID);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Activity not found or already deleted.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting activity: {ex.Message}");
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
        private void ResetGradingUI()
        {
            // Hide all grading-related UI elements
            GradeEntrySidebar.Visibility = Visibility.Collapsed;
            CourseCombo.Visibility = Visibility.Collapsed;
            StudentCombo.Visibility = Visibility.Collapsed;

            // Clear all selections and data
            CourseCombo.SelectedItem = null;
            StudentCombo.SelectedItem = null;
            ActivityGradeListBox.ItemsSource = null;
            SelectedStudentText.Text = string.Empty;
        }

        // When the Grades button is clicked, show the ComboBox to select a course
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            // First reset any existing grading UI
            ResetGradingUI();

            // Then show just the course selection
            CourseCombo.Visibility = Visibility.Visible;

            // Load courses
            LoadCoursesForComboBox();
        }

        private void ToggleGradeEntrySidebar(object sender, RoutedEventArgs e)
        {
            // Hide the grade entry sidebar
            GradeEntrySidebar.Visibility = Visibility.Collapsed;

            // Also hide the course and student combo boxes
            CourseCombo.Visibility = Visibility.Collapsed;
            StudentCombo.Visibility = Visibility.Collapsed;

            // Clear the selections
            CourseCombo.SelectedItem = null;
            StudentCombo.SelectedItem = null;

            // Clear the activities list
            ActivityGradeListBox.ItemsSource = null;
        }

        // Load available courses into the ComboBox
        private void LoadCoursesForComboBox()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = "SELECT CourseID, CourseName FROM Courses";

                CourseCombo.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CourseCombo.Items.Add(new ComboBoxItem
                        {
                            Content = reader.GetString(1),
                            Tag = reader.GetInt32(0)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courses: {ex.Message}");
            }
        }
        

        // Load students for the selected course
        private void LoadStudentsForCourse(int courseId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = @"
            SELECT l.StudentID, l.FULLName 
            FROM LogIn l
            INNER JOIN StudentCourses sc ON l.StudentID = sc.StudentID
            WHERE sc.CourseID = @courseId";

                StudentCombo.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        StudentCombo.Items.Add(new ComboBoxItem
                        {
                            Content = reader.GetString(1),
                            Tag = reader.GetInt32(0)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}");
            }
        }

        // When a student is selected from the combo box
        private void StudentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (StudentCombo.SelectedItem != null)
                {
                    var selectedStudent = (ComboBoxItem)StudentCombo.SelectedItem;
                    int studentId = (int)selectedStudent.Tag;

                    // Load and show activities/grades for selected student
                    LoadActivitiesForStudent(studentId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting student: {ex.Message}");
            }
        }

        // When a course is selected, load the students in that course
        private void CourseCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CourseCombo.SelectedItem != null)
                {
                    var selectedCourse = (ComboBoxItem)CourseCombo.SelectedItem;
                    int courseId = (int)selectedCourse.Tag;

                    // Show student selection
                    StudentCombo.Visibility = Visibility.Visible;

                    // Load students for selected course
                    LoadStudentsForCourse(courseId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting course: {ex.Message}");
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

                // Get the selected student's name
                var selectedStudent = (ComboBoxItem)StudentCombo.SelectedItem;
                SelectedStudentText.Text = $"Student: {selectedStudent.Content}";

                int courseId = (int)((ComboBoxItem)CourseCombo.SelectedItem).Tag;
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                string query = @"
        SELECT 
            a.ActivityID, 
            a.ActivityName, 
            a.ScoreLimit,
            ISNULL(sa.Grade, 0) AS Grade
        FROM Activities a
        LEFT JOIN StudentActivities sa ON a.ActivityID = sa.ActivityID AND sa.StudentID = @studentId
        WHERE a.CourseID = @courseId
        ORDER BY a.DatePosted DESC";

                var activities = new List<ActivityGradeModel>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            activities.Add(new ActivityGradeModel
                            {
                                ActivityID = reader.GetInt32(0),
                                ActivityName = reader.GetString(1),
                                ScoreLimit = reader.GetInt32(2),
                                Grade = reader.GetInt32(3)
                            });
                        }
                    }
                }

                if (activities.Count == 0)
                {
                    MessageBox.Show("No activities found for this student/course.");
                    return;
                }

                ActivityGradeListBox.ItemsSource = activities;
                GradeEntrySidebar.Visibility = Visibility.Visible;
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
                var stackPanel = button.Parent as StackPanel;
                if (stackPanel == null) return;

                // Find the TextBox in the panel
                var gradeBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault();
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

                    string checkQuery = "SELECT COUNT(*) FROM StudentActivities WHERE StudentID = @studentId AND ActivityID = @activityId";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@studentId", studentId);
                        checkCmd.Parameters.AddWithValue("@activityId", activityId);
                        bool exists = (int)checkCmd.ExecuteScalar() > 0;

                        string query = exists ?
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
                                // Refresh the list
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

        




        /////
        ///
        private void AnnouncementButton_Click(object sender, RoutedEventArgs e)
        {
            // Your existing code to show the sidebar
            AnnouncementSidebar.Visibility = Visibility.Visible;
            LoadCoursesForAnnouncement();
        }

        private void LoadCoursesForAnnouncement()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                string query = "SELECT CourseID, CourseName FROM Courses";

                AnnouncementCourseCombo.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AnnouncementCourseCombo.Items.Add(new
                            {
                                CourseID = reader.GetInt32(0),
                                CourseName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courses: {ex.Message}");
            }
        }

        // Placeholder text handlers
        private void AnnouncementTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AnnouncementTextBox.Text == "Enter your announcement here...")
            {
                AnnouncementTextBox.Text = "";
                AnnouncementTextBox.Foreground = Brushes.Black;
            }
        }

        private void AnnouncementTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AnnouncementTextBox.Text))
            {
                AnnouncementTextBox.Text = "Enter your announcement here...";
                AnnouncementTextBox.Foreground = Brushes.Gray;
            }
        }

        // Post announcement
        private void PostAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            if (AnnouncementCourseCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a course first");
                return;
            }

            if (AnnouncementTextBox.Text == "Enter your announcement here..." ||
                string.IsNullOrWhiteSpace(AnnouncementTextBox.Text))
            {
                MessageBox.Show("Please enter announcement text");
                return;
            }

            try
            {
                dynamic selectedCourse = AnnouncementCourseCombo.SelectedItem;
                int courseId = selectedCourse.CourseID;
                string announcementText = AnnouncementTextBox.Text;
                string postedBy = teacherEmail; // Make sure teacherEmail is set

                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                // First check if table exists
                string checkTableQuery = @"
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Announcements')
        BEGIN
            CREATE TABLE Announcements (
                AnnouncementID INT IDENTITY(1,1) PRIMARY KEY,
                CourseID INT NOT NULL,
                AnnouncementText NVARCHAR(MAX) NOT NULL,
                PostedDate DATETIME NOT NULL,
                PostedBy NVARCHAR(100) NOT NULL,
                FOREIGN KEY (CourseID) REFERENCES Courses(CourseID)
            )
        END";

                // Then insert the announcement
                string insertQuery = @"
        INSERT INTO Announcements 
        (CourseID, AnnouncementText, PostedDate, PostedBy)
        VALUES (@courseId, @text, @date, @teacher)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Create table if it doesn't exist
                    using (SqlCommand cmd = new SqlCommand(checkTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insert the announcement
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseId", courseId);
                        cmd.Parameters.AddWithValue("@text", announcementText);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@teacher", postedBy);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Announcement posted successfully!");
                AnnouncementTextBox.Text = "Enter your announcement here...";
                AnnouncementTextBox.Foreground = Brushes.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error posting announcement: {ex.Message}\n\nFull error details:\n{ex.ToString()}");
            }
        }

        // Close sidebar
        private void CloseAnnouncementSidebar_Click(object sender, RoutedEventArgs e)
        {
            AnnouncementSidebar.Visibility = Visibility.Collapsed;
            AnnouncementTextBox.Text = "Enter your announcement here...";
            AnnouncementTextBox.Foreground = Brushes.Gray;
        }



    }
}
