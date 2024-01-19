namespace Magician.Alg.Symbols;

/* Class for additional algebraic functionality for Oper */
public abstract partial class Oper
{
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