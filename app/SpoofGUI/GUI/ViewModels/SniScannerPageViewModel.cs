using SpoofGUI.Core;
using SpoofGUI.Database;
using SpoofGUI.Models;

namespace SpoofGUI.GUI.ViewModels;

public sealed class SniScannerPageViewModel
{
    private readonly SniScannerService _scanner;
    private readonly ProfileRepository _spoofProfiles;

    public SniScannerPageViewModel(SniScannerService scanner, ProfileRepository spoofProfiles)
    {
        _scanner = scanner;
        _spoofProfiles = spoofProfiles;
    }

    public IReadOnlyList<string> ParseDomains(string text) => SniScannerService.ParseDomains(text);

    public Task<IReadOnlyList<SniScanResult>> ScanAsync(
        IReadOnlyList<string> domains,
        bool verifyHttp,
        IProgress<int>? progress,
        CancellationToken ct)
        => _scanner.ScanAsync(domains, verifyHttp, SniScannerService.DefaultConcurrency, SniScannerService.DefaultTimeout, progress, ct);

    public string CreateProfileFromResult(SniScanResult result)
    {
        var existing = _spoofProfiles.All();
        if (existing.Count >= ConfigPageViewModel.MaxProfiles)
            return $"profile limit reached (max {ConfigPageViewModel.MaxProfiles}) — delete one in Configs first";

        var name = UniqueName(existing, result.Domain);
        _spoofProfiles.Upsert(new SpoofProfile
        {
            Name = name,
            ListenHost = "0.0.0.0",
            ListenPort = 40443,
            ConnectIp = string.IsNullOrWhiteSpace(result.Ip) ? "104.19.229.21" : result.Ip,
            ConnectPort = 443,
            FakeSni = result.Domain,
            IsActive = existing.Count == 0,
        });

        return $"created profile \"{name}\" in Configs";
    }

    private static string UniqueName(IReadOnlyList<SpoofProfile> existing, string baseName)
    {
        var names = existing.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!names.Contains(baseName)) return baseName;
        for (var i = 2; i <= 99; i++)
        {
            var candidate = $"{baseName} ({i})";
            if (!names.Contains(candidate)) return candidate;
        }

        return $"{baseName} ({Guid.NewGuid():N})";
    }
}
