namespace Magician.Core;
using Maps;
using Geo;

public class Driver
{
    public CoordMode CMode;
    public DriverMode DMode;
    public TargetMode TMode;
    public Node Target { get; set; }
    protected Func<double, IVal> X;
    protected Func<double, IVal> Y;
    protected Func<double, IVal> Z;
    IVal xCache = new Val(0);
    IVal yCache = new Val(0);
    IVal zCache = new Val(0);

    public Driver(Node m, Direct dm1, Direct dm2, Direct dm3, CoordMode coordMode = CoordMode.XYZ, DriverMode driverMode = DriverMode.SET, TargetMode targetMode = TargetMode.DIRECT)
    {
        // these aren't actually necessary
        X = dm1.Evaluate;
        Y = dm2.Evaluate;
        Z = dm3.Evaluate;
        CMode = coordMode;
        DMode = driverMode;
        TMode = targetMode;
        Target = m;
    }

    public void Drive(double t)
    {
        if (TMode == TargetMode.SUB)
        {
            TMode = TargetMode.DIRECT;
            foreach (Node c in Target)
            {
                Target = c;
                Drive(t);
            }
            TMode = TargetMode.SUB;
            Target = Target.Parent;
            return;
        }

        switch (CMode)
        {
            case CoordMode.XYZ:
                IVal.Add(X.Invoke(t), DMode == DriverMode.SET ? 0 : Target.x.Get(), xCache);
                IVal.Add(Y.Invoke(t), DMode == DriverMode.SET ? 0 : Target.y.Get(), yCache);
                IVal.Add(Z.Invoke(t), DMode == DriverMode.SET ? 0 : Target.z.Get(), zCache);
                Target.To(xCache.Get(), yCache.Get(), zCache.Get());
                break;
            case CoordMode.POLAR:
                IVal.Add(X.Invoke(t), DMode == DriverMode.SET ? 0 : Target.Magnitude, xCache);
                Target.Magnitude = xCache.Get();
                IVal.Add(Y.Invoke(t), DMode == DriverMode.SET ? 0 : Target.PhaseXY, yCache);
                Target.PhaseXY = yCache.Get();
                IVal.Add(Z.Invoke(t), DMode == DriverMode.SET ? 0 : Target.PhaseYZ, zCache);
                Target.PhaseYZ = zCache.Get();
                break;
            case CoordMode.BRANCHED:
                // TODO: implement this
                throw Scribe.Issue("Not implemented");
                //break;
        }
    }
}

public enum CoordMode
{
    XYZ,
    POLAR,
    BRANCHED
}

public enum DriverMode
{
    SET,
    INCR
}

public enum TargetMode
{
    DIRECT,
    SUB
}