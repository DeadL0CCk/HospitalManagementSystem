using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HospitalManagementSystem.UI
{
    public class DoctorScheduleForm : Form
    {
        private int _doctorId;
        private AppointmentService _appointmentService;
        private DataGridView dgvAppointments;
        private TextBox txtSearch;
        private Button btnSearch, btnComplete, btnPrescribe, btnRefresh;
        private Label lblCount;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);
        private int _selectedAppointmentId = -1;

        public DoctorScheduleForm(int doctorId)
        {
            _doctorId = doctorId;
            _appointmentService = new AppointmentService();
            InitializeComponent();
            SetupEvents();
            LoadSchedule();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - My Schedule";
            this.StartPosition = FormStartPosition.CenterScreen;

            // ---- Top Header Panel ----
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PrimaryColor,
                Padding = new Padding(10)
            };

            Label lblHeader = new Label
            {
                Text = "MY SCHEDULE",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Width = 310,
                Location = new Point(400, 15),
                Font = new Font("Segoe UI", 11)
                            };
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
            lblCount = new Label
            {
                Text = "Total: 0",
                Location = new Point(btnSearch.Location.X + btnSearch.Width + 20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlTop.Controls.AddRange(new Control[] { lblHeader, txtSearch, btnSearch, lblCount });

            // ---- Action Buttons Panel ----
            Panel pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = BgLight,
                Padding = new Padding(20)
            };

            btnComplete = new Button
            {
                Text = "Mark as Completed",
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(160, 40),
                Location = new Point(30, 10),
                Cursor = Cursors.Hand
            };
            btnComplete.FlatAppearance.BorderSize = 0;

            btnPrescribe = new Button
            {
                Text = "Write Prescription",
                BackColor = Color.FromArgb(0, 92, 151),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(210, 10),
                Cursor = Cursors.Hand
            };
            btnPrescribe.FlatAppearance.BorderSize = 0;

            btnRefresh = new Button
            {
                Text = "Refresh",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(410, 10),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;

            pnlActions.Controls.AddRange(new Control[] { btnComplete, btnPrescribe, btnRefresh });

            // ---- DataGridView ----
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
            dgvAppointments.CellClick += (s, e) => { if (e.RowIndex >= 0) SelectAppointment(e.RowIndex); };
            dgvAppointments.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OpenPrescription(); };

            // Add label for total count at bottom (optional)
            Label lblFooter = new Label
            {
                Text = "Double‑click a row to write prescription",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = BgLight,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblFooter);
            this.Controls.Add(dgvAppointments);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlTop);
        }

        private void SetupEvents()
        {
            btnSearch.Click += BtnSearch_Click;
            btnComplete.Click += BtnComplete_Click;
            btnPrescribe.Click += (s, e) => OpenPrescription();
            btnRefresh.Click += (s, e) => LoadSchedule();
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnSearch_Click(null, null); };
        }

        private void LoadSchedule()
        {
            var appointments = _appointmentService.GetDoctorSchedule(_doctorId);
            BindGrid(appointments);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                LoadSchedule();
                return;
            }
            var all = _appointmentService.GetDoctorSchedule(_doctorId);
            var filtered = all.Where(a => a.PatientName != null && a.PatientName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            BindGrid(filtered);
        }

        private void BindGrid(List<Appointment> appointments)
        {
            var displayList = appointments.Select(a => new
            {
                a.AppointmentId,
                a.PatientName,
                Date = a.AppointmentDate.ToString("dd-MMM-yyyy"),
                Time = a.AppointmentTime.ToString(@"hh\:mm"),
                a.Status
            }).ToList();

            dgvAppointments.DataSource = null;
            dgvAppointments.DataSource = displayList;
            lblCount.Text = $"Total: {displayList.Count}";

            if (dgvAppointments.Columns.Contains("AppointmentId"))
                dgvAppointments.Columns["AppointmentId"].Visible = false;

            if (dgvAppointments.Columns.Contains("PatientName"))
                dgvAppointments.Columns["PatientName"].HeaderText = "Patient";
            if (dgvAppointments.Columns.Contains("Date"))
                dgvAppointments.Columns["Date"].HeaderText = "Date";
            if (dgvAppointments.Columns.Contains("Time"))
                dgvAppointments.Columns["Time"].HeaderText = "Time";
            if (dgvAppointments.Columns.Contains("Status"))
                dgvAppointments.Columns["Status"].HeaderText = "Status";
        }

        private void SelectAppointment(int rowIndex)
        {
            _selectedAppointmentId = (int)dgvAppointments.Rows[rowIndex].Cells["AppointmentId"].Value;
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (_selectedAppointmentId == -1)
            {
                MessageBox.Show("Please select an appointment from the list.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                bool result = _appointmentService.CompleteAppointment(_selectedAppointmentId);
                if (result)
                {
                    MessageBox.Show("Appointment marked as Completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSchedule(); // refresh grid
                }
                else
                {
                    MessageBox.Show("Failed to update appointment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPrescription()
        {
            if (_selectedAppointmentId == -1)
            {
                MessageBox.Show("Please select an appointment first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Pass the selected appointment ID to the form
            PrescriptionForm presForm = new PrescriptionForm(_doctorId, _selectedAppointmentId);
            presForm.ShowDialog();
            LoadSchedule();  // refresh after closing
        }
    }
}