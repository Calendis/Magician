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
            new List<Oper>{new SumDiff(Val(2)), new SumDiff(Val(1), Val(2), new SumDiff(Val(3)), Val(4)), Val(1), new SumDiff(Val(3))},
            new List<Oper>{Val(5), new SumDiff(Val(1), new SumDiff(Val(5), new SumDiff(Val(3))))}
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
}