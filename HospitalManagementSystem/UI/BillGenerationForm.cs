using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HospitalManagementSystem.BLL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.UI
{
    public class BillGenerationForm : Form
    {
        private BillService _billService;
        private AppointmentService _appointmentService;
        private PatientService _patientService;

        private ComboBox cmbAppointment;
        private TextBox txtAmount;
        private Button btnGenerate, btnMarkPaid, btnRefresh;
        private DataGridView dgvBills;
        private Label lblCount;

        private readonly Color PrimaryColor = Color.FromArgb(0, 92, 151);
        private readonly Color BgLight = Color.FromArgb(248, 250, 252);

        public BillGenerationForm()
        {
            _billService = new BillService();
            _appointmentService = new AppointmentService();
            _patientService = new PatientService();
            InitializeComponent();
            SetupEvents();
            LoadBills();
            LoadAppointments();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.BackColor = Color.White;
            this.Text = "CareTech HMS - Bill Generation";
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
                Text = "BILL MANAGEMENT",
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
                Height = 160,
                BackColor = BgLight,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(20)
            };

            // Appointment selection
            Label lblAppointment = new Label
            {
                Text = "APPOINTMENT",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(30, 20),
                AutoSize = true
            };
            cmbAppointment = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(30, 45),
                Size = new Size(400, 35),
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };
            pnlInput.Controls.Add(lblAppointment);
            pnlInput.Controls.Add(cmbAppointment);

            // Amount
            Label lblAmount = new Label
            {
                Text = "TOTAL AMOUNT (BDT)",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(500, 20),
                AutoSize = true
            };
            txtAmount = new TextBox
            {
                Location = new Point(500, 45),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlInput.Controls.Add(lblAmount);
            pnlInput.Controls.Add(txtAmount);

            // Buttons
            btnGenerate = new Button
            {
                Text = "Generate Bill",
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(140, 40),
                Location = new Point(30, 95),
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            pnlInput.Controls.Add(btnGenerate);

            btnMarkPaid = new Button
            {
                Text = "Mark as Paid",
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(140, 40),
                Location = new Point(190, 95),
                Cursor = Cursors.Hand
            };
            btnMarkPaid.FlatAppearance.BorderSize = 0;
            pnlInput.Controls.Add(btnMarkPaid);

            btnRefresh = new Button
            {
                Text = "Refresh",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(750, 95),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            pnlInput.Controls.Add(btnRefresh);

            // ---- DataGridView ----
            dgvBills = new DataGridView
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
            dgvBills.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dgvBills.CellClick += (s, e) => { if (e.RowIndex >= 0) SelectBill(e.RowIndex); };

            // Total count label
            lblCount = new Label
            {
                Text = "Total Bills: 0",
                Location = new Point(this.Width - 150, this.Height - 40),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = PrimaryColor
            };
            this.Controls.Add(lblCount);

            this.Controls.Add(dgvBills);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlTop);
        }

        private void SetupEvents()
        {
            btnGenerate.Click += BtnGenerate_Click;
            btnMarkPaid.Click += BtnMarkPaid_Click;
            btnRefresh.Click += (s, e) => LoadBills();
        }

        private void LoadAppointments()
        {
            var allAppointments = _appointmentService.GetAllAppointments();
            var completedAppointments = allAppointments.Where(a => a.Status == "Completed").ToList();
            var allBills = _billService.GetAllBills();
            var billedIds = allBills.Select(b => b.AppointmentId).ToHashSet();
            var unbilled = completedAppointments.Where(a => !billedIds.Contains(a.AppointmentId)).ToList();

            // Show counts for debugging – REMOVE THIS AFTER TESTING
            MessageBox.Show($"Total appointments: {allAppointments.Count}\nCompleted: {completedAppointments.Count}\nBilled IDs: {billedIds.Count}\nUnbilled: {unbilled.Count}");

            cmbAppointment.DataSource = unbilled;
            cmbAppointment.DisplayMember = "AppointmentDateAndTime";
            cmbAppointment.ValueMember = "AppointmentId";
        }

        private void LoadBills()
        {
            var bills = _billService.GetAllBills();
            var displayList = bills.Select(b => new
            {
                b.BillId,
                b.PatientName,
                Amount = b.TotalAmount.ToString("0.00"),
                Status = b.PaymentStatus,
                PaymentDate = b.PaymentDate.HasValue ? b.PaymentDate.Value.ToString("dd-MMM-yyyy") : "Not Paid"
                // Removed "Date" because Bill does not have BillDate.
                // If you need the bill creation date, add the property to the Bill model.
            }).ToList();

            dgvBills.DataSource = null;
            dgvBills.DataSource = displayList;
            lblCount.Text = $"Total Bills: {displayList.Count}";

            if (dgvBills.Columns.Contains("BillId"))
                dgvBills.Columns["BillId"].Visible = false;
        }

        private void SelectBill(int rowIndex)
        {
            // Optional: can be used to pre-fill data for marking as paid
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (cmbAppointment.SelectedValue == null)
            {
                MessageBox.Show("Please select an appointment.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int appointmentId = (int)cmbAppointment.SelectedValue;
                var appointment = _appointmentService.GetAppointmentById(appointmentId);
                if (appointment == null)
                {
                    MessageBox.Show("Appointment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var bill = new Bill
                {
                    AppointmentId = appointmentId,
                    TotalAmount = amount,
                    PaymentStatus = "Unpaid",
                    PaymentDate = null
                };

                bool result = _billService.GenerateBill(bill);
                if (result)
                {
                    MessageBox.Show("Bill generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBills();
                    LoadAppointments();  // refresh available appointments
                    txtAmount.Clear();
                    cmbAppointment.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Failed to generate bill.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMarkPaid_Click(object sender, EventArgs e)
        {
            if (dgvBills.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a bill from the list to mark as paid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected bill's ID from the DataGridView (hidden column)
            int billId = (int)dgvBills.SelectedRows[0].Cells["BillId"].Value;

            if (MessageBox.Show("Mark this bill as paid? This action cannot be undone.", "Confirm Payment",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool result = _billService.MarkAsPayment(billId, 0); // amount parameter not used in your BillService, we assume full payment
                    if (result)
                    {
                        MessageBox.Show("Bill marked as paid.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBills();
                        LoadAppointments(); // refreshes in case the appointment had a bill? Not needed but safe.
                    }
                    else
                    {
                        MessageBox.Show("Failed to update payment status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}