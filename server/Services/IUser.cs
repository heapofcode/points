using System.Collections.Generic;
using System.Threading.Tasks;

namespace app.Services.User
{
    public interface IUser
    {
        Task<bool> IsValidUserCredentials(string userName, string password);
        Task<List<string>> GetUserRole(string userName);
        Task<string> GetUserFullName(string userName);
        Task<string> GetUserAvatar(string userName);
    }
}
