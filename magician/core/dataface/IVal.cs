namespace Magician;

public interface IVal
{
    public double[] Quantities {get; protected set;}
    public void Set(params double[] vs);
}