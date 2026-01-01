using AptekaInternetApp.Models.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.TablesDB
{
    public class Manufacturer
    {
        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название производителя должно быть от 2 до 100 символов")]
        public string NameManufacture { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
