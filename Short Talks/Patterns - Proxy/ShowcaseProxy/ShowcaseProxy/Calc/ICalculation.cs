using System;
using System.Collections.Generic;
using System.Text;

namespace ShowcaseProxy.Core
{
    public interface ICalculation<T>
    {

        T Calculate(dynamic x); 

    }
}
