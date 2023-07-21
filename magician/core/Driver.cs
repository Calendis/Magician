namespace Magician;

public class Driver
{
    public CoordMode CMode;
    public DriverMode DMode;
    public TargetMode TMode;
    public Multi Target { get; set; }
    public delegate double Eval(double t);
    protected Eval X;
    protected Eval Y;
    protected Eval Z;

    public Driver(Multi m, DirectMap dm1, DirectMap dm2, DirectMap dm3, CoordMode coordMode = CoordMode.XYZ, DriverMode driverMode = DriverMode.SET, TargetMode targetMode = TargetMode.DIRECT) :
    this(m, new ParamMap(dm1, dm2, dm3), coordMode, driverMode, targetMode)
    {
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
        X = pm.Maps[0].Invoke;
        Y = pm.Maps[1].Invoke;
        Z = pm.Maps[2].Invoke;
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
                double x = X.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.x.Evaluate());
                double y = Y.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.y.Evaluate());
                double z = Z.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.z.Evaluate());
                Target.Positioned(x, y, z);
                break;
            case CoordMode.POLAR:
                double mag   = X.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.Magnitude);
                double theta = Y.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.PhaseXY);
                double phi   = Z.Invoke(t) + (DMode == DriverMode.SET ? 0 : Target.PhaseYZ);
                // TODO: This can't work due to needing a temp variable
                Target.Magnitude = mag;
                Target.PhaseXY = theta + theta;
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