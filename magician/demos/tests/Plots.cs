using Magician.Symbols;
using Magician.Library;
using static Magician.Symbols.Algebra;

namespace Magician.Demos.Tests;

public class Plots : Spell
{
    public override void PreLoop()
    {
        //Origin["plot0"] = ((IMap)new CustomMap(x => 60 * Math.Sin(x / 6))).Plot(0, 0, 0, 96 * Math.PI, 1, HSLA.RandomVisible());
        Origin["plot1"] = new ParamMap(t => 60 * Math.Sin(t), t => 60 * Math.Cos(t / 5))
            .Plot(-300, 0, 0, 0, 96 * Math.PI, 1, HSLA.RandomVisible());
        Origin["plot2"] = new ParamMap(t => 60 * Math.Sin(t), t => 60 * Math.Cos(t / 5))
            .Plot(300, 0, 0, 0, 96 * Math.PI, 1, HSLA.RandomVisible());
        Origin["spring"] = new ParamMap(t => 60*Math.Sin(t), t => 60*Math.Cos(t) + t*4, t => t*40)
            .Plot(0, 0, 0, 0, 120*Math.PI, 0.1, new RGBA(0x00ffff));
        //Origin["plot2"] = new IOMap(2, (x, y) => x + Math.Sin(y)).Plot();
        //Origin["plot2"] = new IOMap(2, x => x + Math.Sin(x)).Plot(0, 0, -50, 50, 0.25, HSLA.RandomVisible());

        // Algebra testing
        //Oper lhs = new Fraction(new SumDiff(Let("x"), N(0), N(1)), new SumDiff(N(4), Let("y")));
        //Oper rhs = new Fraction(Let("z"), N(1), N(3));
        //Equation e = new(lhs, Equation.Fulcrum.EQUALS, rhs);
        //Scribe.Info($"Original equation: {e}");
        //Scribe.Info("Solving for x...");
        //Equation sx = e.Solved(Let("x"));
        //Scribe.Info(sx);
        //Scribe.Info("Solving for y...");
        //Equation sy = e.Solve(Let("y"));
        //Scribe.Info(sy);
        //Scribe.Info("Solving for z...");
        //Equation sz = e.Solved(Let("z"));
        //Scribe.Info(sz);

        Oper lhs = Let("y");
        Oper rhs = new Fraction(N(100), Let("x"), Let("z"));
        Equation e = new(lhs, Equation.Fulcrum.EQUALS, rhs);
        Origin["plotEq"] = e.Plot(1.1,
            (Let("y"), Equation.AxisSpecifier.Y, -1000d, 1000d),
            (Let("x"), Equation.AxisSpecifier.X, -1000d, 1000d),
            (Let("z"), Equation.AxisSpecifier.Z, 0, 100)
        );

        //Equation threePlane = new(
        //    Let("z"),
        //    Equation.Fulcrum.EQUALS,
        //    new SumDiff(new Fraction(N(3), N(0), Let("x")), N(0), new Fraction(N(2), N(0), Let("y")))
        //);
        //Origin["threeplane"] = threePlane.Plot(1.1,
        //    (Let("x"), Equation.AxisSpecifier.X, -600, 600),
        //    (Let("y"), Equation.AxisSpecifier.Y, -600, 600),
        //    (Let("z"), Equation.AxisSpecifier.Z, -600, 600)
        //);

    }

    public override void Loop()
    {
        //
    }
}