using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.Page.Data;
using BulkyBook.Page.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyBook.Page.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext context;

        [BindProperty]
        public Category Category { get; set; }

        public DeleteModel(ApplicationDbContext context)
        {
            this.context = context;
        }
        public void OnGet(int id)
        {
            Category = context.Category.Find(id);
        }

        public async Task<IActionResult> OnPost()
        {
            var fromdb = context.Category.Find(Category.Id);
            if(fromdb != null)
            {
                context.Category.Remove(fromdb);
                await context.SaveChangesAsync();
                TempData["success"] = $"{fromdb.Name} is successfully Deleted.";
                return RedirectToPage("index");
            }
            return Page();
        }
    }
}
