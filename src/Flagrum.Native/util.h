#pragma once

using WebMessageReceivedCallback = void(*)(const char*);
using WebResourceRequestedCallback = char*(*)(const char*, int*, char**);