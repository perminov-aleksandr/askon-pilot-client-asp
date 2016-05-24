using System.ComponentModel.DataAnnotations;

namespace Ascon.Pilot.WebClient.ViewModels
{
    public class LogInViewModel
    {
        [Required]
        [Display(Name = "Название базы данных")]
        public string DatabaseName { get; set; }

        [Required]
        [Display(Name = "Имя пользователя")]
        public string Login { get; set; }

        [Required]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
