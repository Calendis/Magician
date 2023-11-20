namespace Magician.Symbols;

public abstract class Unary : Oper
{
    public Unary(string name, Oper o) : base(name, o) {}
}