#include "NativeWindow.h"

NativeWindow::NativeWindow() :
    window_(new CustomShellWindow())
{
}

NativeWindow::~NativeWindow()
{
    delete window_;
}

QWidget* NativeWindow::GetWidget() const
{
    return window_;
}

void NativeWindow::SetTitle(const char* title) const
{
    window_->setWindowTitle(title);
}

void NativeWindow::Resize(const int width, const int height) const
{
    window_->resize(width, height);
}

void NativeWindow::SetWebView(const NativeWebView* webView) const
{
    window_->SetContentWidget(webView->GetWidget());
}

void NativeWindow::Show() const
{
    window_->show();
}
