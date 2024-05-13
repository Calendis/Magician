namespace Magician.Alg;

using Magician.Core.Maps;
using Symbols;

public partial class Equation
{
    public Equation Rearranged_old(Oper? solveFor = null)
    {
        Equation? rearrEq = null;
        Oper lhCopy = LHS.Copy();
        Oper rhCopy = RHS.Copy();
        // By default, solve for the variable with the highest degree
        if (solveFor == null)
        {
            //Scribe.Info($"Determining minimum-degree unknown for {LHS} = {RHS}");
            Variable? chosenSolveVar = null;
            Oper? minDegree = null;
            foreach (Variable uk in Unknowns)
            {
                Oper deg = new Symbols.Commonfuncs.Max(new Symbols.Commonfuncs.Abs(LHS.Degree(uk)), new Symbols.Commonfuncs.Abs(RHS.Degree(uk)));
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
                    if (deg.Sol().Value.Magnitude == 0)
                        Scribe.Warn($"deg was 0");
                    minDegree = deg;
                    chosenSolveVar = uk;
                }
            }
            if (chosenSolveVar is null)
                throw Scribe.Issue("Could not determine minimum-degree unknown!");
            solveFor = chosenSolveVar;
        }
        Scribe.Info($"Rearranging {this} for {solveFor}");
        (OperLayers LEFT, OperLayers RIGHT) LAYERS = (new(LHS, solveFor), new(RHS, solveFor));
        OperLayers GETLAYER(SolveSide s) { return new List<OperLayers> { LAYERS.LEFT, LAYERS.RIGHT }[(int)s]; }

        // Algebra machine variables
        List<(Instruction, SolveSide, Oper, Oper)> CODE = new()
            {(Instruction.PICK, SolveSide.LEFT, solveFor, Variable.Undefined)};
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
            (Instruction MOD, SolveSide SIDE, Oper VAR, Oper AXIS) INSTRUCTION = CODE[0];
            CODE.RemoveAt(0);

            string STATUS = $"{TOTAL_CHANGES + TOTAL_PICKS}:";
            if (INSTRUCTION.MOD != Instruction.PICK)
                STATUS += $" {CURRENT_PICK}";
            STATUS += $" {INSTRUCTION.MOD} {INSTRUCTION.VAR}";
            if (INSTRUCTION.MOD == Instruction.EXTRACT)
                STATUS += " from";
            else if (INSTRUCTION.MOD != Instruction.PICK)
                STATUS += $" on";
            if (!WAS_PICK)
                Scribe.Info($"  {LAYERS.LEFT.Get(0, 0)} = {LAYERS.RIGHT.Get(0, 0)}");
            if (INSTRUCTION.MOD != Instruction.PICK)
                STATUS += $" the {INSTRUCTION.SIDE} side of the equation:";
            if (INSTRUCTION.MOD != Instruction.PICK)
                Scribe.Info($"{STATUS}\n");

            if (INSTRUCTION.MOD == Instruction.PICK)
            {
                TOTAL_PICKS++;

                Match MS_LEFT = LAYERS.LEFT.GetInfo(0, 0).ms;
                Match MS_RIGHT = LAYERS.RIGHT.GetInfo(0, 0).ms;
                MatchPair MATCH = (MatchPair)((int)MS_LEFT * (int)MS_RIGHT);
                SolveSide MOST_DIRECT_SIDE = MS_LEFT <= MS_RIGHT ? SolveSide.LEFT : SolveSide.RIGHT;

                List<Oper> LIVE_BRANCHES(SolveSide s) => GETLAYER(s).LiveBranches(0, 0);

                switch (MATCH)
                {
                    // TODO: this is not necessarily an error, the variable could have been cancelled
                    case MatchPair.DEAD:
                        throw Scribe.Issue($"No matches for {INSTRUCTION.VAR}");
                    // TODO: handle tautologies
                    case MatchPair.TAUT:
                        throw Scribe.Issue("Tautological equation");
                    // Solved
                    case MatchPair.SOLVED:
                        Oper solvedLeft = LAYERS.LEFT.Get(0, 0).Copy();
                        Oper solvedRight = LAYERS.RIGHT.Get(0, 0).Copy();
                        solvedLeft.Reduce(); solvedRight.Reduce();
                        rearrEq = solvedLeft.Like(solveFor) ? new(solvedLeft, Fulcrum.EQUALS, solvedRight) : new(solvedRight, Fulcrum.EQUALS, solvedLeft);
                        Scribe.Info($"Solved in {TOTAL_CHANGES} operations and {TOTAL_PICKS} picks for {TOTAL_CHANGES + TOTAL_PICKS} total instructions:\n{rearrEq}");
                        SOLVED = true;
                        break;
                    // Solveability known. Create case MatchPairs.SOLVED or approximate
                    case MatchPair.SINGLE:
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
                                throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Concat the solve path to the instructions so that it will be evaluated
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        CURRENT_PICK = (3, 0);
                        break;
                    // Solveability knowable. Attempt to create case MatchPairs.SINGLE by simplifying
                    // If a simplification cannot change the form, approximate
                    case MatchPair.MULTIPLE:
                        // Determine the most common recent ancestor node and invert to that node
                        node = GETLAYER(MOST_DIRECT_SIDE).Get(0, 0);
                        solvePath = new();
                        while (true)
                        {
                            solvePath.Add(node);
                            if (GETLAYER(MOST_DIRECT_SIDE).GetInfo(node).ms == Match.MULTIPLE)
                                break;
                            else if (!node.invertible)
                                throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
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
                            foreach (Oper a in solvePath)
                                PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                            CURRENT_PICK = (4, 2);
                        }
                        break;
                    // Create case MatchPairs.MULTIPLE by extracting from the direct side
                    case MatchPair.PARASINGLE:
                        PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, INSTRUCTION.VAR);
                        CURRENT_PICK = (5, 0);
                        break;
                    // Create case MatchPairs.MULTIPLE by extracting from the direct side
                    case MatchPair.PARAMULTIPLE:
                        PREPAREEXTRACT(MOST_DIRECT_SIDE, INSTRUCTION.VAR, INSTRUCTION.VAR);
                        CURRENT_PICK = (6, 0);
                        break;
                    // Pick inversion path of lesser "degree", up to term
                    case MatchPair.DUAL:
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
                                throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                    // Pick inversion path from SINGLE side, up to term
                    case MatchPair.IMBALANCED:
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
                                throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                    // The hardest case. Attempt to create case MatchPairs.Multiple
                    case MatchPair.FLUID:
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
                                throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                }
                WAS_PICK = true;
                continue;
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

                if (INSTRUCTION.MOD == Instruction.ISOLATE)
                {
                    TOTAL_CHANGES++;
                    if (OPPOSITEROOT[0] is not Invertable)
                    {
                        throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
                    }

                    Oper newNCho, newNOpp;
                    newNOpp = ((Invertable)CHOSENROOT[0]).Inverse(INSTRUCTION.AXIS, OPPOSITEROOT[0]);

                    // Apply changes
                    (NEWCHOSEN, NEWOPPOSITE) = (INSTRUCTION.AXIS, newNOpp.Trim());
                }
                else if (INSTRUCTION.MOD == Instruction.EXTRACT)
                {
                    TOTAL_CHANGES++;
                    (NEWCHOSEN, NEWOPPOSITE) = ExtractOperFrom(CHOSENROOT[0], OPPOSITEROOT[0], INSTRUCTION.AXIS);
                }
                else if (INSTRUCTION.MOD == Instruction.SIMPLIFY)
                {
                    TOTAL_CHANGES++;
                    if (INSTRUCTION.VAR is Variable combV)
                        GETLAYER(INSTRUCTION.SIDE).Get(0, 0).Combine(combV);
                    GETLAYER(INSTRUCTION.SIDE).Get(0, 0).Reduce(2);
                    (NEWCHOSEN, NEWOPPOSITE) = (CHOSENROOT[0].Trim(), OPPOSITEROOT[0].Trim());
                }

                if (NOCHANGE(NEWCHOSEN, NEWOPPOSITE))
                {
                    if (LAST_PICK == CURRENT_PICK && FUEL < 0)
                    {
                        throw Scribe.Error($"Could not rearrange {this} in terms of {solveFor}");
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
        void PREPAREPICK(Oper VAR)
        {
            CODE.Add((Instruction.PICK, SolveSide.EITHER, VAR, Variable.Undefined));
        }
        void PREPAREISOLATE(SolveSide SIDE, Oper VAR, Oper AXIS)
        {
            CODE.Add((Instruction.ISOLATE, SIDE, VAR, AXIS));
        }
        // nice
        void PREPAREEXTRACT(SolveSide SIDE, Oper VAR, Oper AXIS)
        {
            CODE.Add((Instruction.EXTRACT, SIDE, VAR, AXIS));
        }
        void PREPARESIMPLIFY(SolveSide SIDE, Oper VAR)
        {
            CODE.Add((Instruction.SIMPLIFY, SIDE, VAR, Variable.Undefined));
        }

        if (rearrEq is null)
            throw Scribe.Issue("Error in solve loop");
        //solvedEq.IsSolved = true;
        LHS = lhCopy;
        RHS = rhCopy;
        return rearrEq;
    }
    

    // During an extract, the axis crosses the equals sign
    public static (Oper, Oper) ExtractOperFrom(Oper chosenSide, Oper oppositeSide, Oper axis)
    {
        // need extract method
        if (chosenSide is Variable || axis == chosenSide)
            return ExtractOperFrom(new SumDiff(new List<Oper> { chosenSide }, new List<Oper>()), oppositeSide, axis);

        if (chosenSide.posArgs.Contains(axis))
            chosenSide.posArgs.Remove(axis);
        else if (chosenSide.negArgs.Contains(axis))
            chosenSide.negArgs.Remove(axis);
        else
            throw Scribe.Issue($"Could not extract {axis} from {chosenSide.GetType()} {chosenSide}");
        return (chosenSide.Trim(), chosenSide.New(new List<Oper> { oppositeSide }, new List<Oper> { axis }).Trim());
    }
}