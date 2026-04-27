using System;
using System.Drawing;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;

namespace HospitalManagementSystem.UI
{
    public class AdminDashboard : Form
    {
        private int _userId;
        private DoctorService _doctorService;
        private PatientService _patientService;
        private MedicineService _medicineService;
        private AppointmentService _appointmentService;

        private readonly Color SidebarColor = Color.FromArgb(17, 24, 39);
        private readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
        private readonly Color BgLight = Color.FromArgb(243, 244, 246);

        public AdminDashboard(int userId)
        {
            _userId = userId;
            _doctorService = new DoctorService();
            _patientService = new PatientService();
            _medicineService = new MedicineService();
            _appointmentService = new AppointmentService();

            // Form properties
            this.Text = "CareTech HMS - Admin Control Center";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = BgLight;
            this.Font = new Font("Segoe UI", 10F);

            // Build the UI (only once)
            BuildUI();

            // Load stats after UI is ready
            LoadDashboardStats();
        }

        private void BuildUI()
        {
            // ========== SIDEBAR ==========
            Panel pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = SidebarColor };
            Label lblBrand = new Label
            {
                Text = "CareTech Admin",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true
            };
            pnlSidebar.Controls.Add(lblBrand);

            int btnY = 110;
            CreateSidebarButton(pnlSidebar, "Doctor Management", btnY, BtnManageDoctors_Click);
            CreateSidebarButton(pnlSidebar, "Patient Registry", btnY + 60, BtnManagePatients_Click);
            CreateSidebarButton(pnlSidebar, "Pharmacy / Inventory", btnY + 120, BtnManageMedicine_Click);
            CreateSidebarButton(pnlSidebar, "All Appointments", btnY + 180, BtnViewAppointments_Click);

            Button btnLogout = CreateSidebarButton(pnlSidebar, "System Logout", 0, BtnLogout_Click);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Height = 70;
            btnLogout.BackColor = Color.FromArgb(31, 41, 55);

            // ========== MAIN CONTENT AREA ==========
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };

            Label lblWelcome = new Label
            {
                Text = $"System Overview – Admin ID: {_userId}",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            FlowLayoutPanel flowStats = new FlowLayoutPanel
            {
                Location = new Point(0, 60),
                Size = new Size(1000, 200),
                BackColor = Color.Transparent
            };
            // The stats cards will be added in LoadDashboardStats() after we know totals.
            // But we add the container now.
            pnlMain.Controls.Add(lblWelcome);
            pnlMain.Controls.Add(flowStats);
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateSidebarButton(Panel parent, string text, int y, EventHandler clickEvent)
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
                Font = new Font("Segoe UI Semibold", 10F),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 41, 55);
            btn.Click += clickEvent;
            parent.Controls.Add(btn);
            return btn;
        }

        private void LoadDashboardStats()
        {
            // Find the FlowLayoutPanel inside the main panel
            FlowLayoutPanel flowStats = null;
            foreach (Control c in this.Controls)
            {
                if (c is Panel p && p.Dock == DockStyle.Fill)
                {
                    foreach (Control inner in p.Controls)
                    {
                        if (inner is FlowLayoutPanel flp)
                        {
                            flowStats = flp;
                            break;
                        }
                    }
                }
            }
            if (flowStats == null) return;

            flowStats.Controls.Clear();

            int totalPatients = _patientService.GetTotalPatients();
            int totalDoctors = _doctorService.GetAllDoctors().Count;
            int totalMedicines = _medicineService.GetAllMedicines().Count;
            int totalAppointments = _appointmentService.GetAllAppointments().Count;

            AddMetricCard(flowStats, "Total Patients", totalPatients.ToString(), Color.Teal);
            AddMetricCard(flowStats, "Active Doctors", totalDoctors.ToString(), AccentBlue);
            AddMetricCard(flowStats, "Medicines in Stock", totalMedicines.ToString(), Color.FromArgb(16, 185, 129));
            AddMetricCard(flowStats, "Total Appointments", totalAppointments.ToString(), Color.OrangeRed);
        }

        private void AddMetricCard(FlowLayoutPanel parent, string title, string value, Color accent)
        {
            Panel card = new Panel
            {
                Size = new Size(240, 120),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 20, 0)
            };
            Panel bar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
            Label lblTitle = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI Bold", 8F),
                ForeColor = Color.Gray,
                Location = new Point(20, 20),
                AutoSize = true
            };
            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI Black", 22F),
                ForeColor = Color.Black,
                Location = new Point(18, 45),
                AutoSize = true
            };
            card.Controls.AddRange(new Control[] { bar, lblTitle, lblValue });
            parent.Controls.Add(card);
        }

        // Event Handlers
        private void BtnManageDoctors_Click(object sender, EventArgs e)
        {
            // Ensure DoctorManagementForm exists
            DoctorManagementForm form = new DoctorManagementForm();
            form.ShowDialog();
        }

        private void BtnManagePatients_Click(object sender, EventArgs e)
        {
            PatientManagementForm form = new PatientManagementForm();
            form.ShowDialog();
        }

        private void BtnManageMedicine_Click(object sender, EventArgs e)
        {
            MedicineInventoryForm form = new MedicineInventoryForm();
            form.ShowDialog();
        }

        private void BtnViewAppointments_Click(object sender, EventArgs e)
        {
            AppointmentViewForm form = new AppointmentViewForm();
            form.ShowDialog();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
            new LoginForm().Show();
        }
    }
}