namespace Magician.Alg;

using Magician.Core.Maps;
using Symbols;

public partial class Equation
{
    internal enum Instruction
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
    internal enum Match
    {
        DIRECT = 1,
        SINGLE = 2,
        MULTIPLE = 3,
        NONE = 5
    }
    internal enum MatchPair
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
    internal enum SolveState
    {
        UNMODIFIED = -1,
        UNSOLVED = 1,
        APPROX = 2,
        SPECIFIC = 3,
        SOLVED = 4
    }
    class Solver
    {
        Oper? OLDCHOSEN = null, OLDOPPOSITE = null;
        Oper solveFor;
        public Oper LHS;
        public Fulcrum FULCRUM;
        public Oper RHS;
        Oper lhCopy;
        Oper rhCopy;

        (OperLayers LEFT, OperLayers RIGHT) LAYERS;
        // Algebra machine variables
        List<(Instruction, SolveSide, Oper, Oper)> CODE;
        // These default values don't mean anything. They just need to be invalid
        (int, int) LAST_PICK = (0, -2);
        (int, int) CURRENT_PICK = (0, -1);
        bool WAS_PICK = false;

        // The algebra solver state machine
        // You should NOT access the state of Equation within this loop, instead referring to layers
        // The layers variable is reassigned each loop
        public IRelation Relation => solvedRel is not null ? solvedRel : throw Scribe.Error($"Could not get solve relation. ({SOLVED}:{TOTAL_PICKS}, {TOTAL_CHANGES}, {TOTAL_PICKS + TOTAL_CHANGES})");
        int TOTAL_PICKS = 0;
        int TOTAL_CHANGES = 0;
        int MAX_FUEL = 3;
        int FUEL;
        SolveState SOLVED = SolveState.UNMODIFIED;
        IRelation? solvedRel = null;
        public Solver(Oper LHS, Oper RHS, Variable? v = null)
        {
            this.LHS = LHS;
            this.RHS = RHS;

            // By default, solve for the variable with the lowest positive degree
            if (v == null)
            {
                //Scribe.Info($"Determining minimum-degree unknown for {LHS} = {RHS}");
                Variable? chosenSolveVar = null;
                Oper? minDegree = null;
                foreach (Variable uk in LHS.AssociatedVars.Concat(RHS.AssociatedVars).ToList())
                {
                    Oper deg = new Symbols.Commonfuncs.Max(new Symbols.Commonfuncs.Abs(LHS.Degree(uk)), new Symbols.Commonfuncs.Abs(RHS.Degree(uk)));
                    if (minDegree == null)
                    {
                        minDegree = deg;
                        chosenSolveVar = uk;
                    }
                    else if (!(deg > minDegree))
                    {
                        if (deg.Sol().Value.Magnitude == 0)
                            Scribe.Warn($"Solve-for degree is 0");
                        minDegree = deg;
                        chosenSolveVar = uk;
                    }
                }
                if (chosenSolveVar is null)
                    throw Scribe.Issue("Could not determine minimum-degree unknown!");
                v = chosenSolveVar;
            }

            this.solveFor = v;

            LAYERS = (new(LHS, solveFor), new(RHS, solveFor));
            // Algebra machine variables
            CODE = new()
            {(Instruction.PICK, SolveSide.LEFT, solveFor, Variable.Undefined)};
            FUEL = MAX_FUEL;
            lhCopy = LHS.Copy();
            rhCopy = RHS.Copy();
        }

        public void Run(int steps = -1)
        {
            if (steps >= 0)
            {
                for (int i = 0; i < steps; i++)
                {
                    Next();
                }
            }
            else
            {
                Scribe.Info($"Solving {this} for {solveFor}");
                while ((int)SOLVED < 1)
                {
                    Next();
                }
            }
        }

        public void Next()
        {
            if (CODE.Count < 1)
                throw Scribe.Issue("No instructions provided");
            Oper NEWCHOSEN;
            Oper NEWOPPOSITE;

            // Get the current instruction from the queue
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

            // Determine the next step
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
                        solvedRel = solvedLeft is Variable sv ? new SolvedEquation(sv, FULCRUM, solvedRight) : new((Variable)solvedRight, FULCRUM, solvedLeft);
                        SOLVED = SolveState.SOLVED;
                        LHS = lhCopy;
                        RHS = rhCopy;
                        Scribe.Info($"{SOLVED} in {TOTAL_CHANGES} operations and {TOTAL_PICKS} picks for {TOTAL_CHANGES + TOTAL_PICKS} total instructions:\n{solvedRel}");
                        return;
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
                            {
                                solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                                SOLVED = SolveState.APPROX;
                                LHS = lhCopy;
                                RHS = rhCopy;
                                return;
                            }
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
                            {
                                solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                                SOLVED = SolveState.APPROX;
                                LHS = lhCopy;
                                RHS = rhCopy;
                                return;
                            }
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
                            PREPARESIMPLIFY(1 - MOST_DIRECT_SIDE, INSTRUCTION.VAR);
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
                            {
                                solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                                SOLVED = SolveState.APPROX;
                                LHS = lhCopy;
                                RHS = rhCopy;
                                return;
                            }
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
                            {
                                solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                                SOLVED = SolveState.APPROX;
                                LHS = lhCopy;
                                RHS = rhCopy;
                                return;
                            }
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
                            {
                                solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                                SOLVED = SolveState.APPROX;
                                LHS = lhCopy;
                                RHS = rhCopy;
                                return;
                            }
                            node = GETLAYER(MOST_DIRECT_SIDE).LiveBranches(node)[0];
                        }
                        // Evaluate the solve path
                        foreach (Oper a in solvePath)
                            PREPAREISOLATE(MOST_DIRECT_SIDE, INSTRUCTION.VAR, a);
                        break;
                }
                WAS_PICK = true;
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
                        solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                        SOLVED = SolveState.APPROX;
                        LHS = lhCopy;
                        RHS = rhCopy;
                        return;
                    }
                    // One side of the equation is a fraction or product, and the other side is 0
                    // We do not want to divide both sides, we want to find each solution
                    else if (CHOSENROOT[0] is Fraction && OPPOSITEROOT[0].IsDetermined && OPPOSITEROOT[0].Sol().Value.EqValue(0))
                    {
                        //throw Scribe.Issue("factor zeroes");
                        List<Oper> factors = CHOSENROOT[0].posArgs.Where(f => !f.IsDetermined).ToList();
                        //Dictionary<HashSet<Variable>, List<Oper>> factorsByArgs = new();
                        //foreach (Oper factor in factors)
                        //{
                        //    List<Variable> key = factor.Ass
                        //}
                        // the negArgs could be used to get inequalities
                        IRelation[] solutions = new IRelation[factors.Count];
                        for (int i = 0; i < factors.Count; i++)
                        {
                            Solver solver = new(factors[i].Copy(), new Variable(0));
                            solver.Run();
                            solutions[i] = solver.Relation;
                        }
                        solvedRel = new Multivalue(solutions.ToList().Select(f => f.Evaluate()).ToArray());
                        SOLVED = SolveState.SOLVED;
                        LHS = lhCopy;
                        RHS = rhCopy;
                        return;
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
                    NEWCHOSEN = CHOSENROOT[0].Trim();
                    NEWOPPOSITE = OPPOSITEROOT[0].Trim();
                }

                if (NOCHANGE(NEWCHOSEN, NEWOPPOSITE))
                {
                    if (LAST_PICK == CURRENT_PICK && FUEL < 0)
                    {
                        //throw Scribe.Issue($"Failed to solve to equation for {v}");
                        solvedRel = new Approx(LHS.Copy(), FULCRUM, RHS.Copy());
                        SOLVED = SolveState.APPROX;
                        LHS = lhCopy;
                        RHS = rhCopy;
                        return;
                    }
                    FUEL--;
                }
                if (CODE.Count == 0)
                    PREPAREPICK(INSTRUCTION.VAR);
                LAST_PICK = CURRENT_PICK;
                OLDCHOSEN = NEWCHOSEN.Copy();
                OLDOPPOSITE = NEWOPPOSITE.Copy();
                if (INSTRUCTION.SIDE == SolveSide.LEFT)
                    LAYERS = (new(NEWCHOSEN, INSTRUCTION.VAR), new(NEWOPPOSITE, INSTRUCTION.VAR));
                else
                    LAYERS = (new(NEWOPPOSITE, INSTRUCTION.VAR), new(NEWCHOSEN, INSTRUCTION.VAR));
            }
        }

        OperLayers GETLAYER(SolveSide s) { return new List<OperLayers> { LAYERS.LEFT, LAYERS.RIGHT }[(int)s]; }
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
    }
}