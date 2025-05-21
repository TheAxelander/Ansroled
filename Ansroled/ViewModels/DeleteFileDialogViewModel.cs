using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ansroled.Common;
using Ansroled.Common.DialogResponses;
using Ansroled.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Ansroled.ViewModels;

public partial class DeleteFileDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private AnsibleFile? _file;
    
    [ObservableProperty]
    private Editor _editor;

    public ObservableCollection<AnsibleFile> Files { get; }
    public Interaction<DeleteFileResponse?, Unit> CloseDialog { get; }
    
    public Array Editors => Enum.GetValues(typeof(Editor));
    
    private bool CanOkExecute => File is not null;
    
    private readonly RoleViewModel? _currentRole;

    public DeleteFileDialogViewModel()
    {
        _currentRole = null;
        _editor = Editor.Tasks;
        Files = new();
        CloseDialog = new();
    }

    public DeleteFileDialogViewModel(RoleViewModel currentRole) : this()
    {
        _currentRole = currentRole;
    }

    public async Task LoadFilesAsync()
    {
        if (_currentRole is null) return;
        Files.Clear();
        File = null;

        foreach (var file in await _currentRole.GetFilesFromEditorAsync(Editor))
        {
            Files.Add(file);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOkExecute))]
    private async Task<DeleteFileResponse> Ok()
    {
        if (File is null) throw new ArgumentNullException(nameof(File));
        var result = new DeleteFileResponse(File, Editor);
        await CloseDialog.Handle(result);
        return result;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await CloseDialog.Handle(null);
    }
}