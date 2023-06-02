namespace Magician.Algo;
using static Magician.Geo.Create;

public class Equation
{
    int noUnknowns;
    public Variable[] unknowns;
    EquationLayers layers;
    EquationLayers layersBackup;
    public Fulcrum TheFulcrum { get; private set; }
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        Oper opt0 = o0.Optimized();
        TheFulcrum = f;
        Oper opt1 = o1.Optimized();

        layers = new(opt0, opt1);
        layersBackup = new(opt0.Copy(), opt1.Copy());
        List<Variable> isolates = new();
        if (opt0 is Variable v && !v.Found)
        {
            isolates.Add(v);
        }
        if (opt1 is Variable v2 && !v2.Found)
        {
            isolates.Add(v2);
        }

        unknowns = opt0.eventuallyContains.Concat(opt1.eventuallyContains).Union(isolates).ToArray();
        noUnknowns = unknowns.Length;
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public Equation Solved(Variable v)
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
                        if (solved == null)
                        {
                            solved = new Equation(layers.sides[chosenSide][0][0], TheFulcrum, layers.sides[1-chosenSide][0][0]);
                        }
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
                newOffHandRoot = inverse.New(inverse.args.Concat(new Oper[] { layers.sides[1 - chosenSide][0][0] }).ToArray());
                if (needsExtraInvert)
                {
                    newOffHandRoot.PrependIdentity();
                }

                if (chosenSide == 0)
                {
                    layers = new(newChosenSideRoot, newOffHandRoot);
                }
                else
                {
                    layers = new(newOffHandRoot, newChosenSideRoot);
                }
                //Scribe.Info($"Solve step: {newChosenSideRoot} = {newOffHandRoot}");
                solved = new Equation(newChosenSideRoot, TheFulcrum, newOffHandRoot);

            }
            // Multiple direct matches
            else if (indirectMatches == 0)
            {
                //layers.sides[chosenSide][0][0] = layers.sides[chosenSide][0][0].Optimized();
                //if (chosenSide == 0)
                //{
                //   layers = new(layers.sides[chosenSide][0][0].Optimized(), layers.sides[1-chosenSide][0][0]);
                //}
                //else
                //{
                //   layers = new(layers.sides[1-chosenSide][0][0], layers.sides[chosenSide][0][0].Optimized());
                //}
                //continue;
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

        // Revert
        layers = layersBackup;
        layersBackup = new(layersBackup.leftHand[0][0].Copy(), layersBackup.rightHand[0][0].Copy());
        if (solved == null)
        {
            throw Scribe.Error($"Could not even begin to solve {this}");
        }
        return solved;
    }

    // Evaluate a solved equation with an isolated variable
    public double Evaluate(Variable v, params double[] vals)
    {
        if (vals.Length != noUnknowns - 1)
        {
            throw Scribe.Error($"Equation expected {noUnknowns - 1} arguments, got {vals.Length}");
        }
        int counter = 0;
        List<Variable> knowns = unknowns.ToList();
        knowns.Remove(v);
        foreach (Variable x in knowns)
        {
            x.Val = vals[counter++];
        }

        // Find the side we're evaluating
        Oper solvedSide = SolvedSide(v);
        double result = solvedSide.Eval().Val;
        Array.ForEach(unknowns, v => v.Reset());
        return result;
    }

    Oper SolvedSide(Variable v)
    {
        Oper solvedSide;
        if (v == layers.leftHand[0][0])
        {
            solvedSide = layers.rightHand[0][0];
        }
        else if (v == layers.rightHand[0][0])
        {
            solvedSide = layers.leftHand[0][0];
        }
        else
        {
            throw Scribe.Error($"Equation {this} has not been solved for {v}");
        }
        return solvedSide;
    }

    // Approximate a variable that isn't or can't be isolated. Eg. Pow(x, x) = 2
    public double Approximate(Variable v)
    {
        throw Scribe.Issue("not implemented");
    }

    //public Multi Plot(params AxisSpecifier[] outs)
    public Multi Plot(double res, params (Variable VAR, AxisSpecifier AXIS, double MIN, double MAX)[] axes)
    {
        if (axes.Length != noUnknowns)
        {
            throw Scribe.Error("Axis specifiers must match number of unknowns");
        }
        Dictionary<Variable, AxisSpecifier> varAxisMap = new();
        for (int i = 0; i < noUnknowns; i++)
        {
            varAxisMap.Add(axes[i].VAR, axes[i].AXIS);
        }
        Variable outVar = axes[0].VAR;
        Variable[] inVars = axes.Skip(1).Select(t => t.VAR).ToArray();
        Oper solvedExpr = Solved(outVar).SolvedSide(outVar);
        
        // TODO: I think I pass the outVar into the counter as well. This isn't necessary
        NDCounter solveSpace = new NDCounter(res, axes.Select(ax => new Tuple<double, double>(ax.MIN, ax.MAX)).ToArray());
        Multi plot = new Multi().WithFlags(DrawMode.PLOT);
        while (!solveSpace.done)
        {
            // Inject arguments
            for (int i = 0; i < inVars.Length; i++)
            {
                inVars[i].Val = solveSpace.Get(i);
            }

            // Get output
            outVar.Val = solvedExpr.Eval().Val;

            double[] argsByAxis = new double[Math.Max(3, inVars.Length + 1)];
            if (argsByAxis.Length > 3)
            {
                throw Scribe.Issue("Extra variables unsupported so far");
            }
            argsByAxis[(int)varAxisMap[outVar]] = outVar.Val;
            foreach (Variable v in inVars)
            {
                argsByAxis[(int)varAxisMap[v]] = v.Val;
            }
            while (argsByAxis.Length < 3)
            {
                argsByAxis = argsByAxis.Append(0).ToArray();
            }
            //Scribe.List(argsByAxis);
            Multi point = new Multi(argsByAxis[0], argsByAxis[1], argsByAxis[2]);
            plot.Add(point);
            solveSpace.Increment();
        }
        Array.ForEach(inVars, v => v.Reset());
        outVar.Reset();
        return plot;

        //throw Scribe.Issue("Not supported");
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
        return $"{layers.leftHand[0][0]} {fulcrumString} {layers.rightHand[0][0]}";
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
        X=0, Y=1, Z=2, HUE=3
    }

    public enum SolveState
    {
        SOLVED,         // each variable isolated symbolically
        APPROXIMATED,   // one or more variables non-isolatable, approximated
        FAILED          // tbd
    }
}
