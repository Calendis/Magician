namespace Magician.Tests;
using Magician.Symbols;
using static Magician.Symbols.Notate;
using NUnit.Framework;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MultiplyTwoAndTen()
    {
        Oper twoTimesTen = new Fraction(Val(2), Val(1), Val(10));
        Variable result = twoTimesTen.Solution();
        Assert.That(result.Val, Is.EqualTo(20));
    }

    [Test]
    public void AssociateSumDiffs()
    {
        SumDiff sd = new(
            new List<Oper> { new SumDiff(Val(2)), new SumDiff(Val(1), Val(2), new SumDiff(Val(3)), Val(4)), Val(1), new SumDiff(Val(3)) },
            new List<Oper> { Val(5), new SumDiff(Val(1), new SumDiff(Val(5), new SumDiff(Val(3)))) }
        );
        Assert.That(sd.Solution().Val, Is.EqualTo(0));
        sd.Associate();
        Assert.That(sd.Solution().Val, Is.EqualTo(0));
    }

    [Test]
    public void CombineConstants()
    {
        SumDiff sd = new(Val(10), Val(6));
        Scribe.Info(sd);
        sd.Simplify(Val(0));
        Assert.That(sd.AllArgs[0].Solution().Val, Is.EqualTo(Val(4).Solution().Val));
        Assert.That(sd.AllArgs, Has.Count.EqualTo(1));
        Scribe.Info(sd);
    }

    [Test]
    public void SolveAddLike()
    {
        Equation unsolved = new(
            new SumDiff(
                new Fraction(Var("x"), Val(1), Var("y")),
                new Fraction(Var("x"), Val(1), Var("y"), Val(1), Var("x")),
                new Fraction(Var("y"), Val(1), Var("z")),
                new Fraction(Var("y"), Val(1), Var("z"), Val(1), Var("z"))
            ),
            Fulcrum.EQUALS,
            Val(1)
        );
        SolvedEquation solved = unsolved.Solved();
        SolvedEquation manuallySolved = new(
            Var("y"),
            Fulcrum.EQUALS,
            new Fraction(
                Val(1),
                new SumDiff(
                    Var("x"),
                    new Fraction(Var("x"), Val(1), Var("x")),
                    Var("z"),
                    new Fraction(Var("z"), Val(1), Var("z"))
                )
            ),
            Var("y"),
            2
        );
        double one, two;
        one = solved.Evaluate(10.5, -30);
        two = manuallySolved.Evaluate(10.5, -30);
        Scribe.Info($"1,2: {one},{two}");
        Assert.That(one, Is.EqualTo(two));
    }

    [Test]
    public void SolveAddLike2()
    {
        Equation unsolved = new(
            new SumDiff(
                new List<Oper>{
                    Var("x"),
                    Var("z"),
                    new Fraction(Var("z"), Val(1), Var("y")),
                    Var("x"),
                    new Fraction(Val(3.3), Val(1), Var("z")),
                    new Fraction(Var("x"), Val(1), Var("y")),
                    new Fraction(Var("x"), Val(1), Var("z")),
                    new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
                },
                new List<Oper> { }
            ),
            Fulcrum.EQUALS,
            new Fraction(Val(2), Val(1), Var("y"), Val(1), Var("x"), Val(1), Var("z"))
        );
        SolvedEquation s = unsolved.Solved(Var("x"));
        SolvedEquation manual = new(
            Var("x"),
            Fulcrum.EQUALS,
            new Fraction(
                new Fraction(new List<Oper>
                {
                    new SumDiff(new List<Oper>{Val(3.3), Val(1), Var("y")}, new List<Oper>{}),
                    Var("z")
                }, new List<Oper>{}),
                new SumDiff(
                    new List<Oper>
                    {
                        new SumDiff(new List<Oper>{Var("y"), Var("z"), Val(2)}, new List<Oper>{}),
                        new Fraction(new List<Oper>{new SumDiff(Val(0.4), Val(2)), Var("y"), Var("z")}, new List<Oper>{}),
                    },
                    new List<Oper>{}
                ), Val(-1)
            ), Var("x"), 2
        );
        // Chosen arbitrarily
        double[] args = new[] { 4.3, -12.3 };
        Assert.That(s.Evaluate(args), Is.EqualTo(manual.Evaluate(args)));
    }

    [Test]
    public void SolveSaddle()
    {
        Equation plotTest3d = new(
            new SumDiff(new Fraction(
                Var("y"),
                Val(0.2 / 6)
            ),
            Val(0), Var("y"), Val(0), new Fraction(Var("y"), Val(1), Val(0.2))),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );
        plotTest3d.Solved();
    }
    [Test]
    public void SolveSaddle2()
    {
        Equation plotTest3d = new(
            new Fraction(
                new SumDiff(Var("y"), Val(1)),
                Var("time")),
            Fulcrum.EQUALS,
            new Fraction(
                new SumDiff(
                    new Fraction(Var("x"), Val(230), Var("x")),
                    new Fraction(Var("z"), Val(230), Var("z"))
                )
            )
        );
        plotTest3d.Solved();
    }

    [Test]
    public void Expand1()
    {
        SumDiff sd = new(
            new List<Oper>{
                Var("x"),
                Var("z"),
                new Fraction(Var("z"), Val(1), Var("y")),
                Var("x"),
                new Fraction(Val(3.3), Val(1), Var("z")),
                new Fraction(Var("x"), Val(1), Var("y")),
                new Fraction(Var("x"), Val(1), Var("z")),
                new Fraction(Val(0.4), Val(1), Var("x"), Val(1), Var("y"), Val(1), Var("z"))
            },
            new List<Oper> { }
        );

        Var("x").Val = 20.13535;
        Var("y").Val = 0.13585;
        Var("z").Val = -0.27164;
        double sol0 = sd.Solution().Val;
        Var("x").Reset();
        Var("y").Reset();
        Var("z").Reset();
        
        Scribe.Info(sd);

        sd.Associate();
        sd.Simplify(Var("x"));
        sd.Commute();
        Scribe.Info(sd);
        sd.Associate();
        sd.Simplify(Var("x"));
        sd.Commute();
        Scribe.Info(sd);

        Var("x").Val = 20.13535;
        Var("y").Val = 0.13585;
        Var("z").Val = -0.27164;
        double sol1 = sd.Solution().Val;
        Var("x").Reset();
        Var("y").Reset();
        Var("z").Reset();
        Scribe.Info($"{sol0} == {sol1}?");
        Assert.That(sol0, Is.EqualTo(sol1));
    }

    [Test]
    public void SolveSlider()
    {
        Equation e = new
        (
            new Fraction(Var("y"), Var("time")),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Val(10))
        );
        SolvedEquation s = e.Solved(Var("y"));
        double res = s.Evaluate(10.5, 2);
        Assert.That(res, Is.EqualTo(-84));
    }

    [Test]
    public void OperAsIFunction()
    {
        Oper decr = new SumDiff(Var("x"), Val(1));
        Assert.That(decr.Evaluate(0), Is.EqualTo(-1));

        Oper offsetSquares = new SumDiff
        (
            new Fraction(Var("x"), Val(1), Var("x")),
            Var("y")
        );
        List<double> ossNums = Enumerable.Range(0, 5).Select(n => offsetSquares.Evaluate(n+1, n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(ossNums[0], Is.EqualTo(1));
            Assert.That(ossNums[1], Is.EqualTo(3));
            Assert.That(ossNums[2], Is.EqualTo(7));
            Assert.That(ossNums[3], Is.EqualTo(13));
            Assert.That(ossNums[4], Is.EqualTo(21));
        });
        
        Oper triangleNumbers = new Fraction(
            new List<Oper>{Var("x"), new SumDiff(Var("x"), Val(0), Val(1))},
            new List<Oper>{Val(2)}
        );
        List<double> triNums = Enumerable.Range(0, 5).Select(n => triangleNumbers.Evaluate(n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(triNums[0], Is.EqualTo(0));
            Assert.That(triNums[1], Is.EqualTo(1));
            Assert.That(triNums[2], Is.EqualTo(3));
            Assert.That(triNums[3], Is.EqualTo(6));
            Assert.That(triNums[4], Is.EqualTo(10));
        });
    }

    [Test]
    public void SolveFoil()
    {
        throw Scribe.Issue("implement this test");
    }

    [Test]
    public void SolveParasingle()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)))
        );
        Assert.That(eq.Solved(Var("x")).Evaluate(2), Is.EqualTo(1.5));
        Assert.That(eq.Solved(Var("y")).Evaluate(1.5), Is.EqualTo(2));
    }
    [Test]
    public void SolveParamultipleFracSimp()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)), new Fraction(new SumDiff(Var("x"), Var("y")), Val(4)))
        );
        eq.Solved();
    }
    [Test]
    public void SolveParamultiple()
    {
        Equation eq = new(
            Var("x"),
            Fulcrum.EQUALS,
            new SumDiff(Var("y"), new Fraction(Var("x"), Val(3)), new SumDiff(new SumDiff(Var("x"), Var("y")), Val(4)))
        );
        Assert.That(eq.Solved(Var("x")).Evaluate(0), Is.EqualTo(-12));
        Assert.That(eq.Solved(Var("x")).Evaluate(1), Is.EqualTo(-12));
        Assert.That(eq.Solved(Var("x")).Evaluate(100), Is.EqualTo(-12));
    }
    [Test]
    public void SolveImbalanced()
    {
        Equation eq = new(
            new Fraction(Var("x"), Val(3)),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Var("y"), new Fraction(Var("x"), Val(8)))
        );
        eq.Solved(Var("x"));
    }
    [Test]
    public void SolveImbalanced2()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3)),
            Fulcrum.EQUALS,
            new SumDiff(Var("x"), Var("y"), new Fraction(Var("x"), Val(8)))
        );
        Assert.That(eq.Solved(Var("x")).Evaluate(1), Is.EqualTo(-16));
        Assert.That(eq.Solved(Var("x")).Evaluate(2), Is.EqualTo(-8));
    }
    [Test]
    public void SolveDual()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3), Var("y")),
            Fulcrum.EQUALS,
            new SumDiff(Val(1), new Fraction(Var("x"), Val(3)))
        );
        eq.Solved();
        Assert.That(eq.Solved(Var("x")).Evaluate(2), Is.EqualTo(1.5));
        Assert.That(eq.Solved(Var("x")).Evaluate(4), Is.EqualTo(0));
    }
    [Test]
    public void SolveFluid()
    {
        Equation eq = new(
            new SumDiff(Var("x"), Val(3), Var("y"), new SumDiff(Var("x"), Val(1))),
            Fulcrum.EQUALS,
            new SumDiff(Val(1), new Fraction(Var("x"), Val(3)), new Fraction(Var("x"), Val(5), Var("y")))
        );
        SolvedEquation s = eq.Solved(Var("x"));
        SolvedEquation manual = new(Var("x"), Fulcrum.EQUALS,
            new Fraction(
                Val(15),
                new SumDiff(new Fraction(Val(3), Val(1), Var("y")), Val(5)),
                new SumDiff(Var("y"), Val(3))
            ),
        Var("x"), 1);
        Assert.That(s.Evaluate(0.125), Is.EqualTo(manual.Evaluate(0.125)));
    }
}