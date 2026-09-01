using MovieVault.Models;

namespace MovieVault.Handler
{
    public interface IMoviesHandler
    {
        Task<bool> AddMovie(Movies movie);
        Task<IEnumerable<object>> GetMovies();
        Task<bool> RemoveMovie(string id);
        Task<bool> UpdateMovie(string id, Movies movie);
    }
}
