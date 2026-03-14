#include "NativeWebView.h"

#include <QFile>
#include <QShortcut>
#include <QWebChannel>
#include <QWebEngineProfile>
#include <QWebEngineScriptCollection>
#include <QWebEngineUrlScheme>

#include "NativeApplication.h"

NativeWebView::NativeWebView(
    QWidget* window,
    const WebMessageReceivedCallback onMessageReceived,
    const WebResourceRequestedCallback onResourceRequested) :
    devToolsWindow_(nullptr)
{
    // Create Qt components
    webView_ = new QWebEngineView(window);
    webView_->setStyleSheet("background: #181512;");
    page_ = webView_->page();
    profile_ = new QWebEngineProfile(webView_);
    channel_ = new QWebChannel(page_);
    bridge_ = new NativeWebViewBridge(onMessageReceived, channel_);

    // Set up the JS bridge
    channel_->registerObject("native", bridge_);
    page_->setWebChannel(channel_);

    // Hook up F12 to open the dev tools window
    // ReSharper disable once CppDFAMemoryLeak (Qt will free when webView_ is destroyed)
    const auto shortcut = new QShortcut(QKeySequence(Qt::Key_F12), webView_);
    QObject::connect(shortcut, &QShortcut::activated, [&]
    {
        OpenDevTools();
    });

    // Inject necessary scripts into the web view
    InjectScript("qwebchannel", ":/qtwebchannel/qwebchannel.js");
    InjectScript("interop", ":/Resources/interop.js");
}

NativeWebView::~NativeWebView()
{
    delete webView_;
    delete devToolsWindow_;
}

QWidget* NativeWebView::GetWidget() const
{
    return webView_;
}

void NativeWebView::Navigate(const char* url) const
{
    webView_->setUrl(QUrl(url));
}

void NativeWebView::RunJavaScript(const char* script) const
{
    page_->runJavaScript(QString::fromUtf8(script));
}

void NativeWebView::InjectScript(const char* name, const char* resourcePath) const
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
    page_->scripts().insert(script);
}


void NativeWebView::OpenDevTools()
{
    // Create the dev tools window if it does not currently exist
    if (!devToolsWindow_)
    {
        devToolsWindow_ = new NativeWebViewDevToolsWindow();
        devToolsWindow_->SetInspectedPage(page_);
    }

    // Show and focus the dev tools window
    devToolsWindow_->show();
    devToolsWindow_->raise();
    devToolsWindow_->activateWindow();
}
