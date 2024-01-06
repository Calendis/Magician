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
        if (posArgs.Count > 1)
            posArgs = posArgs.Where(o => !(o.IsConstant && ((Variable)o).Value.Trim().Dims == 1 && o.Sol().Value.Get() == Identity)).ToList();
        if (negArgs.Count > 0)
            negArgs = negArgs.Where(o => !(o.IsConstant && ((Variable)o).Value.Trim().Dims == 1 && o.Sol().Value.Get() == Identity)).ToList();
    }
    internal void MakeExplicit(int? i=null)
    {
        if (posArgs.Count > 0)
            return;
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
        OperLike ol = new();
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
            {
                // Update assoc arg info on reduce
                if (o is Variable v && !v.Found)
                {
                    if (AssociatedVars.Contains(v))
                        AssociatedVars.Remove(v);
                }
            }
            else if (coefficient > 0)
            {
                while (coefficient-- > 0)
                    posArgs.Add(o.Copy());
                if (o is Variable v && !v.Found)
                {
                    if (!AssociatedVars.Contains(v))
                        AssociatedVars.Add(v);
                }
            }
            else if (coefficient < 0)
            {
                while (coefficient++ < 0)
                    negArgs.Add(o.Copy());
                if (o is Variable v && !v.Found)
                {
                    if (!AssociatedVars.Contains(v))
                        AssociatedVars.Add(v);
                }
            }
        }
    }
    
    // During an extract, the axis crosses the equals sign
    public static (Oper, Oper) ExtractOperFrom(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        // need extract method
        if (chosenSide is Variable || axis == chosenSide)
            return ExtractOperFrom(new SumDiff(new List<Oper> { chosenSide }, new List<Oper>()), oppositeSide, axis);
            
        if (chosenSide.posArgs.Contains(axis))
            chosenSide.posArgs.Remove(axis);
        else if (chosenSide.negArgs.Contains(axis))
            chosenSide.negArgs.Remove(axis);
        else
            throw Scribe.Issue($"Could not extract {axis} from {chosenSide.GetType()} {chosenSide}");
        return (LegacyForm.Shed(chosenSide), LegacyForm.Shed(chosenSide.New(new List<Oper> { oppositeSide }, new List<Oper> { axis })));
    }
}