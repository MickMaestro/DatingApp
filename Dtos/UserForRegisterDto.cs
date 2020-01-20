using System.ComponentModel.DataAnnotations;
namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(9,MinimumLength=5, ErrorMessage="Password should be within a 5-9 range")]
        public string Password { get; set; }
    }
}