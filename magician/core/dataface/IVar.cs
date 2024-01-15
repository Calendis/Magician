
namespace Magician.Core;

public interface IVar : IVal, IVec
{
    public bool IsVector => Values<IVal>() is not null && Values<IVal>().Count > 0;
    public bool IsScalar => Values<double>() is not null && Values<double>().Count > 0;
    public bool Is1DVector => Values<IVal>().Count == 1;
    public new List<T> Values<T>() => ((IDimensional<T>)this).Values;
    public new int Dims
    {
        get
        {
            if (IsVector)
                return Values<IVal>().Count;
            return Values<double>().Count;
        }
    }
    public new IVal Get(int i = 0)
    {
        if (IsVector)
            return Values<IVal>()[i];
        return new Val(Values<double>().ToArray());
    }
    public static IVar operator +(IVar i, IVar v)
    {
        if (i.IsVector)
            if (v.IsVector)
                return new Var((i.ToIVec() + v.ToIVec()).Values.ToArray());
            else
                return new Var((i.ToIVec() + v.ToIVal()).Values.ToArray());
        else if (v.IsVector)
            return     new Var((v.ToIVec() + i.ToIVal()).Values.ToArray());
        else
            return     new Var((i.ToIVal() + v.ToIVal()).Values.ToArray());
    }
    public static IVar operator -(IVar i, IVar v)
    {
        if (i.IsVector)
            if (v.IsVector)
                return new Var((i.ToIVec() - v.ToIVec()).Values.ToArray());
            else
                return new Var((i.ToIVec() - v.ToIVal()).Values.ToArray());
        else if (v.IsVector)
            return     new Var((v.ToIVec() - i.ToIVal()).Values.ToArray());
        else
            return     new Var((i.ToIVal() - v.ToIVal()).Values.ToArray());
    }
    public static IVar operator *(IVar i, IVar v)
    {
        if (i.Is1DVector && v.Is1DVector)
            return new Var(i.ToIVal() * v.ToIVal());
        else if (i.Is1DVector)
            return new Var(i.ToIVal() * v);
        else if (v.Is1DVector)
            return new Var(v.ToIVal() * i);
        
        if (i.IsVector)
            if (v.IsVector)
                throw Scribe.Error($"Could not multiply vectors {i} and {v}");  // TODO: maybe use geometric (Clifford) algebra to do this
            else
                return new Var(i.Values<IVal>().Select(iv => iv * v.ToIVal()).ToArray());
        else if (v.IsVector)
            return     new Var(v.Values<IVal>().Select(iv => iv * i.ToIVal()).ToArray());
        else
            return     new Var((i.ToIVal() * v.ToIVal()).Values.ToArray());
    }
    public static IVar operator /(IVar i, IVar v)
    {
        if (i.IsVector)
            if (v.IsVector)
                throw Scribe.Error($"Could not divide vectors {i} and {v}");  // maybe use geometric (Clifford) algebra to do this
            else
                return new Var(i.Values<IVal>().Select(iv => iv / v.ToIVal()).ToArray());
        else if (v.IsVector)
            return     new Var(v.Values<IVal>().Select(iv => iv / i.ToIVal()).ToArray());
        else
            return     new Var((i.ToIVal() / v.ToIVal()).Values.ToArray());
    }

    IVal ToIVal()
    {
        if (IsVector && !Is1DVector)
            throw Scribe.Error($"Could not take vector {this} as value");
        else if (IsVector)
            return new Val(((IDimensional<IVal>)this).Values[0]);
        else
            return new Val(((IDimensional<double>)this).Values.ToArray());
    }
    IVec ToIVec()
    {
        if (!IsVector)
            throw Scribe.Error($"Could not take value {this} as vector. Conversion can be done manually");
        return (IVec)this;
    }
}

public class Var : IVar
{
    // This again. I guess it's not that bad
    public bool IsVector => ((IVar)this).IsVector;
    bool IVar.IsVector => vec.Count > 0;
    public double Magnitude => IsVector ? new Vec(((IDimensional<IVal>)this).Values.ToArray()).Magnitude : ((IVal)new Val(((IDimensional<double>)this).Values.ToArray())).Magnitude;
    readonly List<IVal> vec;
    readonly List<double> val;
    List<IVal> IDimensional<IVal>.Values => vec;
    List<double> IDimensional<double>.Values => val;

    public Var(params double[] ds)
    {
        if (ds.Length == 0)
            throw Scribe.Error("Cannot create empty Var scalar");
        //IsVector = false;
        val = ds.ToList();
        vec = new();
    }
    public Var(params IVal[] ivs)
    {
        if (ivs.Length == 0)
            throw Scribe.Error("Cannot create empty Var vector");
        //IsVector = true;
        vec = ivs.ToList();
        val = new();
    }

    // TODO: these
    //public void Set(params double[] vs)
    //{
    //    val = vs.ToList();
    //}

    void IDimensional<double>.Normalize()
    {
        ((IVal)this).Normalize();
    }

    void IDimensional<IVal>.Normalize()
    {
        ((IVec)this).Normalize();
    }

    public override string ToString()
    {
        if (IsVector)
            return $"Var vec {Scribe.Expand<List<IVal>, IVal>(vec)}";
        else
            return $"Var scalar {Scribe.Expand<List<double>, double>(val)}";
    }
}