#pragma once

#include <QWidget>
#include <QWebEngineProfile>
#include <QWebEngineView>
#include <QVBoxLayout>

/**
 * Window to host the Chromium developer tools for the web view.
 */
class NativeWebViewDevToolsWindow final : public QWidget
{
private:
    QWebEngineView* webView_;

public:
    explicit NativeWebViewDevToolsWindow(QWidget* parent = nullptr)
        : QWidget(parent)
    {
        // Set window properties
        setWindowTitle("Developer Tools");
        resize(900, 700);

        // Add the web view to the window in a layout that occupies the entire window with no margins
        webView_ = new QWebEngineView(this);
        // ReSharper disable once CppDFAMemoryLeak (Qt parenting handles disposal)
        const auto layout = new QVBoxLayout(this);
        layout->setContentsMargins(0, 0, 0, 0);
        layout->addWidget(webView_);
        setLayout(layout);
    }

    /**
     * Links the developer tools to the page that is to be inspected.
     * 
     * @param page Page to debug.
     */
    void SetInspectedPage(QWebEnginePage* page) const
    {
        webView_->page()->setInspectedPage(page);
    }
};
