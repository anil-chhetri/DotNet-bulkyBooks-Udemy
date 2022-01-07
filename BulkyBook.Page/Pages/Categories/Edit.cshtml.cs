using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.Page.Data;
using BulkyBook.Page.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Page.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext context;

        [BindProperty]
        public Category Category { get; set; }

        public EditModel(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> OnGet(int Id)
        {
            if(Id == 0)
            {
                return NotFound();
            }
            Category = await context.Category.FindAsync(Id);

            if(Category == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if(Category.Name == Category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("category.Name", "Display order and Name cannot same.");
            }

            if(ModelState.IsValid)
            {
                context.Entry<Category>(Category).State = EntityState.Modified;
                await context.SaveChangesAsync();
                TempData["success"] = $"{Category.Name} is successfully Modified.";
                return RedirectToPage("Index");
            }

            return Page();
        } 
    }
}
