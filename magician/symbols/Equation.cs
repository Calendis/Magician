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
        BOTHL,  // both sides, but reverts to the left side
        BOTHR   // both sides, but reverts to the right side
    }
    internal enum MatchState
    {
        DIRECT = 1,
        INDIRECT = 2,
        NONE = 3
    }
    internal enum MatchPairs
    {
        TAUT = 1,
        ISOLATED = 2,
        SOLVED = 3,
        INDIRECTS = 4,
        CONFINED = 6,
        DEAD = 9
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public SolvedEquation Solved(Variable? v = null)
    {
        SolvedEquation? solvedEq = null;
        // By default, solve for the variable with the highest degree
        if (v == null)
        {
            Variable? chosenSolveVar = null;
            double minDegree = double.MaxValue;
            foreach (Variable uk in Unknowns)
            {
                double deg = Math.Max(Math.Abs(LHS.Degree(uk)), Math.Abs(RHS.Degree(uk)));
                if (deg < minDegree)
                {
                    if (deg == 0)
                        Scribe.Warn($"deg was 0");
                    minDegree = deg;
                    chosenSolveVar = uk;
                }
            }
            if (chosenSolveVar is null)
            {
                throw Scribe.Issue("Could not determine minimum-degree unknown!");
            }
            v = chosenSolveVar;
        }
        Scribe.Info($"Solving {this} for {v}");
        //OperLayers lyrLeft = new(LHS, v);
        //OperLayers lyrRight = new(RHS, v);
        (OperLayers LEFT, OperLayers RIGHT) LAYERS = (new(LHS, v), new(RHS, v));
        OperLayers GETLAYER(SolveSide s) { return new List<OperLayers> { LAYERS.LEFT, LAYERS.RIGHT }[(int)s]; }

        // Algebra machine variables
        double CHOSENDEG, OPPOSITEDEG;
        List<Oper> CHOSENROOT;
        List<Oper> OPPOSITEROOT;
        SolveMode MODE;
        SolveSide SIDE;
        Variable VAR;
        List<(SolveMode, SolveSide, Variable)> CODE = new()
            {(SolveMode.PICK, SolveSide.LEFT, v)};
        // These default values don't mean anything. They just need to be invalid
        (int, int) LAST_PICK = (0, -2);
        (int, int) CURRENT_PICK = (0, -1);
        bool WAS_PICK = false;
        Oper? OLDCHOSEN = null, OLDOPPOSITE = null;
        //Oper?[] TWIG = new[] { LIVE_BRANCH_LEFT, LIVE_BRANCH_RIGHT };
        Oper? LIVE_BRANCH = null;

        // The algebra solver state machine
        // You should NOT access the state of Equation within this loop, instead referring to layers
        // The layers variable is reassigned each loop
        //int TOTAL_INSTRS = 0;
        int TOTAL_PICKS = 0;
        int TOTAL_CHANGES = 0;
        int MAX_FUEL = 10;
        int FUEL = MAX_FUEL;
        bool SOLVED = false;
        while (!SOLVED)
        {
            if (CODE.Count < 1)
                throw Scribe.Issue("No instructions provided");

            (SolveMode, SolveSide, Variable) INSTRUCTION = CODE[0];
            CODE.RemoveAt(0);
            //TOTAL_INSTRS++;

            MODE = INSTRUCTION.Item1;
            SIDE = INSTRUCTION.Item2;
            VAR = INSTRUCTION.Item3;

            Oper NEWCHOSEN;
            Oper NEWOPPOSITE;

            string STATUS = $"{TOTAL_CHANGES + TOTAL_PICKS}:";
            if (MODE != SolveMode.PICK)
                STATUS += $" {CURRENT_PICK}";
            STATUS += $" {MODE} {VAR}";
            if (MODE == SolveMode.EXTRACT)
                STATUS += " from";
            else if (MODE != SolveMode.PICK)
                STATUS += $" on";
            if (!WAS_PICK)
                Scribe.Info($"  {LAYERS.LEFT.Get(0, 0)} = {LAYERS.RIGHT.Get(0, 0)}");
            if (MODE != SolveMode.PICK)
                STATUS += $" the {SIDE} side of the equation:";
            Scribe.Info(STATUS);

            if (MODE == SolveMode.PICK)
            {
                TOTAL_PICKS++;
                //CHOOSEROOTS();

                /* total rewrite of pick */
                //TWIG[0] = null;
                //TWIG[1] = null;

                // Get match information
                //MatchState MS_LEFT = LAYERS.LEFT.GetInfo(0, 0).ms;
                //MatchState MS_RIGHT = LAYERS.RIGHT.GetInfo(0, 0).ms;
                MatchState MS_CHOSEN = GETLAYER(SIDE).GetInfo(0, 0).ms;
                MatchState MS_OPPOSITE = GETLAYER(1 - SIDE).GetInfo(0, 0).ms;
                SolveSide SIDE_HIGHEST_MATCH = LAYERS.LEFT.GetInfo(0, 0).ms < LAYERS.RIGHT.GetInfo(0, 0).ms ? SolveSide.LEFT : SolveSide.RIGHT;
                SolveSide SIDE_MOST_ARGS = LAYERS.LEFT.Root[0].AllArgs.Count > LAYERS.RIGHT.Root[0].AllArgs.Count ? SolveSide.LEFT : SolveSide.RIGHT;
                MatchPairs MATCH = (MatchPairs)((int)MS_CHOSEN * (int)MS_OPPOSITE);

                List<Oper> LIVE_BRANCHES(SolveSide s) => GETLAYER(s).LiveBranches(0, 0);
                List<Oper> DIRECTLY_LIVE(SolveSide s) => LIVE_BRANCHES(s).Where(o => GETLAYER(s).GetInfo(o).ms == MatchState.DIRECT).ToList();

                switch (MATCH)
                {
                    case MatchPairs.DEAD:
                        throw Scribe.Issue($"No matches for {VAR}");
                    case MatchPairs.TAUT:
                        throw Scribe.Issue("Tautological equation");
                    case MatchPairs.SOLVED:
                        Oper newChosen = GETLAYER(SIDE).Get(0, 0).Copy();
                        Oper newOpposite = GETLAYER(1 - SIDE).Get(0, 0).Copy();
                        solvedEq = newChosen is Variable ? new(newChosen, Fulcrum.EQUALS, newOpposite, v, Unknowns.Count - 1) : new(newOpposite, Fulcrum.EQUALS, newChosen, v, Unknowns.Count - 1);
                        Scribe.Info($"Solved in {TOTAL_CHANGES} operations and {TOTAL_PICKS} picks for {TOTAL_CHANGES + TOTAL_PICKS} total instructions: {solvedEq}");
                        SOLVED = true;
                        break;
                    // x is isolated on one side, but also exists on the other side
                    case MatchPairs.ISOLATED:
                        if (LIVE_BRANCHES(1 - SIDE_HIGHEST_MATCH).Count == 0)
                        {
                            throw Scribe.Issue($"This should not occur");
                        }
                        else if (LIVE_BRANCHES(1 - SIDE_HIGHEST_MATCH).Count == 1 && DIRECTLY_LIVE(1 - SIDE_HIGHEST_MATCH).Count == 1)
                        {
                            CURRENT_PICK = (0, 0);
                            PREPAREEXTRACT(1 - SIDE_HIGHEST_MATCH, VAR);
                            LIVE_BRANCH = LIVE_BRANCHES(1 - SIDE_HIGHEST_MATCH)[0];
                        }
                        else
                        {
                            CURRENT_PICK = (0, 1);
                            PREPARESIMPLIFY(1 - SIDE_HIGHEST_MATCH, VAR);
                        }
                        break;
                    // x exists on one side only, but is not isolated
                    case MatchPairs.CONFINED:
                        if (LIVE_BRANCHES(SIDE_HIGHEST_MATCH).Count == 0)
                        {
                            throw Scribe.Issue("This should not occur");
                        }
                        else if (LIVE_BRANCHES(SIDE_HIGHEST_MATCH).Count == 1 && DIRECTLY_LIVE(SIDE_HIGHEST_MATCH).Count == 0)
                        {
                            CURRENT_PICK = (1, 0);
                            PREPAREEXTRACT(SIDE_HIGHEST_MATCH, VAR);
                            LIVE_BRANCH = LIVE_BRANCHES(SIDE_HIGHEST_MATCH)[0];
                        }
                        else if (LIVE_BRANCHES(SIDE_HIGHEST_MATCH).Count == 1 && DIRECTLY_LIVE(SIDE_HIGHEST_MATCH).Count == 1)
                        {
                            CURRENT_PICK = (1, 1);
                            PREPAREISOLATE(SIDE_HIGHEST_MATCH, VAR);
                            LIVE_BRANCH = LIVE_BRANCHES(SIDE_HIGHEST_MATCH)[0];
                        }
                        else
                        {
                            CURRENT_PICK = (1, 2);
                            PREPARESIMPLIFY(SIDE_HIGHEST_MATCH, VAR);
                        }
                        break;
                    // The hardest (most ambiguous) case. x exists on both sides, and is isolated on neither
                    case MatchPairs.INDIRECTS:
                        // If we have a side with no indirect matches, extract to that side
                        // If we have two sides wiht no indirect matches, extract from whichever side has fewer direct matches
                        int DL = DIRECTLY_LIVE(SolveSide.LEFT).Count;
                        int iDL = LIVE_BRANCHES(SolveSide.LEFT).Count - DL;
                        int DR = DIRECTLY_LIVE(SolveSide.RIGHT).Count;
                        int iDR = LIVE_BRANCHES(SolveSide.RIGHT).Count - DR;
                        if (iDL * iDR == 0)
                        {
                            List<SolveSide> sidesWDirectMatches = new List<SolveSide> { SolveSide.LEFT, SolveSide.RIGHT }
                                .Where(ss => DIRECTLY_LIVE(ss).Count > 0).ToList();
                            switch (sidesWDirectMatches.Count)
                            {
                                case 0:
                                    throw Scribe.Issue($"this cannot occur");
                                case 1:
                                    CURRENT_PICK = (2, 0);
                                    LIVE_BRANCH = LIVE_BRANCHES(sidesWDirectMatches[0] == SolveSide.LEFT ? SolveSide.LEFT : SolveSide.RIGHTZ)[0];
                                    PREPAREEXTRACT(sidesWDirectMatches[0], VAR);
                                    break;
                                case 2:
                                    CURRENT_PICK = (2, 1);
                                    LIVE_BRANCH = LIVE_BRANCHES(DL < DR ? SolveSide.RIGHT : SolveSide.LEFT)[0];
                                    PREPAREEXTRACT(DL < DR ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                                    break;
                            }
                        }
                        else
                        {
                            //LIVE_BRANCH = LIVE_BRANCHES(SIDE_MOST_ARGS)[0];
                            CURRENT_PICK = (2, 2);
                            PREPARESIMPLIFY(GENBOTH(), VAR);
                        }
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
                CHOOSEROOTS();
                CHOSENDEG = CHOSENROOT[0].Degree(VAR);
                NEWCHOSEN = CHOSENROOT[0];
                NEWOPPOSITE = OPPOSITEROOT[0];

                if (MODE == SolveMode.ISOLATE)
                {
                    TOTAL_CHANGES++;
                    if (LIVE_BRANCH is null)
                        throw Scribe.Issue($"No live branch to isolate axis from");
                    (NEWCHOSEN, NEWOPPOSITE) = Oper.IsolateOperOn(CHOSENROOT[0], OPPOSITEROOT[0], LIVE_BRANCH, VAR);
                }
                else if (MODE == SolveMode.EXTRACT)
                {
                    TOTAL_CHANGES++;
                    if (LIVE_BRANCH is null)
                        throw Scribe.Issue("No live branch to extract axis from");
                    (NEWCHOSEN, NEWOPPOSITE) = Oper.ExtractOperFrom(CHOSENROOT[0], OPPOSITEROOT[0], LIVE_BRANCH);
                }
                else if (MODE == SolveMode.SIMPLIFY)
                {
                    TOTAL_CHANGES++;
                    if (SIDE == SolveSide.BOTHL || SIDE == SolveSide.BOTHR)
                    {
                        LAYERS.LEFT.Get(0, 0).Associate();
                        LAYERS.LEFT.Get(0, 0).Simplify(v);
                        LAYERS.LEFT.Get(0, 0).Commute();
                        LAYERS.RIGHT.Get(0, 0).Associate();
                        LAYERS.RIGHT.Get(0, 0).Simplify(v);
                        LAYERS.RIGHT.Get(0, 0).Commute();
                    }
                    else if (SIDE == SolveSide.LEFT)
                    {
                        LAYERS.LEFT.Get(0, 0).Associate();
                        LAYERS.LEFT.Get(0, 0).Simplify(v);
                        LAYERS.LEFT.Get(0, 0).Commute();
                    }
                    else if (SIDE == SolveSide.RIGHT)
                    {
                        LAYERS.RIGHT.Get(0, 0).Associate();
                        LAYERS.RIGHT.Get(0, 0).Simplify(v);
                        LAYERS.RIGHT.Get(0, 0).Commute();
                    }
                    else
                    {
                        throw Scribe.Issue($"Bad simplify side");
                    }
                    NEWCHOSEN = CHOSENROOT[0].Trim();
                    NEWOPPOSITE = OPPOSITEROOT[0].Trim();
                }

                if (NOCHANGE(NEWCHOSEN, NEWOPPOSITE))
                {
                    if (LAST_PICK == CURRENT_PICK || FUEL < 0)
                    {
                        throw Scribe.Issue($"Failed to solve to equation for {v} with {FUEL} fuel remaining. Implement approximator");
                    }
                    FUEL--;
                }
                if (MODE != SolveMode.EXTRACT)
                    PREPAREPICK(VAR);
                else if (!NEWCHOSEN.AllArgs.Contains(LIVE_BRANCH!))
                    PREPAREPICK(VAR);
                else
                    CODE.Add(INSTRUCTION);
            }

            LAST_PICK = CURRENT_PICK;
            OLDCHOSEN = NEWCHOSEN.Copy();
            OLDOPPOSITE = NEWOPPOSITE.Copy();
            if (SIDE == SolveSide.LEFT || SIDE == SolveSide.BOTHL)
                LAYERS = (new(NEWCHOSEN, VAR), new(NEWOPPOSITE, VAR));
            else
                LAYERS = (new(NEWOPPOSITE, VAR), new(NEWCHOSEN, VAR));
        }

        /* Algebra machine helper methods */
        SolveSide SMALLESTCHAFFDEGREE()
        {
            List<Variable> chaffVars = Unknowns.Where(b => b != VAR).ToList();
            (double, double) totalDegrees = (0, 0);
            foreach (Variable otr in chaffVars)
            {
                double degL, degR;
                degL = LAYERS.LEFT.Get(0, 0).Degree(VAR);
                degR = LAYERS.RIGHT.Get(0, 0).Degree(VAR);
                totalDegrees.Item1 += degL;
                totalDegrees.Item2 += degR;
            }
            return totalDegrees.Item1 < totalDegrees.Item2 ? SolveSide.LEFT : SolveSide.RIGHT;
        }
        bool NOCHANGE(Oper a, Oper b)
        {
            if (OLDCHOSEN is null || OLDOPPOSITE is null)
                return false;
            return a.Like(OLDCHOSEN) && b.Like(OLDOPPOSITE);
        }
        SolveSide GENBOTH()
        {
            return SIDE == SolveSide.LEFT ? SolveSide.BOTHL : SolveSide.BOTHR;
        }
        void CHOOSEROOTS()
        {
            if (SIDE == SolveSide.BOTHL)
            {
                CHOSENROOT = LAYERS.LEFT.Root;
                OPPOSITEROOT = LAYERS.RIGHT.Root;
            }
            else if (SIDE == SolveSide.BOTHR)
            {
                CHOSENROOT = LAYERS.RIGHT.Root;
                OPPOSITEROOT = LAYERS.LEFT.Root;
            }
            else
            {
                //CHOSENROOT = layers.Hands[(int)SIDE][0];
                //OPPOSITEROOT = layers.Hands[1 - (int)SIDE][0];
                if (SIDE == SolveSide.LEFT)
                {
                    CHOSENROOT = LAYERS.LEFT.Root;
                    OPPOSITEROOT = LAYERS.RIGHT.Root;
                }
                else if (SIDE == SolveSide.RIGHT)
                {
                    CHOSENROOT = LAYERS.RIGHT.Root;
                    OPPOSITEROOT = LAYERS.LEFT.Root;
                }
                else
                {
                    throw Scribe.Issue("Bad solve side");
                }
            }
        }

        // Write the next instruction
        void PREPAREPICK(Variable targ)
        {
            CODE.Add((SolveMode.PICK, SolveSide.LEFT, targ));
        }
        void PREPAREISOLATE(SolveSide s, Variable targ)
        {
            CODE.Add((SolveMode.ISOLATE, s, targ));
        }
        // nice
        void PREPAREEXTRACT(SolveSide s, Variable targ)
        {
            CODE.Add((SolveMode.EXTRACT, s, targ));
        }
        void PREPARESIMPLIFY(SolveSide s, Variable targ)
        {
            CODE.Add((SolveMode.SIMPLIFY, s, targ));
        }

        if (solvedEq is null)
            throw Scribe.Issue("Error in solve loop");
        //solvedEq.IsSolved = true;
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
    X = 0, Y = 1, Z = 2, HUE = 3
}