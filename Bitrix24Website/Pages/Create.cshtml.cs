using Bitrix24Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace Bitrix24Website.Pages
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult OnGet()
        {
            var (clientId, clientSecret) = GetClientCredentials();

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return RedirectToPage("/Login");

            }

            return Page();
        }
        private (string? clientId, string? clientSecret) GetClientCredentials()
        {
            var clientId = HttpContext.Session.GetString("clientId");
            var clientSecret = HttpContext.Session.GetString("clientSecret");
            return (clientId, clientSecret);
        }

        public async Task<IActionResult> OnPostAsync(Contact contact)
        {
            var (clientId, clientSecret) = GetClientCredentials();

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập lại." });

            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                object payload = new
                {
                    FIELDS = new Dictionary<string, object>
                    {
                        ["NAME"] = contact.NAME,
                        ["EMAIL"] = contact.EMAIL,
                        ["PHONE"] = contact.PHONE,
                        ["WEB"] = contact.WEB
                    }
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.contact.add",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in detail: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = "Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"Không thể tạo do {response.ReasonPhrase}" });
                    }
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    return new JsonResult(new { success = true, id = doc.RootElement.GetProperty("result").GetInt32() });
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new JsonResult(new { success = false, message = $"Lỗi mạng: {httpEx.Message}" });
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine("Error in detail: " + jsonEx.Message);
                return new JsonResult(new { success = false, message = $"Định dạng JSON không đúng: {jsonEx.Message}" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}