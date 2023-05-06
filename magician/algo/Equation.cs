namespace Magician.Algo;

// Re-write of IMap
public class Equation
{
    int knowns;
    int unknowns;
    int isolates;
    EquationLayers layers;
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        layers = new(o0, o1);
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public Equation Solve(Variable v)
    {
        // Make sure variable v exists
        if (!layers.vars.Contains(v))
            throw Scribe.Error("TODO: write this error message");

        // Determine which side we will try to isolate on
        int chosenSide;  // 0 for left, 1 for right
        bool varOnLeft = layers.HoldsLeft(v);
        bool varOnRight = layers.HoldsRight(v);
        // Variable exists on both sides, pick the shorter side
        // TODO: find a better metric?
        // TODO: find the higher degree side
        if (varOnLeft && varOnRight)
        {
            chosenSide = layers.symbolsLeft <= layers.symbolsRight ? 0 : 1;
        }
        else
            chosenSide = varOnRight ? 1 : 0;

        bool solved = false;
        //Dictionary<int, List<Oper>> hand = layers.sides[chosenSide];
        //Dictionary<int, List<Oper>> offHand = layers.sides[chosenSide];
        while (!solved)
        {
            // Check to see if the equation is solved
            // An equation is considered solved WHEN
            // 1. the "solve-for" variable, v, exists on one side only
            // 2. the solve-for variable is the sole member of that side
            varOnLeft = layers.HoldsLeft(v);
            varOnRight = layers.HoldsRight(v);
            if (varOnLeft ^ varOnRight)
            {
                // TODO: check condition 2
            }
            // The variable exists on both sides, move things towards the chosen side
            else
            {
                List<Oper> currentExpression = layers.OuterLayer(chosenSide);
                // Iterate through each Oper in the current layer
                for (int i = 0; i < currentExpression.Count; i++)
                {
                    Oper o = currentExpression[i];
                    // If the variable is in our current layer, invert
                    if (o.hasUnknownVars > 0)
                    {
                        Oper inverse = o.Inverse(v);
                    }
                    // Variable is in a deeper layer
                    else
                    {
                        // Check associative property
                    }
                }
            }
        }
        return layers.Build();
    }

    public void Evaluate()
    {
        //
    }

    public Multi Plot(AxisSpecifier[] outs)
    {
        if (outs.Length >= unknowns)
        {
            throw Scribe.Error("TODO: write this error msg");
        }
        throw Scribe.Issue("Not supported");
    }

    // The fulcrum is the =, >, <, etc.
    public enum Fulcrum
    {
        EQUALS, LESSTHAN, GREATERTHAN, LSTHANOREQTO, GRTHANOREQTO
    }
    public enum AxisSpecifier
    {
        X, Y, Z, HUE, TIME
    }

    public enum SolveState
    {
        SOLVED,         // each variable isolated symbolically
        APPROXIMATED,   // one or more variables non-isolatable, approximated
        FAILED          // tbd
    }
}

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
                int numArgs = outerOper.numArgs;
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
                int numArgs = outerOper.numArgs;
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
}