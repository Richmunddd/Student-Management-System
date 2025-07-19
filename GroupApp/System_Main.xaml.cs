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
            SELECT c.CourseName
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
                    while (reader.Read())
                    {
                        string courseName = reader.GetString(0);

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

                        courseBtn.Click += (s, e) =>
                        {
                            MessageBox.Show($"Opening {courseName}...");
                            // TODO: Add navigation to course details window
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
            Close(); // Corrected the method call to close the current window
        }
    }

}