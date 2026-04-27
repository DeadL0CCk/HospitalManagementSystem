using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Utils;

namespace HospitalManagementSystem.DAL
{
    /// <summary>
    /// AppointmentRepository - Data Access Layer for Appointment entity
    /// </summary>
    public class AppointmentRepository
    {
        private SqlConnectionHelper _dbHelper;

        public AppointmentRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        public bool AddAppointment(Appointment appointment)
        {
            try
            {
                string query = @"INSERT INTO Appointment (patient_id, doctor_id, receptionist_id, 
                                 appointment_date, appointment_time, status) 
                                 VALUES (@PatientId, @DoctorId, @ReceptionistId, @Date, @Time, @Status)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PatientId", appointment.PatientId),
                    new SqlParameter("@DoctorId", appointment.DoctorId),
                    new SqlParameter("@ReceptionistId", appointment.ReceptionistId ?? (object)DBNull.Value),
                    new SqlParameter("@Date", appointment.AppointmentDate),
                    new SqlParameter("@Time", appointment.AppointmentTime),
                    new SqlParameter("@Status", appointment.Status ?? "Pending")
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding appointment: " + ex.Message, ex);
            }
        }

        public List<Appointment> GetAllAppointments()
        {
            List<Appointment> appointments = new List<Appointment>();
            try
            {
                string query = @"SELECT a.*, p.name AS PatientName, d.name AS DoctorName 
                                 FROM Appointment a
                                 JOIN Patient p ON a.patient_id = p.patient_id
                                 JOIN Doctor d ON a.doctor_id = d.doctor_id
                                 ORDER BY a.appointment_date DESC, a.appointment_time DESC";

                SqlDataReader reader = _dbHelper.ExecuteQuery(query);

                while (reader.Read())
                {
                    Appointment appointment = new Appointment
                    {
                        AppointmentId = (int)reader["appointment_id"],
                        PatientId = (int)reader["patient_id"],
                        DoctorId = (int)reader["doctor_id"],
                        ReceptionistId = reader["receptionist_id"] == DBNull.Value ? null : (int?)reader["receptionist_id"],
                        AppointmentDate = (DateTime)reader["appointment_date"],
                        AppointmentTime = (TimeSpan)reader["appointment_time"],
                        Status = reader["status"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        DoctorName = reader["DoctorName"].ToString()
                    };
                    appointments.Add(appointment);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving appointments: " + ex.Message, ex);
            }
            return appointments;
        }

        public List<Appointment> GetAppointmentsByDoctor(int doctorId)
        {
            List<Appointment> appointments = new List<Appointment>();
            try
            {
                string query = @"SELECT a.*, p.name AS PatientName, d.name AS DoctorName 
                                 FROM Appointment a
                                 JOIN Patient p ON a.patient_id = p.patient_id
                                 JOIN Doctor d ON a.doctor_id = d.doctor_id
                                 WHERE a.doctor_id = @DoctorId
                                 ORDER BY a.appointment_date DESC";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@DoctorId", doctorId) };
                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                while (reader.Read())
                {
                    Appointment appointment = new Appointment
                    {
                        AppointmentId = (int)reader["appointment_id"],
                        PatientId = (int)reader["patient_id"],
                        DoctorId = (int)reader["doctor_id"],
                        ReceptionistId = reader["receptionist_id"] == DBNull.Value ? null : (int?)reader["receptionist_id"],
                        AppointmentDate = (DateTime)reader["appointment_date"],
                        AppointmentTime = (TimeSpan)reader["appointment_time"],
                        Status = reader["status"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        DoctorName = reader["DoctorName"].ToString()
                    };
                    appointments.Add(appointment);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving doctor appointments: " + ex.Message, ex);
            }
            return appointments;
        }

        public Appointment GetAppointmentById(int appointmentId)
        {
            try
            {
                string query = @"SELECT a.*, p.name AS PatientName, d.name AS DoctorName 
                                 FROM Appointment a
                                 JOIN Patient p ON a.patient_id = p.patient_id
                                 JOIN Doctor d ON a.doctor_id = d.doctor_id
                                 WHERE a.appointment_id = @AppointmentId";

                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AppointmentId", appointmentId) };
                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                if (reader.Read())
                {
                    Appointment appointment = new Appointment
                    {
                        AppointmentId = (int)reader["appointment_id"],
                        PatientId = (int)reader["patient_id"],
                        DoctorId = (int)reader["doctor_id"],
                        ReceptionistId = reader["receptionist_id"] == DBNull.Value ? null : (int?)reader["receptionist_id"],
                        AppointmentDate = (DateTime)reader["appointment_date"],
                        AppointmentTime = (TimeSpan)reader["appointment_time"],
                        Status = reader["status"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        DoctorName = reader["DoctorName"].ToString()
                    };
                    reader.Close();
                    return appointment;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving appointment: " + ex.Message, ex);
            }
            return null;
        }

        public bool UpdateAppointment(Appointment appointment)
        {
            try
            {
                string query = @"UPDATE Appointment SET patient_id = @PatientId, doctor_id = @DoctorId, 
                                 receptionist_id = @ReceptionistId, appointment_date = @Date, 
                                 appointment_time = @Time, status = @Status WHERE appointment_id = @AppointmentId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@AppointmentId", appointment.AppointmentId),
                    new SqlParameter("@PatientId", appointment.PatientId),
                    new SqlParameter("@DoctorId", appointment.DoctorId),
                    new SqlParameter("@ReceptionistId", appointment.ReceptionistId ?? (object)DBNull.Value),
                    new SqlParameter("@Date", appointment.AppointmentDate),
                    new SqlParameter("@Time", appointment.AppointmentTime),
                    new SqlParameter("@Status", appointment.Status)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating appointment: " + ex.Message, ex);
            }
        }

        public bool DeleteAppointment(int appointmentId)
        {
            try
            {
                string query = "DELETE FROM Appointment WHERE appointment_id = @AppointmentId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@AppointmentId", appointmentId) };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting appointment: " + ex.Message, ex);
            }
        }

        public List<Appointment> SearchAppointments(string searchTerm)
        {
            List<Appointment> appointments = new List<Appointment>();
            try
            {
                string query = @"SELECT a.*, p.name AS PatientName, d.name AS DoctorName 
                                 FROM Appointment a
                                 JOIN Patient p ON a.patient_id = p.patient_id
                                 JOIN Doctor d ON a.doctor_id = d.doctor_id
                                 WHERE p.name LIKE @SearchTerm OR p.phone LIKE @SearchTerm
                                 ORDER BY a.appointment_date DESC";

                SqlParameter[] parameters = new SqlParameter[] 
                { 
                    new SqlParameter("@SearchTerm", "%" + searchTerm + "%") 
                };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                while (reader.Read())
                {
                    Appointment appointment = new Appointment
                    {
                        AppointmentId = (int)reader["appointment_id"],
                        PatientId = (int)reader["patient_id"],
                        DoctorId = (int)reader["doctor_id"],
                        ReceptionistId = reader["receptionist_id"] == DBNull.Value ? null : (int?)reader["receptionist_id"],
                        AppointmentDate = (DateTime)reader["appointment_date"],
                        AppointmentTime = (TimeSpan)reader["appointment_time"],
                        Status = reader["status"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        DoctorName = reader["DoctorName"].ToString()
                    };
                    appointments.Add(appointment);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching appointments: " + ex.Message, ex);
            }
            return appointments;
        }
    }

    /// <summary>
    /// DoctorRepository - Data Access Layer for Doctor entity
    /// </summary>
    public class DoctorRepository
    {
        private SqlConnectionHelper _dbHelper;

        public DoctorRepository()
        {
            _dbHelper = SqlConnectionHelper.Instance;
        }

        public bool AddDoctor(Doctor doctor)
        {
            try
            {
                string query = @"INSERT INTO Doctor (name, specialization, department, email, phone) 
                                 VALUES (@Name, @Specialization, @Department, @Email, @Phone)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Name", doctor.Name),
                    new SqlParameter("@Specialization", doctor.Specialization),
                    new SqlParameter("@Department", doctor.Department),
                    new SqlParameter("@Email", doctor.Email ?? ""),
                    new SqlParameter("@Phone", doctor.Phone)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding doctor: " + ex.Message, ex);
            }
        }

        public List<Doctor> GetAllDoctors()
        {
            List<Doctor> doctors = new List<Doctor>();
            try
            {
                string query = "SELECT * FROM Doctor ORDER BY name ASC";
                SqlDataReader reader = _dbHelper.ExecuteQuery(query);

                while (reader.Read())
                {
                    Doctor doctor = new Doctor
                    {
                        DoctorId = (int)reader["doctor_id"],
                        Name = reader["name"].ToString(),
                        Specialization = reader["specialization"].ToString(),
                        Department = reader["department"].ToString(),
                        Email = reader["email"].ToString(),
                        Phone = reader["phone"].ToString()
                    };
                    doctors.Add(doctor);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving doctors: " + ex.Message, ex);
            }
            return doctors;
        }

        public Doctor GetDoctorById(int doctorId)
        {
            try
            {
                string query = "SELECT * FROM Doctor WHERE doctor_id = @DoctorId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@DoctorId", doctorId) };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                if (reader.Read())
                {
                    Doctor doctor = new Doctor
                    {
                        DoctorId = (int)reader["doctor_id"],
                        Name = reader["name"].ToString(),
                        Specialization = reader["specialization"].ToString(),
                        Department = reader["department"].ToString(),
                        Email = reader["email"].ToString(),
                        Phone = reader["phone"].ToString()
                    };
                    reader.Close();
                    return doctor;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving doctor: " + ex.Message, ex);
            }
            return null;
        }

        public bool UpdateDoctor(Doctor doctor)
        {
            try
            {
                string query = @"UPDATE Doctor SET name = @Name, specialization = @Specialization, 
                                 department = @Department, email = @Email, phone = @Phone WHERE doctor_id = @DoctorId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@DoctorId", doctor.DoctorId),
                    new SqlParameter("@Name", doctor.Name),
                    new SqlParameter("@Specialization", doctor.Specialization),
                    new SqlParameter("@Department", doctor.Department),
                    new SqlParameter("@Email", doctor.Email ?? ""),
                    new SqlParameter("@Phone", doctor.Phone)
                };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating doctor: " + ex.Message, ex);
            }
        }

        public bool DeleteDoctor(int doctorId)
        {
            try
            {
                string query = "DELETE FROM Doctor WHERE doctor_id = @DoctorId";
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@DoctorId", doctorId) };

                return _dbHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting doctor: " + ex.Message, ex);
            }
        }

        public List<Doctor> SearchDoctors(string searchTerm)
        {
            List<Doctor> doctors = new List<Doctor>();
            try
            {
                string query = @"SELECT * FROM Doctor 
                                 WHERE name LIKE @SearchTerm OR specialization LIKE @SearchTerm
                                 ORDER BY name ASC";

                SqlParameter[] parameters = new SqlParameter[] 
                { 
                    new SqlParameter("@SearchTerm", "%" + searchTerm + "%") 
                };

                SqlDataReader reader = _dbHelper.ExecuteQuery(query, parameters);

                while (reader.Read())
                {
                    Doctor doctor = new Doctor
                    {
                        DoctorId = (int)reader["doctor_id"],
                        Name = reader["name"].ToString(),
                        Specialization = reader["specialization"].ToString(),
                        Department = reader["department"].ToString(),
                        Email = reader["email"].ToString(),
                        Phone = reader["phone"].ToString()
                    };
                    doctors.Add(doctor);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching doctors: " + ex.Message, ex);
            }
            return doctors;
        }
    }
}
