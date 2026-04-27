using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HospitalManagementSystem.UI
{
    public class PatientSearchForm : Form
    {
        private PatientService _patientService;
        private AppointmentService _appointmentService;
        private PrescriptionService _prescriptionService;

        private TextBox txtSearch;
        private Button btnSearch, btnClose;
        private DataGridView dgvPatients;
        private Label lblCount;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public PatientSearchForm()
        {
            _patientService = new PatientService();
            _appointmentService = new AppointmentService();
            _prescriptionService = new PrescriptionService();
            InitializeComponent();
            SetupEvents();
            LoadPatients(); // Load all initially
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Patient Search";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top header panel
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PrimaryColor,
                Padding = new Padding(10)
            };

            Label lblHeader = new Label
            {
                Text = "PATIENT SEARCH",
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

            // DataGridView
            dgvPatients = new DataGridView
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
            dgvPatients.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dgvPatients.CellDoubleClick += DgvPatients_CellDoubleClick; // Double-click to view details

            // Bottom panel with buttons
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = BgLight
            };

            btnClose = new Button
            {
                Text = "Close",
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(this.Width - 120, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(dgvPatients);
            this.Controls.Add(pnlTop);
            this.Controls.Add(pnlBottom);
        }

        private void SetupEvents()
        {
            btnSearch.Click += BtnSearch_Click;
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnSearch_Click(null, null); };
        }

        private void LoadPatients()
        {
            var patients = _patientService.GetAllPatients();
            BindGrid(patients);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                LoadPatients();
                return;
            }
            var results = _patientService.SearchPatients(keyword);
            BindGrid(results);
        }

        private void BindGrid(List<Patient> patients)
        {
            var displayList = patients.Select(p => new
            {
                p.PatientId,
                p.Name,
                p.Phone,
                p.Email,
                p.Address,
                GenderDisplay = p.Gender == 'M' ? "Male" : (p.Gender == 'F' ? "Female" : "Other"),
                p.BloodGroup,
                p.RegistrationDate
            }).ToList();

            dgvPatients.DataSource = null;
            dgvPatients.DataSource = displayList;
            lblCount.Text = $"Total: {displayList.Count}";

            if (dgvPatients.Columns.Contains("PatientId"))
                dgvPatients.Columns["PatientId"].Visible = false;

            if (dgvPatients.Columns.Contains("Name"))
                dgvPatients.Columns["Name"].HeaderText = "Patient Name";
            if (dgvPatients.Columns.Contains("Phone"))
                dgvPatients.Columns["Phone"].HeaderText = "Phone";
            if (dgvPatients.Columns.Contains("Email"))
                dgvPatients.Columns["Email"].HeaderText = "Email";
            if (dgvPatients.Columns.Contains("Address"))
                dgvPatients.Columns["Address"].HeaderText = "Address";
            if (dgvPatients.Columns.Contains("GenderDisplay"))
                dgvPatients.Columns["GenderDisplay"].HeaderText = "Gender";
            if (dgvPatients.Columns.Contains("BloodGroup"))
                dgvPatients.Columns["BloodGroup"].HeaderText = "Blood Group";
            if (dgvPatients.Columns.Contains("RegistrationDate"))
                dgvPatients.Columns["RegistrationDate"].HeaderText = "Registered On";
        }

        private void DgvPatients_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int patientId = (int)dgvPatients.Rows[e.RowIndex].Cells["PatientId"].Value;
                PatientDetailForm detailForm = new PatientDetailForm(patientId);
                detailForm.ShowDialog();
            }
        }
    }
}