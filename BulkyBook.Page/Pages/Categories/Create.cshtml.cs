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
    [BindProperties] // if there are multiple property to bind.
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext context;

        //[BindProperty] --if 1 bind property is only used.
        public Category Category { get; set; }

        public CreateModel(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if(Category.Name == Category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Category.Name", "Name and display oder are same.");
                ModelState.AddModelError("Category.DisplayOrder", "Name and display oder are same.");
            }
            if (ModelState.IsValid)
            {
                await context.Category.AddAsync(Category);
                await context.SaveChangesAsync();
                TempData["success"] = $"{Category.Name} is successfully created.";
                return RedirectToPage("index");
            }

            return Page();
        }
    }
}
