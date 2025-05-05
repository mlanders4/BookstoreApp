using BookstoreApp.Login.Contracts;
using BookstoreApp.Login.Engine;
using BookstoreApp.Login.Accessor;

namespace BookstoreApp.Login.Manager
{
    public class UserManager : IUserManager
    {
        private readonly UserEngine _engine;
        private readonly UserAccessor _accessor;

        public UserManager(UserEngine engine, UserAccessor accessor)
        {
            _engine = engine;
            _accessor = accessor;
        }

        public bool SignUp(UserDto dto)
        {
            if (_accessor.UserExists(dto.Email))
                return false;

            var hashedPassword = _engine.HashPassword(dto.Password);
            _accessor.CreateUser(
                dto.Email,
                hashedPassword,
                dto.Login,
                dto.FirstName,
                dto.LastName,
                dto.PhoneN
            );

            return true;
        }

        public bool Login(UserDto dto)
        {
            var user = _accessor.GetUserByEmail(dto.Email);
            if (user == null)
                return false;

            return _engine.VerifyPassword(dto.Password, user.HashedPassword);
        }
    }
}
