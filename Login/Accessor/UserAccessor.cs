using Microsoft.EntityFrameworkCore;
using BookstoreApp.Login.Accessor;

namespace BookstoreApp.Login.Accessor
{
    public class UserAccessor
    {
        private readonly AuthDbContext _database;

        public UserAccessor(AuthDbContext database)
        {
            _database = database;
        }

        public bool UserExists(string email)
        {
            return _database.Users.Any(u => u.Email == email);
        }

        public void CreateUser(string email, string hashedPassword, string login, string firstName, string lastName, string phoneN)
        {
            var user = new Users
            {
                Email = email,
                HashedPassword = hashedPassword,
                Login = login,
                FirstName = firstName,
                LastName = lastName,
                PhoneN = phoneN
            };

            _database.Users.Add(user);
            _database.SaveChanges();
        }

        public Users? GetUserByEmail(string email)
        {
            return _database.Users.FirstOrDefault(u => u.Email == email);
        }
    }
}


