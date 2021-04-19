using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class InventoryController: ControllerBase {
    static int _requestCount = 0;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) {
        await Task.Delay(100);
        _requestCount++;

        // return await failingBackend(id);
        return await blockUnauthorized();
    }

    public async Task<IActionResult> failingBackend(int id) {
        _requestCount++;

        if(_requestCount % 4 == 0) {
            return Ok(15);
        }

        return StatusCode((int)HttpStatusCode.InternalServerError, "Deu ruim");
    }

    public async Task<IActionResult> blockUnauthorized() {
        string authCode = Request.Cookies["auth"];

        if(authCode == "GoodAuthCode") 
        {
            return Ok(15);
        }

        return StatusCode((int) HttpStatusCode.Unauthorized, "Not Authorized");
    }
}