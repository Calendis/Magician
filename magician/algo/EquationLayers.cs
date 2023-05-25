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
    public Equation RewrittenRewrittenBuild()
    {
        // Debug.Assert(fulcrum == Equation.Fulcrum.EQUALS);
        (Oper lhs, int _) = RewrittenRewrittenBuildInner(hand: 0, layer: 0, index: 0, offset: 0);
        (Oper rhs, int _) = RewrittenRewrittenBuildInner(hand: 1, layer: 0, index: 0, offset: 0);
        return new Equation(lhs, Equation.Fulcrum.EQUALS, rhs);
    }

    public (Oper, int) RewrittenRewrittenBuildInner(int hand, int layer, int index, int offset)
    {
        //Console.WriteLine($"RewrittenRewrittenBuildInner(hand {hand}, layer {layer}, index {index}, offset {offset})");
        List<Oper> currentLayer = sides[hand][layer];
        Oper oper = currentLayer[index];
        List<Oper> args = new();
        if (oper is Variable v)
        {
            return (v, 0);
        }
        //int parentArgs = index == 0 ? 0 : sides[hand][layer-1].Count;
        //for (int argIndex = 0; argIndex < parentArgs; argIndex++)
        for (int argIndex = 0; argIndex < oper.NumArgs; argIndex++)
        {
            (Oper arg, int consumed) = RewrittenRewrittenBuildInner(hand: hand, layer: layer + 1, index: argIndex, offset: offset);
            offset += consumed;
            args.Add(arg);
        }
        return (oper.New(args.ToArray()), offset);
    }

    public Equation RewrittenBuild()
    {
        for (int handIndex = 0; handIndex < sides.Length; handIndex++)
        {
            int mole = 0;
            //int maxDepth = sides[handIndex].Keys.ToList().Sorted;
            List<int> tunnelLengths = new();
        }

        throw Scribe.Issue("this does not work");
    }

    void GetParentOper(int layer, int pos)
    {
        if (layer == 0)
            throw Scribe.Error("Oper on layer 0 does not and may not have a parent");


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

    public List<Oper> OuterLayer(int side)
    {
        List<int> keys = sides[side].Keys.ToList();
        keys.Sort();
        foreach (int k in keys)
        {
            if (sides[side][k].Count > 0)
                return sides[side][k];
        }
        throw Scribe.Error($"Side {side} is empty");
    }
    public List<Oper> InnerLayer(int side)
    {
        List<int> keys = sides[side].Keys.ToList();
        keys.Sort();
        keys.Reverse();
        foreach (int k in keys)
        {
            if (sides[side][k].Count > 0)
                return sides[side][k];
        }
        throw Scribe.Error($"Side {side} is empty");
    }

    public void IncrKeys(int side)
    {
        Dictionary<int, List<Oper>> incrd = new();
        foreach (int k in sides[side].Keys)
        {
            incrd.Add(k + 1, sides[side][k]);
        }
        sides[side] = incrd;
    }

    public void DecrKeys(int side)
    {
        Dictionary<int, List<Oper>> decrd = new();
        foreach (int k in sides[side].Keys)
        {
            if (k > 0)
                decrd.Add(k - 1, sides[side][k]);
        }
        sides[side] = decrd;
    }

    void InvertKeys(int side)
    {
        Dictionary<int, List<Oper>> inverted = new();// sides[side];
        foreach (int k in sides[side].Keys)
        {
            inverted.Add(k * -1, sides[side][k]);
        }
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