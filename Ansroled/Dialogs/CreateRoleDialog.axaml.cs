using Ansroled.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;

namespace Ansroled.Dialogs;

public partial class CreateRoleDialog : ReactiveWindow<CreateRoleDialogViewModel>
{
    public CreateRoleDialog()
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