using System;
using System.Drawing;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;

namespace HospitalManagementSystem.UI
{
    public class DoctorDashboard : Form
    {
        private int _doctorId;
        private AppointmentService _appointmentService;
        private Label lblStats;

        public DoctorDashboard(int doctorId)
        {
            _doctorId = doctorId;
            _appointmentService = new AppointmentService();
            InitializeComponent();
            LoadStats();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 750);
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Text = "CareTech - Doctor Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.FromArgb(17, 24, 39) };
            Label lblBrand = new Label
            {
                Text = "CareTech Doctor",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true
            };
            pnlSidebar.Controls.Add(lblBrand);
            int y = 100;
            AddSidebarButton(pnlSidebar, "My Schedule", y, (s, e) => new DoctorScheduleForm(_doctorId).ShowDialog());
            AddSidebarButton(pnlSidebar, "Write Prescription", y + 60, (s, e) => new PrescriptionForm(_doctorId).ShowDialog());
            AddSidebarButton(pnlSidebar, "Search Patient", y + 120, (s, e) => new PatientSearchForm().ShowDialog());
            Button btnLogout = AddSidebarButton(pnlSidebar, "Logout", 0, (s, e) => { this.Close(); new LoginForm().Show(); });
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Height = 60;
            btnLogout.BackColor = Color.FromArgb(31, 41, 55);

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };
            Label lblWelcome = new Label
            {
                Text = $"Welcome, Dr. ID: {_doctorId}",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            lblStats = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 12),
                Location = new Point(0, 60),
                AutoSize = true
            };
            pnlMain.Controls.Add(lblWelcome);
            pnlMain.Controls.Add(lblStats);

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlSidebar);
        }

        private Button AddSidebarButton(Panel parent, string text, int y, EventHandler click)
        {
            Button btn = new Button
            {
                Text = "   " + text,
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Height = 50,
                Width = parent.Width,
                Location = new Point(0, y),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 41, 55);
            btn.Click += click;
            parent.Controls.Add(btn);
            return btn;
        }

        private void LoadStats()
        {
            var appointments = _appointmentService.GetDoctorSchedule(_doctorId);
            lblStats.Text = $"You have {appointments.Count} upcoming appointments. {appointments.FindAll(a => a.Status == "Pending").Count} pending.";
        }
    }
}