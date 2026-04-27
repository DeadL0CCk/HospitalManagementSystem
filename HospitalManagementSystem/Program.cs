using System;
using System.Windows.Forms;
using HospitalManagementSystem.UI;
using HospitalManagementSystem.Utils;

namespace HospitalManagementSystem
{
    static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Test database connection on startup
            try
            {
                SqlConnectionHelper dbHelper = SqlConnectionHelper.Instance;
                if (dbHelper.TestConnection())
                {
                    Application.Run(new LoginForm());
                }
                else
                {
                    MessageBox.Show("Failed to connect to the database...");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Application Error: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
