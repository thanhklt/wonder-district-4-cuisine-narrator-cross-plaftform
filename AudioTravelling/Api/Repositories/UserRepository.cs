using Dapper;
using Microsoft.Data.SqlClient;
using Api.Models;

namespace Api.Repositories
{
    public class UserRepository
    {
        private string _connectionString;

        public UserRepository(IConfiguration configuration) {
            this._connectionString = configuration.GetConnectionString("SQLServer");
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var conn = new SqlConnection(this._connectionString);
            try
            {
                var sql = "SELECT * FROM users";
                var res = await conn.QueryAsync<User>(sql); // Dapper tự map đối tượng từ table
                return res;
            }
            finally
            {
                conn.Dispose();
            }
        }

        public async Task<int> CreateUser(User user)
        {
            var conn = new SqlConnection(this._connectionString);
            try
            {
                var sql = "INSERT INTO users VALUES (@Email, @PasswordHash, @PhoneNumber, @FullName, @RoleID, @UserStatus, @CreatedDate); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                var id = await conn.QuerySingleAsync<int>(sql, user);
                return id;
            }
            finally
            {
                conn.Dispose();
            }
        }
    }
}