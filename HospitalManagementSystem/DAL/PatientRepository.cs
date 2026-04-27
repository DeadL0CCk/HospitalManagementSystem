using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Utils;

namespace HospitalManagementSystem.DAL
{
    /// <summary>
    /// PatientRepository - Data Access Layer for Patient entity
    /// CRUD operations for Patient records
    /// </summary>
    public class PatientRepository
    {
        private SqlConnectionHelper _dbHelper;

        public PatientRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        /// <summary>
        /// Add new patient to database
        /// </summary>
        public bool AddPatient(Patient patient)
        {
            try
            {
                string query = @"INSERT INTO Patient (name, date_of_birth, gender, blood_group, address, phone, email) 
                                 VALUES (@Name, @DateOfBirth, @Gender, @BloodGroup, @Address, @Phone, @Email)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Name", patient.Name),
                    new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                    new SqlParameter("@Gender", patient.Gender),
                    new SqlParameter("@BloodGroup", patient.BloodGroup ?? ""),
                    new SqlParameter("@Address", patient.Address ?? ""),
                    new SqlParameter("@Phone", patient.Phone),
                    new SqlParameter("@Email", patient.Email ?? "")
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding patient: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get all patients from database
        /// </summary>
        public List<Patient> GetAllPatients()
        {
            List<Patient> patients = new List<Patient>();
            try
            {
                string query = "SELECT * FROM Patient ORDER BY patient_id DESC";
                SqlDataReader reader = _dbHelper.ExecuteQuery(query);

                while (reader.Read())
                {
                    Patient patient = new Patient
                    {
                        PatientId = (int)reader["patient_id"],
                        Name = reader["name"] != DBNull.Value ? reader["name"].ToString() : string.Empty,
                        DateOfBirth = reader["date_of_birth"] != DBNull.Value
                            ? (DateTime)reader["date_of_birth"]
                            : DateTime.Now,
                        Gender = reader["gender"] != DBNull.Value
                            ? reader["gender"].ToString()[0]
                            : 'M',
                        BloodGroup = reader["blood_group"] != DBNull.Value
                            ? reader["blood_group"].ToString()
                            : "O+",
                        Address = reader["address"] != DBNull.Value
                            ? reader["address"].ToString()
                            : string.Empty,
                        Phone = reader["phone"] != DBNull.Value
                            ? reader["phone"].ToString()
                            : string.Empty,
                        Email = reader["email"] != DBNull.Value
                            ? reader["email"].ToString()
                            : string.Empty,
                        RegistrationDate = reader["registration_date"] != DBNull.Value
                            ? (DateTime)reader["registration_date"]
                            : DateTime.Now
                    };
                    patients.Add(patient);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving patients: " + ex.Message, ex);
            }
            return patients;
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        public Patient GetPatientById(int patientId)
        {
            try
            {
                string query = "SELECT * FROM Patient WHERE patient_id = @PatientId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@PatientId", patientId) };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                if (reader.Read())
                {
                    Patient patient = new Patient
                    {
                        PatientId = (int)reader["patient_id"],
                        Name = reader["name"].ToString(),
                        DateOfBirth = (DateTime)reader["date_of_birth"],
                        Gender = reader["gender"].ToString()[0],
                        BloodGroup = reader["blood_group"].ToString(),
                        Address = reader["address"].ToString(),
                        Phone = reader["phone"].ToString(),
                        Email = reader["email"].ToString(),
                        RegistrationDate = (DateTime)reader["registration_date"]
                    };
                    reader.Close();
                    return patient;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving patient: " + ex.Message, ex);
            }
            return null;
        }

        /// <summary>
        /// Update patient information
        /// </summary>
        public bool UpdatePatient(Patient patient)
        {
            try
            {
                string query = @"UPDATE Patient SET name = @Name, date_of_birth = @DateOfBirth, 
                                 gender = @Gender, blood_group = @BloodGroup, address = @Address, 
                                 phone = @Phone, email = @Email WHERE patient_id = @PatientId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PatientId", patient.PatientId),
                    new SqlParameter("@Name", patient.Name),
                    new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                    new SqlParameter("@Gender", patient.Gender),
                    new SqlParameter("@BloodGroup", patient.BloodGroup ?? ""),
                    new SqlParameter("@Address", patient.Address ?? ""),
                    new SqlParameter("@Phone", patient.Phone),
                    new SqlParameter("@Email", patient.Email ?? "")
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating patient: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Delete patient by ID
        /// </summary>
        public bool DeletePatient(int patientId)
        {
            try
            {
                string query = "DELETE FROM Patient WHERE patient_id = @PatientId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@PatientId", patientId) };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting patient: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Search patients by name or phone
        /// </summary>
        public List<Patient> SearchPatients(string searchTerm)
        {
            List<Patient> patients = new List<Patient>();
            try
            {
                string query = @"SELECT * FROM Patient 
                                 WHERE name LIKE @SearchTerm OR phone LIKE @SearchTerm 
                                 ORDER BY name ASC";

                SqlParameter[] parameters = new SqlParameter[] 
                { 
                    new SqlParameter("@SearchTerm", "%" + searchTerm + "%") 
                };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                while (reader.Read())
                {
                    Patient patient = new Patient
                    {
                        PatientId = (int)reader["patient_id"],
                        Name = reader["name"].ToString(),
                        DateOfBirth = (DateTime)reader["date_of_birth"],
                        Gender = reader["gender"].ToString()[0],
                        BloodGroup = reader["blood_group"].ToString(),
                        Address = reader["address"].ToString(),
                        Phone = reader["phone"].ToString(),
                        Email = reader["email"].ToString(),
                        RegistrationDate = (DateTime)reader["registration_date"]
                    };
                    patients.Add(patient);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching patients: " + ex.Message, ex);
            }
            return patients;
        }

        /// <summary>
        /// Get total number of patients
        /// </summary>
        public int GetTotalPatients()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Patient";
                object result = _dbHelper.ExecuteScalar(query);
                return result != null ? (int)result : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error counting patients: " + ex.Message, ex);
            }
        }
    }
}
