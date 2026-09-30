set(CMAKE_AUTOMOC ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets WebEngineWidgets SvgWidgets)

add_library(Flagrum.Native SHARED
        Library.cpp
        Components/ApplicationHost.hpp
        Components/JavaScriptBridge.hpp
        Components/MainWebView.hpp
        Components/MainWindow.hpp
        Components/SplashScreen.hpp
        Components/LocalizationService.hpp
)

qt_add_resources(Flagrum.Native "resources"
        PREFIX "/"
        FILES
        Resources/flagrum.ico
        Resources/interop.js
        Resources/logo.png
        Resources/logo.svg
        Resources/patreon.png
        Resources/Play-Regular.ttf
)

target_link_libraries(Flagrum.Native
        Qt6::Widgets
        Qt6::WebEngineWidgets
        Qt6::SvgWidgets
)