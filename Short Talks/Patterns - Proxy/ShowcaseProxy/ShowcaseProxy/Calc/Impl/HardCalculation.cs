using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShowcaseProxy.Core.Impl
{
    public class HardCalculation : ICalculation<double>
    {
        const int DELAY = 2;

        public double Calculate(dynamic x)
        {
            Thread.Sleep(TimeSpan.FromSeconds(DELAY));
            return x + x;
        }
    }
}
