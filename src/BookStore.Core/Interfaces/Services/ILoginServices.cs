using BookStore.Core.Models;
using System.Threading.Tasks;

namespace BookStore.Core.Interfaces.Services
{
    public interface ILoginServices
    {
        void Login(string token);
        Task LogOut();
        Task<UserDetail> GetUserDetails();
    }
}
