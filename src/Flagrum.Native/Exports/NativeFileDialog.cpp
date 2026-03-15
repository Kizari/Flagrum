#include <QFileDialog>

#include "../Api/NativeWindow.h"

#define EXPORT extern "C"

EXPORT void NativeFileDialog_OpenFile(
    const NativeWindow* window,
    const char* caption,
    const char* initialDirectory,
    const char* filter,
    char* out)
{
    const auto result = QFileDialog::getOpenFileName(
        window->GetWidget(), caption, initialDirectory, filter);
    const auto utf8 = result.toUtf8();
    memcpy(out, utf8.constData(), utf8.size());
    out[utf8.size()] = '\0';
}

EXPORT void NativeFileDialog_OpenDirectory(
    const NativeWindow* window,
    const char* caption,
    const char* initialDirectory,
    char* out)
{
    const auto result = QFileDialog::getExistingDirectory(
        window->GetWidget(), caption, initialDirectory);
    const auto utf8 = result.toUtf8();
    memcpy(out, utf8.constData(), utf8.size());
    out[utf8.size()] = '\0';
}

EXPORT void NativeFileDialog_SaveFile(
    const NativeWindow* window,
    const char* caption,
    const char* initialDirectory,
    const char* filter,
    char* out)
{
    const auto result = QFileDialog::getSaveFileName(
        window->GetWidget(), caption, initialDirectory, filter);
    const auto utf8 = result.toUtf8();
    memcpy(out, utf8.constData(), utf8.size());
    out[utf8.size()] = '\0';
}