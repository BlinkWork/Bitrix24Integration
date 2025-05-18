using Bitrix24API.Models;
using Bitrix24API.Utils;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bitrix24API.Controllers
{
    [Route("api")]
    [ApiController]
    public class InstallListener : ControllerBase
    {

        [HttpPost("Install")]
        public async Task<IActionResult> PostInstall([FromForm] BitrixAuthPayload information)
        {
            ManageItem.SaveToFile(information);
            return Ok("Installed successfully");
        }
        [HttpPost("CallApi")]
        public async Task<IActionResult> PostCallApi([FromBody] CallApiRequest request)
        {
            if (string.IsNullOrEmpty(request.clientId))
            {
                return BadRequest("Invalid client id.");
            }
            if (string.IsNullOrEmpty(request.clientSecret))
            {
                return BadRequest("Invalid client secret.");
            }
            if (string.IsNullOrEmpty(request.method))
            {
                return BadRequest("Invalid method.");
            }
            BitrixAuthPayload auth = ManageItem.ReadFromFile();
            if (auth == null)
            {
                return StatusCode(500, "Internal server error: Auth data does not exist.");
            }
            BitrixApiService service = new BitrixApiService(auth);
            try
            {
                var response = await service.CallApiAsync(request.clientId, request.clientSecret, request.method, request.payload);
                using var doc = JsonDocument.Parse(response);
                return Ok(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }
    }
}
