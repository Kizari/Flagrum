# Flagrum

This is the Flagrum application. It is responsible for native platform functionality, such as windowing and
file system I/O. Essentially, it initializes the application and opens the main window, which is simply a
custom shell with a web view inside that hosts the Blazor application from `Flagrum.Application`.


### External Dependencies

These live in the `libs` folder in the repository root.

* [`Steamworks.NET`](https://github.com/rlabrecque/Steamworks.NET)
* `steam_api64.dll`—from the same link as above (Download from releases, standalone)

`Steamworks.NET.dll` should be linked to `Flagrum.Application.csproj` as well.