using Bitrix24Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
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
        public List<AddressType> AddressTypes { get; set; } = new List<AddressType>();
        private (string? clientId, string? clientSecret) GetClientCredentials()
        {
            var clientId = HttpContext.Session.GetString("clientId");
            var clientSecret = HttpContext.Session.GetString("clientSecret");
            return (clientId, clientSecret);
        }
        public async Task<IActionResult> OnGetAsync(string id, string? initRequisite)
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
                // Address Type
                requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.enum.addresstype",
                    Payload = payload
                };

                requestJson = JsonSerializer.Serialize(requestObject);
                requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error get address type in detail: {responseMsg}");
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
                    if (root.TryGetProperty("result", out var resultElement) && resultElement.ValueKind == JsonValueKind.Array)
                    {
                        AddressTypes = resultElement.Deserialize<List<AddressType>>() ?? new List<AddressType>();
                    }
                }

                // Requisite
                payload = new
                {
                    FILTER = new Dictionary<string, object>
                    {
                        ["=ENTITY_ID"] = int.Parse(id)
                    },
                    SELECT = new string[] { "ID", "ENTITY_ID", "NAME", "PRESET_ID" },
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

                    // Get first requisite
                    if (string.IsNullOrEmpty(initRequisite))
                    {
                        RequisiteId = Requisites.Count != 0 ? int.Parse(Requisites.First().ID) : -1;
                    }
                    else
                    {
                        RequisiteId = int.Parse(initRequisite);
                    }
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
                int.TryParse(id, out contactId);
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
        public async Task<JsonResult> OnDeleteAddressAsync([FromBody] AddressDeleteModel model)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();
                object payload = new
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["TYPE_ID"] = model.Type_id,
                        ["ENTITY_TYPE_ID"] = model.Entity_type_id,
                        ["ENTITY_ID"] = model.Entity_id + "",
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.address.delete",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in deleting address: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }

            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnPostEditAddressAsync([FromBody] AddressCreateModel model)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();

                object payload = new
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["TYPE_ID"] = model.Type_id,
                        ["ENTITY_TYPE_ID"] = model.Entity_type_id,
                        ["ENTITY_ID"] = model.Entity_id,
                        ["CITY"] = model.City,
                        ["REGION"] = model.Region,
                        ["PROVINCE"] = model.Province,
                        ["COUNTRY"] = model.Country
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.address.update",
                    Payload = payload
                };


                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in updating address: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnPostCreateAddressAsync([FromBody] AddressCreateModel model)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();
                object payload = new
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["TYPE_ID"] = model.Type_id,
                        ["ENTITY_TYPE_ID"] = model.Entity_type_id,
                        ["ENTITY_ID"] = model.Entity_id,
                        ["CITY"] = model.City,
                        ["REGION"] = model.Region,
                        ["PROVINCE"] = model.Province,
                        ["COUNTRY"] = model.Country
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.address.add",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in creating address: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
                else
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseMsg);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnPostCreateBankAsync([FromBody] BankCreateModel model)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();

                object payload = new
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["ENTITY_ID"] = model.Entity_id,
                        ["NAME"] = model.Name,
                        ["RQ_ACC_NUM"] = model.Rq_acc_num,
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.requisite.bankdetail.add",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in creating bank: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnPostEditBankAsync([FromBody] BankUpdateModel model)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();

                object payload = new
                {
                    ID = model.Id,
                    Fields = new Dictionary<string, object>
                    {
                        ["NAME"] = model.Name,
                        ["RQ_ACC_NUM"] = model.Rq_acc_num,
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.requisite.bankdetail.update",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in updating bank: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnDeleteBankAsync([FromBody] int id)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
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
                    Method = "crm.requisite.bankdetail.delete",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in deleting address: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }

            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnGetAddressAsync(int requisiteId)
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
                }
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
                if (responseMsg.Contains("The given key was not present"))
                {
                    return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = $"{responseMsg}" });
                }
            }

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            var data = root.GetProperty("result").Deserialize<List<Address>>();

            return new JsonResult(new { success = true, data });
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
                SELECT = new string[] { "ID", "ENTITY_ID", "NAME", "RQ_ACC_NUM", "ACTIVE" },
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
                if (responseMsg.Contains("The given key was not present"))
                {
                    return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = $"{responseMsg}" });
                }
            }

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            var data = root.GetProperty("result").Deserialize<List<Bank>>();
            int total = root.GetProperty("total").GetInt32();

            return new JsonResult(new PaginationResult<Bank>(pageIndex, total, data, true));
        }
        public async Task<JsonResult> OnPostCreateProfileAsync([FromBody] RequisiteCreateModel model)
        {
            int data = -1;
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();
                object payload = new
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["ENTITY_TYPE_ID"] = model.EntityTypeId,
                        ["ENTITY_ID"] = model.EntityId,
                        ["PRESET_ID"] = model.PresetId,
                        ["NAME"] = model.Name,
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.requisite.add",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in creating requisite: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    var root = doc.RootElement;
                    data = root.GetProperty("result").GetInt32();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }
            return new JsonResult(new { success = true, response = data });
        }
        public async Task<JsonResult> OnPostEditProfileAsync([FromBody] RequisiteEditModel model)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();
                object payload = new
                {
                    ID = model.Id,
                    Fields = new Dictionary<string, object>
                    {
                        ["NAME"] = model.Name,
                    },
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.requisite.update",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in updating requisite: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }
        public async Task<JsonResult> OnDeleteProfileAsync([FromBody] int id)
        {
            try
            {
                var (clientId, clientSecret) = GetClientCredentials();
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new JsonResult(new { success = false, message = "Thiếu thông tin credentials" });
                }
                var client = _httpClientFactory.CreateClient();
                object payload = new
                {
                    ID = id,
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.requisite.delete",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in deleting requisite: {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        return new JsonResult(new { success = false, message = $"Token đã hết hạn, vui lòng thay đổi credentials" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = $"{responseMsg}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Đã có lỗi xảy ra" });
            }


            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostUpdateContactAsync(Contact contact)
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

                object payload = new
                {
                    ID = contact.ID,
                    Fields = new Dictionary<string, object>
                    {
                        ["NAME"] = contact.NAME,
                        ["PHONE"] = contact.PHONE,
                        ["EMAIL"] = contact.EMAIL,
                        ["WEB"] = contact.WEB,
                    }
                };

                var requestObject = new
                {
                    ClientID = clientId,
                    ClientSecret = clientSecret,
                    Method = "crm.contact.update",
                    Payload = payload
                };

                var requestJson = JsonSerializer.Serialize(requestObject);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5010/api/CallApi", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in updating contact : {responseMsg}");
                    if (responseMsg.Contains("The given key was not present"))
                    {
                        TempData["ToastMessage"] = "Token đã hết hạn, vui lòng thay đổi credentials";
                    }
                    else
                    {
                        TempData["ToastMessage"] = $"{responseMsg}";
                    }
                } else
                {
                    TempData["SuccessMessage"] = "Sửa thông tin liên lạc thành công";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"Đã có lỗi xảy ra";
            }

            return RedirectToPage();
        }
    }
}
