using System;

namespace PayPulse.Domain.Entities
{
    public enum AppUserRole
    {
        Admin,
        Manager,
        User
    }

    /// <summary>
    /// Application user used for logging into PayPulse.
    /// Password is stored as a hashed value (BCrypt) in the database.
    /// CreatedByUserId / UpdatedByUserId link to other AppUsers (or null for system).
    /// </summary>
    public class AppUser
    {
        public string UserId { get; set; }        // GUID string
        public string UserName { get; set; }      // login name
        public string DisplayName { get; set; }
        public string Password { get; set; }      // hashed password (BCrypt)
        public AppUserRole Role { get; set; }
        public bool IsActive { get; set; }

        // Audit fields (UTC)
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Who created / last updated this user
        public string CreatedByUserId { get; set; }
        public string UpdatedByUserId { get; set; }

        // If true, user must change password on next successful login
        public bool MustChangePassword { get; set; }
    }
}
