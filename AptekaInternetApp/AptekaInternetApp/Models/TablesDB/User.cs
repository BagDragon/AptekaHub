using AptekaInternetApp.Models.TablesDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.Tables
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 5, ErrorMessage = "ФИО должно быть от 5 до 100 символов")]
        public string FIO { get; set; }

        [Required(ErrorMessage = "Логин обязателен для заполнения")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Логин должен быть от 4 до 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Логин может содержать только буквы, цифры и подчеркивание")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть от 8 до 100 символов")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
       ErrorMessage = "Пароль должен содержать минимум 1 заглавную букву, 1 строчную, 1 цифру и 1 спецсимвол")]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        public string StatusEmail {  get; set; }

        public virtual ICollection<Sale> SalesAsCashier { get; set; }

    }
}
