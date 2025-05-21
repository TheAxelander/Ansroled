using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ansroled.Common;
using Ansroled.Common.DialogResponses;
using Ansroled.Common.Helpers;
using Ansroled.Models;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Ansroled.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private RoleViewModel? _selectedRole;
    public RoleViewModel SelectedRole
    {
        get => _selectedRole ?? new RoleViewModel();
        set
        {
            SetProperty(ref _selectedRole, value);
            SaveAllCommand.NotifyCanExecuteChanged();
            CreateFileCommand.NotifyCanExecuteChanged();
            DeleteFileCommand.NotifyCanExecuteChanged();
        }
    }

    private IStorageFolder? _currentFolder;
    protected IStorageFolder? CurrentFolder
    {
        get => _currentFolder;
        set
        {
            SetProperty(ref _currentFolder, value);
            NewRoleCommand.NotifyCanExecuteChanged();
        }
    }
    
    [ObservableProperty]
    private GridSetting _tasksEditorGridSetting;
    [ObservableProperty]
    private GridSetting _defaultsEditorGridSetting;
    [ObservableProperty]
    private GridSetting _metaEditorGridSetting;
    [ObservableProperty]
    private GridSetting _varsEditorGridSetting;
    [ObservableProperty]
    private GridSetting _templatesEditorGridSetting;
    [ObservableProperty]
    private GridSetting _handlersEditorGridSetting;
    [ObservableProperty]
    private GridSetting _filesEditorGridSetting;

    public ObservableCollection<RoleViewModel> Roles { get; } = new();
    
    public Interaction<CreateRoleDialogViewModel, NewRoleResponse?> NewRoleDialog { get; }
    public Interaction<CreateFileDialogViewModel, NewFileResponse?> NewFileDialog { get; }
    public Interaction<DeleteFileDialogViewModel, DeleteFileResponse?> DeleteFileDialog { get; }
    
    private bool CanExecuteWithProject => CurrentFolder is not null;
    private bool CanExecuteWithRole => !string.IsNullOrWhiteSpace(SelectedRole.RoleName);
    
    public MainWindowViewModel()
    {
        NewRoleDialog = new();
        NewFileDialog = new();
        DeleteFileDialog = new();

        _tasksEditorGridSetting = GridSetting.Box0;
        _defaultsEditorGridSetting = GridSetting.Box1;
        _varsEditorGridSetting = GridSetting.Box2;
        _metaEditorGridSetting = GridSetting.Box3;
        _templatesEditorGridSetting = GridSetting.Box4;
        _handlersEditorGridSetting = GridSetting.Box5;
        _filesEditorGridSetting = GridSetting.Box6;
        
        if (Design.IsDesignMode)
        {
            for (int i = 1; i <= 5; i++)
            {
                Roles.Add(new RoleViewModel()
                {
                    RoleName = "Role" + i,
                });
            }
            SelectedRole = Roles[0];
            for (int i = 1; i <= 5; i++)
            {
                SelectedRole.DefaultsFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
                SelectedRole.FilesFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
                SelectedRole.HandlersFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
                SelectedRole.MetaFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
                SelectedRole.TasksFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
                SelectedRole.TemplatesFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
                SelectedRole.VarsFiles.Add(new EditorViewModel(AnsibleFile.CreateForDesign($"file{i}.yml")));
            }
        }
    }
    
    [RelayCommand]
    private async Task LoadData(CancellationToken token)
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var fullPath = Path.Combine(homePath, "Projects", "theaxelander", "ansible-roles", "roles");
        
        var folderService = ServiceHelper.Current.GetFolderService();
        CurrentFolder = await folderService.OpenFolderAsync(fullPath);
        await CollectAvailableRolesAsync();
    }

    private async Task CollectAvailableRolesAsync()
    {
        if (CurrentFolder is null) return;
        Roles.Clear();
        
        var tasks = new List<Task<RoleViewModel>>();
        
        await foreach (var item in CurrentFolder.GetItemsAsync())
        {
            if (item is not IStorageFolder folder) continue;
            tasks.Add(RoleViewModel.CreateAsync(folder));
        }

        foreach (var role in (await Task.WhenAll(tasks)).OrderBy(i => i.RoleName))
        {
            Roles.Add(role);
        }
    }
    
    [RelayCommand]
    private async Task OpenFolder(CancellationToken token)
    {
        try
        {
            var folderService = ServiceHelper.Current.GetFolderService();
            CurrentFolder = await folderService.OpenFolderAsync();
            await CollectAvailableRolesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanExecuteWithProject))]
    private async Task NewRole(CancellationToken token)
    {
        try
        {
            if (CurrentFolder is null) return;
            
            var result = await NewRoleDialog.Handle(new());
            if (result is null) return;
            
            // Create new Ansible role including sub-folders
            var newRole = await RoleViewModel.GenerateNewRoleAsync(CurrentFolder, result.RoleName);

            // Add new role to list
            var roles = Roles.ToList();
            roles.Add(newRole);
            Roles.Clear();
            foreach (var role in roles.OrderBy(i => i.RoleName))
            {
                Roles.Add(role);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteWithRole))]
    private async Task SaveAll(CancellationToken token)
    {
        try
        {
            await SelectedRole.SaveAllFilesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteWithRole))]
    private async Task CreateFile(CancellationToken token)
    {
        try
        {
            var result = await NewFileDialog.Handle(new());
            if (result is null) return;
            await SelectedRole.CreateFileAsync(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteWithRole))]
    private async Task DeleteFile(CancellationToken token)
    {
        var result = await DeleteFileDialog.Handle(new(SelectedRole));
        if (result is null) return;
        await SelectedRole.DeleteFileAsync(result);
    }

    [RelayCommand]
    private void SwitchEditorVisibility(Editor editor)
    {
        switch (editor)
        {
            case Editor.Tasks:
                // Do nothing as it should be always visible
                break;
            case Editor.Defaults:
                DefaultsEditorGridSetting = DefaultsEditorGridSetting.IsVisible ? GridSetting.Hidden : GridSetting.Box1;
                break;
            case Editor.Vars:
                VarsEditorGridSetting = VarsEditorGridSetting.IsVisible ? GridSetting.Hidden : GridSetting.Box2;
                break;
            case Editor.Meta:
                MetaEditorGridSetting = MetaEditorGridSetting.IsVisible ? GridSetting.Hidden : GridSetting.Box3;
                break;
            case Editor.Templates:
                TemplatesEditorGridSetting = TemplatesEditorGridSetting.IsVisible ? GridSetting.Hidden : GridSetting.Box4;
                break;
            case Editor.Handlers:
                HandlersEditorGridSetting = HandlersEditorGridSetting.IsVisible ? GridSetting.Hidden : GridSetting.Box5;
                break;
            case Editor.Files:
                FilesEditorGridSetting = FilesEditorGridSetting.IsVisible ? GridSetting.Hidden : GridSetting.Box6;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(editor), editor, null);
        }

        UpdateEditorBoxPositions();
    }

    private void UpdateEditorBoxPositions()
    {
        int index = 0;
        TasksEditorGridSetting = GridSetting.GetBoxFromIndex(index);
        if (DefaultsEditorGridSetting != GridSetting.Hidden) DefaultsEditorGridSetting = GridSetting.GetBoxFromIndex(++index);
        if (VarsEditorGridSetting != GridSetting.Hidden) VarsEditorGridSetting = GridSetting.GetBoxFromIndex(++index);
        if (MetaEditorGridSetting != GridSetting.Hidden) MetaEditorGridSetting = GridSetting.GetBoxFromIndex(++index);
        if (TemplatesEditorGridSetting != GridSetting.Hidden) TemplatesEditorGridSetting = GridSetting.GetBoxFromIndex(++index);
        if (HandlersEditorGridSetting != GridSetting.Hidden) HandlersEditorGridSetting = GridSetting.GetBoxFromIndex(++index);
        if (FilesEditorGridSetting != GridSetting.Hidden) FilesEditorGridSetting = GridSetting.GetBoxFromIndex(++index);
    }
}