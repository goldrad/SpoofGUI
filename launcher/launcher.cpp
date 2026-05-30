#include <windows.h>
#include <shellapi.h>
#include <string>

static std::wstring ModuleDirectory()
{
    wchar_t modulePath[MAX_PATH];
    DWORD length = GetModuleFileNameW(nullptr, modulePath, MAX_PATH);
    std::wstring path(modulePath, length);
    size_t slash = path.find_last_of(L"\\/");
    if (slash != std::wstring::npos) path.resize(slash);
    return path;
}

int WINAPI wWinMain(HINSTANCE, HINSTANCE, PWSTR commandLine, int showCommand)
{
    std::wstring appDirectory = ModuleDirectory() + L"\\app";
    std::wstring targetExe = appDirectory + L"\\SpoofGUI.exe";

    SHELLEXECUTEINFOW info{};
    info.cbSize = sizeof(info);
    info.fMask = SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI;
    info.lpFile = targetExe.c_str();
    info.lpParameters = (commandLine && *commandLine) ? commandLine : nullptr;
    info.lpDirectory = appDirectory.c_str();
    info.nShow = showCommand;

    if (!ShellExecuteExW(&info))
    {
        MessageBoxW(nullptr, L"Unable to start SpoofGUI from the app folder.", L"SpoofGUI", MB_ICONERROR);
        return 1;
    }

    if (info.hProcess) CloseHandle(info.hProcess);
    return 0;
}
