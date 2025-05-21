using System;
using Ansroled.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ansroled.Common.Helpers;

public class ServiceHelper
{
    private static ServiceHelper? _current;
    public static ServiceHelper Current => _current ??= new();

    protected ServiceHelper() { }
    
    public IFolderService GetFolderService()
    {
        var folderService = App.Current?.Services?.GetService<IFolderService>();
        if (folderService is null) throw new NullReferenceException("Missing Folder Service instance.");
        return folderService;
    }
    
    public IFilesService GetFileService()
    {
        var fileService = App.Current?.Services?.GetService<IFilesService>();
        if (fileService is null) throw new NullReferenceException("Missing File Service instance.");
        return fileService;
    }
}