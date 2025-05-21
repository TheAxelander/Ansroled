using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ansroled.Common;
using Ansroled.Common.DialogResponses;
using Ansroled.Common.Extensions;
using Ansroled.Common.Helpers;
using Ansroled.Models;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ansroled.ViewModels;

public partial class RoleViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _roleName;
    
    public ObservableCollection<EditorViewModel> DefaultsFiles { get; } = new();
    public ObservableCollection<EditorViewModel> FilesFiles { get; } = new();
    public ObservableCollection<EditorViewModel> HandlersFiles { get; } = new();
    public ObservableCollection<EditorViewModel> MetaFiles { get; } = new();
    public ObservableCollection<EditorViewModel> TasksFiles { get; } = new();
    public ObservableCollection<EditorViewModel> TemplatesFiles { get; } = new();
    public ObservableCollection<EditorViewModel> VarsFiles { get; } = new();
    
    private List<Tuple<string, ObservableCollection<EditorViewModel>>> FolderSet =>
    [
        new("defaults", DefaultsFiles),
        new("files", FilesFiles),
        new("handlers", HandlersFiles),
        new("meta", MetaFiles),
        new("tasks", TasksFiles),
        new("templates", TemplatesFiles),
        new("vars", VarsFiles)
    ];
    
    private readonly IStorageFolder _folderReference = null!;

    public RoleViewModel()
    {
        _roleName = string.Empty;
    }
    
    protected RoleViewModel(IStorageFolder folderReference)
    {
        _folderReference = folderReference;
        _roleName = folderReference.Name;
    }

    public static async Task<RoleViewModel> CreateAsync(IStorageFolder folderReference)
    {
        var result = new RoleViewModel(folderReference);
        await result.GetRoleFilesAsync();
        
        return result;
    }

    public static async Task<RoleViewModel> GenerateNewRoleAsync(IStorageFolder folderReference, string roleName)
    {
        var folderService = ServiceHelper.Current.GetFolderService();
        var newRoleFolder = await folderService.CreateFolderAsync(folderReference, roleName);
        if (newRoleFolder is null) throw new Exception("Failed to create new role folder.");
        var result = new RoleViewModel(newRoleFolder);
            
        // Create Ansible role sub-folders
        await Task.WhenAll(
            result.CreateSubFolder("defaults"), 
            result.CreateSubFolder("files"),
            result.CreateSubFolder("handlers"), 
            result.CreateSubFolder("meta", DefaultFileContentHelper.MetaDefault), 
            result.CreateSubFolder("tasks", DefaultFileContentHelper.TaskDefault), 
            result.CreateSubFolder("templates"), 
            result.CreateSubFolder("vars"));
        
        return result;
    }
    
    public async Task GetRoleFilesAsync()
    {
        DefaultsFiles.Clear();
        FilesFiles.Clear();
        HandlersFiles.Clear();
        MetaFiles.Clear();
        TasksFiles.Clear();
        TemplatesFiles.Clear();
        VarsFiles.Clear();

        foreach (var (folderName, files) in FolderSet)
        {
            var folder = await _folderReference.GetFolderAsync(folderName);
            if (folder is null) continue;
            foreach (var ansibleFile in await ReadFilesFromFolderAsync(folder, includeContent: true))
            {
                files.Add(new EditorViewModel(ansibleFile));
            }
        }
    }
    
    private async Task<List<AnsibleFile>> ReadFilesFromFolderAsync(IStorageFolder folder, bool includeContent = false)
    {
        var tasks = new List<Task<AnsibleFile>>();

        await foreach (var item in folder.GetItemsAsync())
        {
            if (item is not IStorageFile file) continue;
            tasks.Add(ReadFileAsync(file));
        }

        return (await Task.WhenAll(tasks)).ToList();

        async Task<AnsibleFile> ReadFileAsync(IStorageFile file)
        {
            if (!includeContent) return new AnsibleFile(file, file.Name);
            
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return new AnsibleFile(file, file.Name, new TextDocument(content));
        }
    }

    public async Task<List<AnsibleFile>> GetFilesFromEditorAsync(Editor editor)
    {
        var folderService = ServiceHelper.Current.GetFolderService();
        
        var targetFolder = await folderService.OpenFolderAsync(
            Path.Combine(_folderReference.Path.AbsolutePath, editor.GetStringValue()));
        if (targetFolder is null) return new();
        return await ReadFilesFromFolderAsync(targetFolder);
    }
    
    public async Task SaveAllFilesAsync()
    {
        var tasks = new List<Task>();
        foreach (var (_, files) in FolderSet)
        {
            tasks.AddRange(files.Select(i => i.File.SaveFileAsync()));
        }
        
        await Task.WhenAll(tasks);
    }

    public async Task CreateFileAsync(NewFileResponse dialogResponse)
    {
        try
        {
            
            
            var folderService = ServiceHelper.Current.GetFolderService();
            var fileService = ServiceHelper.Current.GetFileService();
            
            var targetFolder = await folderService.OpenFolderAsync(
                                   Path.Combine(_folderReference.Path.AbsolutePath, dialogResponse.Editor.GetStringValue())) 
                               ?? await CreateSubFolder(dialogResponse.Editor.GetStringValue());

            var newFile = await fileService.CreateFileAsync(targetFolder, dialogResponse.FileName);
            if (newFile is null) throw new Exception("Failed to create file.");
            var editorViewModel = new EditorViewModel(new AnsibleFile(newFile, dialogResponse.FileName));

            switch (dialogResponse.Editor)
            {
                case Editor.Tasks:
                    TasksFiles.Add(editorViewModel);
                    break;
                case Editor.Files:
                    FilesFiles.Add(editorViewModel);
                    break;
                case Editor.Handlers:
                    HandlersFiles.Add(editorViewModel);
                    break;
                case Editor.Defaults:
                    DefaultsFiles.Add(editorViewModel);
                    break;
                case Editor.Meta:
                    MetaFiles.Add(editorViewModel);
                    break;
                case Editor.Vars:
                    VarsFiles.Add(editorViewModel);
                    break;
                case Editor.Templates:
                    TemplatesFiles.Add(editorViewModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogResponse.Editor), dialogResponse.Editor, null);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task<IStorageFolder> CreateSubFolder(string name, string initialContent = "")
    {
        var folderService = ServiceHelper.Current.GetFolderService();
        var subFolder = await folderService.CreateFolderAsync(_folderReference, name);
        if (subFolder is null) throw new Exception("Failed to create sub folder.");

        if (initialContent != string.Empty)
        {
            var fileService = ServiceHelper.Current.GetFileService();
            await fileService.CreateFileAsync(subFolder, content: initialContent);
        }
        return subFolder;
    }
    
    public async Task DeleteFileAsync(DeleteFileResponse dialogResponse)
    {
        try
        {
            await dialogResponse.File.FileReference.DeleteAsync();
            switch (dialogResponse.Editor)
            {
                case Editor.Tasks:
                    TasksFiles.Remove(TasksFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
                case Editor.Files:
                    FilesFiles.Remove(FilesFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
                case Editor.Handlers:
                    HandlersFiles.Remove(HandlersFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
                case Editor.Defaults:
                    DefaultsFiles.Remove(DefaultsFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
                case Editor.Meta:
                    MetaFiles.Remove(MetaFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
                case Editor.Vars:
                    VarsFiles.Remove(VarsFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
                case Editor.Templates:
                    TemplatesFiles.Remove(TemplatesFiles.First(i => i.File.FileName == dialogResponse.File.FileName));
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}