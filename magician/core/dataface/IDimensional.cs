namespace Magician;

// TODO: possibly make this a generic, where IVal : IDimensional<double>, and IMultival : IDimensional<IVal>
public interface IDimensional
{
    public int Dims {get;}
}