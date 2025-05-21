using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Ansroled.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
    public Task<IStorageFile?> CreateFileAsync(IStorageFolder currentFolder, string fileName = "main.yml",
        string content = "");
    public Task WriteFileAsync(IStorageFile file, string content);
}