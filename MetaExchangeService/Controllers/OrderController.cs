using MetaExchange;
using MetaExchangeService.Models;
using Microsoft.AspNetCore.Mvc;

namespace MetaExchangeService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderBookService _service;

        public OrderController(IOrderBookService service)
        {
            _service = service;
        }

        [HttpPost()]
        [Route("Sell")]
        [Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Sell([FromBody] Order order)
        {
            if (order.Amount <= 0.0)
            {
                return BadRequest(); // can't sell nothing
            }
            if (order.Amount <= 0.0)
            {
                return BadRequest(); // can't sell nothing
            }

            var (remaining, orders) = _service.Sell(order.Amount);
            if (remaining > 0.0)
            {
                return UnprocessableEntity(orders);
            }
            return Ok(orders);
        }

        [HttpPost()]
        [Route("Buy")]
        [Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Buy([FromBody] Order order)
        {
            if (order.Amount <= 0.0)
            {
                return BadRequest(); // can't buy nothing
            }

            var (remaining, orders) = _service.Buy(order.Amount);
            if (remaining > 0.0)
            {
                return UnprocessableEntity(orders);
            }
            return Ok(orders);
        }
    }
}