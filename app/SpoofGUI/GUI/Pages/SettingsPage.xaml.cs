using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SpoofGUI.GUI.ViewModels;

namespace SpoofGUI.GUI.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly SettingsPageViewModel _vm;
    private bool _initializing = true;

    public SettingsPage()
    {
        InitializeComponent();
        _vm = App.Services.GetRequiredService<SettingsPageViewModel>();
        Loaded += (_, _) => Load();
    }

    private void Load()
    {
        _initializing = true;
        UpdateVersion.Text = $"installed: {_vm.AppVersion}";
        UpdateLastCheck.Text = _vm.LastUpdateCheckText();
        ThemeChoice.SelectedIndex = _vm.Theme == "light" ? 1 : 0;
        SocksPortBox.Text = _vm.SocksPort.ToString();
        HttpPortBox.Text = _vm.HttpPort.ToString();
        AllowInsecureToggle.IsOn = _vm.XrayAllowInsecure;
        CheckOnLaunchToggle.IsOn = _vm.CheckUpdatesOnLaunch;
        LogLevelCombo.SelectedIndex = LogLevelToIndex(_vm.XrayLogLevel);
        DefaultModeCombo.SelectedIndex = ModeToIndex(_vm.V2RayMode);
        DataFolderText.Text = _vm.DataFolder;
        _initializing = false;
    }

    private static int LogLevelToIndex(string level) => level switch
    {
        "none" => 0,
        "error" => 1,
        "info" => 3,
        "debug" => 4,
        _ => 2,
    };

    private static string IndexToLogLevel(int index) => index switch
    {
        0 => "none",
        1 => "error",
        3 => "info",
        4 => "debug",
        _ => "warning",
    };

    private static int ModeToIndex(string mode) => mode switch
    {
        "Tunnel" => 1,
        "SystemProxy" => 2,
        _ => 0,
    };

    private static string IndexToMode(int index) => index switch
    {
        1 => "Tunnel",
        2 => "SystemProxy",
        _ => "Proxy",
    };

    private void OnSavePorts(object sender, object e)
    {
        var error = _vm.SavePorts(SocksPortBox.Text, HttpPortBox.Text);
        if (error is null)
        {
            PortsStatus.Text = $"saved: socks {_vm.SocksPort}, http {_vm.HttpPort} (reconnect to apply)";
            SocksPortBox.Text = _vm.SocksPort.ToString();
            HttpPortBox.Text = _vm.HttpPort.ToString();
        }
        else
        {
            PortsStatus.Text = $"not saved: {error}";
        }
    }

    private void OnResetPorts(object sender, object e)
    {
        PortsStatus.Text = _vm.ResetPorts() + " (reconnect to apply)";
        SocksPortBox.Text = _vm.SocksPort.ToString();
        HttpPortBox.Text = _vm.HttpPort.ToString();
    }

    private void OnAllowInsecureToggled(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _vm.XrayAllowInsecure = AllowInsecureToggle.IsOn;
    }

    private void OnLogLevelChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;
        _vm.XrayLogLevel = IndexToLogLevel(LogLevelCombo.SelectedIndex);
    }

    private void OnDefaultModeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;
        _vm.V2RayMode = IndexToMode(DefaultModeCombo.SelectedIndex);
    }

    private void OnCheckOnLaunchToggled(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _vm.CheckUpdatesOnLaunch = CheckOnLaunchToggle.IsOn;
    }

    private void OnOpenDataFolder(object sender, object e)
    {
        try { _vm.OpenDataFolder(); }
        catch { }
    }

    private async void OnCheckUpdates(object sender, object e)
    {
        CheckUpdatesButton.IsEnabled = false;
        UpdateLastCheck.Text = "checking...";
        UpdateReleaseLink.Visibility = Visibility.Collapsed;

        var res = await _vm.CheckForUpdatesAsync();
        UpdateVersion.Text = res.StatusText;
        UpdateLastCheck.Text = res.LastCheckText;
        UpdateReleaseLink.NavigateUri = new Uri(res.ReleaseUrl);
        UpdateReleaseLink.Content = res.IsUpdateAvailable ? "open new release" : "open latest release";
        UpdateReleaseLink.Visibility = Visibility.Visible;
        CheckUpdatesButton.IsEnabled = true;
    }

    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;
        var theme = ThemeChoice.SelectedIndex == 1 ? "light" : "dark";
        _vm.SetTheme(theme);
        App.CurrentWindow?.ApplyTheme(theme);
    }
}
