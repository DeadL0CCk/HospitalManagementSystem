using System;
using HospitalManagementSystem.DAL;

namespace HospitalManagementSystem.Utils
{
    /// <summary>
    /// Helper class to migrate plain-text passwords to secure PBKDF2 hashes
    /// </summary>
    public class PasswordMigrationHelper
    {
        private LoginRepository _loginRepo;

        public PasswordMigrationHelper()
        {
            _loginRepo = new LoginRepository();
        }

        /// <summary>
        /// Migrates a user's password from plain text to secure hash
        /// Call this after successful login to upgrade the stored password
        /// </summary>
        public bool MigratePasswordOnLogin(int userId, string role, string plainTextPassword)
        {
            try
            {
                return _loginRepo.UpdatePasswordHash(userId, role, plainTextPassword);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the login
                System.Diagnostics.Debug.WriteLine($"Password migration failed for user {userId}: {ex.Message}");
                return false;
            }
        }
    }
}