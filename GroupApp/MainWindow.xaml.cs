using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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
using System.Configuration;  // Required for ConfigurationManager





namespace GroupApp
{
    /// <summary>  
    /// Interaction logic for MainWindow.xaml  
    /// </summary>  
    public partial class MainWindow : Window
    {

        // private string connectionString = "Data Source = MSI\\SQL2022; Initial Catalog = LogInDataBase; Integrated Security = true";
        public MainWindow()
        {
            InitializeComponent();
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
            string username = Username.Text.Trim();
            string password = Password.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and Password cannot be empty.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM LogIn WHERE Username = @Username AND Password = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        int userExists = (int)cmd.ExecuteScalar();

                        if (userExists > 0)
                        {
                            if (username.EndsWith("@mymail.edu.ph", StringComparison.OrdinalIgnoreCase))
                            {
                                // STUDENT window
                                System_Main studentWindow = new System_Main(username);
                                studentWindow.Show();
                            }
                            else if (username.EndsWith("@mymail.ph", StringComparison.OrdinalIgnoreCase))
                            {
                                // TEACHER window
                                Teacher_Main teacherWindow = new Teacher_Main();
                                teacherWindow.Show();
                            }
                            else
                            {
                                MessageBox.Show("Unknown user type. Access denied.");
                                return;
                            }

                            this.Close(); // Close login window after redirect
                        }
                        else
                        {
                            MessageBox.Show("Access Denied: Invalid username or password.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
            }
        }
    }
}