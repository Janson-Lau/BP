using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
