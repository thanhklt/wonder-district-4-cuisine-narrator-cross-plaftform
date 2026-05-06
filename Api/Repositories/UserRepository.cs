using Dapper;
using Microsoft.Data.SqlClient;
using Api.Models;

namespace Api.Repositories
{
    public class UserRepository
    {
        private string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLServer");
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            await using var conn = new SqlConnection(_connectionString);

            var sql = "SELECT * FROM Users";

            var res = await conn.QueryAsync<User>(sql);

            return res;
        }

        // Lấy user theo id
        public async Task<User?> GetUserByIdAsync(int id)
        {
            await using var conn = new SqlConnection(_connectionString);

            var sql = @"
                SELECT *
                FROM Users
                WHERE UserID = @Id;
            ";

            var res = await conn.QuerySingleOrDefaultAsync<User>(sql, new
            {
                Id = id
            });

            return res;
        }

        // Kiểm tra email đã tồn tại chưa
        public async Task<bool> IsEmailExists(string email)
        {
            await using var conn = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(1)
                FROM Users
                WHERE Email = @Email;
            ";

            var res = await conn.ExecuteScalarAsync<int>(sql, new
            {
                Email = email
            });

            return res > 0;
        }

        // Tạo user
        public async Task<int> CreateUser(User user)
        {
            if (await IsEmailExists(user.Email))
            {
                throw new Exception("Email đã tồn tại");
            }

            await using var conn = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO Users
                (
                    Email,
                    PasswordHash,
                    PhoneNumber,
                    FullName,
                    RoleID,
                    UserStatus,
                    CreatedDate
                )
                VALUES
                (
                    @Email,
                    @PasswordHash,
                    @PhoneNumber,
                    @FullName,
                    @RoleID,
                    @UserStatus,
                    @CreatedDate
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            var id = await conn.QuerySingleAsync<int>(sql, user);

            return id;
        }

        public async Task<User?> LoginUser(User user)
        {
            await using var conn = new SqlConnection(_connectionString);

            var sql = @"
                SELECT *
                FROM Users
                WHERE Email = @Email
                  AND PasswordHash = @PasswordHash;
            ";

            var result = await conn.QuerySingleOrDefaultAsync<User>(sql, new
            {
                Email = user.Email,
                PasswordHash = user.PasswordHash
            });

            return result;
        }
    }
}