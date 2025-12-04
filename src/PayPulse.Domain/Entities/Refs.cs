namespace PayPulse.Domain.Entities
{
    public class CurrencyRef
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Display { get { return Code + " - " + Name; } }
    }

    public class UserRef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Display { get { return Name; } }
    }

    public class CashboxRef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Display { get { return Name; } }
    }

    public class IdTypeRef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Display { get { return Name; } }
    }

    public class UserTypeRef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Display { get { return Name; } }
    }
}
