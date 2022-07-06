using System.Net.Mime;
using BookStore.Core.Interfaces.Services;
using BookStore.Core.Models;
using Microsoft.AspNetCore.Mvc;
using SerilogTimings;
using ILogger = Serilog.ILogger;

namespace BookStoreApi.V1.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/" + ApiConstants.ServiceName + "/v{api-version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class BooksController : Controller
    {
        private readonly IBookService _service;
        private readonly ILogger _logger;

        public BooksController(IBookService service, ILogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        [HttpGet(Name = "GetAllBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<Book>>> Get()
        {
            try
            {
                using (Operation.Time("Getting all books"))
                {
                    var response = await _service.GetRecordsAsync().ConfigureAwait(false);
                    if (response == null)
                    {
                        return NoContent();
                    }

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                _logger
                    .ForContext("Controller", nameof(BooksController))
                    .ForContext("Method", nameof(Get))
                    .Warning("Entered");
            }

            return Ok();
        }

        [HttpGet("{id}", Name = "GetBookById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Book>> GetById(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var response = await _service.GetByIdAsync(id).ConfigureAwait(false);

            return response != null ? Ok(response) : NotFound();
        }

        [HttpPost(Name = "CreateBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Book>> Post([FromBody] Book model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var response = await _service.AddAsync(model).ConfigureAwait(false);

            return CreatedAtRoute(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}", Name = "UpdateBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> Put(int id, [FromBody] Book model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _service.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return NotFound();
            }

            var response = await _service.UpdateAsync(model).ConfigureAwait(false);
            if (response)
            {
                return Ok();
            }
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _service.GetByIdAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return NotFound();
            }

            await _service.DeleteAsync(item.Id).ConfigureAwait(false);

            return NoContent();
        }

    }
}
