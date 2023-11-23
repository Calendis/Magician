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
                argsContainingAxis.Add((o, i < posLimit));
            else
                argsNotContainingAxis.Add((o, i < posLimit));
        }
        int c = 0;
        foreach (List<(Oper, bool parity)> flaggedArgs in new List<List<(Oper, bool)>> { argsNotContainingAxis, argsContainingAxis })
        {
            if (Identity is null)
                throw Scribe.Issue($"TODO: support this case");
            if (flaggedArgs.Count % 2 != 0)
                flaggedArgs.Add((new Variable((int)Identity), true));
            for (int i = 0; i < flaggedArgs.Count; i++)
                for (int j = i + 1; j < flaggedArgs.Count; j++)
                    groupedHandshakes[c].Add((i, j, flaggedArgs[i].parity, flaggedArgs[j].parity));
            c++;
        }

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

    internal (Dictionary<string, Oper>, Dictionary<string, int>) ArgBalance()
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
        return (LegacyForm.Shed(axis), LegacyForm.Shed(cs));
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
        return (LegacyForm.Shed(chosenSide), LegacyForm.Shed(chosenSide.New(new List<Oper> { oppositeSide }, new List<Oper> { axis })));
    }
}