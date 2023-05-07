namespace Magician.Algo;

// Formats the Oper tree in a convenient-to-use way
internal class EquationLayers
{
    // All constants, variables, and operators for each side
    public Dictionary<int, List<Oper>> leftHand = new();
    public Dictionary<int, List<Oper>> rightHand = new();
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
    // TODO: test this
    public Equation Build()
    {
        // Find the bottom layer of both hands
        int rightHandHeight = 0;
        int leftHandHeight = 0;
        foreach (int k in rightHand.Keys)
        {
            if (k > rightHandHeight)
                rightHandHeight = k;
        }
        foreach (int k in leftHand.Keys)
        {
            if (k > leftHandHeight)
                leftHandHeight = k;
        }

        // For each hand, rebuild the Oper tree and store in an Oper
        Oper? newRightHand = null;
        List<Oper>? replacedOuterRHLayer = null;
        while (rightHandHeight >= 0)
        {
            List<Oper> innerOpers = rightHand[rightHandHeight];
            List<Oper> outerOpers = rightHand[rightHandHeight - 1];
            int runningTotalArgs = 0;
            int numOuterOpers = replacedOuterRHLayer == null ? outerOpers.Count : replacedOuterRHLayer.Count;
            for (int j = 0; j < numOuterOpers; j++)
            {
                Oper outerOper;
                if (replacedOuterRHLayer == null)
                {
                    outerOper = outerOpers[j];
                    replacedOuterRHLayer = new();
                }
                else
                {
                    outerOper = replacedOuterRHLayer[j];
                }
                List<Oper> currentArgs = new();
                int numArgs = outerOper.NumArgs;
                for (int i = runningTotalArgs; i < numArgs + runningTotalArgs; i++)
                {
                    currentArgs.Add(innerOpers[i]);
                }
                runningTotalArgs += numArgs;
                replacedOuterRHLayer.Add(outerOper.New(outerOper.Name, currentArgs.ToArray()));
            }

            if (replacedOuterRHLayer?.Count == 1)
            {
                Scribe.Info("Top of right hand reached");
                newRightHand = replacedOuterRHLayer[0];
            }

            rightHandHeight--;
        }

        Oper? newLeftHand = null;
        List<Oper>? replacedOuterLHLayer = null;
        while (rightHandHeight >= 0)
        {
            List<Oper> innerOpers = leftHand[leftHandHeight];
            List<Oper> outerOpers = leftHand[leftHandHeight - 1];
            int runningTotalArgs = 0;
            int numOuterOpers = replacedOuterLHLayer == null ? outerOpers.Count : replacedOuterLHLayer.Count;
            for (int j = 0; j < numOuterOpers; j++)
            {
                Oper outerOper;
                if (replacedOuterLHLayer == null)
                {
                    outerOper = outerOpers[j];
                    replacedOuterLHLayer = new();
                }
                else
                {
                    outerOper = replacedOuterLHLayer[j];
                }
                List<Oper> currentArgs = new();
                int numArgs = outerOper.NumArgs;
                for (int i = runningTotalArgs; i < numArgs + runningTotalArgs; i++)
                {
                    currentArgs.Add(innerOpers[i]);
                }
                runningTotalArgs += numArgs;
                replacedOuterLHLayer.Add(outerOper.New(outerOper.Name, currentArgs.ToArray()));
            }

            if (replacedOuterLHLayer?.Count == 1)
            {
                Scribe.Info("Top of left hand reached");
                newLeftHand = replacedOuterLHLayer[0];
            }

            leftHandHeight--;
        }

        if (newLeftHand == null)
            throw Scribe.Issue("Failed to build left hand");
        if (newRightHand == null)
            throw Scribe.Issue("Failed to build right hand");

        return new Equation(newLeftHand, Equation.Fulcrum.EQUALS, newRightHand);
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
        //
    }

    public void DecrKeys(int side)
    {
        //
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
                leftHandStr += $"    {o}\n";
            }
        }
        foreach (int k in rightHand.Keys)
        {
            rightHandStr += $"Layer {k}\n";
            foreach (Oper o in rightHand[k])
            {
                rightHandStr += $"    {o}\n";
            }
        }
        leftHandStr += "---";
        rightHandStr += "---";
        return $"\nLeft hand:{leftHandStr}\nRight hand:{rightHandStr}";
    }
}