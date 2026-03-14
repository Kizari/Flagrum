#include "NativeApplication.h"

#include <execinfo.h>
#include <csignal>
#include <unistd.h>

/**
 * Handles a native crash by dumping the stack trace then exiting.
 * 
 * @param signal Crash signal.
 */
void HandleCrash(int signal)
{
    void* array[50];

    const auto size = backtrace(array, 50);
    backtrace_symbols_fd(array, size, STDERR_FILENO);
    _exit(1);
}

NativeApplication::NativeApplication()
{
    // Install crash handlers
    signal(SIGSEGV, HandleCrash);
    signal(SIGABRT, HandleCrash);
    signal(SIGBUS, HandleCrash);
    signal(SIGILL, HandleCrash);

    // Create the QApplication
    argc_ = 1;
    arg0_ = strdup("Flagrum");
    argv_ = new char*[2]{arg0_, nullptr};
    application_ = new QApplication(argc_, argv_);
}

NativeApplication::~NativeApplication()
{
    delete application_;
    free(arg0_);
    delete[] argv_;
}

int NativeApplication::Run()
{
    return QApplication::exec();
}
