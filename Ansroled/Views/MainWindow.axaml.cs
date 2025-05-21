using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ansroled.Common;
using Ansroled.Common.DialogResponses;
using Ansroled.Dialogs;
using Ansroled.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using ReactiveUI;
using TextMateSharp.Grammars;

namespace Ansroled.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private readonly TextBlock _statusTextBlock;
    
    private readonly TextEditor? _tasksEditor;
    private readonly TextEditor? _defaultsTextEditor;
    private readonly TextEditor? _metaTextEditor;
    private readonly TextEditor? _varsTextEditor;
    private readonly TextEditor? _templatesTextEditor;
    private readonly TextEditor? _handlersTextEditor;
    private readonly TextEditor? _filesTextEditor;

    private readonly RegistryOptions _registryOptions;
    
    private readonly TextMate.Installation _tasksTextMateInstallation;
    private readonly TextMate.Installation _defaultsTextMateInstallation;
    private readonly TextMate.Installation _metaTextMateInstallation;
    private readonly TextMate.Installation _varsTextMateInstallation;
    private readonly TextMate.Installation _templatesTextMateInstallation;
    private readonly TextMate.Installation _handlersTextMateInstallation;
    private readonly TextMate.Installation _filesTextMateInstallation;
    
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG        
        this.AttachDevTools();
#endif
        _statusTextBlock = this.Find<TextBlock>("StatusText")!;
        
        _tasksEditor = this.Find<TextEditor>("TasksEditor")!;
        _defaultsTextEditor = this.Find<TextEditor>("DefaultsEditor")!;
        _metaTextEditor = this.Find<TextEditor>("MetaEditor")!;
        _varsTextEditor = this.Find<TextEditor>("VarsEditor")!;
        _templatesTextEditor = this.Find<TextEditor>("TemplatesEditor")!;
        _handlersTextEditor = this.Find<TextEditor>("HandlersEditor")!;
        _filesTextEditor = this.Find<TextEditor>("FilesEditor")!;
        
        _registryOptions = new RegistryOptions(ThemeName.Dark);

        InitializeTextEditor(_tasksEditor);
        InitializeTextEditor(_defaultsTextEditor);
        InitializeTextEditor(_metaTextEditor);
        InitializeTextEditor(_varsTextEditor);
        InitializeTextEditor(_templatesTextEditor);
        InitializeTextEditor(_handlersTextEditor);
        InitializeTextEditor(_filesTextEditor);
        
        _tasksTextMateInstallation = _tasksEditor.InstallTextMate(_registryOptions);
        _tasksTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        _defaultsTextMateInstallation = _defaultsTextEditor.InstallTextMate(_registryOptions);
        _defaultsTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        _metaTextMateInstallation = _metaTextEditor.InstallTextMate(_registryOptions);
        _metaTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        _varsTextMateInstallation = _varsTextEditor.InstallTextMate(_registryOptions);
        _varsTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        _templatesTextMateInstallation = _templatesTextEditor.InstallTextMate(_registryOptions);
        _templatesTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        _handlersTextMateInstallation = _handlersTextEditor.InstallTextMate(_registryOptions);
        _handlersTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        _filesTextMateInstallation = _filesTextEditor.InstallTextMate(_registryOptions);
        _filesTextMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));
        
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
        
        this.WhenActivated(action => action(ViewModel.NewRoleDialog.RegisterHandler(NewRoleInteractionHandler)));
        this.WhenActivated(action => action(ViewModel.NewFileDialog.RegisterHandler(NewFileInteractionHandler)));
        this.WhenActivated(action => action(ViewModel.DeleteFileDialog.RegisterHandler(DeleteFileInteractionHandler)));
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private async Task NewRoleInteractionHandler(IInteractionContext<CreateRoleDialogViewModel, NewRoleResponse?> interaction)
    {
        var dialog = new CreateRoleDialog
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<NewRoleResponse?>(this);
        interaction.SetOutput(result);
    }
    
    private async Task NewFileInteractionHandler(IInteractionContext<CreateFileDialogViewModel, NewFileResponse?> interaction)
    {
        var dialog = new CreateFileDialog
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<NewFileResponse?>(this);
        interaction.SetOutput(result);
    }
    
    private async Task DeleteFileInteractionHandler(IInteractionContext<DeleteFileDialogViewModel, DeleteFileResponse?> interaction)
    {
        var dialog = new DeleteFileDialog()
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<DeleteFileResponse?>(this);
        interaction.SetOutput(result);
    }

    private void InitializeTextEditor(TextEditor editor)
    {
        editor.Document = null; //Required to hide initial empty row
        editor.Options.AllowToggleOverstrikeMode = true;
        editor.Options.EnableTextDragDrop = true;
        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.ColumnRulerPositions = new List<int>() { 80, 100 };
        editor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy(editor.Options);
        editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        editor.TextArea.RightClickMovesCaret = true;
        editor.Options.HighlightCurrentLine = true;
        editor.Options.CompletionAcceptAction = CompletionAcceptAction.DoubleTapped;
        
        /*var textMateInstallation = editor.InstallTextMate(_registryOptions);
        textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("yaml"));*/
            
        AddHandler(PointerWheelChangedEvent, (_, i) =>
        {
            if (i.KeyModifiers != KeyModifiers.Control) return;
            if (i.Delta.Y > 0) editor.FontSize++;
            else editor.FontSize = editor.FontSize > 1 ? editor.FontSize - 1 : 1;
        }, RoutingStrategies.Bubble, true);
    }
    
    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        if (sender is not Caret caret) return;
        _statusTextBlock.Text = string.Format($"Line {caret.Line} Column {caret.Column}");
    }

    private void TabStrip_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not TabStrip tabStrip) return;
        if (tabStrip.Tag is not Editor editor) return;

        TextMate.Installation textMateInstallation;
        TextEditor? textEditor;

        switch (editor)
        {
            case Editor.Tasks:
                textMateInstallation = _tasksTextMateInstallation;
                textEditor = _tasksEditor;
                break;
            case Editor.Defaults:
                textMateInstallation = _defaultsTextMateInstallation;
                textEditor = _defaultsTextEditor;
                break;
            case Editor.Meta:
                textMateInstallation = _metaTextMateInstallation;
                textEditor = _metaTextEditor;
                break;
            case Editor.Vars:
                textMateInstallation = _varsTextMateInstallation;
                textEditor = _varsTextEditor;
                break;
            case Editor.Templates:
                textMateInstallation = _templatesTextMateInstallation;
                textEditor = _templatesTextEditor;
                break;
            case Editor.Handlers:
                textMateInstallation = _handlersTextMateInstallation;
                textEditor = _handlersTextEditor;
                break;
            case Editor.Files:
                textMateInstallation = _filesTextMateInstallation;
                textEditor = _filesTextEditor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editor), editor, null);
        }
        
        if (textEditor is null) return;
        if (tabStrip.SelectedItem is EditorViewModel editorViewModel)
        {
            UpdateTextMate(textMateInstallation, editorViewModel);
            textEditor.IsEnabled = true;
        }
        else
        {
            textEditor.IsEnabled = false;
        }
    }

    private void UpdateTextMate(TextMate.Installation textMateInstallation, EditorViewModel viewModel)
    {
        try
        {
            var extension = System.IO.Path.GetExtension(viewModel.File.FileReference.Path.AbsolutePath); 
            textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(extension).Id));
        }
        catch (Exception)
        {
            // default to yml in case the extension is not supported
            textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yml").Id));
        }
    }
}