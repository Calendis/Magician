
namespace Magician.Core;

public interface IVar : IVal, IVec
{
    public abstract bool IsVector {get;}
    public IDimensional<T> Get<T>(int i)
    {
        if (typeof(T).GetType() == typeof(double))
        {
            if (IsVector)
                throw Scribe.Error($"{this} is a vector");
            return (IDimensional<T>)new Num(((IVal)this).Values.ToArray());
        }
        else if (typeof(T).GetType() == typeof(IVal))
        {
            if (!IsVector)
                throw Scribe.Error($"{this} is not a vector");
            return (IDimensional<T>)new Vec(((IVec)this).Values.ToArray());
        }
        throw Scribe.Error($"Invalid get type {typeof(T)} for {this}");
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

    public IVal ToIVal()
    {
        if (IsVector)
            throw Scribe.Error($"Could not take vector {this} as value");
        if (((IDimensional<IVal>)this).Values.Count == 1 && ((IDimensional<IVal>)this).Dims == 1)
            return new Num(((IDimensional<IVal>)this).Values[0]);
        else
            return new Num(((IDimensional<double>)this).Values.ToArray());
    }
    public IVec ToIVec()
    {
        if (!IsVector)
            throw Scribe.Error($"Could not take value {this} as vector. Conversion can be done manually");
        return (IVec)this;
    }
    
    //public static IVar operator -(IVal i, IVal v)
    //{
    //    return ((IVar)new Num(i.IDArgs.Zip(v.IDArgs, (a, b) => a - b).ToArray()));
    //}
    //public static IVar operator +(IVal i, double x)
    //{
    //    double[] newAll = i.IDArgs.ToArray();
    //    newAll[0] += x;
    //    return new Num(newAll);
    //}
    //public static IVar operator -(IVal i, double x)
    //{
    //    double[] newAll = i.IDArgs.ToArray();
    //    newAll[0] -= x;
    //    return new Num(newAll);
    //}
}

public class Var : IVar
{
    public bool IsVector {get; set;}
    public double Magnitude => IsVector ? new Vec(((IDimensional<IVal>)this).Values.ToArray()).Magnitude : ((IVal)new Num(((IDimensional<double>)this).Values.ToArray())).Magnitude;
    List<IVal> vec;
    List<double> val;
    List<IVal> IDimensional<IVal>.Values => vec;
    List<double> IDimensional<double>.Values => val;

    public Var(params double[] ds)
    {
        IsVector = false;
        val = ds.ToList();
        vec = new();
    }
    public Var(params IVal[] ivs)
    {
        IsVector = true;
        vec = ivs.ToList();
        val = new();
    }

    public void Set(params double[] vs)
    {
        throw new NotImplementedException();
    }

    void IDimensional<double>.Normalize()
    {
        throw new NotImplementedException();
    }

    void IDimensional<IVal>.Normalize()
    {
        throw new NotImplementedException();
    }
}