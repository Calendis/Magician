namespace Magician.Core;

// TODO: possibly make this a generic, where IVal : IDimensional<double>, and IMultival : IDimensional<IVal>
public interface IDimensional<T>
{
    public int Dims => Values.Count;
    public List<T> Values { get; }
    public T Get(int i = 0) => Values[i];
    public void Set(IDimensional<T> other)
    {
        for (int i = 0; i < other.Values.Count; i++)
        {
            Set(i, other.Get(i));
        }
    }
    public void Set(params T[] vs)
    {
        Values.Clear();
        Values.AddRange(vs);
    }
    public void Set(IEnumerable<T> vs)
    {
        Set(vs.ToArray());
    }
    public void Set(int i, T val)
    {
        if (i < Dims)
            Values[i] = val;
        else if (val is double d && d != 0)
            Values.Add(val);
        else if (val is IVal f && f.Magnitude != 0)
            Values.Add(val);
    }
    public void Push(params T[] vals)
    {
        Values.AddRange(vals);
    }
    public void Push(IDimensional<T> val)
    {
        Push(val.Values.ToArray());
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