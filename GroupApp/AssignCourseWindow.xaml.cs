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
using System.Data;

namespace GroupApp
{
    public partial class AssignCourseWindow : Window
    {
        public string StudentEmail { get; }
        public AssignCourseWindow(string email)
        {
            InitializeComponent();
            StudentEmail = email;
            StudentLabel.Text = $"Assign course to: {email}";
            LoadCourses();
        }

        private void LoadCourses()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            string query = "SELECT CourseID, CourseName FROM Courses";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ComboBoxItem
                        {
                            Content = reader["CourseName"].ToString(),
                            Tag = reader["CourseID"]
                        };
                        CourseComboBox.Items.Add(item);
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CourseComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int courseId = (int)selectedItem.Tag;
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

                // First, update the CourseID in the LogIn table
                string updateQuery = "UPDATE LogIn SET CourseID = @cid WHERE Username = @uname";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@cid", courseId);
                    cmd.Parameters.AddWithValue("@uname", StudentEmail);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Now insert into the StudentCourses table to link the student to the course
                string insertQuery = "INSERT INTO StudentCourses (StudentID, CourseID) " +
                                     "SELECT StudentID, @cid FROM LogIn WHERE Username = @uname";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@cid", courseId);
                    cmd.Parameters.AddWithValue("@uname", StudentEmail);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Course updated and assigned to student.");
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a course.");
            }
        }


        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            // Remove from the StudentCourses table first
            string deleteQuery = "DELETE FROM StudentCourses WHERE StudentID = (SELECT StudentID FROM LogIn WHERE Username = @uname)";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@uname", StudentEmail);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Then set CourseID to NULL in LogIn table
            string updateQuery = "UPDATE LogIn SET CourseID = NULL WHERE Username = @uname";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@uname", StudentEmail);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Student removed from course.");
            this.DialogResult = true;
            this.Close();
        }
    }
}
