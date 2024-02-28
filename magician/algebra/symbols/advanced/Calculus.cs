namespace Magician.Alg.Symbols;

using System.Collections.Generic;

public class Derivative : Oper
{
    Variable axis;
    // TODO: make an interface for unary operators like Derivative and Abs
    // ... well, Derivative might be binary because of the axis
    public Oper Argument => posArgs[0];
    public Derivative(Oper o, Variable v) : base("derivative", o.Trim())
    {
        if (Argument is not IDifferentiable)
            throw Scribe.Error($"{Argument.Name} {Argument} was not differentiable");
        axis = v;
        trivialAssociative = false;
    }
    // TODO: support differentiation with respect to an arbitrary Oper
    //public Derivative(Oper o, Oper v) : base("derivative", o)
    //{
    //    axis = v;
    //}

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new Derivative(pa.ToList()[0], axis);
    }

    public override void ReduceOuter()
    {
        //
    }

    public override Oper Simplified()
    {
        if (IsDetermined)
            return new Variable(0);
        if (Argument is Variable v)
        {
            if (v == axis)
                return new Variable(1);
            return new Variable(0);
        }
        
        if (Argument is SumDiff)
        {
            return new SumDiff(Argument.posArgs.Select(a => new Derivative(a.Copy(), axis)).ToList(), Argument.negArgs.Select(a => new Derivative(a.Copy(), axis)).ToList());
        }

        if (Argument is Fraction frac)
        {
            Oper f, g;
            if (frac.negArgs.Count == 0)
            {
                f = frac.posArgs[0].Copy();
                g = new Fraction(frac.posArgs.Skip(1).ToList(), new List<Oper>{}).Copy();
                return new Derivative(f.Copy(), axis).Mult(g.Copy()).Simplified().Plus(new Derivative(g.Copy(), axis).Mult(f.Copy()).Simplified());
            }
            f = new Fraction(frac.posArgs, new List<Oper>{});
            g = new Fraction(frac.negArgs, new List<Oper>{});
            return new Fraction(g.Mult(new Derivative(f.Copy(), axis).Simplified()).Minus(f.Mult(new Derivative(g.Copy(), axis).Simplified())), g.Pow(new Variable(2)));
        }

        if (Argument is ExpLog expl)
        {
            if (expl.IsLogarithm)
            {
                // TODO: Implement log derivatives
                throw Scribe.Issue("Log derivatives not implemented");
            }
            // Power
            if (expl.Exponent.IsDetermined)
            {
                if (expl.Exponent.Sol().Value.EqValue(0))
                    return new Variable(0);
                return expl.Exponent.Sol().Mult(expl.Base.Pow(expl.Exponent.Sol().Minus(new Variable(1))));
            }
            // Exponential
            else if (expl.Base.IsDetermined)
            {
                if (expl.Base.Sol().Value.EqValue(Math.E))
                    return new Derivative(expl.Exponent.Copy(), axis).Mult(expl);
                return expl.Base.Sol().Log(new Variable(Math.E)).Mult(new Derivative(expl.Exponent.Copy(), axis).Mult(expl));
            }
            // f(x)^g(x)
            else
            {
                Oper f = expl.Base;
                Oper g = expl.Exponent;
                return f.Log(new Variable(Math.E)).Mult(g).Exp(new Variable(Math.E)).Mult(new Derivative(f, axis).Mult(g).Divide(f).Plus(new Derivative(g, axis).Mult(f.Log(new Variable(Math.E)))));
            }
        }
        throw Scribe.Issue($"Unsupported derivative on {Argument.Name} {Argument}");
        //return base.Simplified();
    }

    public override Variable Sol()
    {
        return Simplified().Sol();
    }
    public override Oper Degree(Oper v)
    {
        Oper deg = Argument.Degree();
        if (deg.IsDetermined)
        {
            if (deg.Sol().Value.EqValue(0))
                return deg.Sol();
            return deg.Sol().Minus(new Variable(1));
        }
        return Simplified().Degree();
    }

    public override string ToString()
    {
        return $"d/d{axis} ({Argument})";
    }
}