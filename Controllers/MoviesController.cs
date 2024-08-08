using EntityFramework.Handler;
using EntityFramework.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace EntityFramework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private IMoviesHandler _service;
        public MoviesController(IMoviesHandler service)
        {
            _service = service;
        }


        [Route("AddMovie")]
        [HttpPost]
        [Authorize]
        public async Task AddMovie(Movies movie)
        {
            await _service.AddMovie(movie);
        }


        [Route("GetAllMovies")]
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable> GetAllMovies()
        {
            return await _service.GetMovies();
        }

        [Route("RemoveMovie")]
        [HttpDelete]
        [Authorize]
        public async Task RemoveMovie([FromHeader] string id)
        {
            await _service.RemoveMovie(id);
        }

        [Route("UpdateMovie")]
        [HttpPut]
        [Authorize]
        public async Task UpdateMovie([FromHeader] string id, Movies movie)
        {
            await _service.UpdateMovie(id, movie);
        }
    }
}
