using MovieVault.Context;
using MovieVault.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace MovieVault.Handler
{
    public class MoviesHandler : IMoviesHandler
    {
        private readonly MongoDbContext _context;

        public MoviesHandler(MongoDbContext context)
        {
            _context = context;
        }

        // Exceptions are no longer swallowed here: they bubble up to Program.cs's
        // global exception handler, which logs them and returns a consistent error
        // response instead of a silent 200 with an empty body.

        public async Task<bool> AddMovie(Movies movie)
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> GetMovies()
        {
            var movies = await _context.Movies.ToListAsync();
            return movies.Select(movie => new
            {
                Id = movie.Id.ToString(),
                movie.Title,
                movie.Genre,
                movie.Rating
            });
        }

        public async Task<bool> RemoveMovie(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return false;
            }

            var movie = await _context.Movies.FindAsync(objectId);
            if (movie == null)
            {
                return false;
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMovie(string id, Movies movie)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return false;
            }

            var movieToUpdate = await _context.Movies.FindAsync(objectId);
            if (movieToUpdate == null)
            {
                return false;
            }

            movieToUpdate.Title = movie.Title;
            movieToUpdate.Genre = movie.Genre;
            movieToUpdate.Rating = movie.Rating;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
