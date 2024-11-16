namespace BPClassLibrary
{
    public static class BPMath
    {
        public static double Sigmoid(double x)
        {
            var result = 1 / (1 + Math.Exp(-x));
            return result;
        }

        public static void Sigmoid(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = Sigmoid(matrix[i, j]);
                }
            }
        }

        public static double Tanh(double x)
        {
            return (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
        }

        public static double ReLU(double x)
        {
            return Math.Max(0, x);
        }

        public static double PureLin(double x)
        {
            return x;
        }

        public static double[,] MultiplyMatrices(double[,] matrixA, double[,] matrixB, Func<double, double> func)
        {
            int rowsA = matrixA.GetLength(0);
            int colsA = matrixA.GetLength(1);
            int rowsB = matrixB.GetLength(0);
            int colsB = matrixB.GetLength(1);

            // 检查矩阵是否可以相乘
            if (colsA != rowsB)
            {
                throw new ArgumentException("Matrices cannot be multiplied.");
            }

            double[,] result = new double[rowsA, colsB];

            for (int i = 0; i < rowsA; i++)//0-2
            {
                for (int j = 0; j < colsB; j++)//0-2
                {
                    for (int k = 0; k < colsA; k++)//0-2
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
                        result[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                    if (func != null)
                    {
                        result[i, j] = func(result[i, j]);
                    }
                }
            }

            return result;
        }

        public static double[,] AddMatrices(double[,] matrixA, double[,] matrixB)
        {
            int rowsA = matrixA.GetLength(0);
            int colsA = matrixA.GetLength(1);
            int rowsB = matrixB.GetLength(0);
            int colsB = matrixB.GetLength(1);

            // 检查矩阵的维度是否匹配
            if (rowsA != rowsB || colsA != colsB)
            {
                throw new ArgumentException("两个矩阵的维度必须相同！");
            }

            double[,] resultMatrix = new double[rowsA, colsA];
            // 进行矩阵相加
            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsA; j++)
                {
                    resultMatrix[i, j] = matrixA[i, j] + matrixB[i, j];
                }
            }
            return resultMatrix;
        }

        public static void OperateMatrix(double[,] matrix, Func<double, double> func)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (func != null)
                    {
                        matrix[i, j] = func(matrix[i, j]);
                    }
                }
            }
        }

        public static double[] ToArray(this double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            List<double> result = new List<double>();
            // 进行矩阵相加
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result.Add(matrix[i, j]);
                }
            }
            return result.ToArray();
        }

        public static int[] ToArray(this int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            List<int> result = new List<int>();
            // 进行矩阵相加
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result.Add(matrix[i, j]);
                }
            }
            return result.ToArray();
        }
    }
}