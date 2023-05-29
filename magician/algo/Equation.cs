namespace Magician.Algo;

// Re-write of IMap
public class Equation
{
    int knowns;
    int unknowns;
    int isolates;
    EquationLayers layers;
    EquationLayers layersBackup;
    public Oper Left => layers.leftHand[0][0];
    public Oper Right => layers.rightHand[0][0];
    public Fulcrum TheFulcrum { get; private set; }
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        layers = new(o0, o1);
        layersBackup = new(o0.Copy(), o1.Copy());
        TheFulcrum = f;
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

        //bool solved = false;
        //Dictionary<int, List<Oper>> hand = layers.sides[chosenSide];
        //Dictionary<int, List<Oper>> offHand = layers.sides[chosenSide];
        Equation? solved = null;
        while (true)
        {
            // Check to see if the equation is solved
            // An equation is considered solved WHEN
            // 1. the "solve-for" variable, v, exists on one side only
            // 2. the solve-for variable is the sole member of that side
            varOnLeft = layers.HoldsLeft(v);
            varOnRight = layers.HoldsRight(v);
            List<Oper> currentExpressionLayer = layers.sides[chosenSide][0];//layers.OuterLayer(chosenSide);
            Oper outerExpression = currentExpressionLayer[0];
            if (varOnLeft ^ varOnRight)
            {
                if (currentExpressionLayer.Count == 1)
                {
                    if (outerExpression is Variable v_ && v_ == v)
                    {
                        //Scribe.Info("solved!");
                        break;
                    }
                }
            }

            // Iterate through each Oper in the current layer
            // The outer layer should always contain only one Oper
            if (currentExpressionLayer.Count != 1)
            {
                Scribe.Warn(currentExpressionLayer[1]);
                throw Scribe.Issue($"Outer expression {outerExpression} not strictly above other members");
            }
            if (outerExpression.args.Length < 1)
            {
                throw Scribe.Issue($"Erroneously isolated {outerExpression}");
            }
            // Iterate through args and invert based on args
            int directMatches = 0;
            int indirectMatches = 0;
            int liveBranchIndex = -1;
            int directMatchIndex = -1;
            for (int i = 0; i < outerExpression.NumArgs; i++)
            {
                Oper o = outerExpression.args[i];
                // Count how many times the expression contains our variable
                if (o is Variable v_ && v_ == v)
                {
                    directMatches++;
                    directMatchIndex = i;
                }
                else if (o.Contains(v))
                {
                    indirectMatches++;
                    liveBranchIndex = i;
                }
            }
            if (directMatches + indirectMatches == 0)
            {
                throw Scribe.Issue("No matches found, likely solved on the wrong side");
            }
            // One total match
            else if (directMatches + indirectMatches == 1)
            {
                // invert around the chosen variable
                // shed the outer layer, leaving the current layer as the new layer 0 for the current side
                // that variable will be the only member of the current layer
                // another layer will be added (new layer 0) to the other side
                // the new layer 1 (old layer 0) will have the removed Opers appended

                Oper inverse;
                Oper newChosenSideRoot;
                if (directMatches == 1)
                {
                    inverse = outerExpression.Inverse(directMatchIndex);
                    newChosenSideRoot = layers.sides[chosenSide][1][directMatchIndex];
                }
                else
                {
                    inverse = outerExpression.Inverse(liveBranchIndex);
                    newChosenSideRoot = layers.sides[chosenSide][1][liveBranchIndex];
                }
                bool needsExtraInvert = false;
                if (Math.Max(directMatchIndex, liveBranchIndex) % 2 != 0)
                {
                    needsExtraInvert = true;
                }
                if (inverse.NumArgs % 2 != 0)
                {
                    inverse.AppendIdentity();
                }

                Oper newOffHandRoot;
                newOffHandRoot = inverse.New(inverse.args.Concat(new Oper[]{layers.sides[1 - chosenSide][0][0]}).ToArray());
                if (needsExtraInvert)
                {
                    newOffHandRoot.PushIdentity();
                }

                if (chosenSide == 0)
                {
                    layers = new(newChosenSideRoot, newOffHandRoot);
                    solved = new EquationLayers(newChosenSideRoot, newOffHandRoot).Build();
                }
                else
                {
                    layers = new(newOffHandRoot, newChosenSideRoot);
                    solved = new EquationLayers(newOffHandRoot, newChosenSideRoot).Build();
                }
                Scribe.Info($"Solve step: {newChosenSideRoot} = {newOffHandRoot}");

            }
            // Multiple direct matches
            else if (indirectMatches == 0)
            {
                throw Scribe.Issue("Multiple matches not implemented");
            }
            // Multiple indirect matches
            else if (directMatches == 0)
            {
                throw Scribe.Issue("Multiple matches not implemented");
            }
            // Multiple direct and indirect matches
            else
            {
                throw Scribe.Issue("Multiple matches not implemented");
            }
        }
        //Equation solved = new EquationLayers(newChosenSideRoot, newOffHandRoot).Build();//layers.Build();

        // Revert
        layers = layersBackup;
        if (solved == null)
        {
            throw Scribe.Error("Could not solve");
        }
        return solved;
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

    public override string ToString()
    {
        string fulcrumString = "";
        switch (TheFulcrum)
        {
            case Fulcrum.EQUALS:
                fulcrumString = "=";
                break;
            case Fulcrum.LESSTHAN:
                fulcrumString = "<";
                break;
            case Fulcrum.GREATERTHAN:
                fulcrumString = ">";
                break;
            case Fulcrum.LSTHANOREQTO:
                fulcrumString = "<=";
                break;
            case Fulcrum.GRTHANOREQTO:
                fulcrumString = ">=";
                break;
        }
        return $"{Left} {fulcrumString} {Right}";
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
