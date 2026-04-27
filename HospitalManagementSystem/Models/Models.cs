using System;
using System.Collections.Generic;

namespace HospitalManagementSystem.Models
{
    // ========== BASE CLASSES ==========

    /// <summary>
    /// Abstract base class for all users (Admin, Doctor, Receptionist)
    /// Encapsulation: All properties are private with public getters/setters
    /// </summary>
    public abstract class BaseUser
    {
        private int _userId;
        private string _name;
        private string _email;
        private string _phone;

        public int UserId { get { return _userId; } set { _userId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Phone { get { return _phone; } set { _phone = value; } }

        public abstract void DisplayDashboard();
    }

    // ========== ADMIN CLASS ==========
    public class Admin : BaseUser
    {
        public int AdminId { get; set; }

        public override void DisplayDashboard()
        {
            // Admin dashboard logic
        }
    }

    // ========== DOCTOR CLASS ==========
    public class Doctor : BaseUser
    {
        private string _specialization;
        private string _department;

        public int DoctorId { get; set; }
        public string Specialization { get { return _specialization; } set { _specialization = value; } }
        public string Department { get { return _department; } set { _department = value; } }

        public override void DisplayDashboard()
        {
            // Doctor dashboard logic
        }
    }

    // ========== RECEPTIONIST CLASS ==========
    public class Receptionist : BaseUser
    {
        public int ReceptionistId { get; set; }

        public override void DisplayDashboard()
        {
            // Receptionist dashboard logic
        }
    }

    // ========== PATIENT CLASS ==========
    public class Patient
    {
        private int _patientId;
        private string _name;
        private DateTime _dateOfBirth;
        private char _gender;
        private string _bloodGroup;
        private string _address;
        private string _phone;
        private string _email;
        private DateTime _registrationDate;

        public int PatientId { get { return _patientId; } set { _patientId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public DateTime DateOfBirth { get { return _dateOfBirth; } set { _dateOfBirth = value; } }
        public char Gender { get { return _gender; } set { _gender = value; } }
        public string BloodGroup { get { return _bloodGroup; } set { _bloodGroup = value; } }
        public string Address { get { return _address; } set { _address = value; } }
        public string Phone { get { return _phone; } set { _phone = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public DateTime RegistrationDate { get { return _registrationDate; } set { _registrationDate = value; } }

        public override string ToString()
        {
            return $"ID: {_patientId}, Name: {_name}, Phone: {_phone}";
        }
    }

    // ========== APPOINTMENT CLASS (ADDED) ==========
    public class Appointment
    {
        private int _appointmentId;
        private int _patientId;
        private int _doctorId;
        private int? _receptionistId;
        private DateTime _appointmentDate;
        private TimeSpan _appointmentTime;
        private string _status;

        public int AppointmentId { get { return _appointmentId; } set { _appointmentId = value; } }
        public int PatientId { get { return _patientId; } set { _patientId = value; } }
        public int DoctorId { get { return _doctorId; } set { _doctorId = value; } }
        public int? ReceptionistId { get { return _receptionistId; } set { _receptionistId = value; } }
        public DateTime AppointmentDate { get { return _appointmentDate; } set { _appointmentDate = value; } }
        public TimeSpan AppointmentTime { get { return _appointmentTime; } set { _appointmentTime = value; } }
        public string Status { get { return _status; } set { _status = value; } }

        // Computed property for display in combo boxes
        public string AppointmentDateAndTime => $"{AppointmentDate:dd-MMM-yyyy} {AppointmentTime:hh\\:mm}";

        // Additional properties from joined queries
        public string PatientName { get; set; }
        public string DoctorName { get; set; }

        public override string ToString()
        {
            return $"Appointment ID: {_appointmentId}, Date: {_appointmentDate}, Time: {_appointmentTime}, Status: {_status}";
        }
    }

    // ========== PRESCRIPTION CLASS ==========
    public class Prescription
    {
        private int _prescriptionId;
        private int _appointmentId;
        private string _diagnosis;
        private string _notes;
        private DateTime _prescriptionDate;

        public int PrescriptionId { get { return _prescriptionId; } set { _prescriptionId = value; } }
        public int AppointmentId { get { return _appointmentId; } set { _appointmentId = value; } }
        public string Diagnosis { get { return _diagnosis; } set { _diagnosis = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
        public DateTime PrescriptionDate { get { return _prescriptionDate; } set { _prescriptionDate = value; } }

        // List of medicines in this prescription
        public List<PrescriptionMedicine> PrescribedMedicines { get; set; }

        public override string ToString()
        {
            return $"Prescription ID: {_prescriptionId}, Diagnosis: {_diagnosis}";
        }
    }

    // ========== PRESCRIPTION MEDICINE CLASS ==========
    public class PrescriptionMedicine
    {
        private int _presMedId;
        private int _prescriptionId;
        private int _medicineId;
        private string _dosage;
        private string _frequency;
        private string _duration;

        public int PresMedId { get { return _presMedId; } set { _presMedId = value; } }
        public int PrescriptionId { get { return _prescriptionId; } set { _prescriptionId = value; } }
        public int MedicineId { get { return _medicineId; } set { _medicineId = value; } }
        public string Dosage { get { return _dosage; } set { _dosage = value; } }
        public string Frequency { get { return _frequency; } set { _frequency = value; } }
        public string Duration { get { return _duration; } set { _duration = value; } }

        public string MedicineName { get; set; }
    }

    // ========== MEDICINE CLASS ==========
    public class Medicine
    {
        private int _medicineId;
        private string _name;
        private string _type;
        private int _quantityInStock;
        private decimal _unitPrice;

        public int MedicineId { get { return _medicineId; } set { _medicineId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Type { get { return _type; } set { _type = value; } }
        public int QuantityInStock { get { return _quantityInStock; } set { _quantityInStock = value; } }
        public decimal UnitPrice { get { return _unitPrice; } set { _unitPrice = value; } }

        public override string ToString()
        {
            return $"ID: {_medicineId}, Name: {_name}, Type: {_type}, Stock: {_quantityInStock}";
        }
    }

    // ========== BILL CLASS ==========
    public class Bill
    {
        private int _billId;
        private int _appointmentId;
        private decimal _totalAmount;
        private string _paymentStatus;
        private DateTime? _paymentDate;

        public int BillId { get { return _billId; } set { _billId = value; } }
        public int AppointmentId { get { return _appointmentId; } set { _appointmentId = value; } }
        public decimal TotalAmount { get { return _totalAmount; } set { _totalAmount = value; } }
        public string PaymentStatus { get { return _paymentStatus; } set { _paymentStatus = value; } }
        public DateTime? PaymentDate { get { return _paymentDate; } set { _paymentDate = value; } }

        public string PatientName { get; set; }
        public int PatientId { get; set; }

        public override string ToString()
        {
            return $"Bill ID: {_billId}, Amount: {_totalAmount:C}, Status: {_paymentStatus}";
        }
    }

    // ========== LOGIN CLASS ==========
    public class LoginRecord
    {
        private int _loginId;
        private int _userId;
        private string _role;
        private string _passwordHash;

        public int LoginId { get { return _loginId; } set { _loginId = value; } }
        public int UserId { get { return _userId; } set { _userId = value; } }
        public string Role { get { return _role; } set { _role = value; } }
        public string PasswordHash { get { return _passwordHash; } set { _passwordHash = value; } }
    }
}