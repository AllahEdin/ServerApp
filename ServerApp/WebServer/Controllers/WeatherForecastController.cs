using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TcpClientApp;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ITcpServer _server;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ITcpServer server)
        {
            _logger = logger;
            _server = server;
        }

        [HttpGet]
        public async Task<IActionResult> StartServer()
        {
            _server.Start();

            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> Send(int clientNum, string msg)
        {
            await _server.SendDirect(clientNum, msg);

            return Ok();
        }
    }
}
