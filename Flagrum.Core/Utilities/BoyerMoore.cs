using System;
using System.Collections.Generic;

namespace Flagrum.Core.Utilities;

public class BoyerMoore
{
    private readonly int[] charTable;
    private readonly byte[] needle;
    private readonly int[] offsetTable;

    public BoyerMoore(byte[] needle)
    {
        this.needle = needle;
        charTable = makeByteTable(needle);
        offsetTable = makeOffsetTable(needle);
    }

    public IEnumerable<int> Search(byte[] haystack)
    {
        if (needle.Length == 0)
        {
            yield break;
        }

        for (var i = needle.Length - 1; i < haystack.Length;)
        {
            int j;

            for (j = needle.Length - 1; needle[j] == haystack[i]; --i, --j)
            {
                if (j != 0)
                {
                    continue;
                }

                yield return i;
                i += needle.Length - 1;
                break;
            }

            i += Math.Max(offsetTable[needle.Length - 1 - j], charTable[haystack[i]]);
        }
    }

    private static int[] makeByteTable(byte[] needle)
    {
        const int ALPHABET_SIZE = 256;
        var table = new int[ALPHABET_SIZE];

        for (var i = 0; i < table.Length; ++i)
        {
            table[i] = needle.Length;
        }

        for (var i = 0; i < needle.Length - 1; ++i)
        {
            table[needle[i]] = needle.Length - 1 - i;
        }

        return table;
    }

    private static int[] makeOffsetTable(byte[] needle)
    {
        var table = new int[needle.Length];
        var lastPrefixPosition = needle.Length;

        for (var i = needle.Length - 1; i >= 0; --i)
        {
            if (isPrefix(needle, i + 1))
            {
                lastPrefixPosition = i + 1;
            }

            table[needle.Length - 1 - i] = lastPrefixPosition - i + needle.Length - 1;
        }

        for (var i = 0; i < needle.Length - 1; ++i)
        {
            var slen = suffixLength(needle, i);
            table[slen] = needle.Length - 1 - i + slen;
        }

        return table;
    }

    private static bool isPrefix(byte[] needle, int p)
    {
        for (int i = p, j = 0; i < needle.Length; ++i, ++j)
        {
            if (needle[i] != needle[j])
            {
                return false;
            }
        }

        return true;
    }

    private static int suffixLength(byte[] needle, int p)
    {
        var len = 0;

        for (int i = p, j = needle.Length - 1; i >= 0 && needle[i] == needle[j]; --i, --j)
        {
            ++len;
        }

        return len;
    }
}