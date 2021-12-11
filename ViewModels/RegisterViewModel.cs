using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sporting_Events.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Не указан Логин")]
        [MaxLength(30)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Не указано Имя")]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указано Отчество")]
        [MaxLength(30)]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Не указана Фамилия")]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Не указан Возраст")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Не указан номер телефона")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public int RoleId { get; set; }
        public string Role { get; set; }

    }
}