using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ActivationController: ControllerBase {

    [HttpGet]
    public IActionResult Get() {
        return Ok(new { state = "Activated"});
    }

    [HttpPost]
    public IActionResult Post(Activation activation) {
        if(!ModelState.IsValid) {
            return BadRequest();
        }
        return Created("", new Activation());
    }
}