﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.Models
{
    public class ShoppingCart
    {
        [Key]
        [Required]      
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ValidateNever]
        public Product Product { get; set; }

        [Range(1,1000, ErrorMessage = "should be in between 1 to 1000")]
        public int Count { get; set; }

        public string ApplicationUserId { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
