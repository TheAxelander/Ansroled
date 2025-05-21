using System;
using System.Reactive;
using Ansroled.ViewModels;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ansroled.Dialogs;

public partial class DeleteFileDialog : ReactiveWindow<DeleteFileDialogViewModel>
{
    public DeleteFileDialog()
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

    private async void Editor_OnSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        try
        {
            await ViewModel!.LoadFilesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // TODO Show error message
        }
    }
}