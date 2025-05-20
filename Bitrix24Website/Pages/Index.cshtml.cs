using Bitrix24Website.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace Bitrix24Website.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int MaxPage { get; set; } = 1;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IList<Contact> Contacts { get; set; } = new List<Contact>();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;
        private (string? clientId, string? clientSecret) GetClientCredentials()
        {
            var clientId = HttpContext.Session.GetString("clientId");
            var clientSecret = HttpContext.Session.GetString("clientSecret");
            return (clientId, clientSecret);
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var clientId = HttpContext.Session.GetString("clientId");
            var clientSecret = HttpContext.Session.GetString("clientSecret");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return RedirectToPage("/Login");
            }
            try
            {
                var client = _httpClientFactory.CreateClient();

                object payload = new
                {
                    SELECT = new string[] { "ID", "NAME", "BIRTHDATE", "EMAIL", "PHONE", "WEB" },
                    START = (Page - 1) * 50
                };
                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    payload = new
                    {
                        SELECT = new string[] { "ID", "NAME", "BIRTHDATE", "EMAIL", "PHONE", "WEB" },
                        FILTER = new Dictionary<string, object>
                        {
                            ["%NAME"] = SearchString
                        },
                        START = Page
                    };
                }
                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.contact.list",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in index: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        TempData["ToastMessage"] = $"Token đã hết hạn, vui lòng thay đổi credentials";
                    }
                    else
                    { 
                        TempData["ToastMessage"] = $"Không thể tải do {response.ReasonPhrase}";
                    }
                    return Page();
                }

                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;
                if (root.TryGetProperty("result", out var resultElement) && resultElement.ValueKind == JsonValueKind.Array)
                {
                    Contacts = resultElement.Deserialize<List<Contact>>() ?? new List<Contact>();
                }
                int total = root.GetProperty("total").GetInt32();
                MaxPage = total / 50;
                if (total % 50 != 0) MaxPage++;

            }
            catch (HttpRequestException httpEx)
            {
                TempData["ToastMessage"] = $"Lỗi mạng: {httpEx.Message}"; 
                Console.WriteLine("Error in index: " + httpEx.Message);
            }
            catch (JsonException jsonEx)
            {
                TempData["ToastMessage"] = $"Định dạng JSON không đúng: {jsonEx.Message}";
                Console.WriteLine("Error in index: " + jsonEx.Message);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Lỗi: {ex.Message}"; 
                Console.WriteLine("Error in index: " + ex.Message);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    TempData["ToastMessage"] = $"Thiếu client id và client secret";
                    return RedirectToPage("/Login");
                }
                var client = _httpClientFactory.CreateClient();
                object payload = new
                {
                    ID = id
                };


                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.contact.delete",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error delete in index: {responseMsg}");
                    TempData["ToastMessage"] = $"{responseMsg}";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Đã có lỗi xảy ra";
            }

            return RedirectToPage("Index");
        }
    }
}
