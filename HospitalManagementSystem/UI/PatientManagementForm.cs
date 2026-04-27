using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class PatientManagementForm : Form
    {
        private PatientService _patientService;
        private DataGridView dgvPatients;
        private TextBox txtName, txtPhone, txtEmail, txtAddress;
        private ComboBox cmbGender, cmbBlood;
        private DateTimePicker dtpDateOfBirth;  // <-- NEW
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnClear, btnSearch;
        private Label lblCount;
        private int _selectedPatientId = -1;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public PatientManagementForm()
        {
            _patientService = new PatientService();
            InitializeComponent();
            SetupEvents();
            LoadPatients();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1300, 750);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Patient Registry";
            this.StartPosition = FormStartPosition.CenterScreen;

            // ---- Top Header Panel (Blue) ----
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = PrimaryColor,
                Padding = new Padding(10)
            };

            Label lblHeader = new Label
            {
                Text = "PATIENT REGISTRY",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            txtSearch = new TextBox { Width = 310, Location = new Point(780, 15), Font = new Font("Segoe UI", 11) };
            btnSearch = new Button
            {
                Text = "Search",
                Height = txtSearch.Height,
                Width = 80,
                Location = new Point(780 + txtSearch.Width + 20, 15),
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

            // ---- Middle Input Panel (Horizontal Layout) ----
            Panel pnlInput = new Panel
            {
                Dock = DockStyle.Top,
                Height = 280,  // Increased height to accommodate DOB
                BackColor = BgLight,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20)
            };

            // Row 1: Full Name, Phone, Email
            txtName = CreateInputField(pnlInput, "Full Name", new Point(30, 20));
            txtPhone = CreateInputField(pnlInput, "Phone Number", new Point(430, 20));
            txtEmail = CreateInputField(pnlInput, "Email", new Point(830, 20));

            // Row 2: Date of Birth, Gender, Blood Group
            dtpDateOfBirth = CreateDateField(pnlInput, "Date of Birth", new Point(30, 90));
            cmbGender = CreateComboField(pnlInput, "Gender", new Point(430, 90), new[] { "Male", "Female", "Other" });
            cmbBlood = CreateComboField(pnlInput, "Blood Group", new Point(830, 90), new[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });

            // Row 3: Address
            txtAddress = CreateInputField(pnlInput, "Address", new Point(30, 160));

            // Buttons aligned at the bottom
            int btnY = 220;
            btnAdd = CreateButton(pnlInput, "Add Patient", PrimaryColor, new Point(30, btnY));
            btnEdit = CreateButton(pnlInput, "Edit Patient", Color.FromArgb(34, 197, 94), new Point(210, btnY));
            btnDelete = CreateButton(pnlInput, "Delete Patient", Color.FromArgb(239, 68, 68), new Point(390, btnY));
            btnClear = CreateButton(pnlInput, "Clear Fields", Color.Gray, new Point(570, btnY));

            // ---- DataGridView ----
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
            dgvPatients.CellClick += (s, e) => { if (e.RowIndex >= 0) SelectPatient(e.RowIndex); };
            dgvPatients.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            this.Controls.Add(dgvPatients);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlTop);
        }

        // Helper: TextBox with label
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

        // Helper: DateTimePicker with label
        private DateTimePicker CreateDateField(Panel parent, string labelText, Point location)
        {
            Label lbl = new Label
            {
                Text = labelText.ToUpper(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = location,
                AutoSize = true
            };
            DateTimePicker dtp = new DateTimePicker
            {
                Location = new Point(location.X, location.Y + 20),
                Size = new Size(360, 35),
                Font = new Font("Segoe UI", 11),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Now.AddYears(-1)  // Prevent future dates
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(dtp);
            return dtp;
        }

        // Helper: ComboBox with label
        private ComboBox CreateComboField(Panel parent, string labelText, Point location, string[] items)
        {
            Label lbl = new Label
            {
                Text = labelText.ToUpper(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = location,
                AutoSize = true
            };
            ComboBox cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(location.X, location.Y + 20),
                Size = new Size(360, 35),
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };
            cmb.Items.AddRange(items);
            parent.Controls.Add(lbl);
            parent.Controls.Add(cmb);
            return cmb;
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
            btnSearch.Click += (s, e) => SearchPatients();
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) SearchPatients(); };
        }

        private void LoadPatients()
        {
            var patients = _patientService.GetAllPatients();
            var displayList = patients.Select(p => new
            {
                p.PatientId,
                p.Name,
                p.Phone,
                p.Email,
                p.Address,
                GenderDisplay = p.Gender == 'M' ? "Male" : (p.Gender == 'F' ? "Female" : "Other"),
                p.BloodGroup,
                DateOfBirth = p.DateOfBirth.ToString("dd-MMM-yyyy"),
                RegistrationDate = p.RegistrationDate.ToString("dd-MMM-yyyy")
            }).ToList();

            dgvPatients.DataSource = null;
            dgvPatients.DataSource = displayList;
            lblCount.Text = $"Total: {displayList.Count}";

            if (dgvPatients.Columns.Contains("PatientId"))
                dgvPatients.Columns["PatientId"].Visible = false;
        }

        private void SearchPatients()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadPatients();
                return;
            }
            var results = _patientService.SearchPatients(txtSearch.Text);
            var displayList = results.Select(p => new
            {
                p.PatientId,
                p.Name,
                p.Phone,
                p.Email,
                p.Address,
                GenderDisplay = p.Gender == 'M' ? "Male" : (p.Gender == 'F' ? "Female" : "Other"),
                p.BloodGroup,
                DateOfBirth = p.DateOfBirth.ToString("dd-MMM-yyyy"),
                RegistrationDate = p.RegistrationDate.ToString("dd-MMM-yyyy")
            }).ToList();

            dgvPatients.DataSource = null;
            dgvPatients.DataSource = displayList;
            lblCount.Text = $"Found: {displayList.Count}";

            if (dgvPatients.Columns.Contains("PatientId"))
                dgvPatients.Columns["PatientId"].Visible = false;
        }

        private void SelectPatient(int rowIndex)
        {
            var selected = dgvPatients.Rows[rowIndex].DataBoundItem;
            var idProperty = selected.GetType().GetProperty("PatientId");
            if (idProperty != null)
                _selectedPatientId = (int)idProperty.GetValue(selected);

            txtName.Text = dgvPatients.Rows[rowIndex].Cells["Name"].Value?.ToString() ?? "";
            txtPhone.Text = dgvPatients.Rows[rowIndex].Cells["Phone"].Value?.ToString() ?? "";
            txtEmail.Text = dgvPatients.Rows[rowIndex].Cells["Email"].Value?.ToString() ?? "";
            txtAddress.Text = dgvPatients.Rows[rowIndex].Cells["Address"].Value?.ToString() ?? "";
            dtpDateOfBirth.Value = DateTime.Parse(dgvPatients.Rows[rowIndex].Cells["DateOfBirth"].Value?.ToString() ?? DateTime.Now.ToString());
            string gender = dgvPatients.Rows[rowIndex].Cells["GenderDisplay"].Value?.ToString() ?? "";
            cmbGender.SelectedItem = gender;
            string blood = dgvPatients.Rows[rowIndex].Cells["BloodGroup"].Value?.ToString() ?? "";
            cmbBlood.SelectedItem = blood;
        }

        private void ClearForm()
        {
            _selectedPatientId = -1;
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            dtpDateOfBirth.Value = DateTime.Now.AddYears(-30); // sensible default
            cmbGender.SelectedIndex = -1;
            cmbBlood.SelectedIndex = -1;
            txtSearch.Clear();
            LoadPatients();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                char genderChar = GetGenderChar(cmbGender.SelectedItem?.ToString());
                var patient = new Patient
                {
                    Name = txtName.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text,
                    Address = txtAddress.Text,
                    DateOfBirth = dtpDateOfBirth.Value,   // Now properly set
                    Gender = genderChar,
                    BloodGroup = cmbBlood.SelectedItem?.ToString() ?? "",
                    RegistrationDate = DateTime.Now
                };
                bool result = _patientService.RegisterPatient(patient);
                if (result)
                {
                    MessageBox.Show("Patient added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPatients();
                    ClearForm();
                }
                else
                    MessageBox.Show("Failed to add patient.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == -1)
            {
                MessageBox.Show("Please select a patient from the list to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (ValidateFields())
            {
                char genderChar = GetGenderChar(cmbGender.SelectedItem?.ToString());
                var patient = new Patient
                {
                    PatientId = _selectedPatientId,
                    Name = txtName.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text,
                    Address = txtAddress.Text,
                    DateOfBirth = dtpDateOfBirth.Value,
                    Gender = genderChar,
                    BloodGroup = cmbBlood.SelectedItem?.ToString() ?? ""
                };
                bool result = _patientService.UpdatePatient(patient);
                if (result)
                {
                    MessageBox.Show("Patient updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPatients();
                    ClearForm();
                }
                else
                    MessageBox.Show("Update failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId == -1)
            {
                MessageBox.Show("Please select a patient to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this patient?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool result = _patientService.DeletePatient(_selectedPatientId);
                if (result)
                {
                    MessageBox.Show("Patient deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPatients();
                    ClearForm();
                }
                else
                    MessageBox.Show("Deletion failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private char GetGenderChar(string genderString)
        {
            if (genderString == "Male") return 'M';
            if (genderString == "Female") return 'F';
            if (genderString == "Other") return 'O';
            return ' ';
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Patient name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Phone number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbGender.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a gender.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (dtpDateOfBirth.Value > DateTime.Now)
            {
                MessageBox.Show("Date of birth cannot be in the future.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}