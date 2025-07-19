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
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace GroupApp
{
    /// <summary>
    /// Interaction logic for Student_List.xaml
    /// </summary>
    public partial class Student_List : Window
    {
        public Student_List()
        {
            InitializeComponent(); // ✅ Must be first!

            LoadStudents(); // ✅ Call your method AFTER UI is initialized
        }
        private List<string> allStudents = new List<string>();

        private void LoadStudents()
        {
            allStudents.Clear();
            StudentListBox.Items.Clear();

            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            string query = @"
                SELECT l.FullName, l.Username, c.CourseName
                FROM LogIn l
                LEFT JOIN Courses c ON l.CourseID = c.CourseID
                WHERE l.Username LIKE '%@mymail.edu.ph'";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fullName = reader.IsDBNull(0) ? "[No Name]" : reader.GetString(0);
                        string username = reader.GetString(1);
                        string course = reader.IsDBNull(2) ? "[No Course]" : reader.GetString(2);

                        string display = $"{fullName} ({username}) - {course}";
                        allStudents.Add(display);
                    }
                }
            }

            FilterStudents();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterStudents();
        }

        private void FilterStudents()
        {
            string query = SearchBox.Text.Trim().ToLower();
            StudentListBox.Items.Clear();

            foreach (var student in allStudents)
            {
                if (student.ToLower().Contains(query))
                {
                    StudentListBox.Items.Add(student);
                }
            }

            if (StudentListBox.Items.Count == 0)
                StudentListBox.Items.Add("No matching students.");
        }

        private void StudentListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (StudentListBox.SelectedItem == null) return;

            string? selected = StudentListBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            string email = selected.Substring(selected.IndexOf('(') + 1).Split(')')[0];

            var assignWindow = new AssignCourseWindow(email);
            bool? result = assignWindow.ShowDialog();

            if (result == true)
            {
                LoadStudents(); // Refresh list after update
            }
        }

        private int GetCourseIdByName(string courseName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = "SELECT CourseID FROM Courses WHERE CourseName = @name";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@name", courseName);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        private void UpdateStudentCourse(string email, int courseId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = "UPDATE LogIn SET CourseID = @cid WHERE Username = @uname";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@cid", courseId);
                cmd.Parameters.AddWithValue("@uname", email);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}