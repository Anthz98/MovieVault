using EntityFramework.Context;
using EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;

namespace EntityFramework.Handler
{
    public class MoviesHandler : IMoviesHandler
    {
        private readonly MongoDbContext _context;
        public MoviesHandler(MongoDbContext context)
        {
            _context = context;
        }

        public async Task AddMovie(Movies movie)
        {
            try
            {
                await _context.Movies.AddAsync(movie);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }

        public async Task<IEnumerable> GetMovies()
        {
            var movies =  await _context.Movies.ToListAsync();
            if(movies != null)
            {
                var modifiedMovies = movies.Select(movie => new
                {
                    Id = movie.Id.ToString(),
                    movie.Title,
                    movie.Genre,
                    movie.Rating
                });
                return modifiedMovies;
            }
            return Enumerable.Empty<object>();
        }

        public async Task RemoveMovie(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return;
                }

                var movie = await _context.Movies.FindAsync(objectId);
                if (movie != null)
                {
                    _context.Movies.Remove(movie);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }

        public async Task UpdateMovie(string id, Movies movie)
        {
            try
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return;
                }

                var movieToUpdate = await _context.Movies.FindAsync(objectId);
                if (movieToUpdate != null)
                {
                    movieToUpdate.Title = movie.Title;
                    movieToUpdate.Genre = movie.Genre;
                    movieToUpdate.Rating = movie.Rating;
                    await _context.SaveChangesAsync();
                }

                //var filter = Builders<Movies>.Filter.Eq(m => m.Title, movie.Title);
                //var movie = await _context.Movies.Find(filter).FirstOrDefaultAsync();
                //return movie;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }
    }
}
