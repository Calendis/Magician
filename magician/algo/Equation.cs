namespace Magician.Algo;

// Re-write of IMap
public class Equation
{
    int knowns;
    int unknowns;
    int isolates;
    EquationLayers layers;
    internal EquationLayers Layers => layers;
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
        // TODO: find a better metric. Maybe pick the side with the higher degree of the variable
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
                    // Count how many times the expression contains our variable
                    int matches = 0;
                    if (o is Variable v_ && v_ == v)
                    {
                        matches++;
                    }
                    if (matches == 1)
                    {
                        // invert around the chosen variable
                        // shed the outer layer, leaving the current layer as the new layer 0 for the current side
                        // that variable will be the only member of the current layer
                        // another layer will be added (new layer 0) to the other side
                        // the new layer 1 (old layer 0) will have the removed Opers appended

                        layers.DecrKeys(1-chosenSide);
                        layers.IncrKeys(chosenSide);


                        break;

                    }
                    else if (matches > 1)
                    {
                        //
                    }

                    // TODO: check properties
                    //
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

    /* public string Say()
    {
        string spoken = "";
        foreach ()
    } */

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
