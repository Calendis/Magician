namespace Magician.Symbols;
using Core;

/* Combines powers, exponents, logs, and roots using the form ...logC(logB(logA(a^b^c...)))... */
public class ExpLog : Invertable
{
    protected new int Identity => 1;

    public ExpLog(IEnumerable<Oper> posArgs, IEnumerable<Oper> negArgs) : base("explog", posArgs, negArgs)
    {
        if (!posArgs.Any())
            this.posArgs.Add(new Variable((int)Identity!));
        trivialAssociative = true;
    }
    public ExpLog(params Oper[] ops) : base("explog", ops) { trivialAssociative = true; }

    public override Oper Degree(Oper v)
    {
        if (IsDetermined)
            return new Variable(0);
        if (negArgs.Count == 0 && v.Like(posArgs[0]))
        {
            Oper el = New(posArgs.Skip(1).ToList(), new List<Oper> { });
            if (el.IsDetermined)
                return el.Sol();
            el.ReduceOuter();
            return LegacyForm.Shed(el);
        }
        Fraction fr = new(Log(new Variable(Math.E)), v.Log(new Variable(Math.E)));
        if (fr.IsDetermined)
            return fr.Sol();
        fr.ReduceOuter();
        return LegacyForm.Shed(fr);
    }

    public override FactorMap Factors()
    {
        return new(Copy());
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new ExpLog(pa, na);
    }

    public override Variable Sol()
    {
        if (posArgs.Count == 0)
            return new Variable(1);

        Variable ptBase = posArgs[0].Sol();
        Variable powTow;
        if (posArgs.Count == 0)
            powTow = new Variable(1);
        else if (posArgs.Count == 1)
            powTow = posArgs[0].Sol();
        else if (posArgs.Count == 2)
        {
            bool simpleBase = ptBase.Var.IsScalar || (ptBase.Var.IsVector && ptBase.Var.Is1D);
            bool simpleExponent = posArgs[1].Sol().Var.IsScalar || (posArgs[1].Sol().Var.IsVector && posArgs[1].Sol().Var.Is1D);
            if (!simpleBase)
                throw Scribe.Error($"Could not exponentiate vector {ptBase}");
            if (!simpleExponent)
                throw Scribe.Error($"Could not raise {ptBase} to vector power {posArgs[1]}");

            if (posArgs[1].Sol().Value() is Rational && posArgs[1].Sol().Value().Get() != (int)posArgs[1].Sol().Value().Get())
                throw Scribe.Issue($"TODO: support multivalued exponents");
            powTow = new(IVal.Exp(ptBase.Var.ToIVal(), posArgs[1].Sol().Var.ToIVal()).ToIVal());
        }
        else
            powTow = new ExpLog(new List<Oper> { ptBase, new ExpLog(posArgs.Skip(1), new List<Oper> { }) }, new List<Oper> { }).Sol();

        if (negArgs.Count == 0)
            return powTow;
        else if (negArgs.Count == 1)
        {
            return new(IVal.Log(powTow, negArgs[0].Sol()));
        }
        else
        {
            return new(IVal.Log(
                new ExpLog(new List<Oper> { powTow }, negArgs.SkipLast(1)).Sol(), negArgs[^1].Sol()
            ));
        }
    }

    public override void ReduceOuter()
    {
        /* Truncate power tower at any 1s */
        if (posArgs.Count > 1)
        {
            int? varIdx = null;
            for (int i = 0; i < posArgs.Count; i++)
            {
                if (posArgs[i].IsConstant && posArgs[i].Sol().Value().EqValue(Identity))
                {
                    varIdx = i;
                    break;
                }
            }
            if (varIdx is not null)
                posArgs = posArgs.Take((int)varIdx+1).ToList();
            
            /* Collapse tower at 0s */
            varIdx = null;
            for (int i = 0; i < posArgs.Count; i++)
            {
                if (posArgs[i].IsConstant && posArgs[i].Sol().Value().EqValue(0))
                {
                    varIdx = i;
                    break;
                }
            }
            if (varIdx is not null && varIdx > 0)
                posArgs = posArgs.Take((int)varIdx).ToList();
        }

        // Combine positive constant terms
        posArgs.Reverse();
        int runLen = 0;
        foreach (Oper pa in posArgs)
        {
            if (!pa.IsDetermined)
                break;
            runLen++;
        }
        posArgs.Reverse();
        //if (runLen > 1)
        if (runLen > 0)
        {
            List<Oper> toCombine = posArgs.TakeLast(runLen).ToList();
            Variable combined = new ExpLog(toCombine, new List<Oper> { }).Sol();
            posArgs = posArgs.Take(posArgs.Count - runLen).ToList();
            posArgs.Add(combined);
        }

        if (posArgs.Count == 0)
            throw Scribe.Issue("reduction should not destroy the tower!");
    }

    public override string ToString()
    {
        string s = "";
        foreach (Oper a in posArgs)
            s += $"{a}^";
        s = s.TrimEnd('^');
        s = $"({s})";
        if (negArgs.Count == 0)
            return s;
        for (int i = 0; i < negArgs.Count; i++)
            s = $"log_{negArgs[i]}({s})";
        return s;
    }

    public override Oper Inverse(Oper axis, Oper? opp = null)
    {
        ExpLog inverse = new();
        OperLike ol = new();
        // find axis
        bool pos;
        if (posArgs.Contains(axis, ol))
            pos = true;
        else if (negArgs.Contains(axis, ol))
            pos = false;
        else
            throw Scribe.Error($"Inversion failed, as {name} {this} does not directly contain axis {axis}");
        opp ??= axis;

        // build a new PTRL by shedding args from the current one, starting with any negargs and then getting to the posargs
        List<Oper> tower = new();
        for (int i = 0; i < negArgs.Count; i++)
        {
            int j = negArgs.Count - 1 - i;

            if (!pos && negArgs[j].Like(axis))
            {
                // All preceeding negative arguments will remain negative
                tower.Reverse();
                inverse.negArgs = negArgs.Take(j).ToList();
                inverse.posArgs = posArgs;
                inverse = new ExpLog(new List<Oper> { inverse, new ExpLog(new Fraction(new Variable(1), new ExpLog(tower.Append(opp).ToList(), new List<Oper> { }))) }, new List<Oper> { });


                //inverse.posArgs = new List<Oper> { axis }.Concat(inverse.posArgs).ToList();
                //inverse.negArgs = negArgs.Skip(j+1).ToList();
                ////Scribe.Info($"inverse c1 {Scribe.Expand<List<Oper>, Oper>(posArgs)}, {Scribe.Expand<List<Oper>, Oper>(negArgs)}");
                inverse.ReduceOuter();
                return LegacyForm.Shed(inverse);
            }
            // Add an exponential base to the bottom of the new tower
            //inverse.posArgs = new List<Oper> { negArgs[j] }.Concat(inverse.posArgs).ToList();
            tower.Add(negArgs[j]);
        }
        tower.Reverse();
        inverse.posArgs.AddRange(tower);
        // the position of the axis determines how many logs and how many roots
        for (int i = 0; i < posArgs.Count; i++)
        {
            if (posArgs[i].Like(axis))
            {
                // Everything below becomes a log, everything above becomes a root
                List<Oper> log = posArgs.Take(i).ToList();
                List<Oper> root = posArgs.Skip(i + 1).ToList();
                inverse.negArgs = log;
                inverse.posArgs.Add(opp);
                Fraction fRoot = new(new Variable(1), new ExpLog(root, new List<Oper> { }));
                //inverse.posArgs.Add(new Fraction(new Variable(1), new PowTowRootLog(root, new List<Oper>{})));
                inverse = new ExpLog(new List<Oper> { inverse, fRoot }, new List<Oper> { });
            }
        }

        //throw Scribe.Issue($"no axis {axis.Name} {axis} for inverse {inverse.Name} {inverse}");
        return LegacyForm.Shed(inverse);
    }
}
