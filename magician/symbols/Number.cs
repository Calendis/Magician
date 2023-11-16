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
