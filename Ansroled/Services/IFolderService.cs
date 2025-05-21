using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Ansroled.Services;

public interface IFolderService
{
    public Task<IStorageFolder?> OpenFolderAsync();
    public Task<IStorageFolder?> OpenFolderAsync(string path);
    public Task<IStorageFolder?> CreateFolderAsync(IStorageFolder currentFolder, string name);
}