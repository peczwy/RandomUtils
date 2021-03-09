using System;
using System.Collections.Generic;
using System.Text;

namespace ShowcaseProxy.Core.Impl
{
    public class MemoryProxy<T> : ICalculation<T>
    {
        public ICalculation<T> Proxied { get; }

        private IDictionary<object, T> Cache { get; set;}

        /*
         * Characteristic constructor
         */
        public MemoryProxy(ICalculation<T> proxied)
        {
            Proxied = proxied;
            Cache = new Dictionary<object, T>();
        }

        /*
         * Method uses the object passed in the constructor
         */
        public T Calculate(dynamic x)
        {
            if (!Cache.Keys.Contains(x))
            {
                Cache[x] = Proxied.Calculate(x); ;
            }
            return Cache[x];
        }
    }
}
