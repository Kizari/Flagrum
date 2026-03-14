#include "../Api/NativeWindow.h"

#define EXPORT extern "C"

EXPORT NativeWindow* NativeWindow_Create()
{
    return new NativeWindow();
}

EXPORT void NativeWindow_Destroy(const NativeWindow* instance)
{
    delete instance;
}

EXPORT void NativeWindow_SetTitle(const NativeWindow* instance, const char* title)
{
    instance->SetTitle(title);
}

EXPORT void NativeWindow_Resize(const NativeWindow* instance, const int width, const int height)
{
    instance->Resize(width, height);
}

EXPORT void NativeWindow_SetWebView(const NativeWindow* instance, const NativeWebView* webView)
{
    instance->SetWebView(webView);
}

EXPORT void NativeWindow_Show(const NativeWindow* instance)
{
    instance->Show();
}