namespace Magician.Symbols;
using Magician.Maps;

public class Equation : IRelation
{
    public List<Variable> Unknowns { get; private set; }  // all unknowns
    public List<Variable> Sliders { get; internal set; } = new(); // for unknowns beyond 3
    public Oper LHS { get; private set; }
    public Oper RHS { get; private set; }
    public int Ins { get; set; }
    public int Outs { get { return Unknowns.Count; } set { } }
    Fulcrum TheFulcrum { get; set; }
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
        TheFulcrum = f;
        LHS = o0;
        RHS = o1;

        List<Variable> initialIsolates = new();
        if (o0 is Variable v && !v.Found)
        {
            initialIsolates.Add(v);
        }
        if (o1 is Variable v2 && !v2.Found)
        {
            initialIsolates.Add(v2);
        }
        OperLayers lhs = new(LHS, Variable.Undefined);
        OperLayers rhs = new(RHS, Variable.Undefined);
        //Unknowns = LHS.eventuallyContains.Concat(RHS.Arguments).Union(initialIsolates).ToList();
        Unknowns = lhs.GetInfo(0, 0).assocArgs.Concat(rhs.GetInfo(0, 0).assocArgs).Union(initialIsolates).ToList();
        // Can't do this through the base constructor, unfortunately
        //map = new Func<double[], double[]>(vals => Approximate(vals));
        Ins = Unknowns.Count - 1;
    }

    double[] IRelation.Evaluate(params double[] args)
    {
        return Approximate(args);
    }

    internal enum SolveMode
    {
        PICK,
        ISOLATE,
        EXTRACT,
        SIMPLIFY,
    }
    internal enum SolveSide
    {
        LEFT,
        RIGHT,
        EITHER  // side does not matter
    }
    internal enum MatchState
    {
        DIRECT = 1,
        SINGLE = 2,
        MULTIPLE = 3,
        NONE = 5
    }
    internal enum MatchPairs
    {
        TAUT = 1,
        PARASINGLE = 2,
        PARAMULTIPLE = 3,
        DUAL = 4,
        SOLVED = 5,
        IMBALANCED = 6,
        FLUID = 9,
        SINGLE = 10,
        MULTIPLE = 15,
        DEAD = 25

    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public SolvedEquation Solved(Variable? v = null)
    {
        SolvedEquation? solvedEq = null;
        Oper lhCopy = LHS.Copy();
        Oper rhCopy = RHS.Copy();
        // By default, solve for the variable with the highest degree
        if (v == null)
        {
            //Scribe.Info($"Determining minimum-degree unknown for {LHS} = {RHS}");
            Variable? chosenSolveVar = null;
            Oper? minDegree = null;
            foreach (Variable uk in Unknowns)
            {
                Oper deg = new Funcs.Max(new Funcs.Abs(LHS.Degree(uk)), new Funcs.Abs(RHS.Degree(uk)));
                //Scribe.Info($"Unknown: {uk}, Deg: {deg}, Min: {minDegree}, Deg<Min?: {(minDegree == null ? "nullmin" : deg < minDegree)}");
                //deg = deg.SpecialSimplified();
                //Scribe.Info($"deg after: {deg}");
                if (minDegree == null)
                {
                    minDegree = deg;
                    chosenSolveVar = uk;
                }
                else if (!(deg > minDegree))
                {
                    if (deg.Solution().Val == 0)
                        Scribe.Warn($"deg was 0");
                    minDegree = deg;
                    chosenSolveVar = uk;
                }
            }
            if (chosenSolveVar is null)
                throw Scribe.Issue("Could not determine minimum-degree unknown!");
            v = chosenSolveVar;
        }
        Scribe.Info($"Solving {this} for {v}");
        (OperLayers LEFT, OperLayers RIGHT) LAYERS = (new(LHS, v), new(RHS, v));
        OperLayers GETLAYER(SolveSide s) { return new List<OperLayers> { LAYERS.LEFT, LAYERS.RIGHT }[(int)s]; }

        // Algebra machine variables
        List<(SolveMode, SolveSide, Variable, Oper)> CODE = new()
            {(SolveMode.PICK, SolveSide.LEFT, v, Variable.Undefined)};
        // These default values don't mean anything. They just need to be invalid
        (int, int) LAST_PICK = (0, -2);
        (int, int) CURRENT_PICK = (0, -1);
        bool WAS_PICK = false;
        Oper? OLDCHOSEN = null, OLDOPPOSITE = null;

        // The algebra solver state machine
        // You should NOT access the state of Equation within this loop, instead referring to layers
        // The layers variable is reassigned each loop
        int TOTAL_PICKS = 0;
        int TOTAL_CHANGES = 0;
        int MAX_FUEL = 3;
        int FUEL = MAX_FUEL;
        bool SOLVED = false;
        while (!SOLVED)
        {
            if (CODE.Count < 1)
                throw Scribe.Issue("No instructions provided");

            Oper NEWCHOSEN;
            Oper NEWOPPOSITE;

            // Get the current instruction from the queue
            // TODO: Axis should be a Form, not an Oper
            (SolveMode MOD, SolveSide SIDE, Variable VAR, Oper AXIS) INSTRUCTION = CODE[0];
            CODE.RemoveAt(0);

            string STATUS = $"{TOTAL_CHANGES + TOTAL_PICKS}:";
            if (INSTRUCTION.MOD != SolveMode.PICK)
                STATUS += $" {CURRENT_PICK}";
            STATUS += $" {INSTRUCTION.MOD} {INSTRUCTION.VAR}";
            if (INSTRUCTION.MOD == SolveMode.EXTRACT)
                STATUS += " from";
            else if (INSTRUCTION.MOD != SolveMode.PICK)
                STATUS += $" on";
            if (!WAS_PICK)
                Scribe.Info($"  {LAYERS.LEFT.Get(0, 0)} = {LAYERS.RIGHT.Get(0, 0)}");
            if (INSTRUCTION.MOD != SolveMode.PICK)
                STATUS += $" the {INSTRUCTION.SIDE} side of the equation:";
            if (INSTRUCTION.MOD != SolveMode.PICK)
                Scribe.Info(STATUS);

            if (INSTRUCTION.MOD == SolveMode.PICK)
            {
                TOTAL_PICKS++;

                MatchState MS_LEFT = LAYERS.LEFT.GetInfo(0, 0).ms;
                MatchState MS_RIGHT = LAYERS.RIGHT.GetInfo(0, 0).ms;
                MatchPairs MATCH = (MatchPairs)((int)MS_LEFT * (int)MS_RIGHT);
                SolveSide MOST_DIRECT_SIDE = MS_LEFT <= MS_RIGHT ? SolveSide.LEFT : SolveSide.RIGHT;

                List<Oper> LIVE_BRANCHES(SolveSide s) => GETLAYER(s).LiveBranches(0, 0);

                switch (MATCH)
                {
                    // TODO: this is not necessarily an error, the variable could have been cancelled
                    case MatchPairs.DEAD:
                        throw Scribe.Issue($"No matches for {INSTRUCTION.VAR}");
                    // TODO: handle tautologies
                    case MatchPairs.TAUT:
                        throw Scribe.Issue("Tautological equation");
                    // Solved
                    case MatchPairs.SOLVED:
                        Oper solvedLeft = LAYERS.LEFT.Get(0, 0).Copy();
                        Oper solvedRight = LAYERS.RIGHT.Get(0, 0).Copy();
                        solvedEq = solvedLeft is Variable ? new(solvedLeft, Fulcrum.EQUALS, solvedRight, v, Unknowns.Count - 1) : new(solvedRight, Fulcrum.EQUALS, solvedLeft, v, Unknowns.Count - 1);
                        Scribe.Info($"Solved in {TOTAL_CHANGES} operations and {TOTAL_PICKS} picks for {TOTAL_CHANGES + TOTAL_PICKS} total instructions:\n{solvedEq}");
                        SOLVED = true;
                        break;
                    // Solveability known. Create case MatchPairs.SOLVED or approximate
                    case MatchPairs.SINGLE:
                        // Create a solve path
                        List<Oper> solvePath = new();
                        if (LIVE_BRANCHES(MOST_DIRECT_SIDE).Count != 1)
                            throw Scribe.Issue("MatchPairs is SINGLE, but found multiple live branches");
                        Oper node = LIVE_BRANCHES(MOST_DIRECT_SIDE)[0];
                        while (true)
                        {
                            solvePath.Add(node);
                            if (node == INSTRUCTION.VAR)
                                break;
                            else if (!node.invertible)
                                throw Scribe.Issue("not solveable");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                        {
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        }
                        CURRENT_PICK = (3, 0);

                        break;
                    // Solveability knowable. Attempt to create case MatchPairs.SINGLE by simplifying
                    // If a simplification cannot change the form, approximate
                    case MatchPairs.MULTIPLE:
                        // Determine the most common recent ancestor node and invert to that node
                        node = GETLAYER(MOST_DIRECT_SIDE).Get(0, 0);
                        solvePath = new();
                        while (true)
                        {
                            solvePath.Add(node);
                            if (GETLAYER(MOST_DIRECT_SIDE).GetInfo(node).ms == MatchState.MULTIPLE)
                                break;
                            else if (!node.invertible)
                                throw Scribe.Issue("not solveable 2");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // The nearest common ancestor is already the root
                        if (node == GETLAYER(MOST_DIRECT_SIDE).Get(0, 0))
                        {
                            PREPARESIMPLIFY(MOST_DIRECT_SIDE, INSTRUCTION.VAR);
                            CURRENT_PICK = (4, 1);
                        }
                        else
                        {
                            // Evaluate the solve path
                            foreach (Oper a in solvePath)
                                PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                            CURRENT_PICK = (4, 2);
                        }

                        break;
                    // Create case MatchPairs.MULTIPLE by extracting from the direct side
                    case MatchPairs.PARASINGLE:
                        PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, INSTRUCTION.VAR);
                        CURRENT_PICK = (5, 0);
                        break;
                    // Create case MatchPairs.MULTIPLE by extracting from the direct side
                    case MatchPairs.PARAMULTIPLE:
                        PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, INSTRUCTION.VAR);
                        CURRENT_PICK = (6, 0);
                        break;
                    // Pick inversion path of lesser "degree", up to term
                    case MatchPairs.DUAL:
                        CURRENT_PICK = (7, 0);
                        if (LegacyForm.IsTerm(GETLAYER(MOST_DIRECT_SIDE).Get(0, 0)))
                        {
                            CURRENT_PICK = (8, 1);
                            PREPARESIMPLIFY(1-MOST_DIRECT_SIDE, INSTRUCTION.VAR);
                            PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, GETLAYER(MOST_DIRECT_SIDE).Get(0, 0));
                            break;
                        }
                        node = LIVE_BRANCHES(MOST_DIRECT_SIDE)[0];
                        solvePath = new();
                        while (true)
                        {
                            solvePath.Add(node);
                            if (node == INSTRUCTION.VAR)
                                break;
                            else if (!node.invertible)
                                throw Scribe.Issue("not solveable");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                    // Pick inversion path from SINGLE side, up to term
                    case MatchPairs.IMBALANCED:
                        CURRENT_PICK = (8, 0);
                        if (LegacyForm.IsTerm(GETLAYER(MOST_DIRECT_SIDE).Get(0, 0)))
                        {
                            CURRENT_PICK = (8, 1);
                            PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, GETLAYER(MOST_DIRECT_SIDE).Get(0, 0));
                            break;
                        }
                        node = LIVE_BRANCHES(MOST_DIRECT_SIDE)[0];
                        solvePath = new();
                        while (true)
                        {
                            solvePath.Add(node);
                            if (node == INSTRUCTION.VAR)
                                break;
                            else if (!node.invertible)
                                throw Scribe.Issue("not solveable");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                    // The hardest case. Attempt to create case MatchPairs.Multiple
                    case MatchPairs.FLUID:
                        CURRENT_PICK = (9, 0);
                        if (LegacyForm.IsTerm(GETLAYER(MOST_DIRECT_SIDE).Get(0, 0)))
                        {
                            CURRENT_PICK = (8, 1);
                            PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, GETLAYER(MOST_DIRECT_SIDE).Get(0, 0));
                            break;
                        }
                        node = LIVE_BRANCHES(MOST_DIRECT_SIDE)[0];
                        solvePath = new();
                        while (true)
                        {
                            solvePath.Add(node);
                            if (node == INSTRUCTION.VAR)
                                break;
                            else if (!node.invertible)
                                throw Scribe.Issue("not solveable");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                }
                WAS_PICK = true;
                continue;
                /* end rewrite */
            }
            // Manipulate the tree in favour of being solved
            else
            {
                WAS_PICK = false;
                List<Oper> CHOSENROOT;
                List<Oper> OPPOSITEROOT;

                if (INSTRUCTION.SIDE == SolveSide.LEFT)
                {
                    CHOSENROOT = LAYERS.LEFT.Root;
                    OPPOSITEROOT = LAYERS.RIGHT.Root;
                }
                else if (INSTRUCTION.SIDE == SolveSide.RIGHT)
                {
                    CHOSENROOT = LAYERS.RIGHT.Root;
                    OPPOSITEROOT = LAYERS.LEFT.Root;
                }
                else
                {
                    throw Scribe.Issue("Bad solve side");
                }

                NEWCHOSEN = CHOSENROOT[0];
                NEWOPPOSITE = OPPOSITEROOT[0];

                if (INSTRUCTION.MOD == SolveMode.ISOLATE)
                {
                    TOTAL_CHANGES++;
                    (NEWCHOSEN, NEWOPPOSITE) = Oper.IsolateOperOn(CHOSENROOT[0], OPPOSITEROOT[0], INSTRUCTION.AXIS, INSTRUCTION.VAR);
                }
                else if (INSTRUCTION.MOD == SolveMode.EXTRACT)
                {
                    TOTAL_CHANGES++;
                    (NEWCHOSEN, NEWOPPOSITE) = Oper.ExtractOperFrom(CHOSENROOT[0], OPPOSITEROOT[0], INSTRUCTION.AXIS);
                }
                else if (INSTRUCTION.MOD == SolveMode.SIMPLIFY)
                {
                    TOTAL_CHANGES++;
                    GETLAYER(INSTRUCTION.SIDE).Get(0, 0).Simplify(INSTRUCTION.VAR);
                    GETLAYER(INSTRUCTION.SIDE).Get(0, 0).Reduce();
                    NEWCHOSEN = LegacyForm.Shed(CHOSENROOT[0]);
                    NEWOPPOSITE = LegacyForm.Shed(OPPOSITEROOT[0]);
                }

                if (NOCHANGE(NEWCHOSEN, NEWOPPOSITE))
                {
                    if (LAST_PICK == CURRENT_PICK && FUEL < 0)
                    {
                        throw Scribe.Issue($"Failed to solve to equation for {v}");
                    }
                    FUEL--;
                }
                if (CODE.Count == 0)
                    PREPAREPICK(INSTRUCTION.VAR);
            }

            LAST_PICK = CURRENT_PICK;
            OLDCHOSEN = NEWCHOSEN.Copy();
            OLDOPPOSITE = NEWOPPOSITE.Copy();
            if (INSTRUCTION.SIDE == SolveSide.LEFT)
                LAYERS = (new(NEWCHOSEN, INSTRUCTION.VAR), new(NEWOPPOSITE, INSTRUCTION.VAR));
            else
                LAYERS = (new(NEWOPPOSITE, INSTRUCTION.VAR), new(NEWCHOSEN, INSTRUCTION.VAR));
        }

        bool NOCHANGE(Oper a, Oper b)
        {
            if (OLDCHOSEN is null || OLDOPPOSITE is null)
                return false;
            return a.Like(OLDCHOSEN) && b.Like(OLDOPPOSITE);
        }
        // Write the next instruction
        void PREPAREPICK(Variable VAR)
        {
            CODE.Add((SolveMode.PICK, SolveSide.EITHER, VAR, Variable.Undefined));
        }
        void PREPAREISOLATE(SolveSide SIDE, Variable VAR, Oper AXIS)
        {
            CODE.Add((SolveMode.ISOLATE, SIDE, VAR, AXIS));
        }
        // nice
        void PREPAREEXTRACT(SolveSide SIDE, Variable VAR, Oper AXIS)
        {
            CODE.Add((SolveMode.EXTRACT, SIDE, VAR, AXIS));
        }
        void PREPARESIMPLIFY(SolveSide SIDE, Variable VAR)
        {
            CODE.Add((SolveMode.SIMPLIFY, SIDE, VAR, Variable.Undefined));
        }

        if (solvedEq is null)
            throw Scribe.Issue("Error in solve loop");
        //solvedEq.IsSolved = true;
        LHS = lhCopy;
        RHS = rhCopy;
        return solvedEq;
    }

    // Approximate a variable that isn't or can't be isolated. Eg. Pow(x, x) = 2
    public double[] Approximate(double[] vals, Variable v)
    {
        throw Scribe.Issue("not implemented");
    }
    public double[] Approximate(double[] vals)
    {
        return Approximate(vals, Unknowns[0]);
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
        return $"{LHS} {fulcrumString} {RHS}";
    }
}
// The fulcrum is the =, >, <, etc.
public enum Fulcrum
{
    EQUALS, LESSTHAN, GREATERTHAN, LSTHANOREQTO, GRTHANOREQTO
}
public enum AxisSpecifier
{
    X = 0, Y = 1, Z = 2
}