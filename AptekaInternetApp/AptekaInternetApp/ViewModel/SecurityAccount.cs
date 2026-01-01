using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.ViewModel
{
    public static class SecurityAccount
    {
        public static bool VerifyPasswordHash(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

    }
}
