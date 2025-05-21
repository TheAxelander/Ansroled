using System;
using System.Threading.Tasks;
using Ansroled.Common;
using Ansroled.Common.Helpers;
using Ansroled.Services;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Ansroled.Models;

public partial class AnsibleFile : ObservableObject
{
    [ObservableProperty]
    private string _fileName;
    
    [ObservableProperty]
    private TextDocument _content;
    
    public IStorageFile FileReference { get; } = null!;

    protected AnsibleFile()
    {
        _fileName = string.Empty;
        _content = new TextDocument();
    }

    public AnsibleFile(IStorageFile fileReference)
    {
        FileReference = fileReference;
        _fileName = fileReference.Name;
        _content = new TextDocument();
    }
    
    public AnsibleFile(IStorageFile fileReference, string fileName) : this(fileReference)
    {
        _fileName = fileName;
        _content = new TextDocument();
    }

    public AnsibleFile(IStorageFile fileReference, string fileName, TextDocument content) : 
        this(fileReference, fileName)
    {
        _content = content;
    }

    public static AnsibleFile CreateForDesign(string fileName)
    {
        return new AnsibleFile()
        {
            FileName = fileName,
            Content = new TextDocument(DesignDataHelper.EditorContent)
        };
    }

    public async Task SaveFileAsync()
    {
        var fileService = App.Current?.Services?.GetService<IFilesService>();
        if (fileService is null) throw new NullReferenceException("Missing File Service instance.");
        
        await fileService.WriteFileAsync(FileReference, Content.Text);
    }
}