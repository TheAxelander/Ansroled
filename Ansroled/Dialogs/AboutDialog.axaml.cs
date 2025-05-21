using System.Diagnostics;
using System.Reactive;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Ansroled.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;

namespace Ansroled.Dialogs;

public partial class AboutDialog : ReactiveWindow<AboutDialogViewModel>
{
    public AboutDialog()
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
    
    private void OnLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBlock textBlock) return;
        
        var url = textBlock.Tag as string;
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}