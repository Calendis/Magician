namespace Magician.Geo;

using System;
using Core.Maps;
using Magician.Core;

public class Sampling : Relational
{
    public Sampling(Func<double[], IVal> f) : base(f, 2)
    {
    }
    public static readonly Sampling Spiral = new(xs => 
    {
        double theta = 2*Math.PI*xs[0];
        double radius = xs[1];
        return IVal.Multiply(IVal.ExpI(new Val(theta)), radius);
    });
}

