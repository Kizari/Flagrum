#include "../Api/NativeSplashWindow.hpp"

#define EXPORT extern "C"

EXPORT NativeSplashWindow* NativeSplashWindow_Create()
{
    return new NativeSplashWindow();
}

EXPORT void NativeSplashWindow_Destroy(const NativeSplashWindow* instance)
{
    delete instance;
}

EXPORT void NativeSplashWindow_Show(const NativeSplashWindow* instance)
{
    instance->Show();
}

EXPORT void NativeSplashWindow_Close(const NativeSplashWindow* instance)
{
    instance->Close();
}

EXPORT void NativeSplashWindow_SetLoadingText(const NativeSplashWindow* instance, const char* text)
{
    instance->SetLoadingText(text);
}