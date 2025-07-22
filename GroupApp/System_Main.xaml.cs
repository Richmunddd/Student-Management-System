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
using Microsoft.Data.SqlClient;

namespace GroupApp
{
    /// <summary>
    /// Interaction logic for System_Main.xaml
    /// </summary>
    public partial class System_Main : Window
    {
        private string studentEmail;
        private int studentId;

        public System_Main(string studentEmail)
        {
            this.studentEmail = studentEmail;
            this.studentId = GetStudentId(studentEmail); // Get and store studentId on initialization
            InitializeComponent();
            LoadStudentCourses(studentEmail);
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
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        private void LoadStudentCourses(string studentEmail)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = @"
    SELECT c.CourseID, c.CourseName
    FROM LogIn l
    INNER JOIN StudentCourses sc ON l.StudentID = sc.StudentID
    INNER JOIN Courses c ON sc.CourseID = c.CourseID
    WHERE l.Username = @email";

            CourseButtonPanel.Children.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@email", studentEmail);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        // No courses found for the student
                        MessageBox.Show("No courses found for this student.");
                        return; // Exit the method early if no courses are found
                    }

                    while (reader.Read())
                    {
                        // Check if courseId or courseName is null
                        if (reader.IsDBNull(0) || reader.IsDBNull(1))
                        {
                            MessageBox.Show("Invalid data retrieved from the database.");
                            continue;
                        }

                        string courseName = reader.GetString(1);
                        int courseId = reader.GetInt32(0); // Fetch the courseId

                        Button courseBtn = new Button
                        {
                            Content = courseName,
                            Height = 60,
                            Margin = new Thickness(0, 5, 0, 5),
                            FontSize = 16,
                            FontWeight = FontWeights.Bold,
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Black,
                            Foreground = Brushes.White
                        };

                        // When a course button is clicked, navigate to the appropriate course window
                        courseBtn.Click += (s, e) =>
                        {
                            MessageBox.Show($"Opening {courseName}...");

                            if (courseName == "Software Design")
                            {
                                // If course is Software Design, open Software_Course
                                Software_Course courseWindow = new Software_Course(courseId, studentEmail);
                                courseWindow.Show();
                            }
                            else if (courseName == "Operating Systems")
                            {
                                // If course is Operating System, open OperatingSys_Course
                                OperatingSys_Course courseWindow = new OperatingSys_Course(courseId, studentEmail);
                                courseWindow.Show();
                            }

                            this.Close(); // Close the current window (System_Main)
                        };

                        CourseButtonPanel.Children.Add(courseBtn);
                    }
                }
            }
        }

        // Your existing event handlers
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ShowGradesPopup();
        }

        private void ShowGradesPopup()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                string query = @"
            SELECT 
                c.CourseName,
                ISNULL(SUM(CASE WHEN a.ActivityName LIKE 'CW%' THEN sa.Grade ELSE 0 END), 0) AS CourseWork,
                ISNULL(SUM(CASE WHEN a.ActivityName LIKE 'Quiz%' THEN sa.Grade ELSE 0 END), 0) AS Quiz,
                ISNULL(SUM(CASE WHEN a.ActivityName LIKE 'D%' THEN sa.Grade ELSE 0 END), 0) AS Discussion
            FROM StudentCourses sc
            JOIN Courses c ON sc.CourseID = c.CourseID
            LEFT JOIN Activities a ON c.CourseID = a.CourseID
            LEFT JOIN StudentActivities sa ON a.ActivityID = sa.ActivityID AND sa.StudentID = sc.StudentID
            WHERE sc.StudentID = @studentId
            GROUP BY c.CourseName
            ORDER BY c.CourseName";

                List<CourseGrade> courseGrades = new List<CourseGrade>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            double courseWork = Convert.ToDouble(reader["CourseWork"]);
                            double quiz = Convert.ToDouble(reader["Quiz"]);
                            double discussion = Convert.ToDouble(reader["Discussion"]);

                            double totalPercentage = 0;
                            bool hasGrades = courseWork + quiz + discussion > 0;

                            if (hasGrades)
                            {
                                totalPercentage = (courseWork * 0.4) + (quiz * 0.5) + (discussion * 0.1);
                            }

                            courseGrades.Add(new CourseGrade
                            {
                                CourseName = reader.GetString(0),
                                CourseWork = courseWork,
                                Quiz = quiz,
                                Discussion = discussion,
                                TotalPercentage = Math.Round(totalPercentage, 2),
                                PointEquivalent = hasGrades ? CalculatePointEquivalent(totalPercentage) : "No grades yet"
                            });
                        }
                    }
                }

                GradesWindow gradesWindow = new GradesWindow(courseGrades);
                gradesWindow.Owner = this;
                gradesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading grades: {ex.Message}");
            }
        }
        private string CalculatePointEquivalent(double percentage)
        {
            if (percentage >= 96) return "1.00";
            if (percentage >= 92) return "1.25";
            if (percentage >= 88) return "1.50";
            if (percentage >= 85) return "1.75";
            if (percentage >= 82) return "2.00";
            if (percentage >= 79) return "2.25";
            if (percentage >= 76) return "2.50";
            if (percentage >= 73) return "2.75";
            if (percentage >= 70) return "3.00";
            return "5.00";
        }

    }
    

}