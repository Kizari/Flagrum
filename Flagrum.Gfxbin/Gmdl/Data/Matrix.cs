using System;

namespace Flagrum.Gfxbin.Gmdl.Data
{
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

        public Vector3[] Rows { get; }
    }
}