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

    internal enum ModePick
    {
        PICK,
        ISOLATE,
        EXTRACT,
        EXPAND,
    }
    internal enum SidePick
    {
        LEFT,
        RIGHT,
        BOTH
    }

    // Re-arrange and reconstruct the equation in terms of a certain variable
    public SolvedEquation Solved(Variable? v = null)
    {
        Scribe.Info($"Solving {this} for {v}");
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

        // Algebra machine variables
        int IDX;
        double CHOSENDEG, OPPOSITEDEG;
        List<Oper> CHOSENROOT;
        List<Oper> OPPOSITEROOT;
        ModePick MODE;
        SidePick SIDE;
        Variable VAR;
        List<(ModePick, SidePick, Variable?)> CODE = new()
            {(ModePick.PICK, SidePick.LEFT, v)};
        // These default values don't mean anything. They just need to be invalid
        (int, int) LAST_PICK = (-1, 0);
        (int, int) CURRENT_PICK = (0, -1);
        Oper? OLDCHOSEN = null, OLDOPPOSITE = null;
        Oper? LIVE_BRANCH_LEFT = null;
        Oper? LIVE_BRANCH_RIGHT = null;
        Oper?[] TWIG = new[] { LIVE_BRANCH_LEFT, LIVE_BRANCH_RIGHT };

        // The algebra solver state machine
        // You should NOT access the state of Equation within this loop, instead referring to layers
        // The layers variable is reassigned each loop
        int totalInstructions = 0;
        while (true)
        {
            if (CODE.Count < 1)
                throw Scribe.Issue("No instructions provided");

            (ModePick, SidePick, Variable) INSTRUCTION = CODE[0];
            CODE.RemoveAt(0);
            totalInstructions++;

            MODE = INSTRUCTION.Item1;
            SIDE = INSTRUCTION.Item2;
            VAR = INSTRUCTION.Item3;
            //Scribe.Info($"Var: {VAR}");

            CHOSENROOT = layers.Hands[(int)SIDE][0];
            OPPOSITEROOT = layers.Hands[1 - (int)SIDE][0];
            CHOSENDEG = CHOSENROOT[0].Degree(VAR);

            string STATUS = $"{MODE} {VAR}";
            if (MODE != ModePick.PICK)
            {
                STATUS += $" on the {SIDE} side";
            }
            //Scribe.Info(STATUS);
            //Scribe.Info($"{layers.LeftHand[0][0]} = {layers.RightHand[0][0]}");

            if (MODE == ModePick.PICK)
            {
                TWIG[0] = null;
                TWIG[1] = null;
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
                    Oper newChosenRoot = CHOSENROOT[0].Copy();
                    Oper newOppositeRoot = OPPOSITEROOT[0].Copy();
                    solvedEq = new(newChosenRoot, Fulcrum.EQUALS, newOppositeRoot, v, unknowns.Length-1);
                    Scribe.Info($"Solved in {totalInstructions} instructions: {solvedEq}");
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
                        CODE.Add((ModePick.ISOLATE, NUMOTHERLEFT == 1 ? SidePick.LEFT : SidePick.RIGHT, VAR));
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 0);
                        CODE.Add((ModePick.EXPAND, NUMOTHERLEFT > 0 ? SidePick.LEFT : SidePick.RIGHT, v));
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 0);
                        CODE.Add((ModePick.EXPAND, SidePick.BOTH, v));
                    }
                }
                // One direct match on either side
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

                        CODE.Add((ModePick.ISOLATE, CHOSENDEG == 0 ? 1 - SIDE : SIDE, VAR));
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 1);
                        // Same side?
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) == 0)
                        {
                            CODE.Add((ModePick.EXPAND, NUMOTHERLEFT > 0 ? SidePick.LEFT : SidePick.RIGHT, v));
                        }
                        else
                        {
                            CODE.Add((ModePick.EXTRACT, SMALLESTCHAFFDEGREE(), VAR));
                        }
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 1);
                        // Same side?
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) == 0)
                        {
                            CODE.Add((ModePick.EXPAND, NUMOTHERLEFT > 0 ? SidePick.LEFT : SidePick.RIGHT, v));
                        }
                        else
                        {
                            CODE.Add((ModePick.EXTRACT, SMALLESTCHAFFDEGREE(), VAR));
                        }
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 1);
                        CODE.Add((ModePick.EXTRACT, SMALLESTCHAFFDEGREE(), VAR));
                    }
                }
                // Multiple direct matches on one side
                else if (NUMTERMLEFT * NUMTERMRIGHT == 0)
                {
                    // No indirect matches
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 2);
                        CODE.Add((ModePick.EXPAND, NUMTERMLEFT > 0 ? SidePick.LEFT : SidePick.RIGHT, v));
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 2);
                        // Same side?
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) == 0)
                        {
                            CODE.Add((ModePick.EXPAND, NUMOTHERLEFT > 0 ? SidePick.LEFT : SidePick.RIGHT, v));
                        }
                        else
                        {
                            CODE.Add((ModePick.EXPAND, SidePick.BOTH, VAR));
                        }
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 2);
                        // Same side?
                        if ((NUMTERMLEFT ^ NUMOTHERLEFT) == 0)
                        {
                            CODE.Add((ModePick.EXPAND, NUMOTHERLEFT > 0 ? SidePick.LEFT : SidePick.RIGHT, v));
                        }
                        else
                        {
                            CODE.Add((ModePick.EXPAND, SidePick.BOTH, VAR));
                        }
                    }
                    // At least one indirect match on either side
                    else
                    {
                        CURRENT_PICK = (3, 2);
                        CODE.Add((ModePick.EXPAND, SidePick.BOTH, v));
                    }
                }
                // At least one direct match on both sides
                else
                {
                    // No indirect matches
                    if (NUMOTHERLEFT + NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (0, 3);
                        CODE.Add((ModePick.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SidePick.LEFT : SidePick.RIGHT, VAR));
                    }
                    // One indirect match on either side
                    else if (NUMOTHERLEFT + NUMOTHERRIGHT == 1)
                    {
                        CURRENT_PICK = (1, 3);
                        CODE.Add((ModePick.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SidePick.LEFT : SidePick.RIGHT, VAR));
                    }
                    // Multiple indirect matches on one side
                    else if (NUMOTHERLEFT * NUMOTHERRIGHT == 0)
                    {
                        CURRENT_PICK = (2, 3);
                        CODE.Add((ModePick.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SidePick.LEFT : SidePick.RIGHT, VAR));
                    }
                    // At least one indirect match on both sides
                    else
                    {
                        CURRENT_PICK = (3, 3);
                        CODE.Add((ModePick.EXTRACT, layers.LeftHand[0][0].AllArgs.Count <= layers.RightHand[0][0].AllArgs.Count ? SidePick.LEFT : SidePick.RIGHT, VAR));
                    }
                }
            }

            // Manipulate the tree in favour of being solved
            Oper NEWCHOSEN = CHOSENROOT[0];
            Oper NEWOPPOSITE = OPPOSITEROOT[0];
            if (MODE == ModePick.ISOLATE)
            {
                if (TWIG[(int)SIDE] is null)
                    throw Scribe.Issue("No live branch to isolate axis from");
                (NEWCHOSEN, NEWOPPOSITE) = Oper.IsolateOperOn(CHOSENROOT[0], OPPOSITEROOT[0], TWIG[(int)SIDE]!, VAR);
            }
            else if (MODE == ModePick.EXTRACT)
            {
                if (TWIG[(int)SIDE] is null)
                    throw Scribe.Issue("No live branch to extract axis from");
                (NEWCHOSEN, NEWOPPOSITE) = Oper.ExtractOperFrom(CHOSENROOT[0], OPPOSITEROOT[0], TWIG[(int)SIDE]!);
            }
            else if (MODE == ModePick.EXPAND)
            {
                if (SIDE == SidePick.BOTH)
                {
                    CHOSENROOT[0].Associate();
                    OPPOSITEROOT[0].Associate();
                    CHOSENROOT[0].Simplify(v);
                    OPPOSITEROOT[0].Simplify(v);

                }
                else
                {
                    layers.Hands[(int)SIDE][0][0].Associate();
                    layers.Hands[(int)SIDE][0][0].Simplify(v);
                }
            }

            if (CODE.Count == 0)
            {
                // If no progress was made, enter PICK mode
                if (NOCHANGE(NEWCHOSEN, NEWOPPOSITE))
                {
                    if (LAST_PICK == CURRENT_PICK)
                    {
                        throw Scribe.Issue($"The equation could not be solved for {v}. Implement approximator");
                    }
                    CODE.Add((ModePick.PICK, SIDE, VAR));
                }
                // also PICK if we just isolated something
                else if (MODE == ModePick.ISOLATE)
                {
                    CODE.Add((ModePick.PICK, SIDE, VAR));
                }
                // also pick if our live branch isn't present in our chosen side
                else if (!CHOSENROOT[0].AllArgs.Contains(TWIG[(int)SIDE]!))
                {
                    CODE.Add((ModePick.PICK, SIDE, VAR));
                }
                // Otherwise, just keep doing whatever we were doing
                else
                {
                    CODE.Add(INSTRUCTION);
                }
            }

            LAST_PICK = CURRENT_PICK;
            OLDCHOSEN = NEWCHOSEN.Copy();
            OLDOPPOSITE = NEWOPPOSITE.Copy();
            if (SIDE == SidePick.LEFT)
                layers = new(NEWCHOSEN, NEWOPPOSITE);
            else
                layers = new(NEWOPPOSITE, NEWCHOSEN);
        }

        /* Algebra machine helper methods */
        SidePick SMALLESTCHAFFDEGREE()
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
            return totalDegrees.Item1 < totalDegrees.Item2 ? SidePick.LEFT : SidePick.RIGHT;
        }
        bool NOCHANGE(Oper a, Oper b)
        {
            if (OLDCHOSEN is null || OLDOPPOSITE is null)
                return false;
            return a.Like(OLDCHOSEN) && b.Like(OLDOPPOSITE);
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