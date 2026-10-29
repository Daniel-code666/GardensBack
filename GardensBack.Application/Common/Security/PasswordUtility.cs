using GardensBack.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GardensBack.Application.Common.Security
{
    public static class PasswordUtility
    {
        public static string Hash(User user, string password)
        {
            return new PasswordHasher<User>().HashPassword(user, password);
        }

        public static bool Verify(User user, string password)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
                return false;

            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
