using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Magician
{
    public interface Map
    {
        public abstract double Evaluate(params double[] x);
    }
}