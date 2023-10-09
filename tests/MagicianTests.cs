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
        SolvedEquation s = unsolved.Solved();
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
        Assert.That(s.Evaluate(args.Reverse().ToArray()), Is.EqualTo(manual.Evaluate(args)));
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
        SolvedEquation s = plotTest3d.Solved();
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
    public void OperAsIFunction()
    {
        Oper decr = new SumDiff(Var("x"), Val(1));
        Oper triangleNumbers = new Fraction(
            new List<Oper>{Var("x"), new SumDiff(Var("x"), Val(0), Val(1))},
            new List<Oper>{Val(2)}
        );

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
        
        List<double> triNums = Enumerable.Range(0, 5).Select(n => triangleNumbers.Evaluate(n)).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(triNums[0], Is.EqualTo(0));
            Assert.That(triNums[1], Is.EqualTo(1));
            Assert.That(triNums[2], Is.EqualTo(3));
            Assert.That(triNums[3], Is.EqualTo(6));
            Assert.That(triNums[4], Is.EqualTo(10));
            Assert.That(decr.Evaluate(0), Is.EqualTo(-1));
        });
    }
}