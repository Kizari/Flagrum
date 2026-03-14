#pragma once

#include <QMainWindow>

#include "NativeWebView.h"

/**
 * Convenience wrapper for QMainWindow.
 */
class NativeWindow
{
private:
    QMainWindow* window_;
    
public:
    NativeWindow();
    ~NativeWindow();

    /**
     * Gets the underlying QMainWindow widget.
     */
    QWidget* GetWidget() const;

    /**
     * Sets the window title.
     * 
     * @param title Text to set the window title to.
     */
    void SetTitle(const char* title) const;

    /**
     * Sets the dimensions of the window.
     * 
     * @param width Width of the window, in pixels.
     * @param height Height of the window, in pixels.
     */
    void Resize(int width, int height) const;

    /**
     * Sets the content of this window to a web view.
     * 
     * @param webView Web view to set as the content of the window.
     */
    void SetWebView(const NativeWebView* webView) const;

    /**
     * Shows this window.
     */
    void Show() const;
};
