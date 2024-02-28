namespace Magician.Alg;
using Symbols;
using Core;
using Core.Maps;

public class Approx : IRelation
{
    public IVal Cache { get; private set; }
    public Dictionary<double[], IVal> ioCache = new();
    public Equation Equation => eq;
    // TODO: Store IO pairs so that we don't need to re-approximate values for the same inputs
    // The equation representing the implicit relation
    readonly Equation eq;
    Variable Output => eq.Unknowns[^1];
    // The function is the difference between the two sides
    Oper func;
    Oper funcCopy;
    //public int Ins => eq.Unknowns.Count - eq.Sliders.Count - 1;
    public int Ins => func.Ins - 1;
    public int DerivIns => derivative.Ins;
    // Derivative of the function, used for Newton's method
    readonly Oper derivative;
    readonly int maxIters;
    readonly IVal prevGuess = new Val(0);
    readonly IVal guess = new Val(0);
    readonly IVal newGuess = new Val(0);
    readonly IVal damping = new Val(1);
    // We re-use this for whatever. We avoid Val instantiations this way
    readonly IVal term = new Val(0);
    bool useNewton = true;

    public Approx(Oper lhs, Fulcrum fulc, Oper rhs, int maxIters = 12)
    {
        Cache = new Val(0);
        eq = new(lhs, fulc, rhs);
        this.maxIters = maxIters;
        func = rhs.Minus(lhs).Canonical();
        funcCopy = func.Copy();
        derivative = new Derivative(func, Output).Canonical();

        // If the derivative is zero, we can rule-out Newton's method immediately
        if (derivative.Ins == 0 && derivative.Sol().Value.EqValue(0))
            useNewton = false;
    }

    public IVal Evaluate(params double[] vals)
    {
        if (vals.Length != Ins)
            throw Scribe.Error($"Got {vals.Length} inputs, expected {Ins}");
        //List<double> rawInVals = vals.Take(Ins).ToList();
        int counter = 0;
        // The derivative can have a smaller number of variables than the original function, so we need a separate argument array for it
        (string, IVal)[] funcInVals = func.AssociatedVars.Except(new List<Variable> { Output }).Select(v => (v.Name, (IVal)new Core.Val(vals[counter++]))).ToArray();
        // This is a bit gross...
        (string, IVal)[] derivInVals = funcInVals.Where(iv => derivative.AssociatedVars.Except(new List<Variable> { Output }).Select(v => v.Name).Contains(iv.Item1)).ToArray();


        //Equation simplerEq = new(func, Fulcrum.EQUALS, new Variable(0));
        //Scribe.Info($"Relation {eq} at point {Scribe.Expand<List<double>, double>(funcInVals.Select(tp => tp.Item2.Get()).ToList())}");
        funcInVals.ToList().ForEach(tp => func.Substitute(tp.Item1, tp.Item2));

        ///Scribe.Info($"Can we solve a simpler relation {simplerEq}?");
        ///bool solveable = simplerEq.TrySolve(Output);
        ///if (solveable)
        ///{
        ///    IRelation solved = simplerEq.Solved(Output);
        ///    return solved.Evaluate(vals);
        ///}
        //Scribe.Info($"Not solveable at point {Scribe.Expand<List<double>, double>(funcInVals.Select(tp => tp.Item2.Get()).ToList())}");

        // Pick an initial guess for the approximation
        double initialGuess = 1;
        double minMag = double.MaxValue;
        bool positiveFound = false;
        bool negativeFound = false;
        bool zeroFound = false;
        for (double i = -5; i < 5; i += 0.05)
        {
            IVal iv = func.Evaluate(i);
            if (iv.Magnitude < minMag)
            {
                initialGuess = i;
                minMag = iv.Magnitude;
            }
            if (iv.Get() > 0)
                positiveFound = true;
            else if (iv.Get() < 0)
                negativeFound = true;
            else if (iv.Magnitude == 0)
                zeroFound = true;
        }
        // No sign change indicates no solution
        if (!((positiveFound && negativeFound) || (positiveFound && zeroFound) || (negativeFound && zeroFound)))
        {
            func.AssociatedVars.ForEach(v => v.Reset());
            Output.Reset();
            return new Val(double.NaN);
        }

        guess.Set(initialGuess);

        int currIter = 0;
        double updateThreshold = 0.000001;
        // Iteratively improve the inital guess
        while (currIter < maxIters)
        {
            Output.Set(guess.Values.ToArray());
            IVal dVal = derivative.Evaluate(derivInVals);
            Output.Set(guess.Values.ToArray());
            if (dVal.EqValue(0))
                useNewton = false;
            if (useNewton)
            {
                //damping.Set(1);
                IVal deltaX = IVal.Divide(func.Evaluate(funcInVals), dVal);
                if (IVal.Multiply(deltaX, guess).Get() > 0)
                    IVal.Multiply(deltaX, -1, deltaX);
                if (deltaX.Trim().Values.Count > 1)
                    throw Scribe.Issue($"TODO: handle complex deltas {deltaX}");

                IVal.Add(guess, IVal.Multiply(damping, deltaX, term), newGuess);

                while (newGuess.Magnitude > guess.Magnitude)
                {
                    //Scribe.Info($"{newGuess} vs. {guess}. dx={deltaX}, damping={damping}");
                    if (newGuess.Trim().Values.Count > 1)
                        IVal.Divide(damping, new Val(0, 2), damping);
                    else
                        IVal.Divide(damping, 2, damping);
                    IVal.Add(guess, IVal.Multiply(damping, deltaX, term), newGuess);
                }

                prevGuess.Set(guess);
                guess.Set(newGuess);
            }
            else
            {
                throw Scribe.Issue($"TODO: implement secant method");
                //IVal newGuess = IVal.Divide(IVal.Multiply(prevGuess, diffV, innerTerm), IVal.Subtract(guess, prevGuess, innerTerm2));
                //prevGuess.Set(guess);
                //guess.Set(newGuess);
            }
            if (IVal.Subtract(prevGuess, guess, term).Magnitude < updateThreshold)
                break;
            currIter++;
        }

        //Scribe.Info($"\tGot point ({funcInVals[0].Item2}, {funcInVals[1].Item2}, {guess})\n");
        func = funcCopy;
        func.AssociatedVars.ForEach(v => v.Reset());
        Output.Reset();
        return guess;
    }
}