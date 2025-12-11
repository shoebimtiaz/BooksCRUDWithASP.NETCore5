using System.Threading.Tasks;
using BooksCRUD.Data.Models;

namespace BooksCRUD.Data.Services
{
    public interface IBookLogService
    {
        Task LogAsync(BookLog log);
    }
}
