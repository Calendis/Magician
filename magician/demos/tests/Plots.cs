using Magician.Algo;
using Magician.Library;
using static Magician.Algo.Algebra;

namespace Magician.Demos.Tests;

public class Plots : Spell
{
    public override void PreLoop()
    {
        //Origin["plot0"] = ((IMap)new CustomMap(x => 60 * Math.Sin(x / 6))).Plot(0, 0, 0, 96 * Math.PI, 1, HSLA.RandomVisible());
        //Origin["plot1"] = new IOMap(1, t => 60 * Math.Sin(t), t => 60 * Math.Cos(t / 5)).Plot(-300, -300, 0, 96 * Math.PI, 1, HSLA.RandomVisible());
        //Origin["plot2"] = new IOMap(2, (x, y) => x + Math.Sin(y)).Plot();
        //Origin["plot2"] = new IOMap(2, x => x + Math.Sin(x)).Plot(0, 0, -50, 50, 0.25, HSLA.RandomVisible());

        // Algebra testing
        Oper lhs = new Fraction(new SumDiff(Let("x"), N(0), N(1)), new SumDiff(N(4), Let("y")));
        Oper rhs = new Fraction(Let("z"), N(1), N(3));
        Equation e = new(lhs, Equation.Fulcrum.EQUALS, rhs);
        Scribe.Info($"Original equation: {e}");


        Scribe.Info("Solving for x...");
        Equation sx = e.Solved(Let("x"));
        Scribe.Info(sx);

        //Scribe.Info("Solving for y...");
        //Equation sy = e.Solve(Let("y"));
        //Scribe.Info(sy);

        Scribe.Info("Solving for z...");
        Equation sz = e.Solved(Let("z"));
        Scribe.Info(sz);

        Scribe.Info($"if y=3, z=6, then x={sx.Evaluate(Let("x"), 3, 6)}");
        Scribe.Info($"if y=3, z=7, then x={sx.Evaluate(Let("x"), 3, 7)}");
        Scribe.Info($"if x=17, y=3, then z={sz.Evaluate(Let("z"), 17, 3)}");

        Variable phi =
            new Fraction(N(1),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0),
            new Fraction(N(1), new SumDiff(N(1),N(0)
            ))))))))))))))))))))))))))))))))))))))))))))
            ).Eval();
        Scribe.Info($"phi is roughly equal to {phi}");

        //Oper lhs2 = new SumDiff(Let("x"), N(0), N(3));
        //Oper rhs2 = new Fraction(N(2), Let("y"));
        //Equation e2 = new(lhs2, Equation.Fulcrum.EQUALS, rhs2);
        //Scribe.Info("Equation 2:");
        //Scribe.Info(e2);
        //Equation s2 = e2.Solve(Let("y"));
        //Scribe.Info("Solved for y.");
        //Scribe.Info(s2);

    }

    public override void Loop()
    {
        //
    }
}