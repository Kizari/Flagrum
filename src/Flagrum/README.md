# Flagrum

This is the Flagrum application. It is responsible for native platform functionality, such as windowing and
file system I/O. Essentially, it initializes the application and opens the main window, which is simply a
custom shell with a web view inside that hosts the Blazor application from `Flagrum.Application`.