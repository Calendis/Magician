namespace Magician.Algo;

// Formats the Oper tree in a convenient-to-use way
internal class EquationLayers
{
    // All constants, variables, and operators for each side
    public Dictionary<int, List<Oper>> leftHand => sides[0];
    public Dictionary<int, List<Oper>> rightHand => sides[1];
    public Dictionary<int, List<Oper>>[] sides;
    // Number of variables with a set value, or "constants"
    public int knowns;
    // Number of unique variables with no set value
    public int unknowns;
    // Total number of constants, variables, and operators for each side
    public int symbolsLeft;
    public int symbolsRight;
    public List<Variable> vars = new();
    public EquationLayers(Oper o0, Oper o1)
    {
        sides = new Dictionary<int, List<Oper>>[2];
        sides[0] = new(); sides[1] = new();
        List<Oper> opers = new();
        List<int> layers = new();
        knowns = 0;
        unknowns = 0;
        symbolsLeft = 0;
        symbolsRight = 0;

        // Recursively get operator tree
        o0.CollectOpers(ref opers, ref layers, ref knowns, ref unknowns);
        symbolsLeft = opers.Count;
        o1.CollectOpers(ref opers, ref layers, ref knowns, ref unknowns);
        symbolsRight = opers.Count - symbolsLeft;
        foreach (Oper o in opers)
        {
            if (o is Variable v_)
            {
                if (!vars.Contains(v_))
                    vars.Add(v_);
            }
        }

        // Arrange the tree into a layered format
        for (int i = 0; i < symbolsLeft; i++)
        {
            leftHand.TryAdd(layers[i], new());
            leftHand[layers[i]].Add(opers[i]);
        }
        for (int i = symbolsLeft; i < symbolsLeft + symbolsRight; i++)
        {
            rightHand.TryAdd(layers[i], new());
            rightHand[layers[i]].Add(opers[i]);
        }
        sides = new[] { leftHand, rightHand };
    }

    // Re-create the equation from the layer dictionaries
    // TODO: rewrite this, as it doesn't work
    public Equation Build()
    {
        // Debug.Assert(fulcrum == Equation.Fulcrum.EQUALS);
        (Oper lhs, int _) = BuildInner(hand: 0, layer: 0, index: 0, offset: 0);
        (Oper rhs, int _) = BuildInner(hand: 1, layer: 0, index: 0, offset: 0);
        return new Equation(lhs, Equation.Fulcrum.EQUALS, rhs);
    }

    public (Oper, int) BuildInner(int hand, int layer, int index, int offset)
    {
        List<Oper> currentLayer = sides[hand][layer];
        Oper oper = currentLayer[index];
        List<Oper> args = new();
        if (oper is Variable v)
        {
            return (v, 0);
        }

        for (int argIndex = 0; argIndex < oper.NumArgs; argIndex++)
        {
            (Oper arg, int consumed) = BuildInner(hand: hand, layer: layer + 1, index: argIndex, offset: offset);
            offset += consumed;
            args.Add(arg);
        }
        return (oper.New(args.ToArray()), offset);
    }

    public bool HoldsLeft(Variable v)
    {
        foreach (int k in leftHand.Keys)
        {
            foreach (Oper o in leftHand[k])
            {
                if (o is Variable v_)
                {
                    if (v_ == v)
                        return true;
                }
            }
        }
        return false;
    }

    public bool HoldsRight(Variable v)
    {
        foreach (int k in rightHand.Keys)
        {
            foreach (Oper o in rightHand[k])
            {
                if (o is Variable v_)
                {
                    if (v_ == v)
                        return true;
                }
            }
        }
        return false;
    }

    public override string ToString()
    {
        string leftHandStr = "\n";
        string rightHandStr = "\n";
        foreach (int k in leftHand.Keys)
        {
            leftHandStr += $"Layer {k}\n";
            foreach (Oper o in leftHand[k])
            {
                leftHandStr += $"    {o} (numArgs: {o.NumArgs}, args.Length: {o.args.Length})\n";
            }
        }
        foreach (int k in rightHand.Keys)
        {
            rightHandStr += $"Layer {k}\n";
            foreach (Oper o in rightHand[k])
            {
                rightHandStr += $"    {o} (numArgs: {o.NumArgs}, args.Length: {o.args.Length})\n";
            }
        }
        leftHandStr += "---";
        rightHandStr += "---";
        return $"\nLeft hand:{leftHandStr}\nRight hand:{rightHandStr}";
    }
}