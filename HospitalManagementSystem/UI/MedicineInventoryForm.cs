using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class MedicineInventoryForm : Form
    {
        private MedicineService _medicineService;
        private DataGridView dgvMedicines;
        private TextBox txtName, txtQty, txtPrice, txtSearch;
        private ComboBox cmbType;
        private Button btnAdd, btnEdit, btnDelete, btnClear, btnSearch;
        private Label lblCount;
        private int _selectedId = -1;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public MedicineInventoryForm()
        {
            _medicineService = new MedicineService();
            InitializeComponent();
            SetupEvents();
            LoadMedicines();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1300, 750);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Medicine Inventory";
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
                Text = "MEDICINE INVENTORY",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            // Search TextBox
            txtSearch = new TextBox
            {
                Width = 310,
                Location = new Point(780, 15),
                Font = new Font("Segoe UI", 11)
            };

            // Search Button – same height as TextBox, with 20px gap
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

            // Total count label
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

            // Row 1: Medicine Name, Type, Quantity
            txtName = CreateInputField(pnlInput, "Medicine Name", new Point(30, 20));
            cmbType = CreateComboField(pnlInput, "Type", new Point(430, 20), new[] { "Tablet", "Capsule", "Syrup", "Injection", "Drops" });
            txtQty = CreateInputField(pnlInput, "Quantity In Stock", new Point(830, 20));

            // Row 2: Price (Unit Price) – additional fields can be added later if needed
            txtPrice = CreateInputField(pnlInput, "Unit Price", new Point(30, 90));

            // Buttons aligned at the bottom of the input panel
            int btnY = 160;
            btnAdd = CreateButton(pnlInput, "Add Medicine", PrimaryColor, new Point(30, btnY));
            btnEdit = CreateButton(pnlInput, "Edit Medicine", Color.FromArgb(34, 197, 94), new Point(210, btnY));
            btnDelete = CreateButton(pnlInput, "Delete Medicine", Color.FromArgb(239, 68, 68), new Point(390, btnY));
            btnClear = CreateButton(pnlInput, "Clear Fields", Color.Gray, new Point(570, btnY));

            // ---- 3. Bottom DataGridView ----
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
                RowTemplate = { Height = 40 },
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 250, 252) },
                GridColor = Color.FromArgb(230, 230, 230)
            };
            dgvMedicines.CellClick += (s, e) => { if (e.RowIndex >= 0) SelectMedicine(e.RowIndex); };
            dgvMedicines.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            // Order of adding: Grid (Fill) first, then Input (Top), then TopHeader (Top)
            this.Controls.Add(dgvMedicines);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlTop);
        }

        // Helper to create a text input field with label
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

        // Helper to create a combo box field with label
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
            btnSearch.Click += (s, e) => SearchMedicines();
            txtSearch.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) SearchMedicines(); };
        }

        private void LoadMedicines()
        {
            var medicines = _medicineService.GetAllMedicines();
            var displayList = medicines.Select(m => new
            {
                m.MedicineId,
                m.Name,
                m.Type,
                m.QuantityInStock,
                m.UnitPrice
            }).ToList();

            dgvMedicines.DataSource = null;
            dgvMedicines.DataSource = displayList;
            lblCount.Text = $"Total: {displayList.Count}";

            if (dgvMedicines.Columns.Contains("MedicineId"))
                dgvMedicines.Columns["MedicineId"].Visible = false;

            if (dgvMedicines.Columns.Contains("Name"))
                dgvMedicines.Columns["Name"].HeaderText = "Medicine Name";
            if (dgvMedicines.Columns.Contains("Type"))
                dgvMedicines.Columns["Type"].HeaderText = "Type";
            if (dgvMedicines.Columns.Contains("QuantityInStock"))
                dgvMedicines.Columns["QuantityInStock"].HeaderText = "Stock";
            if (dgvMedicines.Columns.Contains("UnitPrice"))
                dgvMedicines.Columns["UnitPrice"].HeaderText = "Price (BDT)";
        }

        private void SearchMedicines()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadMedicines();
                return;
            }
            var results = _medicineService.SearchMedicines(txtSearch.Text);
            var displayList = results.Select(m => new
            {
                m.MedicineId,
                m.Name,
                m.Type,
                m.QuantityInStock,
                m.UnitPrice
            }).ToList();

            dgvMedicines.DataSource = null;
            dgvMedicines.DataSource = displayList;
            lblCount.Text = $"Found: {displayList.Count}";

            if (dgvMedicines.Columns.Contains("MedicineId"))
                dgvMedicines.Columns["MedicineId"].Visible = false;
        }

        private void SelectMedicine(int rowIndex)
        {
            var selected = dgvMedicines.Rows[rowIndex].DataBoundItem;
            var idProperty = selected.GetType().GetProperty("MedicineId");
            if (idProperty != null)
                _selectedId = (int)idProperty.GetValue(selected);

            txtName.Text = dgvMedicines.Rows[rowIndex].Cells["Name"].Value?.ToString() ?? "";
            cmbType.SelectedItem = dgvMedicines.Rows[rowIndex].Cells["Type"].Value?.ToString() ?? "";
            txtQty.Text = dgvMedicines.Rows[rowIndex].Cells["QuantityInStock"].Value?.ToString() ?? "";
            txtPrice.Text = dgvMedicines.Rows[rowIndex].Cells["UnitPrice"].Value?.ToString() ?? "";
        }

        private void ClearForm()
        {
            _selectedId = -1;
            txtName.Clear();
            cmbType.SelectedIndex = -1;
            txtQty.Clear();
            txtPrice.Clear();
            txtSearch.Clear();
            LoadMedicines();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                var med = new Medicine
                {
                    Name = txtName.Text,
                    Type = cmbType.SelectedItem.ToString(),
                    QuantityInStock = int.Parse(txtQty.Text),
                    UnitPrice = decimal.Parse(txtPrice.Text)
                };
                bool result = _medicineService.AddMedicine(med);
                if (result)
                {
                    MessageBox.Show("Medicine added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMedicines();
                    ClearForm();
                }
                else
                    MessageBox.Show("Failed to add medicine.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedId == -1)
            {
                MessageBox.Show("Please select a medicine to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (ValidateFields())
            {
                var med = new Medicine
                {
                    MedicineId = _selectedId,
                    Name = txtName.Text,
                    Type = cmbType.SelectedItem.ToString(),
                    QuantityInStock = int.Parse(txtQty.Text),
                    UnitPrice = decimal.Parse(txtPrice.Text)
                };
                bool result = _medicineService.UpdateMedicine(med);
                if (result)
                {
                    MessageBox.Show("Medicine updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMedicines();
                    ClearForm();
                }
                else
                    MessageBox.Show("Update failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedId == -1)
            {
                MessageBox.Show("Please select a medicine to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this medicine?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool result = _medicineService.DeleteMedicine(_selectedId);
                if (result)
                {
                    MessageBox.Show("Medicine deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMedicines();
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
                MessageBox.Show("Medicine name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a medicine type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtQty.Text, out int qty) || qty < 0)
            {
                MessageBox.Show("Quantity must be a valid non‑negative number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Price must be a valid non‑negative number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}