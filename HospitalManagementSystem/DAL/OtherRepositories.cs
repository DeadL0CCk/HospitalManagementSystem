using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Utils;

namespace HospitalManagementSystem.DAL
{
    /// <summary>
    /// LoginRepository - Data Access Layer for Login/Authentication
    /// </summary>
    public class LoginRepository
    {
        private SqlConnectionHelper _dbHelper;

        public LoginRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        public static string HashPassword(string password)
        {
            // Generate random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash password with PBKDF2 (industry standard)
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combine salt + hash
            byte[] hashWithSalt = new byte[36];
            Array.Copy(salt, 0, hashWithSalt, 0, 16);
            Array.Copy(hash, 0, hashWithSalt, 16, 20);

            return Convert.ToBase64String(hashWithSalt);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Validate hash format
                if (string.IsNullOrWhiteSpace(hash))
                    return false;

                // Handle plain text passwords (legacy support - for existing unencrypted passwords)
                if (hash == password)
                    return true;

                // Try PBKDF2 format (48 chars, Base64)
                if (hash.Length == 48)
                {
                    try
                    {
                        byte[] hashWithSalt = Convert.FromBase64String(hash);
                        
                        if (hashWithSalt.Length == 36)
                        {
                            byte[] salt = new byte[16];
                            Array.Copy(hashWithSalt, 0, salt, 0, 16);

                            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                            byte[] computedHash = pbkdf2.GetBytes(20);

                            // Constant-time comparison
                            for (int i = 0; i < 20; i++)
                            {
                                if (hashWithSalt[i + 16] != computedHash[i])
                                    return false;
                            }
                            return true;
                        }
                    }
                    catch (FormatException)
                    {
                        // Not valid Base64, continue to legacy check
                    }
                }

                // Legacy SHA1 format (40 hex chars)
                if (hash.Length == 40)
                {
                    return VerifyLegacySHA1(password, hash);
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error verifying password: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifies legacy SHA1 hashed passwords (40 character hex strings)
        /// </summary>
        private static bool VerifyLegacySHA1(string password, string legacyHash)
        {
            try
            {
                using (var sha1 = System.Security.Cryptography.SHA1.Create())
                {
                    byte[] hashedBytes = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    string computedHash = System.BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                    return computedHash == legacyHash.ToLower();
                }
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateLogin(int userId, string password, string role)
        {
            try
            {
                // First, get the stored password hash from the database
                string query = "SELECT password_hash FROM Login WHERE user_id = @UserId AND role = @Role";
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Role", role)
                };

                object result = _dbHelper.ExecuteScalar(query, parameters);
                if (result == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] No record found for userId={userId}, role={role}");
                    return false;
                }

                string storedHash = result.ToString();
                
                // Validate that hash is not empty
                if (string.IsNullOrWhiteSpace(storedHash))
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Empty hash for userId={userId}, role={role}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Hash length: {storedHash.Length}, Password length: {password.Length}");
                System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Hash value: {storedHash}");
                System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Password value: {password}");

                // Use VerifyPassword to check against the stored hash
                bool result_verify = VerifyPassword(password, storedHash);
                System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] VerifyPassword returned: {result_verify}");
                
                return result_verify;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Exception: {ex.Message}");
                throw new Exception("Error validating login: " + ex.Message, ex);
            }
        }

        public string GetRoleByUserId(int userId)
        {
            try
            {
                string query = "SELECT role FROM Login WHERE user_id = @UserId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@UserId", userId) };

                object result = _dbHelper.ExecuteScalar(query, parameters);
                return result?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving role: " + ex.Message, ex);
            }
        }
        /// <summary>
        /// Validates user credentials and returns the role if successful.
        /// Checks all rows for the given user_id to find a matching password.
        /// </summary>
        public string ValidateLoginAndGetRole(int userId, string password)
        {
            try
            {
                // Get all roles and password hashes for this user
                string query = "SELECT password_hash, role FROM Login WHERE user_id = @UserId";
                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@UserId", userId)
                };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);
                while (reader.Read())
                {
                    string storedHash = reader["password_hash"].ToString();
                    string role = reader["role"].ToString();

                    if (!string.IsNullOrWhiteSpace(storedHash) && VerifyPassword(password, storedHash))
                    {
                        reader.Close();
                        return role;
                    }
                }
                reader.Close();
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error validating login: " + ex.Message, ex);
            }
        }

        public bool AddLoginRecord(int userId, string role, string password)
        {
            try
            {
                string passwordHash = HashPassword(password);
                string query = "INSERT INTO Login (user_id, role, password_hash) VALUES (@UserId, @Role, @PasswordHash)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Role", role),
                    new SqlParameter("@PasswordHash", passwordHash)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding login record: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates a login record's password hash to the new secure format
        /// Use this to migrate existing plain-text passwords to PBKDF2 hashed passwords
        /// </summary>
        public bool UpdatePasswordHash(int userId, string role, string newPassword)
        {
            try
            {
                string passwordHash = HashPassword(newPassword);
                string query = "UPDATE Login SET password_hash = @PasswordHash WHERE user_id = @UserId AND role = @Role";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Role", role)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating password hash: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets all login records (for admin purposes)
        /// </summary>
        public List<(int UserId, string Role)> GetAllLoginRecords()
        {
            List<(int, string)> records = new List<(int, string)>();
            try
            {
                string query = "SELECT user_id, role FROM Login ORDER BY user_id";
                SqlDataReader reader = _dbHelper.ExecuteQuery(query);

                while (reader.Read())
                {
                    int userId = (int)reader["user_id"];
                    string role = reader["role"].ToString();
                    records.Add((userId, role));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving login records: " + ex.Message, ex);
            }
            return records;
        }
    }

    /// <summary>
    /// MedicineRepository - Data Access Layer for Medicine entity
    /// </summary>
    public class MedicineRepository
    {
        private SqlConnectionHelper _dbHelper;

        public MedicineRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        public bool AddMedicine(Medicine medicine)
        {
            try
            {
                string query = @"INSERT INTO Medicine (name, type, quantity_in_stock, unit_price) 
                                 VALUES (@Name, @Type, @Quantity, @Price)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Name", medicine.Name),
                    new SqlParameter("@Type", medicine.Type),
                    new SqlParameter("@Quantity", medicine.QuantityInStock),
                    new SqlParameter("@Price", medicine.UnitPrice)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding medicine: " + ex.Message, ex);
            }
        }

        public List<Medicine> GetAllMedicines()
        {
            List<Medicine> medicines = new List<Medicine>();
            try
            {
                string query = "SELECT * FROM Medicine ORDER BY name ASC";
                SqlDataReader reader = _dbHelper.ExecuteQuery(query);

                while (reader.Read())
                {
                    Medicine medicine = new Medicine
                    {
                        MedicineId = (int)reader["medicine_id"],
                        Name = reader["name"].ToString(),
                        Type = reader["type"].ToString(),
                        QuantityInStock = (int)reader["quantity_in_stock"],
                        UnitPrice = (decimal)reader["unit_price"]
                    };
                    medicines.Add(medicine);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving medicines: " + ex.Message, ex);
            }
            return medicines;
        }

        public Medicine GetMedicineById(int medicineId)
        {
            try
            {
                string query = "SELECT * FROM Medicine WHERE medicine_id = @MedicineId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@MedicineId", medicineId) };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                if (reader.Read())
                {
                    Medicine medicine = new Medicine
                    {
                        MedicineId = (int)reader["medicine_id"],
                        Name = reader["name"].ToString(),
                        Type = reader["type"].ToString(),
                        QuantityInStock = (int)reader["quantity_in_stock"],
                        UnitPrice = (decimal)reader["unit_price"]
                    };
                    reader.Close();
                    return medicine;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving medicine: " + ex.Message, ex);
            }
            return null;
        }

        public bool UpdateMedicine(Medicine medicine)
        {
            try
            {
                string query = @"UPDATE Medicine SET name = @Name, type = @Type, 
                                 quantity_in_stock = @Quantity, unit_price = @Price WHERE medicine_id = @MedicineId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MedicineId", medicine.MedicineId),
                    new SqlParameter("@Name", medicine.Name),
                    new SqlParameter("@Type", medicine.Type),
                    new SqlParameter("@Quantity", medicine.QuantityInStock),
                    new SqlParameter("@Price", medicine.UnitPrice)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating medicine: " + ex.Message, ex);
            }
        }

        public bool DeleteMedicine(int medicineId)
        {
            try
            {
                string query = "DELETE FROM Medicine WHERE medicine_id = @MedicineId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@MedicineId", medicineId) };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting medicine: " + ex.Message, ex);
            }
        }

        public List<Medicine> SearchMedicines(string searchTerm)
        {
            List<Medicine> medicines = new List<Medicine>();
            try
            {
                string query = @"SELECT * FROM Medicine 
                                 WHERE name LIKE @SearchTerm OR type LIKE @SearchTerm
                                 ORDER BY name ASC";

                SqlParameter[] parameters = new SqlParameter[] 
                { 
                    new SqlParameter("@SearchTerm", "%" + searchTerm + "%") 
                };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                while (reader.Read())
                {
                    Medicine medicine = new Medicine
                    {
                        MedicineId = (int)reader["medicine_id"],
                        Name = reader["name"].ToString(),
                        Type = reader["type"].ToString(),
                        QuantityInStock = (int)reader["quantity_in_stock"],
                        UnitPrice = (decimal)reader["unit_price"]
                    };
                    medicines.Add(medicine);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching medicines: " + ex.Message, ex);
            }
            return medicines;
        }
    }

    /// <summary>
    /// PrescriptionRepository - Data Access Layer for Prescription entity
    /// </summary>
    public class PrescriptionRepository
    {
        private SqlConnectionHelper _dbHelper;

        public PrescriptionRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        public bool AddPrescription(Prescription prescription)
        {
            try
            {
                string query = @"INSERT INTO Prescription (appointment_id, diagnosis, notes, prescription_date) 
                                 VALUES (@AppointmentId, @Diagnosis, @Notes, @PrescriptionDate)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AppointmentId", prescription.AppointmentId),
                    new SqlParameter("@Diagnosis", prescription.Diagnosis),
                    new SqlParameter("@Notes", prescription.Notes ?? ""),
                    new SqlParameter("@PrescriptionDate", prescription.PrescriptionDate)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding prescription: " + ex.Message, ex);
            }
        }

        public Prescription GetPrescriptionByAppointmentId(int appointmentId)
        {
            try
            {
                string query = "SELECT * FROM Prescription WHERE appointment_id = @AppointmentId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AppointmentId", appointmentId) };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                if (reader.Read())
                {
                    Prescription prescription = new Prescription
                    {
                        PrescriptionId = (int)reader["prescription_id"],
                        AppointmentId = (int)reader["appointment_id"],
                        Diagnosis = reader["diagnosis"].ToString(),
                        Notes = reader["notes"].ToString(),
                        PrescriptionDate = (DateTime)reader["prescription_date"]
                    };
                    reader.Close();
                    return prescription;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving prescription: " + ex.Message, ex);
            }
            return null;
        }

        public List<PrescriptionMedicine> GetPrescriptionMedicines(int prescriptionId)
        {
            List<PrescriptionMedicine> medicines = new List<PrescriptionMedicine>();
            try
            {
                string query = @"SELECT pm.*, m.name FROM PrescriptionMedicine pm
                                 JOIN Medicine m ON pm.medicine_id = m.medicine_id
                                 WHERE pm.prescription_id = @PrescriptionId";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@PrescriptionId", prescriptionId) };
                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                while (reader.Read())
                {
                    PrescriptionMedicine pm = new PrescriptionMedicine
                    {
                        PresMedId = (int)reader["pres_med_id"],
                        PrescriptionId = (int)reader["prescription_id"],
                        MedicineId = (int)reader["medicine_id"],
                        Dosage = reader["dosage"].ToString(),
                        Frequency = reader["frequency"].ToString(),
                        Duration = reader["duration"].ToString(),
                        MedicineName = reader["name"].ToString()
                    };
                    medicines.Add(pm);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving prescription medicines: " + ex.Message, ex);
            }
            return medicines;
        }

        public bool AddPrescriptionMedicine(PrescriptionMedicine presMed)
        {
            try
            {
                string query = @"INSERT INTO PrescriptionMedicine (prescription_id, medicine_id, dosage, frequency, duration) 
                                 VALUES (@PrescriptionId, @MedicineId, @Dosage, @Frequency, @Duration)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PrescriptionId", presMed.PrescriptionId),
                    new SqlParameter("@MedicineId", presMed.MedicineId),
                    new SqlParameter("@Dosage", presMed.Dosage),
                    new SqlParameter("@Frequency", presMed.Frequency),
                    new SqlParameter("@Duration", presMed.Duration ?? "")
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding prescription medicine: " + ex.Message, ex);
            }
        }
    }

    /// <summary>
    /// BillRepository - Data Access Layer for Bill entity
    /// </summary>
    public class BillRepository
    {
        private SqlConnectionHelper _dbHelper;

        public BillRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        public bool AddBill(Bill bill)
        {
            try
            {
                string query = @"INSERT INTO Bill (appointment_id, total_amount, payment_status) 
                                 VALUES (@AppointmentId, @TotalAmount, @PaymentStatus)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AppointmentId", bill.AppointmentId),
                    new SqlParameter("@TotalAmount", bill.TotalAmount),
                    new SqlParameter("@PaymentStatus", bill.PaymentStatus ?? "Unpaid")
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding bill: " + ex.Message, ex);
            }
        }

        public List<Bill> GetAllBills()
        {
            List<Bill> bills = new List<Bill>();
            try
            {
                string query = @"SELECT b.*, p.patient_id, p.name FROM Bill b
                                 JOIN Appointment a ON b.appointment_id = a.appointment_id
                                 JOIN Patient p ON a.patient_id = p.patient_id
                                 ORDER BY b.bill_date DESC";

                SqlDataReader reader = _dbHelper.ExecuteQuery(query);

                while (reader.Read())
                {
                    Bill bill = new Bill
                    {
                        BillId = (int)reader["bill_id"],
                        AppointmentId = (int)reader["appointment_id"],
                        TotalAmount = (decimal)reader["total_amount"],
                        PaymentStatus = reader["payment_status"].ToString(),
                        PaymentDate = reader["payment_date"] == DBNull.Value ? null : (DateTime?)reader["payment_date"],
                        PatientId = (int)reader["patient_id"],
                        PatientName = reader["name"].ToString()
                    };
                    bills.Add(bill);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving bills: " + ex.Message, ex);
            }
            return bills;
        }

        public Bill GetBillByAppointmentId(int appointmentId)
        {
            try
            {
                string query = @"SELECT b.*, p.patient_id, p.name FROM Bill b
                                 JOIN Appointment a ON b.appointment_id = a.appointment_id
                                 JOIN Patient p ON a.patient_id = p.patient_id
                                 WHERE b.appointment_id = @AppointmentId";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AppointmentId", appointmentId) };
                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                if (reader.Read())
                {
                    Bill bill = new Bill
                    {
                        BillId = (int)reader["bill_id"],
                        AppointmentId = (int)reader["appointment_id"],
                        TotalAmount = (decimal)reader["total_amount"],
                        PaymentStatus = reader["payment_status"].ToString(),
                        PaymentDate = reader["payment_date"] == DBNull.Value ? null : (DateTime?)reader["payment_date"],
                        PatientId = (int)reader["patient_id"],
                        PatientName = reader["name"].ToString()
                    };
                    reader.Close();
                    return bill;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving bill: " + ex.Message, ex);
            }
            return null;
        }

        public bool UpdateBillPaymentStatus(int billId, string paymentStatus, DateTime? paymentDate)
        {
            try
            {
                string query = @"UPDATE Bill SET payment_status = @PaymentStatus, payment_date = @PaymentDate WHERE bill_id = @BillId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@BillId", billId),
                    new SqlParameter("@PaymentStatus", paymentStatus),
                    new SqlParameter("@PaymentDate", paymentDate ?? (object)DBNull.Value)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating bill: " + ex.Message, ex);
            }
        }
    }
}
