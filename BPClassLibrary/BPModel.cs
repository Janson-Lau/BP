using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPClassLibrary
{
    public class BPModel
    {
        public double[,] X;

        public double[,] V;

        public double[,] U;

        public double[,] W;

        public double[,] Y;

        public BPModel(double[,] x, int outPutCount = 10, int u1Count = 16 /*params int[] uCounts*/)
        {
            Random random = new Random();

            X = x;
            int inputCount = x.GetLength(0);
            V = new double[u1Count, inputCount];
            for (int i = 0; i < u1Count; i++)
            {
                for (int j = 0; j < inputCount; j++)
                {
                    var r = random.NextDouble();
                    var n = (2 * r - 1) / Math.Sqrt(inputCount);
                    V[i, j] = n;
                }
            }

            U = new double[u1Count, 1];

            W = new double[outPutCount, u1Count];
            for (int i = 0; i < outPutCount; i++)
            {
                for (int j = 0; j < u1Count; j++)
                {
                    var r = random.NextDouble();
                    var n = (2 * r - 1) / Math.Sqrt(u1Count);
                    W[i, j] = n;
                }
            }
            Y = new double[outPutCount, 1];
        }

        public void ComputeU()
        {
            U = BPMath.MultiplyMatrices(V, X, null);
        }

        public void ComputeY()
        {
            Y = BPMath.MultiplyMatrices(W, U, null);
        }
    }
}