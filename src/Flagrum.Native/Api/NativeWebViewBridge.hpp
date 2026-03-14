#pragma once

#include <QObject>

#include "../util.h"

/**
 * Handles receiving messages from the web view.
 */
class NativeWebViewBridge final : public QObject
{
    Q_OBJECT

private:
    WebMessageReceivedCallback onMessage_;

public:
    explicit NativeWebViewBridge(const WebMessageReceivedCallback callback, QObject* parent = nullptr)
        : QObject(parent),
          onMessage_(callback)
    {
    }

public slots:
    /**
     * Function that the web view's JS can call to post a message back to native code.
     * 
     * @param message Message to post.
     */
    void postMessage(const QString& message) const
    {
        onMessage_(message.toUtf8().constData());
    }
};
