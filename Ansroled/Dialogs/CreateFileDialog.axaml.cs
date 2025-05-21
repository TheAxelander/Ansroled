using System.Reactive;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Ansroled.ViewModels;
using Avalonia.Controls;

namespace Ansroled.Dialogs;

public partial class CreateFileDialog : ReactiveWindow<CreateFileDialogViewModel>
{
    public CreateFileDialog()
    {
        InitializeComponent();
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
        
        this.WhenActivated(_ =>
        {
            ViewModel!.CloseDialog.RegisterHandler(interaction =>
            {
                Close(interaction.Input);
                interaction.SetOutput(Unit.Default);
            });
        });
    }
}