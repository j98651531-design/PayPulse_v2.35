using PayPulse.Domain.Entities;

namespace PayPulse.Core.Services
{
    /// <summary>
    /// Holds the currently logged-in user and exposes convenience helpers
    /// for role and permission checks. This is the single place that defines
    /// what each role (Admin / Manager / User) is allowed to do.
    /// </summary>
    public class CurrentUserContext
    {
        public AppUser CurrentUser { get; private set; }

        public bool IsAuthenticated => CurrentUser != null;

        public bool IsAdmin => CurrentUser?.Role == AppUserRole.Admin;
        public bool IsManager => CurrentUser?.Role == AppUserRole.Manager;
        public bool IsUser => CurrentUser?.Role == AppUserRole.User;

        // High-level permission helpers
        public bool CanViewTransfers => IsAuthenticated;
        public bool CanFetchFromProviders => IsAuthenticated;
        public bool CanNormalizeTransfers => IsAuthenticated;

        public bool CanAddToPos => IsAdmin || IsManager;

        public bool CanEditSettings => IsAdmin;
        public bool CanManageProfiles => IsAdmin || IsManager;
        public bool CanManageProviders => IsAdmin;
        public bool CanManageUsers => IsAdmin;

        // Security report (user audit) – by default Admin-only, can be relaxed later
        public bool CanViewSecurityReport => IsAdmin;

        public bool CanViewLogs => IsAdmin || IsManager;
        public bool CanViewReports => IsAuthenticated;

        public bool CanControlBackgroundWorker => IsAdmin || IsManager;

        public void Set(AppUser user)
        {
            CurrentUser = user;
        }
    }
}
