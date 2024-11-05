using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Excel;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Xml;
using Task2.Models;

namespace Task2.API
{
    [Route("api/files")]
    [ApiController]
    public class FileController(Task2DbContext dbContext, IWebHostEnvironment webHost) : ControllerBase
    {
        private readonly Task2DbContext? _db = dbContext;
        private readonly IWebHostEnvironment? _hostEnvironment = webHost;

        [HttpGet("get/{id}")]
        public async Task<ActionResult<Models.File>> Get(int id)
        {
            var file = await _db?.Files.FirstOrDefaultAsync(file => file.Id == id)!;
            return file == null ? NotFound() : new JsonResult(file);
        }

        // сериализация в json формат
        [HttpGet("download/{id}")]
        public IActionResult Download(int id)
        {
            var file = _db?.Files.FirstOrDefault(file => file.Id == id)!;

            if (file == null)
            {
                return NotFound($"No file with {id} id");
            }

            var bills = _db?.Bills.Where(bill => bill.FileId == file.Id).Select(bill => new
            {
                bookNumber = bill.BookNumber,
                insaldoActive = bill.InsaldoActive,
                insaldoPassive = bill.InsaldoPassive,
                turnoversDebit = bill.TurnoversDebit,
                turnoversCreit = bill.TurnoversCredit,
                outsaldoActive = bill.OutsaldoActive,
                outsaldoPassive = bill.OutsaldoPassive
            });

            var result = new
            {
                fileName = file.Name,
                fileDescription = file.Description,
                startDate = file.StartDate,
                endDate = file.EndDate,
                bankName = _db!.Banks.FirstOrDefault(bank => bank.Id == file.BankId)!.Name,
                bills,
                sums = new
                {
                    insaldoActive = _db!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.InsaldoActive),
                    insaldoPassive = _db!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.InsaldoPassive),
                    turnoversDebit = _db!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.TurnoversDebit),
                    turnoversCreit = _db!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.TurnoversCredit),
                    outsaldoActive = _db!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.OutsaldoActive),
                    outsaldoPassive = _db!.Bills.Where(bill => bill.FileId == file.Id && bill.BookNumber >= 1000).Sum(bill => bill.OutsaldoPassive),
                }
            };

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            string resultToJson = JsonSerializer.Serialize(result, options);
            var fileName = $"{file.Name}.json";
            var mimeType = "text/plain";
            var resultBytes = Encoding.UTF8.GetBytes(resultToJson);

            return new FileContentResult(resultBytes, mimeType)
            {
                FileDownloadName = fileName,
            };
        }

        [HttpPost]
        public async Task<ActionResult<Models.File>> Post(IFormFile file)
        {
            bool uploadStatus = true;

            if (file == null || !ModelState.IsValid)
            {
                uploadStatus = false;
                Response.Cookies.Append("uploadStatus", uploadStatus ? "1" : "0");
                _db?.Database.CloseConnection();
                return RedirectToAction("index", "home");
            }

            string fileNameWOExtension = Path.GetFileNameWithoutExtension(file.FileName);
            string dateTime = DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss");
            string newFileName = $"{fileNameWOExtension} ({dateTime})";
            string filePath = $"/files/{newFileName}{Path.GetExtension(file.FileName)}";

            // сохраним файл на сервер
            using FileStream stream = new(_hostEnvironment?.WebRootPath + filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // сохранение данных из файла в бд
            var result = await SaveToDb(newFileName, filePath);

            if (result == false)
            {
                System.IO.File.Delete(_hostEnvironment?.WebRootPath + filePath);
            }

            Response.Cookies.Append("uploadStatus", result ? "1" : "0");
            _db?.Database.CloseConnection();
            return RedirectToAction("index", "home");
        }

        private async Task<bool> SaveToDb(string fileName, string filePath)
        {
            Application excel = new();
            Workbook? workbook;
            Worksheet? worksheet;

            workbook = excel.Workbooks.Open(_hostEnvironment?.WebRootPath + filePath);
            worksheet = workbook.Worksheets[1];

            // Используем транзакцию, чтобы всё сохранилось за раз
            using var transaction = _db?.Database.BeginTransaction();
            try
            {
                string bankName = worksheet.Range["A1"].Value2;
                string description = worksheet.Range["A2"].Value2 + " " + worksheet.Range["A3"].Value2;

                Regex regex = new(@"\d{2}\.\d{2}\.\d{4}");
                var dates = regex.Matches(description);

                Bank? searchBank = _db?.Banks.FirstOrDefault(bank => bank.Name.ToLower() == bankName.ToLower());
                Bank? newBank = new()
                {
                    Name = bankName[0].ToString().ToUpper() + bankName[1..].ToLower(),
                };

                if (searchBank == null)
                {
                    _db?.Banks.Add(newBank);
                    await _db!.SaveChangesAsync();

                    if (newBank?.Id == 0)
                    {
                        return false;
                    }

                    searchBank = newBank;
                }

                Models.File? file = new()
                {
                    Name = fileName,
                    Description = description,
                    StartDate = DateOnly.ParseExact(
                        dates[0].ToString(),
                        "dd.mm.yyyy",
                        new CultureInfo("ru-RU")
                    ),
                    EndDate = DateOnly.ParseExact(
                        dates[1].ToString(),
                        "dd.mm.yyyy",
                        new CultureInfo("ru-RU")
                    ),
                    BankId = searchBank!.Id
                };

                _db?.Files.Add(file);
                await _db!.SaveChangesAsync();

                if (file.Id == 0)
                {
                    return false;
                }

                int dataStartCell = 10;
                int billTypeId = 1;

                while (true)
                {
                    var bookNumberCellValue = worksheet.Range[$"A{dataStartCell}"].Value2;

                    if (bookNumberCellValue.ToString().Contains("ПО") && billTypeId == 9)
                    {
                        break;
                    }
                    else if (bookNumberCellValue.ToString().Contains("ПО"))
                    {
                        dataStartCell += 2;
                        billTypeId++;
                    }
                    else
                    {
                        Console.WriteLine(bookNumberCellValue);
                        _db?.Bills.Add(new()
                        {
                            BookNumber = (int)bookNumberCellValue,
                            InsaldoActive = (decimal)worksheet.Range[$"B{dataStartCell}"].Value2,
                            InsaldoPassive = (decimal)worksheet.Range[$"C{dataStartCell}"].Value2,
                            TurnoversDebit = (decimal)worksheet.Range[$"D{dataStartCell}"].Value2,
                            TurnoversCredit = (decimal)worksheet.Range[$"E{dataStartCell}"].Value2,
                            OutsaldoActive = (decimal)worksheet.Range[$"F{dataStartCell}"].Value2,
                            OutsaldoPassive = (decimal)worksheet.Range[$"G{dataStartCell}"].Value2,
                            BillTypeId = billTypeId,
                            FileId = file.Id
                        });

                        dataStartCell++;
                    }
                }

                int result = await _db!.SaveChangesAsync();

                if (result == 0)
                {
                    return false;
                }

                await transaction!.CommitAsync();

                // Очистка пямяти от ресурсов Excel
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Marshal.ReleaseComObject(worksheet);

                workbook.Close();
                Marshal.ReleaseComObject(workbook);

                excel.Quit();
                Marshal.ReleaseComObject(excel);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source);
                await transaction!.RollbackAsync();
                return false;
            }
        }
    }
}
