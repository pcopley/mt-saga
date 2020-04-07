using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CartTrackingService
{
    // What kind of object-oriented hellscape is this?
    public static class SagaDbContextFactoryProvider
    {
        private static readonly Lazy<string> _connectionString = new Lazy<string>(GetLocalDbConnectionString);

        private static readonly string[] _connectionStrings = new[]
                {
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=MTSaga;"
        };

        public static string ConnectionString => _connectionString.Value;

        public static string GetLocalDbConnectionString()
        {
            foreach (var connectionString in _connectionStrings)
            {
                try
                {
                    using var connection = new SqlConnection(connectionString);

                    return connectionString;
                }
                catch (Exception)
                {
                    // no op
                }
            }

            throw new InvalidOperationException("No valid connection strings provided");
        }
    }
}