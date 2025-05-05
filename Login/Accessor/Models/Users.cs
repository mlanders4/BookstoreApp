using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookstoreApp.Login.Accessor
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("hashedpassword")]
        public string HashedPassword { get; set; }

        [Column("login")]
        public string Login { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("phone_n")]
        public string PhoneN { get; set; }
    }
}
