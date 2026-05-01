using Api.Models;
using Api.Repositories;

namespace Api.Services
{
    public class UserService
    {
        private UserRepository _repo;
        public UserService(UserRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var res = await _repo.GetAllUsersAsync();
            return res;
        }
        public async Task<int> CreateUser(User user)
        {
            var res = await _repo.CreateUser(user);
            return res;
        }
    }
}