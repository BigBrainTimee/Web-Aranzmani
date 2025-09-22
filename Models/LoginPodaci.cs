using System.ComponentModel.DataAnnotations;

namespace WebAranzmani.Models
{
    public class LoginPodaci
    {
        [Required]
        public string KorisnickoIme { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Lozinka { get; set; } = string.Empty;
    }
}
