using System;
using System.Collections.Generic;

namespace Flagrum.Core.Gfxbin.Gmdl.Constructs;

public class Matrix
{
    public Matrix(params Vector3[] rows)
    {
        if (rows.Length != 4)
        {
            throw new InvalidOperationException($"{nameof(Matrix)} must have 4 rows.");
        }

        Rows = rows;
    }

    public IList<Vector3> Rows { get; }
}