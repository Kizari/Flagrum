#include "../Api/NativeWindow.h"

#define EXPORT extern "C"

EXPORT NativeWebView* NativeWebView_Create(
    const NativeWindow* window,
    const WebMessageReceivedCallback onMessageReceived,
    const WebResourceRequestedCallback onResourceRequested)
{
    return new NativeWebView(window->GetWidget(), onMessageReceived, onResourceRequested);
}

EXPORT void NativeWebView_Destroy(const NativeWebView* instance)
{
    delete instance;
}

EXPORT void NativeWebView_Navigate(const NativeWebView* instance, const char* url)
{
    instance->Navigate(url);
}

EXPORT void NativeWebView_RunJavaScript(const NativeWebView* instance, const char* script)
{
    instance->RunJavaScript(script);
}