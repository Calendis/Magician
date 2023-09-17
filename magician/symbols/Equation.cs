namespace Magician.Symbols;

public class Equation
{
    int noUnknowns;
    Variable[] unknowns;
    EquationLayers layers;
    EquationLayers layersBackup;
    List<Variable> isolates;
    public Oper LHS;
    public Oper RHS;
    Fulcrum TheFulcrum { get; set; }
    public bool IsSolved { get; private set; } = false;
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        //o0.Simplify();
        //o1.Simplify();
        TheFulcrum = f;
        LHS = o0;
        RHS = o1;

        layers = new(LHS, RHS);
        layersBackup = new(o0.Copy(), o1.Copy());
        isolates = new();
        if (o0 is Variable v && !v.Found)
        {
            isolates.Add(v);
        }
        if (o1 is Variable v2 && !v2.Found)
        {
            isolates.Add(v2);
        }

        unknowns = o0.eventuallyContains.Concat(o1.eventuallyContains).Union(isolates).ToArray();
        noUnknowns = unknowns.Length;
    }

    enum ManipMode
    {
        ISOLATE,
        EXTRACT,
        PICK
    }
    enum Side
    {
        LEFT,
        RIGHT
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public Equation Solved(Variable? v = null)
    {
        Scribe.Info("Begin solve...");
        // By default, solve for the variable with the highest degree
        // TODO: move code from Solve to here
        if (v == null)
        {
            throw Scribe.Issue("move code from Plot to here");
        }

        //throw Scribe.Issue("");
        Equation? solvedEq = null;

        // Algebra machine variables
        int IDX;
        double CHOSENDEG, OPPOSITEDEG;
        List<Oper> CHOSENROOT;
        List<Oper> OPPOSITEROOT;
        ManipMode MODE;
        Side SIDE;
        Variable VAR;
        List<(ManipMode, Side, Variable)> CODE = new()
            {(ManipMode.PICK, Side.LEFT, v)};
        // These default values don't mean anything. They just need to be invalid and not equal
        (int, int) LAST_PICK = (-1, 0);
        (int, int) CURRENT_PICK = (0, -1);
        Oper? LIVE_BRANCH_LEFT = null;
        Oper? LIVE_BRANCH_RIGHT = null;
        Oper?[] TWIG = new[] { LIVE_BRANCH_LEFT, LIVE_BRANCH_RIGHT };

        // The algebra solver state machine
        // You should NOT access the state of Equation within this loop, instead referring to layers
        // The layers variable is reassigned each loop
        while (true)
        {
            Scribe.Info($"{layers.LeftHand[0][0]} = {layers.RightHand[0][0]}");

            if (CODE.Count < 1)
                throw Scribe.Issue("No instructions provided");

            (ManipMode, Side, Variable) INSTRUCTION = CODE[0];
            CODE.RemoveAt(0);

            MODE = INSTRUCTION.Item1;
            SIDE = INSTRUCTION.Item2;
            VAR = INSTRUCTION.Item3;

            CHOSENROOT = layers.Hands[(int)SIDE][0];
            OPPOSITEROOT = layers.Hands[1 - (int)SIDE][0];
            CHOSENDEG = CHOSENROOT[0].Degree(VAR);

            Scribe.Info($"{MODE}, {SIDE}, {VAR}");
            bool NEEDS_SIMPLIFY = false;

            if (MODE == ManipMode.PICK)
            {
                // First, find matches on both sides
                List<Oper> MATCHES_TERMS_LEFT = new();
                List<Oper> MATCHES_OTHER_LEFT = new();
                List<Oper> MATCHES_TERMS_RIGHT = new();
                List<Oper> MATCHES_OTHER_RIGHT = new();
                List<Oper>[] MATCHES_TERMS_ALL = { MATCHES_TERMS_LEFT, MATCHES_TERMS_RIGHT };
                List<Oper>[] MATCHES_OTHER_ALL = { MATCHES_OTHER_LEFT, MATCHES_OTHER_RIGHT };

                // Determine matches on either side
                int LR_SWITCH = 0;
                foreach (Dictionary<int, List<Oper>> h in layers.Hands)
                {
                    if (h[0][0] is Variable v_ && v_ == v)
                    {
                        MATCHES_TERMS_ALL[LR_SWITCH].Add(h[0][0]);
                        TWIG[LR_SWITCH] = v;
                    }
                    foreach (Oper o in h[0][0].AllArgs)
                    {
                        // If we see the variable in question, that counts as a term automatically
                        if (o is Variable v__ && v__ == v)
                        {
                            MATCHES_TERMS_ALL[LR_SWITCH].Add(o);
                            TWIG[LR_SWITCH] = o;
                        }
                        // otherwise, test to see if o is a term
                        else if (o.Contains(v))
                        {
                            if (o.IsTerm)
                            {
                                MATCHES_TERMS_ALL[LR_SWITCH].Add(o);
                            }
                            else
                            {
                                MATCHES_OTHER_ALL[LR_SWITCH].Add(o);
                            }
                            TWIG[LR_SWITCH] = o;
                        }
                    }
                    LR_SWITCH++;
                }
                if (TWIG[0] is null && TWIG[1] is null)
                {
                    throw Scribe.Issue("Tree died");
                }

                int NUMTERMLEFT = MATCHES_TERMS_LEFT.Count;
                int NUMTERMRIGHT = MATCHES_TERMS_RIGHT.Count;
                int NUMOTHERLEFT = MATCHES_OTHER_LEFT.Count;
                int NUMOTHERRIGHT = MATCHES_OTHER_RIGHT.Count;

                // Are we done yet?
                bool alone = !layers.Hands[(int)SIDE].ContainsKey(1);
                if (NUMTERMLEFT + NUMTERMRIGHT == 1 && NUMOTHERLEFT + NUMOTHERRIGHT == 0 && alone)
                {
                    solvedEq = new(CHOSENROOT[0].Copy(), Fulcrum.EQUALS, OPPOSITEROOT[0].Copy());
                    Scribe.Info($"Solved: {solvedEq}");
                    break;
                }

                /* 4x4 truth table for determining next instruction */
                // No direct matches on either side
                if (NUMTERMLEFT + NUMTERMRIGHT == 0)
                {
                    // No indirect matches on either side
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        //CURRENT_PICK = (0, 0);
                        throw Scribe.Issue($"Given variable {VAR} was not found");
                    }
                    // One indirect match
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 0);
                        CODE.Add((ManipMode.ISOLATE, NUMOTHERLEFT == 1 ? Side.LEFT : Side.RIGHT, VAR));
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 0);
                        NEEDS_SIMPLIFY = true;
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 0);
                        NEEDS_SIMPLIFY = true;
                    }
                }
                // One direct match
                else if (NUMTERMLEFT + NUMTERMRIGHT == 1)
                {
                    // No indirect matches on either side
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 1);
                        if (CHOSENROOT[0].AllArgs.Count == 0)
                        {
                            Scribe.Info("Solved!");
                            break;
                        }

                        CODE.Add((ManipMode.ISOLATE, CHOSENDEG == 0 ? 1 - SIDE : SIDE, VAR));
                    }
                    // One indirect match
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 1);
                        CODE.Add((ManipMode.EXTRACT, NUMOTHERLEFT == 1 ? Side.LEFT : Side.RIGHT, VAR));
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 1);
                        // Move the indirect matches to the side with the direct match, if any
                        if (NUMTERMLEFT * NUMOTHERLEFT == 0)
                        {
                            CODE.Add((ManipMode.EXTRACT, NUMTERMLEFT == 0 ? Side.LEFT : Side.RIGHT, VAR));
                        }
                        // In the case where a mixture of direct and indirect matches lie one one side, we must simplify
                        else
                        {
                            NEEDS_SIMPLIFY = true;
                        }
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 1);
                        CODE.Add((ManipMode.EXTRACT, NUMTERMLEFT == 0 ? Side.LEFT : Side.RIGHT, VAR));
                    }
                }
                // Multiple direct matches on one side
                else if (NUMTERMLEFT * NUMTERMRIGHT == 0)
                {
                    // No indirect matches
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 2);
                        NEEDS_SIMPLIFY = true;
                    }
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 2);
                        NEEDS_SIMPLIFY = true;
                    }
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 2);
                        NEEDS_SIMPLIFY = true;
                    }
                    else
                    {
                        CURRENT_PICK = (3, 2);
                        NEEDS_SIMPLIFY = true;
                    }
                }
                // At least one direct match one both sides
                else
                {
                    // No indirect matches
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 3);
                        CODE.Add((ManipMode.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? Side.LEFT : Side.RIGHT, VAR));
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 3);
                        //NEEDS_SIMPLIFY = true;
                        CODE.Add((ManipMode.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? Side.LEFT : Side.RIGHT, VAR));
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 3);
                        //NEEDS_SIMPLIFY = true;
                        CODE.Add((ManipMode.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? Side.LEFT : Side.RIGHT, VAR));
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 3);
                        //NEEDS_SIMPLIFY = true;
                        CODE.Add((ManipMode.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? Side.LEFT : Side.RIGHT, VAR));
                    }
                }
            }

            if (NEEDS_SIMPLIFY)
            {
                //CHOSENROOT[0].Simplify(v);
                //OPPOSITEROOT[0].Simplify(v);
                //CODE.Add((ManipMode.PICK, SIDE, VAR));
                //continue;
            }

            // Manipulate the tree in favour of being solved
            Oper NEWCHOSEN = CHOSENROOT[0];
            Oper NEWOPPOSITE = OPPOSITEROOT[0];
            if (MODE == ManipMode.ISOLATE)
            {
                if (TWIG[(int)SIDE] is null)
                {
                    throw Scribe.Issue("Isolated with no live branch");
                }
                (NEWCHOSEN, NEWOPPOSITE) = Oper.InvertEquationAround(CHOSENROOT[0], OPPOSITEROOT[0], TWIG[(int)SIDE]!);
            }
            else if (MODE == ManipMode.EXTRACT)
            {
                throw Scribe.Issue("Extract not implemented");
            }

            if (CODE.Count == 0)
            {
                // If no progress was made, enter PICK mode
                if (NEWCHOSEN == CHOSENROOT[0] && NEWOPPOSITE == OPPOSITEROOT[0])
                {
                    if (LAST_PICK == CURRENT_PICK)
                    {
                        throw Scribe.Issue($"The equation could not be solved for {v}. Implement approximator");
                    }
                    CODE.Add((ManipMode.PICK, SIDE, VAR));
                }
                // also PICK if we just isolated something
                else if (MODE == ManipMode.ISOLATE)
                {
                    CODE.Add((ManipMode.PICK, SIDE, VAR));
                }
                // Otherwise, just keep doing whatever we were doing
                else
                {
                    CODE.Add(INSTRUCTION);
                }
            }

            Scribe.Info($"{LAST_PICK} => {CURRENT_PICK}");
            LAST_PICK = CURRENT_PICK;
            if (SIDE == Side.LEFT)
                layers = new(NEWCHOSEN, NEWOPPOSITE);
            else
                layers = new(NEWOPPOSITE, NEWCHOSEN);


        }


        // Revert
        layers = layersBackup;
        layersBackup = new(layersBackup.LeftHand[0][0].Copy(), layersBackup.RightHand[0][0].Copy());

        if (solvedEq is null)
            throw Scribe.Issue("Error in solve loop");
        solvedEq.IsSolved = true;
        return solvedEq;
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
        Variable result = solvedSide.Solution();
        Array.ForEach(unknowns, v => v.Reset());
        Array.ForEach<Variable>(knowns.ToArray(), v => v.Reset());
        return result.Val;//.ToList().Select<Variable, double>((v, i) => v.Val).ToArray();
    }

    Oper SolvedSide(Variable v)
    {
        Oper solvedSide;
        if (v == layers.LeftHand[0][0])
        {
            solvedSide = layers.RightHand[0][0];
        }
        else if (v == layers.RightHand[0][0])
        {
            solvedSide = layers.LeftHand[0][0];
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

    public Multi Plot(params (Variable VAR, AxisSpecifier AXIS, double MIN, double MAX, double res)[] axes)
    {
        // TODO: refine AxisSpecifier
        if (axes.Length != noUnknowns)
            throw Scribe.Error("Axis specifiers must match number of unknowns");

        Dictionary<Variable, AxisSpecifier> axesByVar = new();
        Dictionary<Variable, (double, double, double)> rangesByVar = new();
        for (int i = 0; i < noUnknowns; i++)
        {
            axesByVar.Add(axes[i].VAR, axes[i].AXIS);
            rangesByVar.Add(axes[i].VAR, (axes[i].MIN, axes[i].MAX, axes[i].res));
        }

        Variable outVar;
        // If we have isolated variables in the equation, we can treat it as solved
        if (isolates.Count > 0)
        {
            outVar = isolates[0];
        }
        // Otherwise, determine which variable to solve for
        // TODO: move this code to Solve
        else
        {
            Variable? chosenSolveVar = null;
            double minDegree = double.MaxValue;
            foreach (Variable v in unknowns)
            {
                double deg = Math.Max(Math.Abs(LHS.Degree(v)), Math.Abs(RHS.Degree(v)));
                if (deg < minDegree && deg != 0)
                {
                    minDegree = deg;
                    chosenSolveVar = v;
                }
            }
            if (chosenSolveVar is null)
            {
                throw Scribe.Issue("Could not determine minimum-degree unknown!");
            }
            outVar = chosenSolveVar;
            //Scribe.Info($"Solving for {outVar} with degree {Degree(outVar)}");
        }

        List<Variable> inVars = new();
        foreach (var a in axes)
        {
            if (a.VAR != outVar)
            {
                inVars.Add(a.VAR);
            }
        }
        Oper solvedExpr = Solved(outVar).SolvedSide(outVar);
        NDCounter solveSpace = new(axes.Where(ax => ax.VAR != outVar).Select(ax => (ax.MIN, ax.MAX, ax.res)).ToArray());
        List<(Variable, AxisSpecifier, double, double, double)> orderedAxes = new();

        bool threeD = solveSpace.Dims >= 2;
        List<int[]> faces = new();
        Multi plot = new Multi().Flagged(DrawMode.PLOT);

        do
        {
            // Get arguments from the counter
            for (int i = 0; i < inVars.Count; i++)
            {
                inVars[i].Val = solveSpace.Get(i);
            }
            outVar.Val = solvedExpr.Solution().Val;

            double[] argsByAxis = new double[Math.Max(3, inVars.Count + 1)];
            argsByAxis[(int)axesByVar[outVar]] = outVar.Val;
            foreach (Variable v in inVars)
            {
                argsByAxis[(int)axesByVar[v]] = v.Val;
            }
            // Pad to three dimensions
            while (argsByAxis.Length < 3)
            {
                argsByAxis = argsByAxis.Append(0).ToArray();
            }

            // Determine faces
            bool edgeRow = false;
            bool edgeCol = false;
            if (threeD)
            {
                double w = solveSpace.AxisLen(0);
                double h = solveSpace.AxisLen(1);
                int n = solveSpace.Val;
                if (solveSpace.Positional[0] >= Math.Ceiling(solveSpace.AxisLen(0) - 1))
                {
                    edgeCol = true;
                }
                if (n >= solveSpace.Max - w)
                {
                    edgeRow = true;
                }
                if (!edgeCol && !edgeRow && n + Math.Ceiling(w) + 1 < solveSpace.Max)
                {
                    faces.Add(new int[] { (int)Math.Ceiling(w) + n + 1, (int)Math.Ceiling(w) + n, n, n + 1 });
                }
            }

            // Plot colouring code can go here
            // TODO: provide an API for this
            double x = argsByAxis[0];
            double y = argsByAxis[1];
            double z = argsByAxis[2];
            //y += 80*Math.Sin(x/60 - z/60);
            double hue;
            double sat = 1;
            double ligh = 1;

            hue = 4 * solveSpace.Positional[1] / solveSpace.AxisLen(1) - solveSpace.Positional[0] / solveSpace.AxisLen(0);
            hue = Math.Abs(y) / 100;
            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                hue = 0;
                y = 0;
            }

            //Multi point = new(argsByAxis[0], argsByAxis[1], argsByAxis[2]);
            Multi point = new(x, y, z);
            point.Colored(new HSLA(hue, sat, ligh, 255));
            plot.Add(point);

        } while (!solveSpace.Increment());

        //Array.ForEach(inVars, v => v.Reset());
        inVars.ForEach(v => v.Reset());
        outVar.Reset();
        // 3D plot format
        if (threeD)
        {
            Multi3D plot3d = new(plot);
            plot3d.SetFaces(faces);
            return plot3d.Flagged(DrawMode.INNER);
        }
        // 2D plot
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
        return $"{layers.LeftHand[0][0]} {fulcrumString} {layers.RightHand[0][0]}";
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
        X = 0, Y = 1, Z = 2, HUE = 3
    }

    public enum SolveState
    {
        SOLVED,         // each variable isolated symbolically
        APPROXIMATED,   // one or more variables non-isolatable, approximated
        FAILED          // tbd
    }
}
