using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ansroled.Common.DialogResponses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Ansroled.ViewModels;

public partial class CreateRoleDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private string _roleName;
    
    public Interaction<NewRoleResponse?, Unit> CloseDialog { get; }
    
    private bool CanOkExecute => !string.IsNullOrWhiteSpace(RoleName);

    public CreateRoleDialogViewModel()
    {
        _roleName = string.Empty;
        CloseDialog = new();
    }
    
    [RelayCommand(CanExecute = nameof(CanOkExecute))]
    private async Task<NewRoleResponse> Ok()
    {
        var result = new NewRoleResponse(RoleName);
        await CloseDialog.Handle(result);
        return result;
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await CloseDialog.Handle(null);
    }
}