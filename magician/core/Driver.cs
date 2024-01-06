namespace Magician.Core;
using Maps;
using Geo;

public class Driver
{
    public CoordMode CMode;
    public DriverMode DMode;
    public TargetMode TMode;
    public Multi Target { get; set; }
    protected Func<double, IVal> X;
    protected Func<double, IVal> Y;
    protected Func<double, IVal> Z;

    public Driver(Multi m, DirectMap dm1, DirectMap dm2, DirectMap dm3, CoordMode coordMode = CoordMode.XYZ, DriverMode driverMode = DriverMode.SET, TargetMode targetMode = TargetMode.DIRECT) :
    this(m, new ParamMap(dm1, dm2, dm3), coordMode, driverMode, targetMode)
    {
        // these aren't actually necessary
        X = dm1.Evaluate;
        Y = dm2.Evaluate;
        Z = dm3.Evaluate;
    }

    public Driver(Multi m, ParamMap pm, CoordMode coordMode = CoordMode.XYZ, DriverMode driverMode = DriverMode.SET, TargetMode targetMode = TargetMode.DIRECT)
    {
        if (pm.Outs != 3)
        {
            Scribe.Error($"ParamMap has {pm.Outs}, must have 3");
        }
        CMode = coordMode;
        DMode = driverMode;
        TMode = targetMode;
        Target = m;
        X = pm.Maps[0];
        Y = pm.Maps[1];
        Z = pm.Maps[2];
    }

    public void Drive(double t)
    {
        if (TMode == TargetMode.SUB)
        {
            TMode = TargetMode.DIRECT;
            foreach (Multi c in Target)
            {
                Target = c;
                Drive(t);
            }
            TMode = TargetMode.SUB;
            Target = Target.Parent;
            return;
        }

        // TODO: support and handke complex numbers
        switch (CMode)
        {
            case CoordMode.XYZ:
                double x = (X.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.x.Get())).Get();
                double y = (Y.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.y.Get())).Get();
                double z = (Z.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.z.Get())).Get();
                Target.To(x, y, z);
                break;
            case CoordMode.POLAR:
                IVal mag   = X.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.Magnitude);
                Target.Magnitude = mag.Get();
                IVal theta = Y.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.PhaseXY);
                Target.PhaseXY = theta.Get();
                IVal phi   = Z.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.PhaseYZ);
                Target.PhaseYZ = phi.Get();
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