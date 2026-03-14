set(CMAKE_AUTOMOC ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets WebEngineWidgets Svg)

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
        Components/CustomShellWindow.hpp
)

qt_add_resources(Flagrum.Native "resources"
        PREFIX "/"
        FILES
            Resources/interop.js
            Resources/logo_white.svg            
)

target_link_libraries(Flagrum.Native
        Qt6::Widgets
        Qt6::WebEngineWidgets
        Qt6::Svg
)