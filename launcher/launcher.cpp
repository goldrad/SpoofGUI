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

static void ShowError(const std::wstring& detail)
{
    std::wstring message = L"Unable to start SpoofGUI.\n\n" + detail;
    MessageBoxW(nullptr, message.c_str(), L"SpoofGUI", MB_ICONERROR);
}

int WINAPI wWinMain(HINSTANCE, HINSTANCE, PWSTR commandLine, int showCommand)
{
    std::wstring appDirectory = ModuleDirectory() + L"\\app";
    std::wstring targetExe = appDirectory + L"\\SpoofGUI.exe";

    if (GetFileAttributesW(targetExe.c_str()) == INVALID_FILE_ATTRIBUTES)
    {
        ShowError(L"Missing file:\n" + targetExe);
        return 1;
    }

    std::wstring command = L"\"" + targetExe + L"\"";
    if (commandLine && *commandLine)
    {
        command += L" ";
        command += commandLine;
    }

    STARTUPINFOW startupInfo{};
    startupInfo.cb = sizeof(startupInfo);
    PROCESS_INFORMATION processInfo{};
    std::wstring mutableCommand = command;

    if (CreateProcessW(targetExe.c_str(), &mutableCommand[0], nullptr, nullptr, FALSE,
                       0, nullptr, appDirectory.c_str(), &startupInfo, &processInfo))
    {
        CloseHandle(processInfo.hThread);
        CloseHandle(processInfo.hProcess);
        return 0;
    }

    DWORD createError = GetLastError();

    SHELLEXECUTEINFOW shellInfo{};
    shellInfo.cbSize = sizeof(shellInfo);
    shellInfo.fMask = SEE_MASK_NOCLOSEPROCESS;
    shellInfo.lpVerb = L"open";
    shellInfo.lpFile = targetExe.c_str();
    shellInfo.lpDirectory = appDirectory.c_str();
    shellInfo.nShow = showCommand;

    if (ShellExecuteExW(&shellInfo))
    {
        if (shellInfo.hProcess) CloseHandle(shellInfo.hProcess);
        return 0;
    }

    ShowError(L"CreateProcess error " + std::to_wstring(createError) +
              L"\nShellExecute error " + std::to_wstring(GetLastError()) +
              L"\n" + targetExe);
    return 1;
}
