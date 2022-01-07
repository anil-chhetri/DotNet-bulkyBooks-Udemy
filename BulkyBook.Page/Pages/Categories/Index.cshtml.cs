using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.Page.Data;
using BulkyBook.Page;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyBook.Page.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext context;

        public IEnumerable<Model.Category> categories { get; set; }
        
        public IndexModel(ApplicationDbContext context)
        {
            this.context = context;
        }
        public void OnGet()
        {
            categories = context.Category.ToList();
            
        }
    }
}
