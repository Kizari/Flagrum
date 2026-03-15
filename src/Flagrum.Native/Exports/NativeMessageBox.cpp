#include <QMessageBox>

#include "../Api/NativeWindow.h"

#define EXPORT extern "C"

EXPORT void NativeMessageBox_Information(const NativeWindow* window, const char* title, const char* message)
{
    const auto parent = window ? window->GetWidget() : nullptr;
    QMessageBox::information(parent, title, message);
}

EXPORT void NativeMessageBox_Warning(const NativeWindow* window, const char* title, const char* message)
{
    const auto parent = window ? window->GetWidget() : nullptr;
    QMessageBox::warning(parent, title, message);
}

EXPORT void NativeMessageBox_Critical(const NativeWindow* window, const char* title, const char* message)
{
    const auto parent = window ? window->GetWidget() : nullptr;
    QMessageBox::critical(parent, title, message);
}