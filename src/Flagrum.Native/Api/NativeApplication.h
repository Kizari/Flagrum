#pragma once

#include <QApplication>

/**
 * Convenience wrapper for QApplication.
 */
class NativeApplication
{
private:
    int argc_;
    char* arg0_;
    char** argv_;
    QApplication* application_;

public:
    NativeApplication();
    ~NativeApplication();

    /**
     * Runs the application.
     * 
     * @return Exit code.
     * @remarks This function blocks until the user quits the application.
     */
    [[nodiscard]] static int Run();
};