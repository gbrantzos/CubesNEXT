using System.Collections.Generic;
using System.Linq;

namespace Cubes.Core.Security
{
    public class AuthenticateUserResponse
    {
        public string UserName { get; }
        public string DisplayName { get; }
        public string Email { get; set; }
        public string Token { get; }
        public List<string> Roles { get; set; }

        public AuthenticateUserResponse(User user, string token)
        {
            UserName    = user.UserName;
            DisplayName = user.DisplayName;
            Email       = user.Email;
            Token       = token;
            Roles       = user.Roles.ToList();
        }
    }
}