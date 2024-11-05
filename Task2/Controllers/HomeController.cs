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
            var file = dbContext?.Files.FirstOrDefault(file => file.Id == id);
            if (file == null)
            {
                return NotFound("404 - Not found");
            }

            ViewBag.File = file;
            ViewBag.BankName = dbContext?.Banks.FirstOrDefault(bank => bank.Id == file.BankId)!.Name;
            ViewBag.GroupedBills = dbContext?.Bills.Where(bill => bill.FileId == file.Id).GroupBy(bill => bill.BillType);

            List<decimal> sumResult = [];
            sumResult.Add(dbContext!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.InsaldoActive));
            sumResult.Add(dbContext!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.InsaldoPassive));
            sumResult.Add(dbContext!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.TurnoversDebit));
            sumResult.Add(dbContext!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.TurnoversCredit));
            sumResult.Add(dbContext!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.OutsaldoActive));
            sumResult.Add(dbContext!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.OutsaldoPassive));
            ViewBag.Sums = sumResult;

            return View();
        }
    }
}
