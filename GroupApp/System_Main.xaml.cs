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
        public System_Main(string studentEmail)
        {
            InitializeComponent();
            LoadStudentCourses(studentEmail);
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
    }

}