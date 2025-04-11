# Format Information

### Xml Binary (.exml)

This particular format often confuses people due to its many names.

**EBEX (Ebony Entity XML)**—This is the name given to the raw XML and is most commonly seen in file URIs.  
**EXML (Ebony XML)**—Strangely, this name is usually the file extension for XMB and XMB2 files.  
**XMB (XML Binary)**—The first version of XMB, only used in the FFXV demos.  
**XMB2 (XML Binary Version 2)**—A binary representation of XML, which is used to store EBEX more efficiently.  
**Prefab**—an alternative URI extension for EBEX files representing prefabs.

Ultimately, what is to be understood about these formats is that the game is scripted using node graphs.
The XML is a representation of these node graphs, and the formats above are just the various ways this
information is referred to across the engine. Interestingly, the game will still read these scripts even if
the file is put back into the game's archives as raw XML, provided the XML is correct and well formed.
This is not advised though as XMB2 is far more optimised.

**NOTE:** All release versions of FFXV and Forspoken use XMB2. The first version XMB was only used in the FFXV demos.

# Luminaire

The code pertaining to XMB2 in this directory comes from Sai's "Luminaire" repository, which unfortunately is no
longer maintained.

More about Luminaire here: https://github.com/youarebritish/Luminaire

---

### Luminaire License Information (Retrieved 2023-04-20)

MIT License

Copyright (c) 2020 youarebritish

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.