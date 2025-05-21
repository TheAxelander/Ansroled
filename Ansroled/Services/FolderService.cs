using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Ansroled.Services;

public class FolderService : IFolderService
{
    private readonly Window _target;
    
    public FolderService(Window target)
    {
        _target = target;
    }
    
    public async Task<IStorageFolder?> OpenFolderAsync()
    {
        var folders = await _target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open Ansible roles project",
            AllowMultiple = false
        });

        return folders.Any() ? folders[0] : null;
    }

    public async Task<IStorageFolder?> OpenFolderAsync(string path)
    {
        return await _target.StorageProvider.TryGetFolderFromPathAsync(new Uri(path));
    }

    public async Task<IStorageFolder?> CreateFolderAsync(IStorageFolder currentFolder, string name)
    {
        return await currentFolder.CreateFolderAsync(name);
    }
}