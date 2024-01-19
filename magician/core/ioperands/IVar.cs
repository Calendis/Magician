
namespace Magician.Core;

public interface IVar : IVal, IVec
{
    public bool IsVector => Values<IVal>() is not null && Values<IVal>().Count > 0;
    public bool IsScalar => Values<double>() is not null && Values<double>().Count > 0;
    public bool Is1DVector => Values<IVal>().Count == 1;
    public new List<T> Values<T>() => ((IDimensional<T>)this).Values;
    (List<double>, List<int>) Flatten()
    {
        if (IsScalar)
            throw Scribe.Error($"Cannot flatten scalar {this}");
        List<double> vals = new();
        List<int> delim = new();
        vals.AddRange(Values<IVal>()[0].Values);
        for (int i = 1; i < Values<IVal>().Count; i++)
        {
            vals.AddRange(Values<IVal>()[i].Values);
            delim.Add(Values<IVal>()[i].Values.Count);
        }
        return (vals, delim);
    }
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
    public static IVar Add(IVar i, IVar v, IVar? output=null)
    {
        if (i.IsVector)
            if (v.IsVector)
                return new Var((i.ToIVec() + v.ToIVec()).Values.ToArray());
            else
                return new Var((i.ToIVec() + v.ToIVal()).Values.ToArray());
        else if (v.IsVector)
            return     new Var((v.ToIVec() + i.ToIVal()).Values.ToArray());
        else
        {
            if (output is null)
                return     new Var(Add(i.ToIVal(), v.ToIVal()).Values.ToArray());
            output.Set(Add(i.ToIVal(), v.ToIVal()));
            return output;
        }
    }
    public static IVar Subtract(IVar i, IVar v, IVar? output=null)
    {
        if (i.IsVector)
            if (v.IsVector)
                return new Var((i.ToIVec() - v.ToIVec()).Values.ToArray());
            else
                return new Var((i.ToIVec() - v.ToIVal()).Values.ToArray());
        else if (v.IsVector)
            return     new Var((v.ToIVec() - i.ToIVal()).Values.ToArray());
        else
        {
            if (output is null)
                return     new Var(Subtract(i.ToIVal(), v.ToIVal()).Values.ToArray());
            output.Set(Subtract(i.ToIVal(), v.ToIVal()));
            return output;
        }
    }
    public static IVar Multiply(IVar i, IVar v, IVar? output=null)
    {
        if (i.Is1DVector && v.Is1DVector)
        {
            if (output is null)
                return new Var(Multiply(i.ToIVal(), v.ToIVal()));
            output.Set(Multiply(i.ToIVal(), v.ToIVal(), output));
            return output;
        }
        else if (i.Is1DVector)
        {
            if (output is null)
                return new Var(Multiply(i.ToIVal(), v));
            output.Set(Multiply(i.ToIVal(), v, output));
            return output;
        }
        else if (v.Is1DVector)
        {
            if (output is null)
                return new Var(Multiply(v.ToIVal(), i));
            output.Set(Multiply(v.ToIVal(), i, output));
            return output;
        }
        
        if (i.IsVector)
            if (v.IsVector)
                throw Scribe.Error($"Could not multiply vectors {i} and {v}");  // TODO: maybe use geometric (Clifford) algebra to do this
            else
            {
                if (output is null)
                    return new Var(i.Values<IVal>().Select(iv => Multiply(iv, v.ToIVal())).ToArray());
                output.Set(i.Values<IVal>().Select(iv => Multiply(iv, v.ToIVal())));
                return output;
            }
        else if (v.IsVector)
        {
            if (output is null)
                return     new Var(v.Values<IVal>().Select(iv => Multiply(iv, i.ToIVal())).ToArray());
            output.Set(v.Values<IVal>().Select(iv => Multiply(iv, i.ToIVal())));
            return output;
        }
        else
            return     new Var(Multiply(i.ToIVal(), v.ToIVal()).Values.ToArray());
    }
    public static IVar Divide(IVar i, IVar v, IVar? output=null)
    {
        if (i.Is1DVector && v.Is1DVector)
        {
            if (output is null)
                return new Var(Divide(i.ToIVal(), v.ToIVal()));
            output.Set(Divide(i.ToIVal(), v.ToIVal(), output));
            return output;
        }
        else if (i.Is1DVector)
        {
            if (output is null)
                return new Var(Divide(i.ToIVal(), v));
            output.Set(Divide(i.ToIVal(), v, output));
            return output;
        }
        else if (v.Is1DVector)
        {
            if (output is null)
                return new Var(Divide(v.ToIVal(), i));
            output.Set(Divide(v.ToIVal(), i, output));
            return output;
        }
        
        if (i.IsVector)
            if (v.IsVector)
                throw Scribe.Error($"Could not divide vectors {i} and {v}");  // TODO: maybe use geometric (Clifford) algebra to do this
            else
            {
                if (output is null)
                    return new Var(i.Values<IVal>().Select(iv => Divide(iv, v.ToIVal())).ToArray());
                output.Set(i.Values<IVal>().Select(iv => Divide(iv, v.ToIVal())));
                return output;
            }
        else if (v.IsVector)
        {
            if (output is null)
                return     new Var(v.Values<IVal>().Select(iv => Divide(iv, i.ToIVal())).ToArray());
            output.Set(v.Values<IVal>().Select(iv => Divide(iv, i.ToIVal())));
            return output;
        }
        else
            return     new Var(Divide(i.ToIVal(), v.ToIVal()).Values.ToArray());
    }

    IVal ToIVal()
    {
        if (IsVector && !Is1DVector)
            throw Scribe.Error($"Could not take vector {this} as value");
        else if (IsVector)
            return ((IDimensional<IVal>)this).Values[0];
        else
            return (IVal)this;
            //return new Val(((IDimensional<double>)this).Values.ToArray());
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
        //Scribe.Tick();
        if (ds.Length == 0)
            throw Scribe.Error("Cannot create empty Var scalar");
        //IsVector = false;
        val = ds.ToList();
        vec = new();
    }
    public Var(params IVal[] ivs)
    {
        //Scribe.Tick();
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