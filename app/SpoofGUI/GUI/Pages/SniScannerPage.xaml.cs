using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SpoofGUI.Core;
using SpoofGUI.GUI.ViewModels;

namespace SpoofGUI.GUI.Pages;

public sealed partial class SniScannerPage : Page
{
    private readonly SniScannerPageViewModel _vm;
    private readonly DispatcherQueue _dispatcher = DispatcherQueue.GetForCurrentThread();
    private bool _scanning;

    public SniScannerPage()
    {
        InitializeComponent();
        _vm = App.Services.GetRequiredService<SniScannerPageViewModel>();
    }

    private async void OnScan(object sender, object e)
    {
        if (_scanning) return;

        var domains = _vm.ParseDomains(DomainsBox.Text);
        if (domains.Count == 0)
        {
            ScanStatus.Text = "enter at least one hostname";
            return;
        }

        var capped = domains.Count > SniScannerService.MaxDomains;
        if (capped) domains = domains.Take(SniScannerService.MaxDomains).ToList();

        SetScanning(true);
        var total = domains.Count;
        ScanStatus.Text = capped ? $"scanning {total} (capped from more)…" : $"scanning 0 / {total}…";

        var progress = new Progress<int>(done =>
            _dispatcher.TryEnqueue(() => ScanStatus.Text = $"scanning {done} / {total}…"));

        try
        {
            var results = await _vm.ScanAsync(domains, VerifyHttpToggle.IsOn, progress, CancellationToken.None);
            ResultsList.ItemsSource = results;
            var usable = results.Count(r => r.UsableAsSni);
            ResultSummary.Text = $"{usable} usable · {results.Count} checked";
            ScanStatus.Text = usable > 0 ? $"done — {usable} usable Fake SNI target(s)" : "done — no usable targets";
        }
        catch (Exception ex)
        {
            ScanStatus.Text = $"scan failed: {ex.Message}";
        }
        finally
        {
            SetScanning(false);
        }
    }

    private void OnClear(object sender, object e)
    {
        DomainsBox.Text = "";
        ResultsList.ItemsSource = null;
        ResultSummary.Text = "";
        ScanStatus.Text = "";
    }

    private void OnUseDomain(object sender, object e)
    {
        if (sender is not FrameworkElement { DataContext: SniScanResult result }) return;

        ScanStatus.Text = _vm.CreateProfileFromResult(result);
    }

    private void SetScanning(bool on)
    {
        _scanning = on;
        ScanButton.IsEnabled = !on;
        ClearButton.IsEnabled = !on;
        ScanButtonLabel.Text = on ? "scanning…" : "scan";
        ScanSpinner.IsActive = on;
        ScanSpinner.Visibility = on ? Visibility.Visible : Visibility.Collapsed;
    }
}
