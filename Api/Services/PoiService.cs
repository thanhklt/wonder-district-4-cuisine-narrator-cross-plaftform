using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public class PoiService
    {
        private PoiRepository _repo;

        public PoiService (PoiRepository repo)
        {
            this._repo = repo;
        }
         
        public async Task<IEnumerable<Poi>> GetAllAsync()
        {
            var res = await _repo.GetAllPoisAsync();
            return res;
        }
    }
}