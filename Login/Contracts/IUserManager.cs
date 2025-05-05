namespace BookstoreApp.Login.Contracts
{
    public interface IUserManager
    {
        bool SignUp(UserDto dto);
        bool Login(UserDto dto);
    }
}



