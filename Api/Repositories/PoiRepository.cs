using Dapper;
using Microsoft.Data.SqlClient;
using Api.Models;

namespace Api.Repositories
{
    public class PoiRepository
    {
        private string _connectionString;

        public PoiRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLServer");
        }
        
        // async để tránh block thread
        public async Task<IEnumerable<Poi>> GetAllPoisAsync()
        {
            var conn = new SqlConnection(this._connectionString);
            try
            {
                var sql = "SELECT * FROM pois";
                var res = await conn.QueryAsync<Poi>(sql); // Có Task thì phải có await khi gán.
                return res;
            }
            finally
            {
                conn.Dispose();
            }

        }
    }
    
}