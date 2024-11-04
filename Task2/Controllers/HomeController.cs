using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task2.Models;

namespace Task2.Controllers
{
    public class HomeController(Task2DbContext dbContext) : Controller
    {
        private readonly Task2DbContext? dbContext = dbContext;

        public IActionResult Index(int id)
        {
            ViewBag.Files = dbContext?.Files.Select(file => new 
            {
                file.Id,
                file.Name,
                Bank = file.Bank.Name
            }).ToArray();

            if (Request.Cookies["uploadStatus"] != null)
            {
                ViewBag.UploadStatus = Request.Cookies["uploadStatus"] == "1";
                Response.Cookies.Delete("uploadStatus");
            }

            dbContext?.Database.CloseConnection();
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult File(int id)
        {
            return View();
        }
    }
}
