using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieTicketsAdminapplication.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace MovieTicketsAdminapplication.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ImportUsers(IFormFile file)
        {
            string pathToUpload = $"{Directory.GetCurrentDirectory()}\\files\\{file.FileName}";

            using(FileStream filestream = System.IO.File.Create(pathToUpload))
            {
                file.CopyTo(filestream);
                filestream.Flush();
            }

            List<User> users = getAllUsersFromFile(file.FileName);

            HttpClient client = new HttpClient();

            string URI = "https://localhost:44396/api/admin/ImportAllUsers";

            HttpContent content = new StringContent(JsonConvert.SerializeObject(users), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URI, content).Result;

            return RedirectToAction("Index", "Order");
        }
        private List<User> getAllUsersFromFile(string fileName)
        {
            List<User> users = new List<User>();

            string filePath = $"{Directory.GetCurrentDirectory()}\\files\\{fileName}";

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Name = reader.GetValue(0).ToString(),
                            Surname = reader.GetValue(1).ToString(),
                            Email = reader.GetValue(2).ToString(),
                            Password = reader.GetValue(3).ToString(),
                            ConfirmPassword = reader.GetValue(4).ToString(),
                            PhoneNumber = reader.GetValue(5).ToString()
                        });
                    }
                }
                
            }
            return users;
        }
    }
}

