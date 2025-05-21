using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ansroled.Common;
using Ansroled.Common.DialogResponses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Ansroled.ViewModels;

public partial class CreateFileDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private string _fileName;

    [ObservableProperty]
    private Editor _editor;
    
    public Array Editors => Enum.GetValues(typeof(Editor));
    private bool CanOkExecute => !string.IsNullOrWhiteSpace(FileName);
    
    public Interaction<NewFileResponse?, Unit> CloseDialog { get; }

    public CreateFileDialogViewModel()
    {
        _fileName = string.Empty;
        _editor = Editor.Tasks;
        CloseDialog = new();
    }
    
    [RelayCommand(CanExecute = nameof(CanOkExecute))]
    private async Task<NewFileResponse> Ok()
    {
        var result = new NewFileResponse(FileName, Editor);
        await CloseDialog.Handle(result);
        return result;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await CloseDialog.Handle(null);
    }
}