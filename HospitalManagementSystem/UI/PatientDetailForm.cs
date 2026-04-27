using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class PatientDetailForm : Form
    {
        private int _patientId;
        private PatientService _patientService;
        private AppointmentService _appointmentService;
        private PrescriptionService _prescriptionService;

        private TabControl tabControl;
        private TabPage tabDetails, tabAppointments, tabPrescriptions;
        private Label lblName, lblPhone, lblEmail, lblAddress, lblGender, lblBlood, lblRegDate;
        private DataGridView dgvAppointments, dgvPrescriptions;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);

        public PatientDetailForm(int patientId)
        {
            _patientId = patientId;
            _patientService = new PatientService();
            _appointmentService = new AppointmentService();
            _prescriptionService = new PrescriptionService();
            InitializeComponent();
            LoadPatientData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 600);
            this.Text = "Patient Details";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Tab control
            tabControl = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Patient Details
            tabDetails = new TabPage("Patient Information");
            TableLayoutPanel tlpDetails = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(20),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            tlpDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            lblName = new Label { Text = "Name:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            lblPhone = new Label { Text = "Phone:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            lblEmail = new Label { Text = "Email:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            lblAddress = new Label { Text = "Address:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            lblGender = new Label { Text = "Gender:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            lblBlood = new Label { Text = "Blood Group:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            lblRegDate = new Label { Text = "Registered:", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };

            tlpDetails.Controls.Add(lblName, 0, 0);
            tlpDetails.Controls.Add(lblPhone, 0, 1);
            tlpDetails.Controls.Add(lblEmail, 0, 2);
            tlpDetails.Controls.Add(lblAddress, 0, 3);
            tlpDetails.Controls.Add(lblGender, 0, 4);
            tlpDetails.Controls.Add(lblBlood, 0, 5);
            tlpDetails.Controls.Add(lblRegDate, 0, 6);

            // Value labels - will be populated later
            var valName = new Label { Name = "valName", AutoSize = true };
            var valPhone = new Label { Name = "valPhone", AutoSize = true };
            var valEmail = new Label { Name = "valEmail", AutoSize = true };
            var valAddress = new Label { Name = "valAddress", AutoSize = true };
            var valGender = new Label { Name = "valGender", AutoSize = true };
            var valBlood = new Label { Name = "valBlood", AutoSize = true };
            var valRegDate = new Label { Name = "valRegDate", AutoSize = true };

            tlpDetails.Controls.Add(valName, 1, 0);
            tlpDetails.Controls.Add(valPhone, 1, 1);
            tlpDetails.Controls.Add(valEmail, 1, 2);
            tlpDetails.Controls.Add(valAddress, 1, 3);
            tlpDetails.Controls.Add(valGender, 1, 4);
            tlpDetails.Controls.Add(valBlood, 1, 5);
            tlpDetails.Controls.Add(valRegDate, 1, 6);

            tabDetails.Controls.Add(tlpDetails);

            // Tab 2: Appointments
            tabAppointments = new TabPage("Appointment History");
            dgvAppointments = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            tabAppointments.Controls.Add(dgvAppointments);

            // Tab 3: Prescriptions
            tabPrescriptions = new TabPage("Prescription History");
            dgvPrescriptions = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            tabPrescriptions.Controls.Add(dgvPrescriptions);

            tabControl.TabPages.Add(tabDetails);
            tabControl.TabPages.Add(tabAppointments);
            tabControl.TabPages.Add(tabPrescriptions);

            this.Controls.Add(tabControl);
        }

        private void LoadPatientData()
        {
            Patient patient = _patientService.GetPatientById(_patientId);
            if (patient == null)
            {
                MessageBox.Show("Patient not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Populate details tab
            var valName = (Label)tabDetails.Controls.Find("valName", true)[0];
            var valPhone = (Label)tabDetails.Controls.Find("valPhone", true)[0];
            var valEmail = (Label)tabDetails.Controls.Find("valEmail", true)[0];
            var valAddress = (Label)tabDetails.Controls.Find("valAddress", true)[0];
            var valGender = (Label)tabDetails.Controls.Find("valGender", true)[0];
            var valBlood = (Label)tabDetails.Controls.Find("valBlood", true)[0];
            var valRegDate = (Label)tabDetails.Controls.Find("valRegDate", true)[0];

            valName.Text = patient.Name;
            valPhone.Text = patient.Phone;
            valEmail.Text = patient.Email;
            valAddress.Text = patient.Address;
            valGender.Text = patient.Gender == 'M' ? "Male" : (patient.Gender == 'F' ? "Female" : "Other");
            valBlood.Text = patient.BloodGroup;
            valRegDate.Text = patient.RegistrationDate.ToString("dd-MMM-yyyy");

            // Load appointments
            var allAppointments = _appointmentService.GetAllAppointments();
            var patientAppointments = allAppointments.Where(a => a.PatientId == _patientId).ToList();
            dgvAppointments.DataSource = patientAppointments.Select(a => new
            {
                a.AppointmentId,
                a.DoctorName,
                Date = a.AppointmentDate.ToString("dd-MMM-yyyy"),
                Time = a.AppointmentTime.ToString(@"hh\:mm"),
                a.Status
            }).ToList();
            if (dgvAppointments.Columns.Contains("AppointmentId"))
                dgvAppointments.Columns["AppointmentId"].Visible = false;

            // Load prescriptions (from appointments)
            // Note: This requires joining Prescription table via AppointmentId
            var prescriptions = new System.Collections.Generic.List<object>();
            foreach (var apt in patientAppointments)
            {
                var pres = _prescriptionService.GetPrescriptionByAppointmentId(apt.AppointmentId);
                if (pres != null)
                {
                    prescriptions.Add(new
                    {
                        pres.PrescriptionId,
                        Date = pres.PrescriptionDate.ToString("dd-MMM-yyyy"),
                        pres.Diagnosis,
                        Medicines = string.Join(", ", _prescriptionService.GetPrescriptionMedicines(pres.PrescriptionId).Select(m => m.MedicineName))
                    });
                }
            }
            dgvPrescriptions.DataSource = prescriptions;
            if (dgvPrescriptions.Columns.Contains("PrescriptionId"))
                dgvPrescriptions.Columns["PrescriptionId"].Visible = false;
        }
    }
}