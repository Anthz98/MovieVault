using EntityFramework.Models;
using System.Collections;

namespace EntityFramework.Handler
{
    public interface IMoviesHandler
    {
        Task AddMovie(Movies movie);
        Task<IEnumerable> GetMovies();
        Task RemoveMovie(string id);
        Task UpdateMovie(string id, Movies movie);
    }
}
