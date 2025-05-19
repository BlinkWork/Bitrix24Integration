using Bitrix24Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace Bitrix24Website.Pages
{
    public class DetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DetailModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        private const int PageSize = 50;
        public int RequisiteId;
        public Contact Contact { get; set; } = new Contact();
        public List<Requisite> Requisites { get; set; } = new List<Requisite>();
        private (string? clientId, string? clientSecret) GetClientCredentials()
        {
            var clientId = HttpContext.Session.GetString("clientId");
            var clientSecret = HttpContext.Session.GetString("clientSecret");
            return (clientId, clientSecret);
        }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            var (clientId, clientSecret) = GetClientCredentials();

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return RedirectToPage("/Login");

            try
            {
                var client = _httpClientFactory.CreateClient();

                // CONTACT 
                object payload = new
                {
                    ID = id,
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.contact.get",
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
                        TempData["ToastMessage"] = $"Token đã hết hạn, vui lòng thay đổi credentials";
                    }
                    else
                    {
                        TempData["ToastMessage"] = $"Không thể tải do {response.ReasonPhrase}";
                    }
                    RequisiteId = -2;
                    return Page();
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(responseString);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("result", out var resultElement))
                    {
                        Contact = resultElement.Deserialize<Contact>() ?? new Contact();
                    }
                }
                // Requisite
                payload = new
                {
                    FILTER = new Dictionary<string, object>
                    {
                        ["=ENTITY_ID"] = int.Parse(id)
                    },
                    SELECT = new string[] { "ID", "ENTITY_ID" },
                };
                requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.requisite.list",
                    Payload = payload
                };

                requestJson = JsonSerializer.Serialize(requestObject);
                requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in detail: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        TempData["ToastMessage"] = $"Token đã hết hạn, vui lòng thay đổi credentials";
                    }
                    else
                    {
                        TempData["ToastMessage"] = $"Không thể tải do {response.ReasonPhrase}";
                    }
                    RequisiteId = -2;
                    return Page();
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    using var docRequisite = JsonDocument.Parse(responseString);
                    var rootRequisite = docRequisite.RootElement;
                    if (rootRequisite.TryGetProperty("result", out var resultElement) && resultElement.ValueKind == JsonValueKind.Array)
                    {
                        Requisites = resultElement.Deserialize<List<Requisite>>() ?? new List<Requisite>();
                    }

                    // Hard fix only one requisite per person
                    RequisiteId = Requisites.Count != 0 ? int.Parse(Requisites.First().ID) : -1;
                }
            }
            catch (HttpRequestException httpEx)
            {
                TempData["ToastMessage"] = $"Lỗi mạng: {httpEx.Message}";
                RequisiteId = -2;
            }
            catch (JsonException jsonEx)
            {
                TempData["ToastMessage"] = $"Định dạng JSON không đúng: {jsonEx.Message}";
                Console.WriteLine("Error in detail: " + jsonEx.Message);
                RequisiteId = -2;
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Lỗi: {ex.Message}";
                RequisiteId = -2;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDelete(string id)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    TempData["ToastMessage"] = $"Thiếu client id và client secret";
                    return RedirectToPage();
                }
                var client = _httpClientFactory.CreateClient();
                int contactId = -1;
                int.TryParse(clientId, out contactId);
                if (contactId == -1)
                {
                    TempData["ToastMessage"] = $"Contact không đúng";
                    return RedirectToPage();
                }
                object payload = new
                {
                    ID = contactId
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
                    Console.WriteLine($"Error : {responseMsg}");
                    TempData["ToastMessage"] = $"{responseMsg}";
                    return RedirectToPage();
                }

            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Đã có lỗi xảy ra";
                return RedirectToPage();

            }


            return RedirectToPage("Index");
        }

        public async Task<JsonResult> OnGetAddressAsync(int requisiteId, int pageIndex = 1)
        {
            var (clientId, clientSecret) = GetClientCredentials();
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return new JsonResult(new { success = false, message = "Unauthorized" });

            List<Address> addresses = new List<Address>();
            var client = _httpClientFactory.CreateClient();
            object payload = new
            {
                FILTER = new Dictionary<string, object>
                {
                    ["=ENTITY_ID"] = requisiteId
                },
                SELECT = new string[] { "ENTITY_ID", "ADDRESS_1", "ADDRESS_2", "CITY", "REGION", "PROVINCE", "COUNTRY" },
                START = (pageIndex - 1) * 50
            };

            var requestObject = new
            {
                ClientID = clientId,
                ClientSecret = clientSecret,
                Method = "crm.address.list",
                Payload = payload
            };

            var requestJson = JsonSerializer.Serialize(requestObject);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error : {responseMsg}");
                return new JsonResult(new { success = false, message = "Failed to fetch address data" });
            }

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            var data = root.GetProperty("result").Deserialize<List<Address>>();
            int total = root.GetProperty("total").GetInt32();

            return new JsonResult(new PaginationResult<Address>(pageIndex, total, data));
        }


        public async Task<JsonResult> OnGetBankAsync(int requisiteId, int pageIndex = 1)
        {
            var (clientId, clientSecret) = GetClientCredentials();
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return new JsonResult(new { success = false, message = "Unauthorized" });

            List<Bank> banks = new List<Bank>();
            var client = _httpClientFactory.CreateClient();
            object payload = new
            {
                FILTER = new Dictionary<string, object>
                {
                    ["=ENTITY_ID"] = requisiteId
                },
                SELECT = new string[] { "ID", "NAME", "RQ_ACC_NUM", "ACTIVE" },
                START = (pageIndex - 1) * 50
            };

            var requestObject = new
            {
                ClientID = clientId,
                ClientSecret = clientSecret,
                Method = "crm.requisite.bankdetail.list",
                Payload = payload
            };

            var requestJson = JsonSerializer.Serialize(requestObject);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error : {responseMsg}");
                return new JsonResult(new { success = false, message = "Failed to fetch bank data" });
            }

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            var data = root.GetProperty("result").Deserialize<List<Bank>>();
            int total = root.GetProperty("total").GetInt32();

            return new JsonResult(new PaginationResult<Bank>(pageIndex, total, data));
        }
    }
}
