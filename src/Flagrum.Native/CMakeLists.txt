set(CMAKE_AUTOMOC ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets WebEngineWidgets SvgWidgets)

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
        Exports/NativeMessageBox.cpp
        Exports/NativeFileDialog.cpp
        Api/NativeSplashWindow.hpp
        Exports/NativeSplashWindow.cpp
)

qt_add_resources(Flagrum.Native "resources"
        PREFIX "/"
        FILES
        Resources/flagrum.ico
        Resources/logo.svg
        Resources/interop.js
        Resources/Play-Regular.ttf
)

target_link_libraries(Flagrum.Native
        Qt6::Widgets
        Qt6::WebEngineWidgets
        Qt6::SvgWidgets
)