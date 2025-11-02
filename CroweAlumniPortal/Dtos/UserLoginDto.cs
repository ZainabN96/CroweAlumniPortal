using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Dtos
{
    public class UserLoginDto
    {
        [Required]
        public string LoginId { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        
    }
}
