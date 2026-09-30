#pragma once

#include <QObject>

using WebMessageReceivedCallback = void(*)(const char*);

/**
 * Handles receiving messages from the web view.
 */
class JavaScriptBridge final : public QObject
{
    Q_OBJECT

private:
    WebMessageReceivedCallback messageHandler_ = nullptr;

public:
    explicit JavaScriptBridge(QObject* parent = nullptr) : QObject(parent) {}

    /**
     * Sets the function that handles messages posted back here by the web view's JS.
     * 
     * @param callback Function that handles the web message.
     */
    void SetWebMessageHandler(const WebMessageReceivedCallback callback)
    {
        messageHandler_ = callback;
    }

public slots:
    /**
     * Function that the web view's JS can call to post a message back to native code.
     * 
     * @param message Message to post.
     */
    void postMessage(const QString& message) const
    {
        if (messageHandler_)
        {
            messageHandler_(message.toUtf8().constData());
        }
    }
};
