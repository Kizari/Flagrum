set(CMAKE_AUTOMOC ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets WebEngineWidgets)

add_library(Flagrum.Native SHARED
        Api/NativeApplication.cpp
        Api/NativeWindow.cpp
        Api/NativeWebView.cpp
        Exports/NativeApplication.cpp
        Exports/NativeWindow.cpp
        Exports/NativeWebView.cpp
        Api/NativeWebViewBridge.hpp
        Api/NativeWebViewDevToolsWindow.hpp
        Api/NativeDispatcher.hpp
        Exports/NativeDispatcher.cpp
)

qt_add_resources(Flagrum.Native "resources"
        PREFIX "/scripts"
        FILES interop.js)

target_link_libraries(Flagrum.Native
        Qt6::Widgets
        Qt6::WebEngineWidgets
)