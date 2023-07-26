namespace Magician;

public class Driver
{
    public CoordMode CMode;
    public DriverMode DMode;
    public TargetMode TMode;
    public Multi Target { get; set; }
    protected Func<double, double> X;
    protected Func<double, double> Y;
    protected Func<double, double> Z;

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
        if (pm.Params != 3)
        {
            Scribe.Error($"ParamMap has {pm.Params}, must have 3");
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

        switch (CMode)
        {
            case CoordMode.XYZ:
                double x = X.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.x.Get());
                double y = Y.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.y.Get());
                double z = Z.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.z.Get());
                Target.Positioned(x, y, z);
                break;
            case CoordMode.POLAR:
                double mag   = X.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.Magnitude);
                Target.Magnitude = mag;
                double theta = Y.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.PhaseXY);
                Target.PhaseXY = theta;
                double phi   = Z.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.PhaseYZ);
                Target.PhaseYZ = phi;
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