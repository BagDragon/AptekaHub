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
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required, StringLength(100)]
        public string PatientName { get; set; }

        [Required, StringLength(20)]
        public string Number { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        [StringLength(100)]
        public string Doctor { get; set; }

        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }

        public virtual Product Product { get; set; }
    }

}
