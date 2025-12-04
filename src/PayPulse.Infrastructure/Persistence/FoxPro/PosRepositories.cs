using System;
using System.Collections.Generic;
using System.Data.OleDb;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.Infrastructure.Persistence.FoxPro
{
    public class PosCustomerRepository : IPosCustomerRepository
    {
        private readonly FoxProConnectionFactory _factory;

        public PosCustomerRepository(FoxProConnectionFactory factory)
        {
            _factory = factory;
        }

        public PosCustomer FindByIdNumber(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber)) return null;
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT TOP 1 Id, FirstName, LastName, Phone, IdNumber FROM Customers WHERE IdNumber = @IdNumber";
                cmd.Parameters.AddWithValue("@IdNumber", idNumber);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return ReadCustomer(rd);
                }
            }
        }

        public PosCustomer FindByPhone(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT TOP 1 Id, FirstName, LastName, Phone, IdNumber FROM Customers WHERE Phone = @Phone";
                cmd.Parameters.AddWithValue("@Phone", phoneNumber);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return ReadCustomer(rd);
                }
            }
        }

        public PosCustomer GetById(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId)) return null;
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, FirstName, LastName, Phone, IdNumber FROM Customers WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", customerId);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return ReadCustomer(rd);
                }
            }
        }

        private PosCustomer ReadCustomer(OleDbDataReader rd)
        {
            var c = new PosCustomer();
            c.CustomerId = rd["Id"].ToString();
            c.FirstName = rd["FirstName"] as string;
            c.LastName = rd["LastName"] as string;
            c.PhoneNumber = rd["Phone"] as string;
            c.IdNumber = rd["IdNumber"] as string;
            return c;
        }

        public string Insert(PosCustomer c)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Customers (FirstName, LastName, Phone, IdNumber)
VALUES (@FirstName, @LastName, @Phone, @IdNumber);";
                cmd.Parameters.AddWithValue("@FirstName", (object)c.FirstName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastName", (object)c.LastName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)c.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdNumber", (object)c.IdNumber ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT @@IDENTITY";
                object id = cmd.ExecuteScalar();
                return Convert.ToString(id);
            }
        }
    }

    public class PosOperationRepository : IPosOperationRepository
    {
        private readonly FoxProConnectionFactory _factory;

        public PosOperationRepository(FoxProConnectionFactory factory)
        {
            _factory = factory;
        }

        public void InsertOperation(Transfer t, PosCustomer c, string currencyId, string userId, string cashboxId)
        {
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO Operations (CustomerId, Amount, CurrencyId, UserId, CashboxId, ReservationCode, CreatedAt)
VALUES (@CustomerId, @Amount, @CurrencyId, @UserId, @CashboxId, @ReservationCode, @CreatedAt);";
                cmd.Parameters.AddWithValue("@CustomerId", c.CustomerId);
                cmd.Parameters.AddWithValue("@Amount", t.PosAmount);
                cmd.Parameters.AddWithValue("@CurrencyId", currencyId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CashboxId", cashboxId);
                cmd.Parameters.AddWithValue("@ReservationCode", (object)t.ReservationCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class PosMetadataRepository : IPosMetadataRepository
    {
        private readonly FoxProConnectionFactory _factory;

        public PosMetadataRepository(FoxProConnectionFactory factory)
        {
            _factory = factory;
        }

        public IList<CurrencyRef> GetCurrencies()
        {
            var list = new List<CurrencyRef>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Code, Name FROM Currencies ORDER BY Code";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var c = new CurrencyRef();
                        c.Id = rd["Id"].ToString();
                        c.Code = rd["Code"] as string;
                        c.Name = rd["Name"] as string;
                        list.Add(c);
                    }
                }
            }
            return list;
        }

        public IList<UserRef> GetUsers()
        {
            var list = new List<UserRef>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name FROM Users ORDER BY Name";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var u = new UserRef();
                        u.Id = rd["Id"].ToString();
                        u.Name = rd["Name"] as string;
                        list.Add(u);
                    }
                }
            }
            return list;
        }

        public IList<CashboxRef> GetCashboxes()
        {
            var list = new List<CashboxRef>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name FROM Cashboxes ORDER BY Name";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var c = new CashboxRef();
                        c.Id = rd["Id"].ToString();
                        c.Name = rd["Name"] as string;
                        list.Add(c);
                    }
                }
            }
            return list;
        }

        public IList<IdTypeRef> GetIdTypes()
        {
            var list = new List<IdTypeRef>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name FROM IdTypes ORDER BY Name";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var t = new IdTypeRef();
                        t.Id = rd["Id"].ToString();
                        t.Name = rd["Name"] as string;
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        public IList<UserTypeRef> GetUserTypes()
        {
            var list = new List<UserTypeRef>();
            using (var conn = _factory.Create())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name FROM UserTypes ORDER BY Name";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var t = new UserTypeRef();
                        t.Id = rd["Id"].ToString();
                        t.Name = rd["Name"] as string;
                        list.Add(t);
                    }
                }
            }
            return list;
        }
    }
}
