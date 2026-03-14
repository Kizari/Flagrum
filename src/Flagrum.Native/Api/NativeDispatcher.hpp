#pragma once

#include <QObject>

/**
 * Simple object to use for dispatching callbacks to the Qt UI thread.
 */
class NativeDispatcher final : public QObject
{
    Q_OBJECT

public:
    explicit NativeDispatcher(QObject* parent = nullptr) : QObject(parent) {}
};