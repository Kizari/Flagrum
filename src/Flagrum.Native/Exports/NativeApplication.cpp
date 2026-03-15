#include <QClipboard>

#include "../Api/NativeApplication.h"

#define EXPORT extern "C"

EXPORT NativeApplication* NativeApplication_Create()
{
    return new NativeApplication();
}

EXPORT void NativeApplication_Destroy(const NativeApplication* instance)
{
    delete instance;
}

EXPORT int NativeApplication_Run()
{
    return QApplication::exec();
}

EXPORT void NativeApplication_Exit(const int exitCode)
{
    QApplication::exit(exitCode);
}

EXPORT void NativeApplication_SetClipboardText(const char* text)
{
    QApplication::clipboard()->setText(text);
}