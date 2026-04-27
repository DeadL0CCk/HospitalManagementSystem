using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class AppointmentBookingForm : Form
    {
        private AppointmentService _appointmentService;
        private PatientService _patientService;
        private DoctorService _doctorService;

        private ComboBox cmbPatient, cmbDoctor;
        private DateTimePicker dtpDate, dtpTime;
        private Button btnBook, btnClear;
        private DataGridView dgvAppointments;
        private Label lblCount;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public AppointmentBookingForm()
        {
            _appointmentService = new AppointmentService();
            _patientService = new PatientService();
            _doctorService = new DoctorService();
            InitializeComponent();
            SetupEvents();
            LoadDropdowns();
            LoadAppointments();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Book Appointment";
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
                Text = "BOOK APPOINTMENT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblHeader);

            // ---- Input Panel (Horizontal Layout) ----
            Panel pnlInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = BgLight,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20)
            };

            // Row 1: Patient and Doctor (side by side)
            Label lblPatient = new Label
            {
                Text = "PATIENT",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(30, 20),
                AutoSize = true
            };
            cmbPatient = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(30, 45),
                Size = new Size(350, 35),
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };
            pnlInput.Controls.Add(lblPatient);
            pnlInput.Controls.Add(cmbPatient);

            Label lblDoctor = new Label
            {
                Text = "DOCTOR",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(450, 20),
                AutoSize = true
            };
            cmbDoctor = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(450, 45),
                Size = new Size(350, 35),
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };
            pnlInput.Controls.Add(lblDoctor);
            pnlInput.Controls.Add(cmbDoctor);

            // Row 2: Date, Time, and Book button
            Label lblDate = new Label
            {
                Text = "APPOINTMENT DATE",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(30, 95),
                AutoSize = true
            };
            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(30, 120),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 11),
                MinDate = DateTime.Now.Date
            };
            pnlInput.Controls.Add(lblDate);
            pnlInput.Controls.Add(dtpDate);

            Label lblTime = new Label
            {
                Text = "TIME",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(260, 95),
                AutoSize = true
            };
            dtpTime = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(260, 120),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 11)
            };
            pnlInput.Controls.Add(lblTime);
            pnlInput.Controls.Add(dtpTime);

            btnBook = new Button
            {
                Text = "BOOK APPOINTMENT",
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(450, 115),
                Cursor = Cursors.Hand
            };
            btnBook.FlatAppearance.BorderSize = 0;
            pnlInput.Controls.Add(btnBook);

            btnClear = new Button
            {
                Text = "Clear Fields",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(660, 115),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            pnlInput.Controls.Add(btnClear);

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

            // Label for total count (bottom right)
            lblCount = new Label
            {
                Text = "Total Appointments: 0",
                Location = new Point(this.Width - 200, this.Height - 40),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            this.Controls.Add(lblCount);

            this.Controls.Add(dgvAppointments);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlTop);
        }

        private void SetupEvents()
        {
            btnBook.Click += BtnBook_Click;
            btnClear.Click += (s, e) => ClearForm();
        }

        private void LoadDropdowns()
        {
            // Load patients
            var patients = _patientService.GetAllPatients();
            cmbPatient.DataSource = patients;
            cmbPatient.DisplayMember = "Name";
            cmbPatient.ValueMember = "PatientId";

            // Load doctors
            var doctors = _doctorService.GetAllDoctors();
            cmbDoctor.DataSource = doctors;
            cmbDoctor.DisplayMember = "Name";
            cmbDoctor.ValueMember = "DoctorId";
        }

        private void LoadAppointments()
        {
            var appointments = _appointmentService.GetAllAppointments();
            var displayList = appointments.Select(a => new
            {
                a.AppointmentId,
                a.PatientName,
                a.DoctorName,
                Date = a.AppointmentDate.ToString("dd-MMM-yyyy"),
                Time = a.AppointmentTime.ToString(@"hh\:mm"),
                a.Status
            }).ToList();

            dgvAppointments.DataSource = null;
            dgvAppointments.DataSource = displayList;
            lblCount.Text = $"Total Appointments: {displayList.Count}";

            if (dgvAppointments.Columns.Contains("AppointmentId"))
                dgvAppointments.Columns["AppointmentId"].Visible = false;
        }

        private void ClearForm()
        {
            cmbPatient.SelectedIndex = -1;
            cmbDoctor.SelectedIndex = -1;
            dtpDate.Value = DateTime.Now;
            dtpTime.Value = DateTime.Now;
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (cmbPatient.SelectedValue == null)
            {
                MessageBox.Show("Please select a patient.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbDoctor.SelectedValue == null)
            {
                MessageBox.Show("Please select a doctor.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var appointment = new Appointment
                {
                    PatientId = (int)cmbPatient.SelectedValue,
                    DoctorId = (int)cmbDoctor.SelectedValue,
                    AppointmentDate = dtpDate.Value.Date,
                    AppointmentTime = dtpTime.Value.TimeOfDay,
                    Status = "Pending"
                };

                bool result = _appointmentService.BookAppointment(appointment);
                if (result)
                {
                    MessageBox.Show("Appointment booked successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAppointments();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to book appointment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}