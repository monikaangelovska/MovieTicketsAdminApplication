using ClosedXML.Excel;
using GemBox.Document;
using Microsoft.AspNetCore.Mvc;
using MovieTicketsAdminapplication.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace MovieTicketsAdminapplication.Controllers
{
    public class OrderController : Controller
    {
        public OrderController()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        }

        [HttpGet]
        public IActionResult ExportAllOrders()
        {
            string fileName = "Orders.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            using(var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("All Orders");

                worksheet.Cell(1, 1).Value = "Order Id";
                worksheet.Cell(1, 2).Value = "Customer Email";

                HttpClient client = new HttpClient();

                string URL = "https://localhost:44396/api/Admin/GetAllActiveOrders";

                HttpResponseMessage response = client.GetAsync(URL).Result;

                var data = response.Content.ReadAsAsync<List<Order>>().Result;

                for (int i = 1; i <= data.Count; i++)
                {
                    var order = data[i - 1];

                    worksheet.Cell(i + 1, 1).Value = order.Id.ToString();
                    worksheet.Cell(i + 1, 2).Value = order.User.Email;

                    for (int m = 0; m < order.MovieInOrders.Count(); m++)
                    {
                        worksheet.Cell(1, m + 3).Value = "Movie-" + (m + 1);
                        worksheet.Cell(i + 1, m + 3).Value = order.MovieInOrders.ElementAt(m).Movie.Name;
                    }
                }

                using(var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, contentType, fileName);
                }

            }

        }
        public IActionResult Index()
        {
            HttpClient client = new HttpClient();

            string URL = "https://localhost:44396/api/Admin/GetAllActiveOrders";

            HttpResponseMessage response = client.GetAsync(URL).Result;

            var data = response.Content.ReadAsAsync<List<Order>>().Result;

            return View(data);
        }

        public IActionResult Details(Guid orderId)
        {
            HttpClient client = new HttpClient();

            string URL = "https://localhost:44396/api/Admin/GetDetailsForOrder";

            var model = new
            {
                Id = orderId,
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var data = response.Content.ReadAsAsync<Order>().Result;

            return View(data);
        }
        public FileContentResult CreateInvoice(Guid orderId)
        {
            HttpClient client = new HttpClient();

            string URL = "https://localhost:44396/api/Admin/GetDetailsForOrder";

            var model = new
            {
                Id = orderId,
            };

            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Invoice.docx");

            var data = response.Content.ReadAsAsync<Order>().Result;

            var document = DocumentModel.Load(templatePath);

            document.Content.Replace("{{OrderNumber}}", data.Id.ToString());
            document.Content.Replace("{{UserName}}", data.User.Name);

            StringBuilder sb = new StringBuilder();

            var totalPrice = 0.0;

            foreach(var item in data.MovieInOrders)
            {
                totalPrice += item.Amount * item.Movie.TicketPrice;
                sb.AppendLine(item.Movie.Name + " with amount of: " + item.Amount + " and price of: " + item.Movie.TicketPrice + " ДЕН");
            }

            document.Content.Replace("{{MovieList}}", sb.ToString());
            document.Content.Replace("{{TotalPrice}}", totalPrice.ToString() + " ДЕН");

            var stream = new MemoryStream();

            document.Save(stream, new PdfSaveOptions());

            return File(stream.ToArray(), new PdfSaveOptions().ContentType, "ExportInvoice.pdf");
        }   

    }
}
