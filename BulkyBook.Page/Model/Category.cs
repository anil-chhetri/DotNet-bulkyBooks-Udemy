using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Page.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Display Name")]
        public string Name { get; set; }

        [Display(Name ="Display Order")]
        public int DisplayOrder { get; set; }

    }
}
