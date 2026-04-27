using System;
using System.Collections.Generic;
using HospitalManagementSystem.DAL;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.BLL
{
    /// <summary>
    /// PatientService - Business Logic for Patient Management
    /// </summary>
    public class PatientService
    {
        private PatientRepository _patientRepo;

        public PatientService()
        {
            _patientRepo = new PatientRepository();
        }

        public bool RegisterPatient(Patient patient)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(patient.Name))
                throw new Exception("Patient name is required");
            if (string.IsNullOrWhiteSpace(patient.Phone))
                throw new Exception("Patient phone is required");
            if (patient.DateOfBirth > DateTime.Now)
                throw new Exception("Date of birth cannot be in the future");

            return _patientRepo.AddPatient(patient);
        }

        public List<Patient> GetAllPatients()
        {
            return _patientRepo.GetAllPatients();
        }

        public Patient GetPatientById(int patientId)
        {
            return _patientRepo.GetPatientById(patientId);
        }

        public bool UpdatePatient(Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patient.Name))
                throw new Exception("Patient name is required");

            return _patientRepo.UpdatePatient(patient);
        }

        public bool DeletePatient(int patientId)
        {
            return _patientRepo.DeletePatient(patientId);
        }

        public List<Patient> SearchPatients(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllPatients();

            return _patientRepo.SearchPatients(searchTerm);
        }

        public int GetTotalPatients()
        {
            return _patientRepo.GetTotalPatients();
        }
    }

    /// <summary>
    /// DoctorService - Business Logic for Doctor Management
    /// </summary>
    public class DoctorService
    {
        private DoctorRepository _doctorRepo;

        public DoctorService()
        {
            _doctorRepo = new DoctorRepository();
        }

        public bool AddDoctor(Doctor doctor)
        {
            if (string.IsNullOrWhiteSpace(doctor.Name))
                throw new Exception("Doctor name is required");
            if (string.IsNullOrWhiteSpace(doctor.Specialization))
                throw new Exception("Specialization is required");
            if (string.IsNullOrWhiteSpace(doctor.Phone))
                throw new Exception("Phone number is required");

            return _doctorRepo.AddDoctor(doctor);
        }

        public List<Doctor> GetAllDoctors()
        {
            return _doctorRepo.GetAllDoctors();
        }

        public Doctor GetDoctorById(int doctorId)
        {
            return _doctorRepo.GetDoctorById(doctorId);
        }

        public bool UpdateDoctor(Doctor doctor)
        {
            if (string.IsNullOrWhiteSpace(doctor.Name))
                throw new Exception("Doctor name is required");

            return _doctorRepo.UpdateDoctor(doctor);
        }

        public bool DeleteDoctor(int doctorId)
        {
            return _doctorRepo.DeleteDoctor(doctorId);
        }

        public List<Doctor> SearchDoctors(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllDoctors();

            return _doctorRepo.SearchDoctors(searchTerm);
        }
    }

    /// <summary>
    /// AppointmentService - Business Logic for Appointment Management
    /// </summary>
    public class AppointmentService
    {
        private AppointmentRepository _appointmentRepo;

        public AppointmentService()
        {
            _appointmentRepo = new AppointmentRepository();
        }

        public bool BookAppointment(Appointment appointment)
        {
            if (appointment.PatientId <= 0)
                throw new Exception("Invalid patient");
            if (appointment.DoctorId <= 0)
                throw new Exception("Invalid doctor");
            if (appointment.AppointmentDate < DateTime.Now.Date)
                throw new Exception("Appointment date cannot be in the past");

            appointment.Status = "Pending";
            return _appointmentRepo.AddAppointment(appointment);
        }

        public List<Appointment> GetAllAppointments()
        {
            return _appointmentRepo.GetAllAppointments();
        }

        public List<Appointment> GetDoctorSchedule(int doctorId)
        {
            return _appointmentRepo.GetAppointmentsByDoctor(doctorId);
        }

        public Appointment GetAppointmentById(int appointmentId)
        {
            return _appointmentRepo.GetAppointmentById(appointmentId);
        }

        public bool UpdateAppointment(Appointment appointment)
        {
            return _appointmentRepo.UpdateAppointment(appointment);
        }

        public bool CompleteAppointment(int appointmentId)
        {
            Appointment appointment = _appointmentRepo.GetAppointmentById(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "Completed";
            return _appointmentRepo.UpdateAppointment(appointment);
        }

        public bool CancelAppointment(int appointmentId)
        {
            Appointment appointment = _appointmentRepo.GetAppointmentById(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "Cancelled";
            return _appointmentRepo.UpdateAppointment(appointment);
        }

        public List<Appointment> SearchAppointments(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllAppointments();

            return _appointmentRepo.SearchAppointments(searchTerm);
        }
    }

    /// <summary>
    /// MedicineService - Business Logic for Medicine Management
    /// </summary>
    public class MedicineService
    {
        private MedicineRepository _medicineRepo;

        public MedicineService()
        {
            _medicineRepo = new MedicineRepository();
        }

        public bool AddMedicine(Medicine medicine)
        {
            if (string.IsNullOrWhiteSpace(medicine.Name))
                throw new Exception("Medicine name is required");
            if (medicine.QuantityInStock < 0)
                throw new Exception("Quantity cannot be negative");
            if (medicine.UnitPrice < 0)
                throw new Exception("Price cannot be negative");

            return _medicineRepo.AddMedicine(medicine);
        }

        public List<Medicine> GetAllMedicines()
        {
            return _medicineRepo.GetAllMedicines();
        }

        public Medicine GetMedicineById(int medicineId)
        {
            return _medicineRepo.GetMedicineById(medicineId);
        }

        public bool UpdateMedicine(Medicine medicine)
        {
            if (string.IsNullOrWhiteSpace(medicine.Name))
                throw new Exception("Medicine name is required");

            return _medicineRepo.UpdateMedicine(medicine);
        }

        public bool DeleteMedicine(int medicineId)
        {
            return _medicineRepo.DeleteMedicine(medicineId);
        }

        public List<Medicine> SearchMedicines(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllMedicines();

            return _medicineRepo.SearchMedicines(searchTerm);
        }

        public bool UpdateStock(int medicineId, int quantity)
        {
            Medicine medicine = _medicineRepo.GetMedicineById(medicineId);
            if (medicine == null)
                throw new Exception("Medicine not found");

            medicine.QuantityInStock = quantity;
            return _medicineRepo.UpdateMedicine(medicine);
        }
    }

    /// <summary>
    /// PrescriptionService - Business Logic for Prescription Management
    /// </summary>
    public class PrescriptionService
    {
        private PrescriptionRepository _prescriptionRepo;

        public PrescriptionService()
        {
            _prescriptionRepo = new PrescriptionRepository();
        }

        public bool WritePrescription(Prescription prescription)
        {
            if (prescription.AppointmentId <= 0)
                throw new Exception("Invalid appointment");
            if (string.IsNullOrWhiteSpace(prescription.Diagnosis))
                throw new Exception("Diagnosis is required");

            return _prescriptionRepo.AddPrescription(prescription);
        }

        public Prescription GetPrescriptionByAppointmentId(int appointmentId)
        {
            return _prescriptionRepo.GetPrescriptionByAppointmentId(appointmentId);
        }

        public List<PrescriptionMedicine> GetPrescriptionMedicines(int prescriptionId)
        {
            return _prescriptionRepo.GetPrescriptionMedicines(prescriptionId);
        }

        public bool AddPrescriptionMedicine(PrescriptionMedicine presMed)
        {
            if (presMed.PrescriptionId <= 0)
                throw new Exception("Invalid prescription");
            if (presMed.MedicineId <= 0)
                throw new Exception("Invalid medicine");
            if (string.IsNullOrWhiteSpace(presMed.Dosage))
                throw new Exception("Dosage is required");
            if (string.IsNullOrWhiteSpace(presMed.Frequency))
                throw new Exception("Frequency is required");

            return _prescriptionRepo.AddPrescriptionMedicine(presMed);
        }
    }

    /// <summary>
    /// BillService - Business Logic for Billing
    /// </summary>
    public class BillService
    {
        private BillRepository _billRepo;

        public BillService()
        {
            _billRepo = new BillRepository();
        }

        public bool GenerateBill(Bill bill)
        {
            if (bill.AppointmentId <= 0)
                throw new Exception("Invalid appointment");
            if (bill.TotalAmount < 0)
                throw new Exception("Amount cannot be negative");

            return _billRepo.AddBill(bill);
        }

        public List<Bill> GetAllBills()
        {
            return _billRepo.GetAllBills();
        }

        public Bill GetBillByAppointmentId(int appointmentId)
        {
            return _billRepo.GetBillByAppointmentId(appointmentId);
        }

        public bool MarkAsPayment(int billId, decimal paidAmount)
        {
            return _billRepo.UpdateBillPaymentStatus(billId, "Paid", DateTime.Now);
        }

        public bool MarkAsUnpaid(int billId)
        {
            return _billRepo.UpdateBillPaymentStatus(billId, "Unpaid", null);
        }
    }

    /// <summary>
    /// AuthenticationService - Business Logic for Login/Authentication
    /// </summary>
    public class AuthenticationService
    {
        private LoginRepository _loginRepo;

        public AuthenticationService()
        {
            _loginRepo = new LoginRepository();
        }

        public bool AuthenticateUser(int userId, string password, string role)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
                throw new Exception("Invalid login credentials");

            return _loginRepo.ValidateLogin(userId, password, role);
        }

        public string GetUserRole(int userId)
        {
            return _loginRepo.GetRoleByUserId(userId);
        }

        // ⬇️ Add this new method here ⬇️
        /// <summary>
        /// Authenticate user and return role if credentials are valid
        /// </summary>
        /// <returns>Role string ("Admin", "Doctor", "Receptionist") or null if invalid</returns>
        public string AuthenticateAndGetRole(int userId, string password)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(password))
                throw new Exception("Invalid login credentials");

            // This requires a new method in LoginRepository: ValidateLoginAndGetRole
            return _loginRepo.ValidateLoginAndGetRole(userId, password);
        }
    }
}
