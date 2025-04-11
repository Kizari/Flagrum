using System;
using System.Collections.Generic;

namespace Flagrum.Core.Utilities;

public class BoyerMooreSearch
{
    private readonly int[] _charTable;
    private readonly byte[] _needle;
    private readonly int[] _offsetTable;

    public BoyerMooreSearch(byte[] needle)
    {
        _needle = needle;
        _charTable = MakeByteTable(needle);
        _offsetTable = MakeOffsetTable(needle);
    }

    public IEnumerable<int> Search(byte[] haystack)
    {
        if (_needle.Length == 0)
        {
            yield break;
        }

        for (var i = _needle.Length - 1; i < haystack.Length;)
        {
            int j;

            for (j = _needle.Length - 1; _needle[j] == haystack[i]; --i, --j)
            {
                if (j != 0)
                {
                    continue;
                }

                yield return i;
                i += _needle.Length - 1;
                break;
            }

            i += Math.Max(_offsetTable[_needle.Length - 1 - j], _charTable[haystack[i]]);
        }
    }

    private static int[] MakeByteTable(IReadOnlyList<byte> needle)
    {
        const int ALPHABET_SIZE = 256;
        var table = new int[ALPHABET_SIZE];

        for (var i = 0; i < table.Length; ++i)
        {
            table[i] = needle.Count;
        }

        for (var i = 0; i < needle.Count - 1; ++i)
        {
            table[needle[i]] = needle.Count - 1 - i;
        }

        return table;
    }

    private static int[] MakeOffsetTable(IReadOnlyList<byte> needle)
    {
        var table = new int[needle.Count];
        var lastPrefixPosition = needle.Count;

        for (var i = needle.Count - 1; i >= 0; --i)
        {
            if (IsPrefix(needle, i + 1))
            {
                lastPrefixPosition = i + 1;
            }

            table[needle.Count - 1 - i] = lastPrefixPosition - i + needle.Count - 1;
        }

        for (var i = 0; i < needle.Count - 1; ++i)
        {
            var slen = SuffixLength(needle, i);
            table[slen] = needle.Count - 1 - i + slen;
        }

        return table;
    }

    private static bool IsPrefix(IReadOnlyList<byte> needle, int p)
    {
        for (int i = p, j = 0; i < needle.Count; ++i, ++j)
        {
            if (needle[i] != needle[j])
            {
                return false;
            }
        }

        return true;
    }

    private static int SuffixLength(IReadOnlyList<byte> needle, int p)
    {
        var len = 0;

        for (int i = p, j = needle.Count - 1; i >= 0 && needle[i] == needle[j]; --i, --j)
        {
            ++len;
        }

        return len;
    }
}