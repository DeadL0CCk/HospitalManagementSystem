using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HospitalManagementSystem.UI
{
    public class AppointmentViewForm : Form
    {
        private AppointmentService _appointmentService;
        private DataGridView dgvAppointments;
        private TextBox txtSearch;
        private ComboBox cmbStatusFilter;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private Button btnSearch, btnReset, btnBack;
        private Label lblCount;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public AppointmentViewForm()
        {
            _appointmentService = new AppointmentService();
            InitializeComponent();
            SetupEvents();
            LoadAppointments();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1300, 750);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - All Appointments";
            this.StartPosition = FormStartPosition.CenterScreen;

            // ---- 1. Top Header Panel (Blue) ----
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PrimaryColor,
                Padding = new Padding(10)
            };

            Label lblHeader = new Label
            {
                Text = "ALL APPOINTMENTS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            // Search TextBox (PlaceholderText not available in .NET Framework; use a label or omit)
            txtSearch = new TextBox
            {
                Width = 250,
                Location = new Point(400, 15),
                Font = new Font("Segoe UI", 11)
            };

            // Search button
            btnSearch = new Button
            {
                Text = "Search",
                Height = txtSearch.Height,
                Width = 80,
                Location = new Point(400 + txtSearch.Width + 20, 15),
                BackColor = Color.White,
                ForeColor = PrimaryColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // Reset button
            btnReset = new Button
            {
                Text = "Reset",
                Height = txtSearch.Height,
                Width = 80,
                Location = new Point(btnSearch.Location.X + btnSearch.Width + 10, 15),
                BackColor = Color.White,
                ForeColor = PrimaryColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // Total count label
            lblCount = new Label
            {
                Text = "Total: 0",
                Location = new Point(btnReset.Location.X + btnReset.Width + 20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            pnlTop.Controls.AddRange(new Control[] { lblHeader, txtSearch, btnSearch, btnReset, lblCount });

            // ---- 2. Middle Filter Panel (Date Range & Status) ----
            Panel pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = BgLight,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblFrom = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(30, 25),
                AutoSize = true
            };
            dtpStartDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(75, 22),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10)
            };
            dtpStartDate.Value = DateTime.Now.AddDays(-30);

            Label lblTo = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(210, 25),
                AutoSize = true
            };
            dtpEndDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(245, 22),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10)
            };
            dtpEndDate.Value = DateTime.Now.AddMonths(1);

            Label lblStatus = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(400, 25),
                AutoSize = true
            };
            cmbStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(450, 22),
                Size = new Size(140, 35),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat
            };
            cmbStatusFilter.Items.AddRange(new[] { "All", "Pending", "Completed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;

            Button btnApply = new Button
            {
                Text = "Apply Filters",
                Height = 35,
                Width = 120,
                Location = new Point(620, 20),
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnApply.Click += (s, e) => ApplyFilters();

            pnlFilter.Controls.AddRange(new Control[] { lblFrom, dtpStartDate, lblTo, dtpEndDate, lblStatus, cmbStatusFilter, btnApply });

            // ---- 3. DataGridView ----
            dgvAppointments = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 40 },
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 250, 252) },
                GridColor = Color.FromArgb(230, 230, 230)
            };
            dgvAppointments.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            // Back button
            btnBack = new Button
            {
                Text = "Back",
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(this.Width - 120, this.Height - 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnBack.Click += (s, e) => this.Close();
            this.Controls.Add(btnBack);

            // Order of adding
            this.Controls.Add(dgvAppointments);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlTop);
        }

        private void SetupEvents()
        {
            btnSearch.Click += (s, e) => ApplyFilters();
            btnReset.Click += (s, e) => ResetFilters();
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) ApplyFilters(); };
        }

        private void LoadAppointments()
        {
            var appointments = _appointmentService.GetAllAppointments();
            BindGrid(appointments);
        }

        private void ApplyFilters()
        {
            var appointments = _appointmentService.GetAllAppointments();

            // Date range
            DateTime start = dtpStartDate.Value.Date;
            DateTime end = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);
            appointments = appointments.Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end).ToList();

            // Status filter
            if (cmbStatusFilter.SelectedItem?.ToString() != "All")
            {
                string status = cmbStatusFilter.SelectedItem.ToString();
                appointments = appointments.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Text search (patient or doctor name)
            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                appointments = appointments.Where(a =>
                    (a.PatientName?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.DoctorName?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }

            BindGrid(appointments);
        }

        private void BindGrid(List<Appointment> appointments)
        {
            var displayList = appointments.Select(a => new
            {
                a.AppointmentId,
                a.PatientName,
                a.DoctorName,
                AppointmentDate = a.AppointmentDate.ToString("dd-MMM-yyyy"),
                AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                a.Status
            }).ToList();

            dgvAppointments.DataSource = null;
            dgvAppointments.DataSource = displayList;
            lblCount.Text = $"Total: {appointments.Count}";

            if (dgvAppointments.Columns.Contains("AppointmentId"))
                dgvAppointments.Columns["AppointmentId"].Visible = false;

            if (dgvAppointments.Columns.Contains("PatientName"))
                dgvAppointments.Columns["PatientName"].HeaderText = "Patient";
            if (dgvAppointments.Columns.Contains("DoctorName"))
                dgvAppointments.Columns["DoctorName"].HeaderText = "Doctor";
            if (dgvAppointments.Columns.Contains("AppointmentDate"))
                dgvAppointments.Columns["AppointmentDate"].HeaderText = "Date";
            if (dgvAppointments.Columns.Contains("AppointmentTime"))
                dgvAppointments.Columns["AppointmentTime"].HeaderText = "Time";
            if (dgvAppointments.Columns.Contains("Status"))
                dgvAppointments.Columns["Status"].HeaderText = "Status";
        }

        private void ResetFilters()
        {
            txtSearch.Clear();
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            dtpEndDate.Value = DateTime.Now.AddMonths(1);
            cmbStatusFilter.SelectedIndex = 0;
            LoadAppointments();
        }
    }
}