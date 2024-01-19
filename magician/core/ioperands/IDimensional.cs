namespace Magician.Core;

// TODO: possibly make this a generic, where IVal : IDimensional<double>, and IMultival : IDimensional<IVal>
public interface IDimensional<T>
{
    public int Dims => Values.Count;
    public List<T> Values { get; }
    public T Get(int i = 0) => Values[i];
    public void Set(IDimensional<T> other) => Set(other.Values.ToArray());
    public void Set(params T[] vs)
    {
        Values.Clear();
        Values.AddRange(vs);
    }
    public void Set(IEnumerable<T> vs)
    {
        Set(vs.ToArray());
    }
    public void Normalize();
    public double Magnitude {get; set;}
    //public IVal Theta {get;}
    
    //abstract public static IDimensional<T> operator +(IDimensional<T> i, IDimensional<T> v);
    //abstract public static IDimensional<T> operator -(IDimensional<T> i, IDimensional<T> v);
    //abstract public static IDimensional<T> operator *(IDimensional<T> i, IDimensional<T> x);
    //abstract public static IDimensional<T> operator *(IDimensional<T> i, T x);
    //abstract public static IDimensional<T> operator /(IDimensional<T> i, T x);
}