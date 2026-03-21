// ReSharper disable CppDFAMemoryLeak (Qt widgets handle disposing their children)
#pragma once

#include <QFile>
#include <QShortcut>
#include <QVBoxLayout>
#include <QWebChannel>
#include <QWebEngineScriptCollection>
#include <QWebEngineView>

#include "JavaScriptBridge.hpp"

/**
 * Web view that hosts the Blazor application in the main application window.
 */
class MainWebView final : public QWebEngineView
{
private:
    QWidget* devToolsWindow_ = nullptr;
    JavaScriptBridge* javaScriptBridge_ = nullptr;
    
public:
    /**
     * Creates, styles, and initializes the web view.
     * 
     * @param parent Widget to own this web view.
     */
    explicit MainWebView(QWidget* parent = nullptr) : QWebEngineView(parent)
    {
        setStyleSheet("background: #181512;");
        InitializeJavaScriptBridge();
        EnableDevTools();
        InjectScript("qwebchannel", ":/qtwebchannel/qwebchannel.js");
        InjectScript("interop", ":/Resources/interop.js");
    }

    /**
     * Cleans up native resources.
     */
    ~MainWebView() override
    {
        if (devToolsWindow_)
        {
            delete devToolsWindow_;
            devToolsWindow_ = nullptr;
        }
    }

    /**
     * Sets the function that handles messages posted back here by the web view's JS.
     * 
     * @param callback Function that handles the web message.
     */
    void SetWebMessageHandler(const WebMessageReceivedCallback callback) const
    {
        javaScriptBridge_->SetWebMessageHandler(callback);
    }

protected:
    /**
     * Disables the Chromium context menu.
     * 
     * @param event Event that triggered the context menu.
     */
    void contextMenuEvent(QContextMenuEvent* event) override
    {
        event->ignore();
    }
    
private:
    /**
     * Hooks up the JS-to-native bridge.
     */
    void InitializeJavaScriptBridge()
    {
        const auto channel = new QWebChannel(page());
        javaScriptBridge_ = new JavaScriptBridge(channel);
        channel->registerObject("native", javaScriptBridge_);
        page()->setWebChannel(channel);
    }
    
    /**
     * Enables opening the dev tools window by pressing F12.
     */
    void EnableDevTools()
    {
        const auto shortcut = new QShortcut(QKeySequence(Qt::Key_F12), this);
        connect(shortcut, &QShortcut::activated, [&]
        {
            OpenDevTools();
        });
    }
    
    /**
     * Injects an embedded script into the web view.
     * 
     * @param name Name to assign to the imported script.
     * @param resourcePath Qt resource path of the embedded script.
     */
    void InjectScript(const char* name, const char* resourcePath) const
    {
        // Open the script file
        auto file = QFile(resourcePath);
        if (!file.open(QIODeviceBase::ReadOnly))
        {
            throw std::runtime_error(std::format("Could not open script file '{}'", resourcePath));
        }

        // Read the script into a string
        const auto sourceCode = QString::fromUtf8(file.readAll());
        file.close();

        // Inject the script
        auto script = QWebEngineScript{};
        script.setName(name);
        script.setInjectionPoint(QWebEngineScript::DocumentCreation);
        script.setRunsOnSubFrames(true);
        script.setWorldId(QWebEngineScript::MainWorld);
        script.setSourceCode(sourceCode);
        page()->scripts().insert(script);
    }

    /**
     * Opens the Chromium developer tools window.
     */
    void OpenDevTools()
    {
        // Create the dev tools window if it does not currently exist
        if (!devToolsWindow_)
        {
            // Create window
            devToolsWindow_ = new QWidget();
            devToolsWindow_->setWindowTitle("Developer Tools");
            devToolsWindow_->resize(900, 700);
            const auto webView = new QWebEngineView(devToolsWindow_);

            // Create layout
            const auto layout = new QVBoxLayout(devToolsWindow_);
            layout->setContentsMargins(0, 0, 0, 0);
            layout->addWidget(webView);

            // Link dev tools to the main web view
            webView->page()->setInspectedPage(page());
        }

        // Show and focus the dev tools window
        devToolsWindow_->show();
        devToolsWindow_->raise();
        devToolsWindow_->activateWindow();
    }
};
