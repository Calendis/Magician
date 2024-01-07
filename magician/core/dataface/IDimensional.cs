namespace Magician.Core;

// TODO: possibly make this a generic, where IVal : IDimensional<double>, and IMultival : IDimensional<IVal>
public interface IDimensional<T>
{
    // TODO: rename this
    public List<T> Values {get;}
    public int Dims {get;}
    public double Magnitude {get;}
    public void Normalize();
    //abstract public static IDimensional<T> operator +(IDimensional<T> i, IDimensional<T> v);
    //abstract public static IDimensional<T> operator -(IDimensional<T> i, IDimensional<T> v);
    //abstract public static IDimensional<T> operator *(IDimensional<T> i, IDimensional<T> x);
    //abstract public static IDimensional<T> operator *(IDimensional<T> i, T x);
    //abstract public static IDimensional<T> operator /(IDimensional<T> i, T x);
}