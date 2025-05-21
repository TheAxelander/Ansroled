using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Ansroled.Services;

public class FilesService : IFilesService
{
    private readonly Window _target;

    public FilesService(Window target)
    {
        _target = target;
    }

    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open File",
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync()
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save File"
        });
    }
    
    public async Task<IStorageFile?> CreateFileAsync(IStorageFolder currentFolder, string fileName = "main.yml", string content = "")
    {
        var file = await currentFolder.CreateFileAsync(fileName);
        if (file is null || content == string.Empty) return file;

        await WriteFileAsync(file, content);
        
        return file;
    }

    public async Task WriteFileAsync(IStorageFile file, string content)
    {
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
    }
}