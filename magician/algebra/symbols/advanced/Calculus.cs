namespace Magician.Alg.Symbols;
using System.Collections.Generic;

public enum DerivativeKind
{
    PARTIAL,
    IMPLICIT
}

public class Derivative : Oper
{
    public Oper Argument => posArgs[0];
    Variable axis;
    public DerivativeKind dk;

    public Derivative(Oper argument, Variable axis, DerivativeKind dk=DerivativeKind.PARTIAL) : base("derivative", argument)
    {
        this.axis = axis;
        trivialAssociative = false;
        this.dk = dk;
    }
    // TODO: support differentiation with respect to an arbitrary Oper
    //public Derivative(Oper o, Oper v) : base("derivative", o)
    //{
    //    axis = v;
    //}

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new Derivative(pa.ToList()[0], axis, dk);
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
            if (dk == DerivativeKind.PARTIAL || v.IsDetermined)
                return new Variable(0);
            else
            {
                //Scribe.Info("We need to get here");
                return this;
            }
            //else return new Derivative(v, axis, dk);
        }

        if (Argument is SumDiff)
        {
            return new SumDiff(Argument.posArgs.Select(a => new Derivative(a.Copy(), axis, dk)).ToList(), Argument.negArgs.Select(a => new Derivative(a.Copy(), axis, dk)).ToList());
        }

        if (Argument is Fraction frac)
        {
            Oper f, g;
            if (frac.negArgs.Count == 0)
            {
                f = frac.posArgs[0].Copy();
                g = new Fraction(frac.posArgs.Skip(1).ToList(), new List<Oper>{}).Copy();
                return new Derivative(f.Copy(), axis, dk).Mult(g.Copy()).Simplified().Plus(new Derivative(g.Copy(), axis, dk).Mult(f.Copy()).Simplified());
            }
            f = new Fraction(frac.posArgs, new List<Oper>{});
            g = new Fraction(frac.negArgs, new List<Oper>{});
            return new Fraction(g.Mult(new Derivative(f.Copy(), axis, dk).Simplified()).Minus(f.Mult(new Derivative(g.Copy(), axis, dk).Simplified())), g.Pow(new Variable(2)));
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
                return expl.Exponent.Sol().Mult(expl.Base.Pow(expl.Exponent.Sol().Minus(new Variable(1)))).Mult(new Derivative(expl.Base.Copy(), axis, dk).Simplified());
            }
            // Exponential
            else if (expl.Base.IsDetermined)
            {
                if (expl.Base.Sol().Value.EqValue(Math.E))
                    return new Derivative(expl.Exponent.Copy(), axis, dk).Mult(expl);
                Scribe.Info("got here");
                return expl.Base.Sol().Log(new Variable(Math.E)).Mult(new Derivative(expl.Exponent.Copy(), axis, dk).Mult(expl))
                    .Mult(new Derivative(expl.Exponent.Copy(), axis, dk).Simplified());
            }
            // f(x)^g(x)
            else
            {
                if (dk == DerivativeKind.IMPLICIT)
                {
                    throw Scribe.Issue("todo");
                }
                Oper f = expl.Base;
                Oper g = expl.Exponent;
                return f.Log(new Variable(Math.E)).Mult(g).Exp(new Variable(Math.E)).Mult(new Derivative(f, axis, dk).Mult(g).Divide(f).Plus(new Derivative(g, axis, dk).Mult(f.Log(new Variable(Math.E)))));
            }
        }

        if (Argument is Derivative d)
        {
            return new Derivative(d.Simplified(), axis, dk);
        }
        throw Scribe.Issue($"Unsupported derivative on {Argument.Name} {Argument}");
        //return base.Simplified();
    }

    public override Variable Sol()
    {
        Oper s = Simplified();
        if (s == this)
        {
            // TODO: this might be evil
            throw Scribe.Error("could not get solution here");
            return Notate.Var(Ord());
        }
        return Simplified().Sol();
    }

    public override Oper Copy()
    {
        return new Derivative(posArgs[0], axis, dk);
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
        return dk == DerivativeKind.IMPLICIT && Argument is Variable v && !v.Found ? $"d{Argument}/d{axis}" : $"d/d{axis} ({Argument})";
    }
}