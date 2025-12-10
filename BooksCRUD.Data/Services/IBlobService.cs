using System.IO;
using System.Threading.Tasks;

namespace BooksCRUD.Data.Services
{
    public interface IBlobService
    {
        Task<string> UploadFileAsync(Stream file, string fileName);
    }
}
