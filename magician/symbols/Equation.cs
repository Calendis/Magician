namespace Magician.Symbols;

public class Equation
{
    int noUnknowns;
    Variable[] unknowns;
    EquationLayers layers;
    EquationLayers layersBackup;
    List<Variable> isolates;
    Oper leftHandSide;
    Oper rightHandSide;
    public Fulcrum TheFulcrum { get; private set; }
    public double Degree(Variable v)
    {
        return Math.Abs(leftHandSide.Degree(v) - rightHandSide.Degree(v));
    }
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        o0.Simplify();
        o1.Simplify();
        TheFulcrum = f;
        leftHandSide = o0;
        rightHandSide = o1;

        layers = new(leftHandSide, rightHandSide);
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
        EXTRACT
    }
    enum Side
    {
        CHOSEN,
        OPPOSITE
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public Equation Solved(Variable? v = null)
    {
        // By default, solve for the variable with the highest degree
        // TODO: move code from Solve to here
        if (v == null)
        {
            throw Scribe.Issue("move code from Plot to here");
        }

        //throw Scribe.Issue("");
        Equation? solved;

        /* The algebra state machine */
        // Determine side to solve for
        List<Oper> CHOSENROOT;
        List<Oper> OPPOSITEROOT;
        double CHOSENDEG, OPPOSITEDEG;
        ManipMode MODE = ManipMode.ISOLATE;
        int IDX;
        Variable VAR = v;
        List<(ManipMode, Side, Variable)> CODE = new();
        while (true)
        {
            // By default, choose the side with the lower degree
            double degLeft = Math.Abs(layers.Hands[0][0][0].Degree(v));
            double degRight = Math.Abs(layers.Hands[1][0][0].Degree(v));
            //IDX = degLeft < degRight ? 1 : degLeft == degRight ? (layers.Hands[0][0][0].Size() < layers.Hands[1][0][0].Size() ? 0 : 1) : 0;
            IDX = degLeft < degRight ? 1 : 0;
            MODE = ManipMode.ISOLATE;

            // Set controls
            if (CODE.Count > 0)
            {
                MODE = CODE[^1].Item1;
                IDX = (int)CODE[^1].Item2;
                VAR = CODE[^1].Item3;
                CODE = CODE.SkipLast(1).ToList();
            }

            // Simplify the outer oper
            //Scribe.Info("pre opt");
            //Scribe.Info(layers.Hands[IDX][0][0]);
            layers.Hands[IDX][0][0].Simplify();
            //Scribe.Info("post opt");
            //Scribe.Info(layers.Hands[IDX][0][0]);
            layers.Hands[1 - IDX][0][0].Simplify();

            CHOSENROOT = layers.Hands[IDX][0];
            OPPOSITEROOT = layers.Hands[1 - IDX][0];
            CHOSENDEG = CHOSENROOT[0].Degree(VAR);
            OPPOSITEDEG = OPPOSITEROOT[0].Degree(VAR);

            Scribe.Info($"ALGEBRA STATE");
            Console.WriteLine("____________________________________________________________________________");
            Scribe.Info($"Chosen: {CHOSENROOT[0]}");
            Scribe.Info($"Opposite: {OPPOSITEROOT[0]}");
            Scribe.Info($"{MODE} {VAR} (degree {CHOSENDEG}) from {IDX}: {CHOSENROOT[0]} = {OPPOSITEROOT[0]}\n");

            // Variable exists on one side only
            if (OPPOSITEDEG == 0 && CHOSENDEG >= 1)
            {
                // Solved
                if (CHOSENROOT[0] is Variable v_ && v_ == v)
                {
                    solved = new Equation(CHOSENROOT[0], TheFulcrum, OPPOSITEROOT[0]);
                    break;
                }
            }

            // Gather information about the Oper tree
            int DIRECTMATCHES = 0;
            int INDIRECTMATCHES = 0;
            int LASTDIRECTMS = 0;
            int LASTINDRECTMS = 0;
            int MATCHIDX = -1;
            int COUNTER = 0;
            foreach (Oper o in CHOSENROOT[0].args)
            {
                if (o is Variable v_ && v_ == v)
                {
                    DIRECTMATCHES++;
                    MATCHIDX = COUNTER;
                }
                else if (o.Contains(v))
                {
                    INDIRECTMATCHES++;
                    MATCHIDX = COUNTER;
                }
                COUNTER++;
            }
            
            // No change in matches from previous step, determine correct action
            if (DIRECTMATCHES == LASTDIRECTMS && INDIRECTMATCHES == LASTINDRECTMS)
            {   
                if (OPPOSITEDEG == 0 || CHOSENDEG == 0)
                {
                    throw Scribe.Issue("This should never occur");
                }

                // Variable exists on both sides, so extract it from the biggest side
                CODE.Add((ManipMode.EXTRACT, CHOSENROOT[0].Size() > OPPOSITEROOT[0].Size() ? Side.CHOSEN : Side.OPPOSITE, v));
                continue;
                //throw Scribe.Issue($"Could not solve {this}. Approximate instead");
            }

            // Operate on the Oper tree
            Oper MANIP;
            Oper? NEWCHOSENROOT = null;
            Oper? NEWOPPOSITEROOT = null;
            switch (MODE)
            {
                case ManipMode.ISOLATE:
                    MANIP = CHOSENROOT[0].Inverse(MATCHIDX);
                    if (MANIP.NumArgs % 2 != 0)
                    {
                        MANIP.AppendIdentity();
                    }
                    NEWCHOSENROOT = layers.Hands[IDX][1][MATCHIDX];
                    NEWOPPOSITEROOT = MANIP.New(MANIP.args.Append(OPPOSITEROOT[0]).ToArray());

                    if (MATCHIDX % 2 != 0)
                    {
                        NEWOPPOSITEROOT.PrependIdentity();
                    }
                    break;

                case ManipMode.EXTRACT:
                    MANIP = CHOSENROOT[0].New(layers.Hands[IDX][1][MATCHIDX]);
                    // Note: this is reversed because we're extracting rather than isolating
                    if (MATCHIDX % 2 == 0)
                        MANIP.PrependIdentity();
                    Oper[] extractedArgs = MANIP.args;
                    //OPPOSITEROOT[0].args = OPPOSITEROOT[0].args.Concat(new Oper[]{MANIP}).ToArray();
                    NEWCHOSENROOT = CHOSENROOT[0].Inverse(MATCHIDX);
                    NEWOPPOSITEROOT = CHOSENROOT[0].New(OPPOSITEROOT[0].args.Concat(extractedArgs).ToArray());
                    //CODE.Add((ManipMode.EXTRACT, CHOSENROOT[0].Size() > OPPOSITEROOT[0].Size() ? Side.CHOSEN : Side.OPPOSITE, v));
                    break;
            }
            LASTDIRECTMS = DIRECTMATCHES;
            LASTINDRECTMS = INDIRECTMATCHES;
            if (NEWCHOSENROOT == null || NEWOPPOSITEROOT == null)
            {
                throw Scribe.Issue("Null roots");
            }
            NEWCHOSENROOT.Simplify();
            NEWOPPOSITEROOT.Simplify();

            if (IDX == 0)
            {
                layers = new(NEWCHOSENROOT, NEWOPPOSITEROOT);
            }
            else
            {
                layers = new(NEWOPPOSITEROOT, NEWCHOSENROOT);
            }
            // Simplify the outer oper
            //layers.Hands[IDX][0][0] = layers.Hands[IDX][0][0].Optimized();
            //layers.Hands[1 - IDX][0][0] = layers.Hands[1 - IDX][0][0].Optimized();
        }

        // Revert
        layers = layersBackup;
        layersBackup = new(layersBackup.LeftHand[0][0].Copy(), layersBackup.RightHand[0][0].Copy());

        if (solved == null)
        {
            throw Scribe.Issue("Error in solve loop");
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
        if (axes.Length != noUnknowns)
        {
            throw Scribe.Error("Axis specifiers must match number of unknowns");
        }
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
                double deg = Degree(v);
                if (deg < minDegree)
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
