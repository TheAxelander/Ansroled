using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Ansroled.ViewModels;

public partial class AboutDialogViewModel : ViewModelBase
{
    public Interaction<object?, Unit> CloseDialog { get; }

    public AboutDialogViewModel()
    {
        CloseDialog = new();
    }
    
    [RelayCommand]
    private async Task Close()
    {
        await CloseDialog.Handle(null);
    }
}