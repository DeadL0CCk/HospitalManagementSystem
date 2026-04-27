using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class PrescriptionForm : Form
    {
        private int _doctorId;
        private int? _preSelectedAppointmentId;
        private PrescriptionService _prescriptionService;
        private MedicineService _medicineService;
        private AppointmentService _appointmentService;

        private ComboBox cmbAppointment, cmbMedicine, cmbFrequency;
        private TextBox txtDiagnosis, txtNotes, txtDosage;
        private Button btnAddMedicine, btnSavePrescription;
        private DataGridView dgvMedicines;
        private Label lblCount;
        private List<PrescriptionMedicine> _tempMedList = new List<PrescriptionMedicine>();

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public PrescriptionForm(int doctorId, int? appointmentId = null)
        {
            _doctorId = doctorId;
            _preSelectedAppointmentId = appointmentId;
            _prescriptionService = new PrescriptionService();
            _medicineService = new MedicineService();
            _appointmentService = new AppointmentService();

            InitializeComponent();
            SetupEvents();
            LoadMedicines();
            LoadAppointments();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);  // Slightly wider, standard height
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Write Prescription";
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
                Text = "WRITE PRESCRIPTION",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblHeader);

            // ---- Input Panel (more compact) ----
            Panel pnlInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,   // Reduced from 280
                BackColor = BgLight,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20)
            };

            int currentY = 15;
            int labelWidth = 100;
            int controlWidth = 350;

            // Appointment
            Label lblAppointment = new Label { Text = "Appointment:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, currentY), AutoSize = true };
            cmbAppointment = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, currentY - 3), Size = new Size(400, 30), Font = new Font("Segoe UI", 10) };
            pnlInput.Controls.Add(lblAppointment);
            pnlInput.Controls.Add(cmbAppointment);
            currentY += 45;

            // Diagnosis (shorter)
            Label lblDiagnosis = new Label { Text = "Diagnosis:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, currentY), AutoSize = true };
            txtDiagnosis = new TextBox { Location = new Point(120, currentY - 3), Size = new Size(600, 30), Font = new Font("Segoe UI", 10) };
            pnlInput.Controls.Add(lblDiagnosis);
            pnlInput.Controls.Add(txtDiagnosis);
            currentY += 45;

            // Notes (shorter multiline)
            Label lblNotes = new Label { Text = "Notes:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, currentY), AutoSize = true };
            txtNotes = new TextBox { Location = new Point(120, currentY - 3), Size = new Size(600, 50), Multiline = true, Font = new Font("Segoe UI", 10) };
            pnlInput.Controls.Add(lblNotes);
            pnlInput.Controls.Add(txtNotes);
            currentY += 65;

            // Medicine row (inline)
            Label lblMedicine = new Label { Text = "Medicine:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, currentY), AutoSize = true };
            cmbMedicine = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(100, currentY - 3), Size = new Size(180, 30), Font = new Font("Segoe UI", 10) };
            Label lblDosage = new Label { Text = "Dosage:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(300, currentY), AutoSize = true };
            txtDosage = new TextBox { Location = new Point(370, currentY - 3), Size = new Size(100, 30), Font = new Font("Segoe UI", 10) };
            Label lblFrequency = new Label { Text = "Freq:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(490, currentY), AutoSize = true };
            cmbFrequency = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(540, currentY - 3), Size = new Size(120, 30), Font = new Font("Segoe UI", 10) };
            cmbFrequency.Items.AddRange(new[] { "Once daily", "Twice daily", "Three times daily", "Every 4h", "As needed" });
            btnAddMedicine = new Button { Text = "Add", BackColor = PrimaryColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(70, 30), Location = new Point(680, currentY - 3), Cursor = Cursors.Hand };
            btnAddMedicine.FlatAppearance.BorderSize = 0;

            pnlInput.Controls.AddRange(new Control[] { lblMedicine, cmbMedicine, lblDosage, txtDosage, lblFrequency, cmbFrequency, btnAddMedicine });
            currentY += 45;

            // No more controls in input panel; remaining space for DataGridView

            // ---- DataGridView for medicines ----
            dgvMedicines = new DataGridView
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
                RowTemplate = { Height = 35 },
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 250, 252) }
            };
            dgvMedicines.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            dgvMedicines.CellMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    var menu = new ContextMenuStrip();
                    menu.Items.Add("Remove", null, (obj, args) =>
                    {
                        _tempMedList.RemoveAt(e.RowIndex);
                        RefreshMedicineGrid();
                    });
                    menu.Show(dgvMedicines, e.Location);
                }
            };

            // ---- Save button (bottom right) ----
            btnSavePrescription = new Button
            {
                Text = "Save Prescription",
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(180, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnSavePrescription.FlatAppearance.BorderSize = 0;

            // Count label
            lblCount = new Label
            {
                Text = "Medicines: 0",
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = PrimaryColor
            };

            // Add controls in order
            this.Controls.Add(dgvMedicines);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlTop);
            this.Controls.Add(btnSavePrescription);
            this.Controls.Add(lblCount);

            // Position save button and label after form loads
            this.Load += (s, e) =>
            {
                btnSavePrescription.Location = new Point(this.ClientSize.Width - btnSavePrescription.Width - 20, this.ClientSize.Height - btnSavePrescription.Height - 20);
                lblCount.Location = new Point(20, this.ClientSize.Height - lblCount.Height - 30);
            };
            this.Resize += (s, e) =>
            {
                btnSavePrescription.Location = new Point(this.ClientSize.Width - btnSavePrescription.Width - 20, this.ClientSize.Height - btnSavePrescription.Height - 20);
                lblCount.Location = new Point(20, this.ClientSize.Height - lblCount.Height - 30);
            };
        }

        private void SetupEvents()
        {
            btnAddMedicine.Click += BtnAddMedicine_Click;
            btnSavePrescription.Click += BtnSavePrescription_Click;
        }

        private void LoadAppointments()
        {
            var appointments = _appointmentService.GetDoctorSchedule(_doctorId);
            cmbAppointment.DataSource = appointments;
            cmbAppointment.DisplayMember = "AppointmentDateAndTime";
            cmbAppointment.ValueMember = "AppointmentId";

            if (_preSelectedAppointmentId.HasValue && appointments.Count > 0)
            {
                for (int i = 0; i < appointments.Count; i++)
                {
                    if (appointments[i].AppointmentId == _preSelectedAppointmentId.Value)
                    {
                        cmbAppointment.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (appointments.Count > 0)
                cmbAppointment.SelectedIndex = 0;
        }

        private void LoadMedicines()
        {
            var medicines = _medicineService.GetAllMedicines();
            cmbMedicine.DataSource = medicines;
            cmbMedicine.DisplayMember = "Name";
            cmbMedicine.ValueMember = "MedicineId";
        }

        private void BtnAddMedicine_Click(object sender, EventArgs e)
        {
            if (cmbMedicine.SelectedItem == null || string.IsNullOrWhiteSpace(txtDosage.Text) || cmbFrequency.SelectedIndex == -1)
            {
                MessageBox.Show("Please select medicine, enter dosage, and select frequency.", "Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var med = new PrescriptionMedicine
            {
                MedicineId = (int)cmbMedicine.SelectedValue,
                MedicineName = cmbMedicine.Text,
                Dosage = txtDosage.Text,
                Frequency = cmbFrequency.SelectedItem.ToString(),
                Duration = ""
            };
            _tempMedList.Add(med);
            RefreshMedicineGrid();
            txtDosage.Clear();
            cmbFrequency.SelectedIndex = -1;
        }

        private void RefreshMedicineGrid()
        {
            var displayList = _tempMedList.Select(m => new { m.MedicineName, m.Dosage, m.Frequency }).ToList();
            dgvMedicines.DataSource = null;
            dgvMedicines.DataSource = displayList;
            lblCount.Text = $"Medicines: {_tempMedList.Count}";
        }

        private void BtnSavePrescription_Click(object sender, EventArgs e)
        {
            if (cmbAppointment.SelectedValue == null)
            {
                MessageBox.Show("Select an appointment.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDiagnosis.Text))
            {
                MessageBox.Show("Enter diagnosis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_tempMedList.Count == 0)
            {
                MessageBox.Show("Add at least one medicine.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var prescription = new Prescription
                {
                    AppointmentId = (int)cmbAppointment.SelectedValue,
                    Diagnosis = txtDiagnosis.Text,
                    Notes = txtNotes.Text,
                    PrescriptionDate = DateTime.Now
                };
                if (!_prescriptionService.WritePrescription(prescription))
                {
                    MessageBox.Show("Failed to save prescription header.");
                    return;
                }

                var savedPres = _prescriptionService.GetPrescriptionByAppointmentId(prescription.AppointmentId);
                if (savedPres == null)
                {
                    MessageBox.Show("Could not retrieve saved prescription.");
                    return;
                }

                foreach (var med in _tempMedList)
                {
                    med.PrescriptionId = savedPres.PrescriptionId;
                    _prescriptionService.AddPrescriptionMedicine(med);
                }

                MessageBox.Show("Prescription saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}