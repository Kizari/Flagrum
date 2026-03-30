#include "Components/ApplicationHost.hpp"

#define EXPORT extern "C"

using Action = void(*)();

EXPORT ApplicationHost* ApplicationHost_Create()
{
    return new ApplicationHost();
}

EXPORT void ApplicationHost_Destroy(const ApplicationHost* instance)
{
    delete instance;
}

EXPORT int ApplicationHost_Run(const ApplicationHost* instance)
{
    return instance->Run();
}

EXPORT void ApplicationHost_Restart(const ApplicationHost* instance, const int exitCode, const char* executablePath)
{
    instance->Restart(exitCode, executablePath);
}

EXPORT void ApplicationHost_Exit(const ApplicationHost* instance, const int exitCode)
{
    instance->Exit(exitCode);
}

EXPORT void ApplicationHost_Invoke(ApplicationHost* instance, Action action)
{
    instance->Invoke(action);
}

EXPORT void ApplicationHost_OpenSplash(ApplicationHost* instance)
{
    instance->OpenSplash();
}

EXPORT void ApplicationHost_CloseSplash(ApplicationHost* instance)
{
    instance->CloseSplash();
}

EXPORT void ApplicationHost_SetSplashText(ApplicationHost* instance, const char* text)
{
    instance->SetSplashText(text);
}

EXPORT void ApplicationHost_OpenMainWindow(ApplicationHost* instance)
{
    instance->OpenMainWindow();
}

EXPORT void ApplicationHost_CloseMainWindow(ApplicationHost* instance)
{
    instance->CloseMainWindow();
}

EXPORT void ApplicationHost_SetPatreonButtonVisible(ApplicationHost* instance, const bool isVisible)
{
    instance->SetPatreonButtonVisible(isVisible);
}

EXPORT void ApplicationHost_SetPatreonButtonCallback(const ApplicationHost* instance, void(*callback)())
{
    instance->SetPatreonButtonCallback(callback);
}

EXPORT void ApplicationHost_SetWebMessageHandler(
    const ApplicationHost* instance,
    const WebMessageReceivedCallback callback)
{
    instance->SetWebMessageHandler(callback);
}

EXPORT void ApplicationHost_NavigateWebView(ApplicationHost* instance, const char* url)
{
    instance->NavigateWebView(url);
}

EXPORT void ApplicationHost_RunJavaScript(ApplicationHost* instance,  const char* script)
{
    instance->RunJavaScript(script);
}

EXPORT void ApplicationHost_SetClipboardText(ApplicationHost* instance, const char* text)
{
    instance->SetClipboardText(text);
}

EXPORT void ApplicationHost_ShowMessageBox(
    ApplicationHost* instance,
    const char* title,
    const char* message,
    const MessageBoxType type)
{
    instance->ShowMessageBox(title, message, type);
}

EXPORT void ApplicationHost_OpenFile(
    ApplicationHost* instance,
    const char* caption,
    const char* directory,
    const char* filter,
    char* out)
{
    instance->OpenFile(caption, directory, filter, out);
}

EXPORT void ApplicationHost_SaveFile(
    ApplicationHost* instance,
    const char* caption,
    const char* directory,
    const char* filter,
    char* out)
{
    instance->SaveFile(caption, directory, filter, out);
}

EXPORT void ApplicationHost_OpenDirectory(
    ApplicationHost* instance,
    const char* caption,
    const char* directory,
    char* out)
{
    instance->OpenDirectory(caption, directory, out);
}