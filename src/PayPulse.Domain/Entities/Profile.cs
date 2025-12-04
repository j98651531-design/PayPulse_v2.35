using System;

namespace PayPulse.Domain.Entities
{
    public class Profile
    {
        public string ProfileId { get; set; }
        public string Name { get; set; }
        public string ProviderType { get; set; } // STB/GMT/WIC later
        public string ManualToken { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSyncUtc { get; set; }

        public string PosUserId { get; set; }
        public string PosCashboxId { get; set; }

        public string LoginEmail { get; set; }
        public string LoginPhoneNumber { get; set; }
        public string LoginPassword { get; set; }
        public string TotpSecret { get; set; }
    }
}
