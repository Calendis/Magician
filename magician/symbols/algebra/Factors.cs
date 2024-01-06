namespace Magician.Symbols;

public class FactorMap
{
    Dictionary<Oper, Oper> factors = new();
    public FactorMap(params Oper[] fs)
    {
        OperLike ol = new();
        foreach (Oper factor in fs)
        {
            Oper fc = factor.Copy();
            //fc.SimplifyMax();
            Oper facSimple = LegacyForm.Canonical(fc);
            Oper bas;
            Oper exp;

            if (facSimple is ExpLog && facSimple.negArgs.Count == 0)
            {
                //Scribe.Info($"  Factor was {factor}");
                bas = facSimple.posArgs[0];

                if (facSimple.posArgs.Count > 2)
                    exp = new ExpLog(facSimple.posArgs.Skip(1).ToList(), new List<Oper> { });
                else if (facSimple.posArgs.Count == 2)
                    exp = facSimple.posArgs[1];
                else
                    exp = new Variable(1);
                
                while (bas is ExpLog && bas.posArgs.Count > 1)
                {
                    exp = exp.Mult(new ExpLog(bas.posArgs.Skip(1).ToList(), new List<Oper>{}));
                    bas = bas.posArgs[0];
                    exp.SimplifyMax();
                }
                
                if (factors.Keys.Contains(bas, ol))
                {
                    Oper key = factors.Keys.First(o => o.Like(bas));
                    factors[key] = factors[key].Add(exp);
                    //factors[key].Simplify();
                    //Scribe.Info($"  ...got exp {exp}. Adding {bas}^{exp}");
                }
                else
                {
                    factors.Add(bas, exp);
                    //Scribe.Info($"  ...got exp {exp}. Adding {bas}^{exp}");
                }
            }
            else
            {
                if (factors.Keys.Contains(facSimple, ol))
                    factors[facSimple] = factors[facSimple].Add(new Variable(1));
                else
                    factors.Add(facSimple, new Variable(1));
            }
        }
    }
    public FactorMap(Dictionary<Oper, Oper> fs)
    {
        factors = fs;
    }

    public FactorMap Common(FactorMap fs)
    {
        Dictionary<Oper, Oper> newFactors = new();
        OperLike ol = new();
        int c = 0;

        List<Oper> uncommon = fs.factors.Keys.Where(o => !factors.Keys.Contains(o, ol)).ToList();
        foreach (Oper uo in uncommon)
            fs.factors.Remove(uo);

        foreach (Oper fac in factors.Keys.Concat(fs.factors.Keys))
        {
            Oper a = new Variable(0);
            Oper b = new Variable(0);
            int commonFlag = -1;

            if (c < factors.Keys.Count)
            {
                a = factors[fac];
                commonFlag++;
                if (fs.factors.Keys.Contains(fac, ol))
                {
                    b = fs.factors[fs.factors.Keys.First(o => o.Like(fac))];
                    commonFlag++;
                }
            }
            else
            {
                b = fs.factors[fac];
                commonFlag++;
                if (factors.Keys.Contains(b, ol))
                {
                    a = factors[factors.Keys.First(o => o.Like(b))];
                    commonFlag++;
                }
            }

            Oper exp = new Funcs.Min(a, b);
            if (commonFlag > 0)
                newFactors.Add(fac, exp);
            c++;
        }

        return new(newFactors);
    }

    public Fraction ToFraction()
    {
        List<Oper> posArgs = new();
        List<Oper> negArgs = new();
        foreach (Oper f in factors.Keys)
        {
            Oper exp = factors[f];
            if (exp.IsDetermined)
            {
                IVal x = exp.Sol().Value.Trim();
                if (x.Dims == 1)
                {
                    if (x.Get() < 0)
                    {
                        if (x.Get() == -1)
                        {
                            negArgs.Add(f);
                        }
                        else
                        {
                            negArgs.Add(f.Pow(new Variable(x * -1)));
                        }
                    }
                    else if (x.Get() > 0)
                    {
                        if (x.Get() == 1)
                        {
                            posArgs.Add(f);
                        }
                        else
                        {
                            posArgs.Add(f.Pow(exp.Sol()));
                        }
                    }
                }
                else
                {
                    posArgs.Add(f.Pow(exp.Sol()));
                }

            }
            else
            {
                posArgs.Add(f.Pow(exp));
            }

        }
        Fraction frac = new(posArgs, negArgs);
        //Scribe.Info($"... got factors frac {frac}");
        frac.Reduce();
        //Scribe.Info($"... then {frac}");
        return frac;
    }

    //public override string ToString()
    //{
    //    return factors == null ? "nullfac" : factors.ToString();
    //}
}