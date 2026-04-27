using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class DoctorManagementForm : Form
    {
        private DoctorService _doctorService;
        private DataGridView dgvDoctors;
        private TextBox txtName, txtSpecialization, txtDepartment, txtPhone, txtEmail, txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnClear, btnSearch;
        private Label lblCount;
        private int _selectedDoctorId = -1;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public DoctorManagementForm()
        {
            _doctorService = new DoctorService();
            InitializeComponent();
            SetupEvents();
            LoadDoctors();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1300, 750);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Doctor Management";
            this.StartPosition = FormStartPosition.CenterScreen;

            // ---- 1. Top Search & Header Panel ----
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PrimaryColor,
                Padding = new Padding(10)
            };

            Label lblHeader = new Label
            {
                Text = "DOCTOR MANAGEMENT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            txtSearch = new TextBox { Width = 310, Location = new Point(780, 15), Font = new Font("Segoe UI", 11) };
            btnSearch = new Button
            {
                Text = "Search",
                Height = txtSearch.Height,      // <-- Matches the TextBox height exactly
                Width = 80,
                Location = new Point(780 + txtSearch.Width + 20, 15), // <-- Matches the TextBox Y-coordinate exactly
                BackColor = Color.White,
                ForeColor = PrimaryColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), // Slightly smaller font to fit the slim height
                Cursor = Cursors.Hand
            };
            lblCount = new Label
            {
                Text = "Total: 0",
                Location = new Point(btnSearch.Location.X + btnSearch.Width + 15, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlTop.Controls.AddRange(new Control[] { lblHeader, txtSearch, btnSearch, lblCount });

            // ---- 2. Middle Input Panel (Horizontal Layout) ----
            Panel pnlInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                BackColor = BgLight,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Row 1 Inputs
            txtName = CreateInputField(pnlInput, "Full Name", new Point(30, 20));
            txtSpecialization = CreateInputField(pnlInput, "Specialization", new Point(430, 20));
            txtDepartment = CreateInputField(pnlInput, "Department", new Point(830, 20));

            // Row 2 Inputs
            txtPhone = CreateInputField(pnlInput, "Phone Number", new Point(30, 90));
            txtEmail = CreateInputField(pnlInput, "Email Address", new Point(430, 90));

            // Buttons aligned at the bottom of the input panel
            int btnY = 160;
            btnAdd = CreateButton(pnlInput, "Add Doctor", PrimaryColor, new Point(30, btnY));
            btnEdit = CreateButton(pnlInput, "Edit Doctor", Color.FromArgb(34, 197, 94), new Point(210, btnY));
            btnDelete = CreateButton(pnlInput, "Delete Doctor", Color.FromArgb(239, 68, 68), new Point(390, btnY));
            btnClear = CreateButton(pnlInput, "Clear Fields", Color.Gray, new Point(570, btnY));

            // ---- 3. Bottom DataGridView ----
            dgvDoctors = new DataGridView
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
            dgvDoctors.CellClick += (s, e) => { if (e.RowIndex >= 0) SelectDoctor(e.RowIndex); };
            dgvDoctors.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            // Order of adding matters for DockStyle: Add Grid (Fill) first, then Input (Top), then TopHeader (Top)
            this.Controls.Add(dgvDoctors);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlTop);
        }

        private TextBox CreateInputField(Panel parent, string labelText, Point location)
        {
            Label lbl = new Label
            {
                Text = labelText.ToUpper(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = location,
                AutoSize = true
            };
            TextBox txt = new TextBox
            {
                Location = new Point(location.X, location.Y + 20),
                Size = new Size(360, 35),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateButton(Panel parent, string text, Color color, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Size = new Size(160, 40),
                Location = location,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(btn);
            return btn;
        }

        private void SetupEvents()
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();
            btnSearch.Click += (s, e) => SearchDoctors();
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) SearchDoctors(); };
        }

        private void LoadDoctors()
        {
            var doctors = _doctorService.GetAllDoctors();
            var displayList = doctors.Select(d => new
            {
                d.DoctorId,
                d.Name,
                d.Specialization,
                d.Department,
                d.Phone,
                d.Email
            }).ToList();

            dgvDoctors.DataSource = null;
            dgvDoctors.DataSource = displayList;
            lblCount.Text = $"Total: {displayList.Count}";

            if (dgvDoctors.Columns.Contains("DoctorId"))
                dgvDoctors.Columns["DoctorId"].Visible = false;

            if (dgvDoctors.Columns.Contains("Name"))
                dgvDoctors.Columns["Name"].HeaderText = "Doctor Name";
            if (dgvDoctors.Columns.Contains("Specialization"))
                dgvDoctors.Columns["Specialization"].HeaderText = "Specialization";
            if (dgvDoctors.Columns.Contains("Department"))
                dgvDoctors.Columns["Department"].HeaderText = "Department";
            if (dgvDoctors.Columns.Contains("Phone"))
                dgvDoctors.Columns["Phone"].HeaderText = "Phone";
            if (dgvDoctors.Columns.Contains("Email"))
                dgvDoctors.Columns["Email"].HeaderText = "Email";
        }

        private void SearchDoctors()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadDoctors();
                return;
            }
            var results = _doctorService.SearchDoctors(txtSearch.Text);
            var displayList = results.Select(d => new
            {
                d.DoctorId,
                d.Name,
                d.Specialization,
                d.Department,
                d.Phone,
                d.Email
            }).ToList();

            dgvDoctors.DataSource = null;
            dgvDoctors.DataSource = displayList;
            lblCount.Text = $"Found: {displayList.Count}";

            if (dgvDoctors.Columns.Contains("DoctorId"))
                dgvDoctors.Columns["DoctorId"].Visible = false;
        }

        private void SelectDoctor(int rowIndex)
        {
            var selected = dgvDoctors.Rows[rowIndex].DataBoundItem;
            var doctorIdProperty = selected.GetType().GetProperty("DoctorId");
            if (doctorIdProperty != null)
                _selectedDoctorId = (int)doctorIdProperty.GetValue(selected);

            txtName.Text = dgvDoctors.Rows[rowIndex].Cells["Name"].Value?.ToString() ?? "";
            txtSpecialization.Text = dgvDoctors.Rows[rowIndex].Cells["Specialization"].Value?.ToString() ?? "";
            txtDepartment.Text = dgvDoctors.Rows[rowIndex].Cells["Department"].Value?.ToString() ?? "";
            txtPhone.Text = dgvDoctors.Rows[rowIndex].Cells["Phone"].Value?.ToString() ?? "";
            txtEmail.Text = dgvDoctors.Rows[rowIndex].Cells["Email"].Value?.ToString() ?? "";
        }

        private void ClearForm()
        {
            _selectedDoctorId = -1;
            txtName.Clear();
            txtSpecialization.Clear();
            txtDepartment.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtSearch.Clear();
            LoadDoctors();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                var doctor = new Doctor
                {
                    Name = txtName.Text,
                    Specialization = txtSpecialization.Text,
                    Department = txtDepartment.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text
                };
                bool result = _doctorService.AddDoctor(doctor);
                if (result)
                {
                    MessageBox.Show("Doctor added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDoctors();
                    ClearForm();
                }
                else
                    MessageBox.Show("Failed to add doctor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedDoctorId == -1)
            {
                MessageBox.Show("Please select a doctor from the list to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (ValidateFields())
            {
                var doctor = new Doctor
                {
                    DoctorId = _selectedDoctorId,
                    Name = txtName.Text,
                    Specialization = txtSpecialization.Text,
                    Department = txtDepartment.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text
                };
                bool result = _doctorService.UpdateDoctor(doctor);
                if (result)
                {
                    MessageBox.Show("Doctor updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDoctors();
                    ClearForm();
                }
                else
                    MessageBox.Show("Update failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedDoctorId == -1)
            {
                MessageBox.Show("Please select a doctor to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this doctor?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool result = _doctorService.DeleteDoctor(_selectedDoctorId);
                if (result)
                {
                    MessageBox.Show("Doctor deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDoctors();
                    ClearForm();
                }
                else
                    MessageBox.Show("Deletion failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Doctor name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtSpecialization.Text))
            {
                MessageBox.Show("Specialization is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDepartment.Text))
            {
                MessageBox.Show("Department is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Phone number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}