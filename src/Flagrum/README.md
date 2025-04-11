# Flagrum.Desktop

This project is simply an empty Window for housing the Flagrum.Application application and contains logic for bridging any
necessary OS gaps and including application dependencies for the installer.

### External Dependencies

These reside in a folder named "Dependencies" in the Solution Directory. The following files must be present:

* MicrosoftEdgeWebview2Setup.exe (Evergreen Bootstrapper)
    - https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section
* steam_api64.dll (Download from releases, standalone) - https://github.com/rlabrecque/Steamworks.NET
* VCRedist - Copy all dlls from all directories under C:\Program Files (x86)\Microsoft Visual Studio\\[YEAR]\\[TYPE]
  \VC\Redist\MSVC\[VERSION]\x64 (requires C++)

Steamworks.NET.dll from the above standalone release should also be placed in the Solution Directory and linked to
Flagrum.Application to enable the use of Steam Workshop in Flagrum.