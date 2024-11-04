using Microsoft.AspNetCore.Mvc;
using Task2.Models;

namespace Task2.Controllers
{
    public class HomeController(Task2DbContext dbContext) : Controller
    {
        private readonly Task2DbContext? dbContext = dbContext;

        public IActionResult Index()
        {
            var files = dbContext?.Files.Select(file => new
            {
                file.Id,
                file.Name,
                Bank = file.Bank.Name
            }).ToArray();

            ViewBag.Files = files;
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }
    }
}
