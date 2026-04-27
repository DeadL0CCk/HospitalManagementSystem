using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;

namespace HospitalManagementSystem.UI
{
    public class LoginForm : Form
    {
        private AuthenticationService _authService;
        private TextBox txtUserId;
        private TextBox txtPassword;
        private Button btnLogin;

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);

        public LoginForm()
        {
            _authService = new AuthenticationService();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 550);
            this.BackColor = Color.White;

            // Left panel: branding
            Panel pnlLeft = new Panel
            {
                Width = 380,
                Height = this.Height,
                Left = 0,
                Top = 0,
                BackColor = PrimaryColor
            };
            Label lblTitle = new Label
            {
                Text = "CareTech\nHMS",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 40,
                Top = 150,
                AutoSize = true
            };
            Label lblSub = new Label
            {
                Text = "Enterprise Hospital\nManagement System",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(220, 240, 255),
                Left = 55,
                Top = 360,
                AutoSize = true
            };
            pnlLeft.Controls.Add(lblTitle);
            pnlLeft.Controls.Add(lblSub);

            // Right panel: form inputs
            Panel pnlRight = new Panel
            {
                Left = 380,
                Top = 0,
                Width = this.Width - 380,
                Height = this.Height,
                BackColor = Color.White
            };

            Label lblWelcome = new Label
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Left = 60,
                Top = 40,
                AutoSize = true
            };

            // User ID
            Label lblUserId = new Label
            {
                Text = "USER ID",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                Left = 60,
                Top = 110,
                AutoSize = true
            };
            txtUserId = new TextBox
            {
                Location = new Point(60, 130),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password
            Label lblPassword = new Label
            {
                Text = "PASSWORD",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                Left = 60,
                Top = 180,
                AutoSize = true
            };
            txtPassword = new TextBox
            {
                Location = new Point(60, 200),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Sign In button
            btnLogin = new Button
            {
                Text = "SIGN IN",
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 50),
                Location = new Point(60, 280),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Exit button
            Button btnExit = new Button
            {
                Text = "EXIT",
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(380, 50),
                Location = new Point(60, 350),
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => Application.Exit();

            // Close button (top‑right X)
            Button btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gray,
                Location = new Point(pnlRight.Width - 45, 15),
                Size = new Size(35, 35),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            pnlRight.Controls.Add(lblWelcome);
            pnlRight.Controls.Add(lblUserId);
            pnlRight.Controls.Add(txtUserId);
            pnlRight.Controls.Add(lblPassword);
            pnlRight.Controls.Add(txtPassword);
            pnlRight.Controls.Add(btnLogin);
            pnlRight.Controls.Add(btnExit);
            pnlRight.Controls.Add(btnClose);

            this.Controls.Add(pnlLeft);
            this.Controls.Add(pnlRight);

            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0x112, 0xf012, 0); } };
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string userIdText = txtUserId.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(userIdText) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter User ID and Password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(userIdText, out int userId))
            {
                MessageBox.Show("User ID must be a number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Auto‑detect role from database
                string role = _authService.AuthenticateAndGetRole(userId, password);
                if (role != null)
                {
                    MessageBox.Show($"Login successful! Welcome, {role}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Form dashboard = null;
                    switch (role.ToLower())
                    {
                        case "admin":
                            dashboard = new AdminDashboard(userId);
                            break;
                        case "doctor":
                            dashboard = new DoctorDashboard(userId);
                            break;
                        case "receptionist":
                            dashboard = new ReceptionistDashboard(userId);
                            break;
                        default:
                            MessageBox.Show("Unknown role.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                    }
                    this.Hide();
                    dashboard.Show();
                    dashboard.FormClosed += (s, args) => this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid User ID or Password.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtUserId.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}