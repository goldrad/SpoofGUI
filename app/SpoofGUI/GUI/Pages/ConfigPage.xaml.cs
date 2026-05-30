using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SpoofGUI.GUI.ViewModels;
using SpoofGUI.Models;

namespace SpoofGUI.GUI.Pages;

public sealed partial class ConfigPage : Page
{
    private readonly ConfigPageViewModel _vm;
    private SpoofProfile? _selected;
    private bool _suppressSelection;

    public ConfigPage()
    {
        InitializeComponent();
        _vm = App.Services.GetRequiredService<ConfigPageViewModel>();
        Loaded += (_, _) => ReloadList();
    }

    private void ReloadList(long? selectId = null)
    {
        var profiles = _vm.All();
        var target = selectId is long id
            ? profiles.FirstOrDefault(p => p.Id == id)
            : profiles.FirstOrDefault(p => p.Id == _selected?.Id) ?? profiles.FirstOrDefault();

        _suppressSelection = true;
        ProfileList.ItemsSource = profiles;
        ProfileList.SelectedItem = target;
        _suppressSelection = false;

        _selected = target;
        LoadEditor(target);
        UpdateHeader(profiles.Count);
    }

    private void UpdateHeader(int count)
    {
        CountText.Text = $"{count:D2} / {ConfigPageViewModel.MaxProfiles}";
        AddButton.IsEnabled = count < ConfigPageViewModel.MaxProfiles;
        DeleteButton.IsEnabled = count > 1 && _selected is not null;
    }

    private void LoadEditor(SpoofProfile? p)
    {
        var has = p is not null;
        NameBox.IsEnabled = has;
        ListenHost.IsEnabled = has;
        ListenPort.IsEnabled = has;
        ConnectIp.IsEnabled = has;
        ConnectPort.IsEnabled = has;
        FakeSni.IsEnabled = has;
        SaveButton.IsEnabled = has;
        RevertButton.IsEnabled = has;
        SetActiveButton.IsEnabled = has && !p!.IsActive;

        if (!has)
        {
            EditorTitle.Text = "no profile";
            EditorActiveBadge.Visibility = Visibility.Collapsed;
            return;
        }

        EditorTitle.Text = p!.Name;
        NameBox.Text = p.Name;
        ListenHost.Text = p.ListenHost;
        ListenPort.Text = p.ListenPort.ToString();
        ConnectIp.Text = p.ConnectIp;
        ConnectPort.Text = p.ConnectPort.ToString();
        FakeSni.Text = p.FakeSni;
        EditorActiveBadge.Visibility = p.IsActive ? Visibility.Visible : Visibility.Collapsed;
        StatusText.Text = "";
    }

    private void OnProfileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection) return;
        _selected = ProfileList.SelectedItem as SpoofProfile;
        LoadEditor(_selected);
        DeleteButton.IsEnabled = _vm.Count() > 1 && _selected is not null;
    }

    private void OnAddProfile(object sender, object e)
    {
        if (!_vm.CanAdd)
        {
            StatusText.Text = $"limit reached — max {ConfigPageViewModel.MaxProfiles} profiles";
            return;
        }

        var draft = _vm.NewDraft();
        var id = _vm.Save(draft);
        ReloadList(id);
        StatusText.Text = $"added: {draft.Name}";
    }

    private void OnSave(object sender, object e)
    {
        if (_selected is null) return;

        var name = NameBox.Text.Trim();
        if (name.Length == 0)
        {
            StatusText.Text = "name cannot be empty";
            return;
        }
        if (_vm.NameIsTaken(name, _selected.Id))
        {
            StatusText.Text = $"name already in use: {name}";
            return;
        }
        if (!int.TryParse(ListenPort.Text, out var lp) || lp <= 0 || lp > 65535)
        {
            StatusText.Text = "invalid listen port";
            return;
        }
        if (!int.TryParse(ConnectPort.Text, out var cp) || cp <= 0 || cp > 65535)
        {
            StatusText.Text = "invalid connect port";
            return;
        }

        _selected.Name = name;
        _selected.ListenHost = ListenHost.Text.Trim();
        _selected.ListenPort = lp;
        _selected.ConnectIp = ConnectIp.Text.Trim();
        _selected.ConnectPort = cp;
        _selected.FakeSni = FakeSni.Text.Trim();

        try
        {
            var id = _vm.Save(_selected);
            _selected.Id = id;
            ReloadList(id);
            StatusText.Text = $"saved: {name}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"save failed: {ex.Message}";
        }
    }

    private void OnSetActive(object sender, object e)
    {
        if (_selected is null || _selected.Id == 0) return;
        _vm.SetActive(_selected.Id);
        var name = _selected.Name;
        ReloadList(_selected.Id);
        StatusText.Text = $"active: {name}";
    }

    private void OnRevert(object sender, object e) => ReloadList(_selected?.Id);

    private void OnDelete(object sender, object e)
    {
        if (_selected is null || _selected.Id == 0) return;
        if (_vm.Count() <= 1)
        {
            StatusText.Text = "keep at least one profile";
            return;
        }

        var name = _selected.Name;
        _vm.Delete(_selected.Id);
        _selected = null;
        ReloadList();
        StatusText.Text = $"deleted: {name}";
    }
}
