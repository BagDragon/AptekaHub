using AptekaInternetApp.Models.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptekaInternetApp.Models.TablesDB
{
    public class Subcategory
    {
        [Key]
        public int ID_Subcategory { get; set; }

        [Required(ErrorMessage = "Название продукта обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 50 символов")]
        public string Name { get; set; }
      
        public string ImageSubcategory { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
       
        public virtual Category Category { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
