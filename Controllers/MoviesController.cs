using MovieVault.Handler;
using MovieVault.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieVault.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesHandler _service;

        public MoviesController(IMoviesHandler service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMovie(Movies movie)
        {
            var success = await _service.AddMovie(movie);
            return success
                ? Ok(new GlobalResponse { code = 0, message = "Success" })
                : StatusCode(StatusCodes.Status500InternalServerError, new GlobalResponse { code = 1, message = "Failed to add movie" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _service.GetMovies();
            return Ok(movies);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveMovie([FromHeader] string id)
        {
            var success = await _service.RemoveMovie(id);
            return success
                ? Ok(new GlobalResponse { code = 0, message = "Success" })
                : NotFound(new GlobalResponse { code = 1, message = "Movie not found" });
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateMovie([FromHeader] string id, Movies movie)
        {
            var success = await _service.UpdateMovie(id, movie);
            return success
                ? Ok(new GlobalResponse { code = 0, message = "Success" })
                : NotFound(new GlobalResponse { code = 1, message = "Movie not found" });
        }
    }
}
