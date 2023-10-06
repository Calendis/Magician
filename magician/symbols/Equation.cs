namespace Magician.Symbols;
using Magician.Maps;

public class Equation : RelationalMap
{
    Variable[] unknowns;
    public Variable[] Unknowns => unknowns;
    EquationLayers layers;
    EquationLayers layersBackup;
    List<Variable> isolates;
    public Oper LHS;
    public Oper RHS;
    public Oper Side(bool s) { return s ? LHS : RHS; }
    Fulcrum TheFulcrum { get; set; }
    SolvedEquation? solvedEq = null;
    public Equation(Oper o0, Fulcrum f, Oper o1)
    {
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
        // Can't do this through the base constructor, unfortunately
        map = new Func<double[], double[]>(vals => Approximate(vals));
        ins = unknowns.Length - 1;
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
        BOTHR,  // both sides, but reverts to the right side
        EITHER
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public SolvedEquation Solved(Variable? v = null)
    {
        // By default, solve for the variable with the highest degree
        if (v == null)
        {
            Variable? chosenSolveVar = null;
            double minDegree = double.MaxValue;
            foreach (Variable uk in unknowns)
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

        // Algebra machine variables
        int IDX;
        double CHOSENDEG, OPPOSITEDEG;
        List<Oper> CHOSENROOT;
        List<Oper> OPPOSITEROOT;
        SolveMode MODE;
        SolveSide SIDE;
        Variable VAR;
        List<(SolveMode, SolveSide, Variable)> CODE = new()
            {(SolveMode.PICK, SolveSide.EITHER, v)};
        // These default values don't mean anything. They just need to be invalid
        (int, int) LAST_PICK = (0, -1);
        (int, int) CURRENT_PICK = (0, 0);
        bool WAS_PICK = false;
        Oper? OLDCHOSEN = null, OLDOPPOSITE = null;
        Oper? LIVE_BRANCH_LEFT = null;
        Oper? LIVE_BRANCH_RIGHT = null;
        Oper?[] TWIG = new[] { LIVE_BRANCH_LEFT, LIVE_BRANCH_RIGHT };

        // The algebra solver state machine
        // You should NOT access the state of Equation within this loop, instead referring to layers
        // The layers variable is reassigned each loop
        //int TOTAL_INSTRS = 0;
        int TOTAL_PICKS = 0;
        int TOTAL_CHANGES = 0;
        int MAX_FUEL = 10;
        int FUEL = MAX_FUEL;
        while (true)
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
                Scribe.Info($"  {layers.LeftHand[0][0]} = {layers.RightHand[0][0]}");
            if (MODE != SolveMode.PICK)
                STATUS += $" the {SIDE} side of the equation:";
            Scribe.Info(STATUS);

            if (MODE == SolveMode.PICK)
            {
                TOTAL_PICKS++;
                //WAS_PICK = 2;
                TWIG[0] = null;
                TWIG[1] = null;
                // First, find matches on both sides
                List<Oper> MATCHES_TERMS_LEFT = new();
                List<Oper> MATCHES_OTHER_LEFT = new();
                List<Oper> MATCHES_TERMS_RIGHT = new();
                List<Oper> MATCHES_OTHER_RIGHT = new();
                int[] NON_MATCHES_ALL = new[] { 0, 0 };
                List<Oper>[] MATCHES_TERMS_ALL = { MATCHES_TERMS_LEFT, MATCHES_TERMS_RIGHT };
                List<Oper>[] MATCHES_OTHER_ALL = { MATCHES_OTHER_LEFT, MATCHES_OTHER_RIGHT };

                // Determine matches on either side
                int LR_SWITCH = 0;
                foreach (Dictionary<int, List<Oper>> h in layers.Hands)
                {
                    // Are we a variable?
                    if (h[0][0] is Variable v_ && v_ == VAR)
                    {
                        MATCHES_TERMS_ALL[LR_SWITCH].Add(h[0][0]);
                        TWIG[LR_SWITCH] = VAR;
                    }
                    // Are we a term containing v?
                    else if (h[0][0].IsTerm && h[0][0].Contains(VAR))
                    {
                        MATCHES_TERMS_ALL[LR_SWITCH].Add(h[0][0]);
                        TWIG[LR_SWITCH] = VAR;
                    }
                    foreach (Oper o in h[0][0].AllArgs)
                    {
                        // If we see the variable in question, that counts as a term automatically
                        if (o is Variable v__ && v__ == VAR)
                        {
                            MATCHES_TERMS_ALL[LR_SWITCH].Add(o);
                            TWIG[LR_SWITCH] = o;
                        }
                        // otherwise, test to see if o is a term
                        else if (o.Contains(VAR))
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
                        // otherwise, this didn't match at all
                        else
                        {
                            NON_MATCHES_ALL[LR_SWITCH] = NON_MATCHES_ALL[LR_SWITCH] + 1;
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
                        PREPAREISOLATE(NUMOTHERLEFT == 1 ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 0);
                        PREPARESIMPLIFY(NUMOTHERLEFT > 0 ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 0);
                        PREPARESIMPLIFY(GENBOTH(), VAR);
                    }
                }
                // One direct match on either side
                else if (NUMTERMLEFT + NUMTERMRIGHT == 1)
                {
                    // No indirect matches on either side
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 1);
                        // Solved
                        if (layers.LeftHand[0][0].AllArgs.Count == 0 || layers.RightHand[0][0].AllArgs.Count == 0)
                        {
                            Oper newChosenRoot = layers.LeftHand[0][0].Copy();
                            Oper newOppositeRoot = layers.RightHand[0][0].Copy();
                            solvedEq = new(newChosenRoot, Fulcrum.EQUALS, newOppositeRoot, v, unknowns.Length - 1);
                            Scribe.Info($"Solved in {TOTAL_CHANGES} operations and {TOTAL_PICKS} picks for {TOTAL_CHANGES + TOTAL_PICKS} total instructions: {solvedEq}");
                            break;
                        }
                        PREPAREISOLATE(layers.LeftHand[0][0].Degree(v) != 0 ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 1);
                        SolveSide newSide = NUMOTHERLEFT > 0 ? SolveSide.LEFT : SolveSide.RIGHT;
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) != 0 && NON_MATCHES_ALL[(int)newSide] == 0)
                        {
                            PREPAREEXTRACT(newSide, VAR);
                        }
                        else
                        {
                            PREPARESIMPLIFY(newSide, VAR);
                        }
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 1);
                        SolveSide newSide = NUMOTHERLEFT > 0 ? SolveSide.LEFT : SolveSide.RIGHT;
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) != 0 && NON_MATCHES_ALL[(int)newSide] == 0)
                        {
                            PREPAREEXTRACT(newSide, VAR);
                        }
                        else
                        {
                            PREPARESIMPLIFY(newSide, VAR);
                        }
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 1);
                        //PREPAREEXTRACT(SMALLESTCHAFFDEGREE(), VAR);
                        PREPARESIMPLIFY(GENBOTH(), VAR);
                    }
                }
                // Multiple direct matches on one side
                else if (NUMTERMLEFT * NUMTERMRIGHT == 0)
                {
                    // No indirect matches
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 2);
                        PREPARESIMPLIFY(NUMTERMLEFT > 0 ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 2);
                        // Same side?
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) == 0)
                        {
                            PREPARESIMPLIFY(NUMOTHERLEFT > 0 ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                        }
                        else
                        {
                            PREPARESIMPLIFY(GENBOTH(), VAR);
                        }
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 2);
                        // Same side?
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) == 0)
                        {
                            PREPARESIMPLIFY(NUMOTHERLEFT > 0 ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                        }
                        else
                        {
                            PREPARESIMPLIFY(GENBOTH(), VAR);
                        }
                    }
                    // At least one indirect match on either side
                    else
                    {
                        CURRENT_PICK = (3, 2);
                        PREPARESIMPLIFY(GENBOTH(), VAR);
                    }
                }
                // At least one direct match on both sides
                else
                {
                    // No indirect matches
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 3);
                        PREPAREEXTRACT(layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 3);
                        PREPAREEXTRACT(layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 3);
                        PREPAREEXTRACT(layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 3);
                        PREPAREEXTRACT(layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SolveSide.LEFT : SolveSide.RIGHT, VAR);
                    }
                }
                WAS_PICK = true;
                continue;
            }
            // Manipulate the tree in favour of being solved
            else
            {
                WAS_PICK = false;
                if (SIDE == SolveSide.BOTHL)
                {
                    CHOSENROOT = layers.LeftHand[0];
                    OPPOSITEROOT = layers.RightHand[0];
                }
                else if (SIDE == SolveSide.BOTHR)
                {
                    CHOSENROOT = layers.RightHand[0];
                    OPPOSITEROOT = layers.LeftHand[0];
                }
                else
                {
                    CHOSENROOT = layers.Hands[(int)SIDE][0];
                    OPPOSITEROOT = layers.Hands[1 - (int)SIDE][0];
                }
                CHOSENDEG = CHOSENROOT[0].Degree(VAR);
                NEWCHOSEN = CHOSENROOT[0];
                NEWOPPOSITE = OPPOSITEROOT[0];

                if (MODE == SolveMode.ISOLATE)
                {
                    TOTAL_CHANGES++;
                    if (TWIG[(int)SIDE] is null)
                        throw Scribe.Issue($"No live branch to isolate axis from");
                    (NEWCHOSEN, NEWOPPOSITE) = Oper.IsolateOperOn(CHOSENROOT[0], OPPOSITEROOT[0], TWIG[(int)SIDE]!, VAR);
                }
                else if (MODE == SolveMode.EXTRACT)
                {
                    TOTAL_CHANGES++;
                    if (TWIG[(int)SIDE] is null)
                        throw Scribe.Issue("No live branch to extract axis from");
                    (NEWCHOSEN, NEWOPPOSITE) = Oper.ExtractOperFrom(CHOSENROOT[0], OPPOSITEROOT[0], TWIG[(int)SIDE]!);
                }
                else if (MODE == SolveMode.SIMPLIFY)
                {
                    TOTAL_CHANGES++;
                    if (SIDE == SolveSide.BOTHL || SIDE == SolveSide.BOTHR)
                    {
                        layers.LeftHand[0][0].Associate();
                        layers.LeftHand[0][0].Simplify(v);
                        layers.LeftHand[0][0].Commute();
                        layers.RightHand[0][0].Associate();
                        layers.RightHand[0][0].Simplify(v);
                        layers.RightHand[0][0].Commute();
                    }
                    else
                    {
                        layers.Hands[(int)SIDE][0][0].Associate();
                        layers.Hands[(int)SIDE][0][0].Simplify(v);
                        layers.Hands[(int)SIDE][0][0].Commute();
                    }
                }

                // If no progress was made, enter PICK mode
                if (NOCHANGE(NEWCHOSEN, NEWOPPOSITE))
                {
                    if (LAST_PICK == CURRENT_PICK || FUEL < 0)
                    {
                        throw Scribe.Issue($"Failed to solve to equation for {v} with {FUEL} fuel remaining. Implement approximator");
                    }
                    PREPAREPICK(VAR);
                    FUEL--;
                }
                // also pick if our live branch isn't present in our chosen side
                else if (!NEWCHOSEN.AllArgs.Contains(TWIG[(int)SIDE]!))
                {
                    PREPAREPICK(VAR);
                }
                // also PICK if we isolated or simplified
                else if (MODE == SolveMode.ISOLATE || MODE == SolveMode.SIMPLIFY)
                {
                    PREPAREPICK(VAR);
                }
                
                // Otherwise, keep extracting
                else
                {
                    CODE.Add(INSTRUCTION);
                }
            }

            LAST_PICK = CURRENT_PICK;
            OLDCHOSEN = NEWCHOSEN.Copy();
            OLDOPPOSITE = NEWOPPOSITE.Copy();
            if (SIDE == SolveSide.LEFT)
                layers = new(NEWCHOSEN, NEWOPPOSITE);
            else
                layers = new(NEWOPPOSITE, NEWCHOSEN);
        }

        /* Algebra machine helper methods */
        SolveSide SMALLESTCHAFFDEGREE()
        {
            List<Variable> chaffVars = unknowns.Where(b => b != VAR).ToList();
            (double, double) totalDegrees = (0, 0);
            foreach (Variable otr in chaffVars)
            {
                double degL, degR;
                degL = layers.LeftHand[0][0].Degree(VAR);
                degR = layers.RightHand[0][0].Degree(VAR);
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

        // Write the next instruction
        void PREPAREPICK(Variable targ)
        {
            CODE.Add((SolveMode.PICK, SolveSide.EITHER, targ));
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

        // Revert
        layers = layersBackup;
        layersBackup = new(layersBackup.LeftHand[0][0].Copy(), layersBackup.RightHand[0][0].Copy());

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
        return Approximate(vals, unknowns[0]);
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