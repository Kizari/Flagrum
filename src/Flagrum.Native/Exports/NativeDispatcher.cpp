#include <QCoreApplication>
#include <QDebug>

#include "../Api/NativeDispatcher.hpp"

#define EXPORT extern "C"

using Action = void(*)();

EXPORT NativeDispatcher* NativeDispatcher_Create()
{
    return new NativeDispatcher();
}

EXPORT void NativeDispatcher_Destroy(const NativeDispatcher* instance)
{
    delete instance;
}

EXPORT void NativeDispatcher_Invoke(NativeDispatcher* instance, Action callback)
{
    QMetaObject::invokeMethod(
        instance,
        [callback]
        {
            callback();
        },
        Qt::QueuedConnection);
}