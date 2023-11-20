namespace Magician.Symbols;

/* Class for additional algebraic functionality for Oper */
public abstract partial class Oper
{
    // Flagged grouped handshakes, flagged grouped opers, pos/neg filtered args
    //public abstract void Combine(Variable axis);
    internal (List<List<(int, int, bool, bool)>>, List<List<(Oper, bool)>>) GrpFlagHshakes(Variable axis)
    {

        List<(Oper, bool)> argsContainingAxis = new();
        List<(Oper, bool)> argsNotContainingAxis = new();
        List<List<(int, int, bool, bool)>> groupedHandshakes = new() { new() { }, new() { } };

        // Group the Opers into terms containing the axis, and terms not containing the axis
        int posLimit = posArgs.Count;
        for (int i = 0; i < AllArgs.Count; i++)
        {
            Oper o = AllArgs[i];
            if (o.AssociatedVars.Contains(axis) || (o is Variable v && v == axis))
            {
                argsContainingAxis.Add((o, i < posLimit));
                //Scribe.Info($"\t{o} does contain {axis}, degree {o.Degree(axis)}, total degree {o.Degree()}");
            }
            else
            {
                argsNotContainingAxis.Add((o, i < posLimit));
                //Scribe.Info($"\t{o} does not contain {axis}");
            }
        }
        int c = 0;
        foreach (List<(Oper, bool parity)> flaggedArgs in new List<List<(Oper, bool)>> { argsNotContainingAxis, argsContainingAxis })
        {
            if (Identity is null)
                throw Scribe.Issue($"TODO: support this case");
            if (flaggedArgs.Count % 2 != 0)
                flaggedArgs.Add((new Variable((int)Identity), true));
            for (int i = 0; i < flaggedArgs.Count; i++)
            {
                for (int j = i + 1; j < flaggedArgs.Count; j++)
                {
                    groupedHandshakes[c].Add((i, j, flaggedArgs[i].parity, flaggedArgs[j].parity));
                }
            }
            c++;
        }
        //posArgs.AddRange(posNegFilteredArgs[0]);
        //negArgs.AddRange(posNegFilteredArgs[1]);

        return (groupedHandshakes, new List<List<(Oper, bool)>>
            { argsNotContainingAxis, argsContainingAxis });
    }
    internal void DropIdentities()
    {
        if (Identity is null)
            throw Scribe.Issue($"{this} has no identity to drop");
        // Drop identities
        posArgs = posArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == Identity)).ToList();
        negArgs = negArgs.Where(o => !(o.IsConstant && ((Variable)o).Val == Identity)).ToList();
    }
    internal void MakeExplicit(int? i=null)
    {
        if (Identity is null && i is null)
            throw Scribe.Error($"Could not make explicit {this}");
        i ??= Identity;
        if (posArgs.Count == 0 && this is not Variable)
            posArgs.Add(New(new List<Oper> { Notate.Val((int)i!) }, new List<Oper> { }));
    }

    (Dictionary<string, Oper>, Dictionary<string, int>) ArgBalance()
    {
        Dictionary<string, Oper> selectedOpers = new();
        Dictionary<string, int> operCoefficients = new();
        foreach (Oper o in posArgs)
        {
            string ord = o.Ord();
            if (selectedOpers.ContainsKey(ord))
            {
                operCoefficients[ord]++;
            }
            else
            {
                selectedOpers.Add(ord, o);
                operCoefficients.Add(ord, 1);
            }
        }
        foreach (Oper o in negArgs)
        {
            string ord = o.Ord();
            if (selectedOpers.ContainsKey(ord))
            {
                operCoefficients[ord]--;
            }
            else
            {
                selectedOpers.Add(ord, o);
                operCoefficients.Add(ord, -1);
            }
        }
        return (selectedOpers, operCoefficients);
    }
    public void Balance()
    {
        var (selectedOpers, operCoefficients) = ArgBalance();
        posArgs.Clear();
        negArgs.Clear();
        foreach (string ord in operCoefficients.Keys)
        {
            int coefficient = operCoefficients[ord];
            Oper o = selectedOpers[ord];
            if (coefficient == 0)
                continue;
            else if (coefficient > 0)
            {
                while (coefficient-- > 0)
                    posArgs.Add(o.Copy());
            }
            else if (coefficient < 0)
            {
                while (coefficient++ < 0)
                    negArgs.Add(o.Copy());
            }
        }
    }
    //internal void AbsorbTrivial(Oper parent)
    //{
    //    // Absorb trivial
    //    if (absorbable && posArgs.Count == 1 && negArgs.Count == 0)
    //    {
    //        if (parent!.negArgs.Contains(this))
    //        {
    //            parent.negArgs.Remove(this);
    //            parent.negArgs.AddRange(posArgs);
    //        }
    //        else if (parent.posArgs.Contains(this))
    //        {
    //            parent.posArgs.Remove(this);
    //            parent.posArgs.AddRange(posArgs);
    //        }
    //    }
    //}
    public static (Oper, Oper) IsolateOperOn(Oper chosenSide, Oper oppositeSide, Oper axis, Variable v)
    {
        if (!chosenSide.commutative)
            throw Scribe.Issue("You need to fix this to support non-commutative Opers");
        //Oper os = oppositeSide.Copy();
        bool invertedAxis = false;
        if (chosenSide.negArgs.Contains(axis))
        {
            invertedAxis = true;
            chosenSide.negArgs.Remove(axis);
        }
        else if (chosenSide.posArgs.Contains(axis))
        {
            chosenSide.posArgs.Remove(axis);
        }
        else
        {
            throw Scribe.Issue($"Axis {axis} not in {chosenSide}");
        }

        Oper cs = chosenSide.New(new List<Oper> { }, new List<Oper> { });
        cs.posArgs.AddRange(chosenSide.posArgs);
        cs.negArgs.AddRange(chosenSide.negArgs);
        (cs.posArgs, cs.negArgs) = (cs.negArgs, cs.posArgs);

        //cs = Upcast(cs, axis);
        //cs.posArgs.Add(chosenSide);

        cs.posArgs.Add(oppositeSide.Copy());
        // Perform an additional inversion if necessary
        if (invertedAxis)
            (cs.posArgs, cs.negArgs) = (cs.negArgs, cs.posArgs);

        //cs.Simplify(v);
        return (Form.Shed(axis), Form.Shed(cs));
    }

    public static (Oper, Oper) ExtractOperFrom(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        // need extract method
        if (chosenSide is Variable || axis == chosenSide)
            return ExtractOperFrom(SumDiff.StaticNew(new List<Oper> { chosenSide }, new List<Oper>()), oppositeSide, axis);
        if (chosenSide.posArgs.Contains(axis))
            chosenSide.posArgs.Remove(axis);
        else if (chosenSide.negArgs.Contains(axis))
            chosenSide.negArgs.Remove(axis);
        else
            throw Scribe.Issue($"Could not extract {axis} from {chosenSide.GetType()} {chosenSide}");
        return (Form.Shed(chosenSide), Form.Shed(chosenSide.New(new List<Oper> { oppositeSide }, new List<Oper> { axis })));
    }

    //public static Oper Intersect(Oper o, Oper p)
    //{
    //    if (o.GetType() != p.GetType())
    //        throw Scribe.Error($"Could not intersect {o.name} {o} with {p.name} {p}");
    //    if (o.Like(p))
    //        return o;
    //}

    public static Oper Intersect(Oper o, Oper p)
    {
        if ((o.IsDetermined && o.Solution().Val == 0) || (p.IsDetermined && p.Solution().Val == 0))
            return new Variable(0);
        else if (o is Variable v0 && p is Variable v1)
            if (v0.Found ^ v1.Found)
                return new Variable(1);
            else if (v0.Found)
                return new Variable(1);
            else
                return o == p ? o : new Variable(1);
        else if (o is Variable ^ p is Variable)
        {
            if (o is Variable v)
            {
                if (v.Found && v.Val == 0)
                    return new Variable(0);
                else if (v.Found)
                    return new Variable(1);
            }
            if (p is Variable u)
            {
                if (u.Found && u.Val == 0)
                    return new Variable(0);
                else if (u.Found)
                    return new Variable(1);
            }
        }

        if (o is not Fraction)
            o = new Fraction(o);
        if (p is not Fraction)
            p = new Fraction(p);
            
        var (selectedOpers0, operCoefficients0) = o.ArgBalance();
        var (selectedOpers1, operCoefficients1) = p.ArgBalance();
        List<Oper> newPos = new(); List<Oper> newNeg = new();
        foreach (string ord in selectedOpers0.Keys.Intersect(selectedOpers1.Keys))
        {
            int co0 = operCoefficients0[ord];
            int co1 = operCoefficients1[ord];
            int coNew = 0;
            if (co0*co1 > 0)
                if (co0 > 0)
                    coNew = Math.Min(co0, co1);
                else
                    coNew = Math.Max(co0, co1);
            while (coNew > 0)
            {
                newPos.Add(selectedOpers0[ord].Copy());
                coNew--;
            }
            while (coNew < 0)
            {
                newNeg.Add(selectedOpers0[ord].Copy());
                coNew++;
            }
        }
        return new Fraction(newPos, newNeg);
    }
}