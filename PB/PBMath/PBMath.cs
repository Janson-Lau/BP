using System;

namespace PB.PBMath
{
    public class PBMath
    {
        public double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        public double Tanh(double x)
        {
            return (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
        }

        public double ReLU(double x)
        {
            if (x > 0)
            {
                return x;
            }
            else
            {
                return 0;
            }
        }

        public double PureLin(double x)
        {
            return x;
        }

        public static double[,] MultiplyMatrices(double[,] matrix1, double[,] matrix2, Func<double, double> func)
        {
            int rows1 = matrix1.GetLength(0);
            int cols1 = matrix1.GetLength(1);
            int rows2 = matrix2.GetLength(0);
            int cols2 = matrix2.GetLength(1);

            // 检查矩阵是否可以相乘  
            if (cols1 != rows2)
            {
                throw new ArgumentException("Matrices cannot be multiplied.");
            }

            double[,] result = new double[rows1, cols2];

            for (int i = 0; i < rows1; i++)//0-2
            {
                for (int j = 0; j < cols2; j++)//0-2
                {
                    for (int k = 0; k < cols1; k++)//0-2
                    {
                        // i = 0; j = 0; k = 0-2;
                        //[0,0] [0,0]
                        //[0,1] [1,0]
                        //[0,2] [2,0]

                        // i = 0; j = 1; k = 0-2;
                        //[0,0] [0,1]
                        //[0,1] [1,1]
                        //[0,2] [2,1]

                        // i = 0; j = 2; k = 0-2;
                        //[0,0] [0,2]
                        //[0,1] [1,2]
                        //[0,2] [2,2]
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                    if (func != null)
                    {
                        result[i, j] = func(result[i, j]);
                    }
                }
            }

            return result;
        }
        
    }
}
