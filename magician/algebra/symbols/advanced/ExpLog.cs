namespace Magician.Alg.Symbols;
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

    IVal powTow = new Variable(1);
    Multivalue solMult;
    IVal[] multOuts;
    IVal[] param2;
    public override Variable Sol()
    {
        if (posArgs.Count == 0)
        {
            solution.Set(1);
            return solution;
        }
        solution.Value.Set(posArgs[0].Sol());

        if (posArgs.Count == 2)
        {
            Rational? r = null;
            if (posArgs[1] is Rational r0)
                r = r0;
            else if (posArgs[1].Sol() is Rational r1)
                r = r1;
            // Rational exponent, find all solutions
            if (r is not null)
            {
                int a = r.Numerator;
                int b = r.Denominator;

                if (multOuts is null || multOuts.Length != b)
                {
                    multOuts = new Val[b];
                    param2 = new Val[b];
                }

                double mag = solution.Value.Magnitude;
                double at2a = solution.Value.Get(); double at2b = solution.Value.Dims < 2 ? 0 : solution.Value.Get(1);
                double theta = Math.Atan2(at2b, at2a);

                List<IVal> solutions = new();
                for (int k = 0; k < b; k++)
                {
                    if (multOuts[k] is null)
                        multOuts[k] = new Val(double.NaN);
                    if (param2[k] is null)
                        param2[k] = new Val(double.NaN);
                    param2[k].Set(0d, (a * theta + 2 * k * Math.PI) / b);
                    IVal.Multiply(IVal.Exp(Runes.Numbers.e, param2[k], multOuts[k]), Math.Pow(mag, (double)a / b), multOuts[k]);
                    //Scribe.Info($"Found solution {solution} to {ptBase}^({a}/{b})");
                    solutions.Add(multOuts[k]);
                }

                //return new Multivalue(solutions.ToArray());
                if (solMult is null)
                    solMult = new(solutions.ToArray());
                else
                    solMult.Set(solutions);
                return solMult;
            }

            // Real exponent, find principal solution
            //powTow = new(IVal.Exp(ptBase.Var.ToIVal(), posArgs[1].Sol().Var.ToIVal()));
            IVal.Exp(posArgs[0].Sol(), posArgs[1].Sol(), solution);
        }
        else if (posArgs.Count > 2)
        {
            powTow.Set(posArgs[^1].Sol());
            for (int i = 0; i < posArgs.Count - 1; i++) { IVal.Exp(posArgs[^(i + 2)].Sol(), powTow, powTow); }
            solution.Value.Set(powTow);
        }

        if (negArgs.Count == 0)
        {
            return solution;
        }
        else if (negArgs.Count == 1)
        {
            IVal.Log(solution, negArgs[0].Sol(), solution);
            return solution;
        }
        else
        {
            for (int i = 0; i < negArgs.Count; i++) { IVal.Log(solution, negArgs[i].Sol(), solution); }
            return solution;
        }
    }

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
            return el.Trim();
        }
        Fraction fr = new(Log(new Variable(Math.E)), v.Log(new Variable(Math.E)));
        if (fr.IsDetermined)
            return fr.Sol();
        fr.ReduceOuter();
        return fr.Trim();
    }

    public override FactorMap Factors()
    {
        return new(Copy());
    }

    public override Oper New(IEnumerable<Oper> pa, IEnumerable<Oper> na)
    {
        return new ExpLog(pa, na);
    }


    public override void ReduceOuter()
    {
        /* Truncate power tower at any 1s */
        if (posArgs.Count > 1)
        {
            int? varIdx = null;
            for (int i = 0; i < posArgs.Count; i++)
            {
                if (posArgs[i].IsConstant && posArgs[i].Sol().Value.EqValue(Identity))
                {
                    varIdx = i;
                    break;
                }
            }
            if (varIdx is not null)
                posArgs = posArgs.Take((int)varIdx + 1).ToList();

            /* Collapse tower at 0s */
            varIdx = null;
            for (int i = 0; i < posArgs.Count; i++)
            {
                if (posArgs[i].IsConstant && posArgs[i].Sol().Value.EqValue(0))
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
                return inverse.Trim();
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
        return inverse.Trim();
    }
}
