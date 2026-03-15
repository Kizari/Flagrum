#include "NativeApplication.h"

NativeApplication::NativeApplication()
{
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
