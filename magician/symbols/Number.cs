namespace Magician.Symbols;

public enum Numberkind : short
{
    Real = 0b00000001,
    Rational = 0b00000010,
    Integer = 0b00000110,
    Negative = 0b10000000,
    Imaginary = 0b00001000,
    Complex = 0b00010001,
}

public interface IAdd
{
    public SumDiff Add(params Oper[] os);
}
public interface ISubtract
{
    public SumDiff Subtract(params Oper[] os);
}
public interface IMult
{
    public Fraction Mult(params Oper[] os);
}
public interface IDivide
{
    public Fraction Divide(params Oper[] os);
}

public interface ISum : IAdd, ISubtract {}
public interface IFrac : IMult, IDivide {}
public interface IArithmetic : ISum, IFrac {}