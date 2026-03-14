#include "../Api/NativeApplication.h"

#define EXPORT extern "C"

EXPORT NativeApplication* NativeApplication_Create()
{
    return new NativeApplication();
}

EXPORT void NativeApplication_Destroy(const NativeApplication* instance)
{
    delete instance;
}

EXPORT int NativeApplication_Run()
{
    return NativeApplication::Run();
}
