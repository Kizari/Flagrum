### Notes on the Luminous implementation of MessagePack

The format enumeration and the way these are encoded are identical to MessagePack.

However, the key differences are:

1. Luminous uses Little Endian, where MessagePack uses Big Endian
2. Luminous doesn't encapsulate the file in an array the way MessagePack does

The types in this directory cater to these differences and can be referred to for further explanation.