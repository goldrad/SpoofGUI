using SpoofGUI.Database;
using SpoofGUI.Models;

namespace SpoofGUI.GUI.ViewModels;

public sealed class ConfigPageViewModel
{
    public const int MaxProfiles = 10;

    private readonly ProfileRepository _profiles;
    public ConfigPageViewModel(ProfileRepository profiles) => _profiles = profiles;

    public IReadOnlyList<SpoofProfile> All() => _profiles.All();
    public int Count() => _profiles.Count();
    public bool CanAdd => _profiles.Count() < MaxProfiles;

    public SpoofProfile NewDraft()
    {
        var existing = _profiles.All();
        return new SpoofProfile
        {
            Name = UniqueName(existing, "profile"),
            ListenHost = "0.0.0.0",
            ListenPort = 40443,
            ConnectIp = "104.19.229.21",
            ConnectPort = 443,
            FakeSni = "www.hcaptcha.com",
            IsActive = existing.Count == 0,
        };
    }

    public bool NameIsTaken(string name, long exceptId) =>
        _profiles.All().Any(p => p.Id != exceptId && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    public long Save(SpoofProfile profile) => _profiles.Upsert(profile);
    public void SetActive(long id) => _profiles.SetActive(id);
    public void Delete(long id) => _profiles.Delete(id);

    private static string UniqueName(IReadOnlyList<SpoofProfile> existing, string baseName)
    {
        var names = existing.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i <= 99; i++)
        {
            var candidate = $"{baseName} {i}";
            if (!names.Contains(candidate)) return candidate;
        }

        return $"{baseName} {Guid.NewGuid():N}";
    }
}
