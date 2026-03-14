#pragma once

#include "NativeWebViewBridge.hpp"
#include "NativeWebViewDevToolsWindow.hpp"

/**
 * Convenience wrapper for QWebEngineView.
 */
class NativeWebView
{
private:
    QWebEngineView* webView_;
    QWebChannel* channel_;
    QWebEngineProfile* profile_;
    QWebEnginePage* page_;
    NativeWebViewBridge* bridge_;
    NativeWebViewDevToolsWindow* devToolsWindow_;

public:
    NativeWebView(
        QWidget* window,
        WebMessageReceivedCallback onMessageReceived,
        WebResourceRequestedCallback onResourceRequested);
    
    ~NativeWebView();

    /**
     * Gets the underlying QWebEngineView widget.
     */
    QWidget* GetWidget() const;

    /**
     * Navigates the web view to the target URL.
     * 
     * @param url URL to navigate to.
     */
    void Navigate(const char* url) const;

    /**
     * Evaluates a JavaScript string in the web view.
     * 
     * @param script Script to evaluate.
     */
    void RunJavaScript(const char* script) const;

private:
    /**
     * Injects an embedded script into the web view.
     * 
     * @param name Name to assign to the imported script.
     * @param resourcePath Qt resource path of the embedded script.
     */
    void InjectScript(const char* name, const char* resourcePath) const;

    /**
     * Opens the Chromium developer tools window.
     */
    void OpenDevTools();
};
