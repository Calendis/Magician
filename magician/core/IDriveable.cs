namespace Magician;
public interface IDriveable
{
    public virtual void DriveQuants(params double[] x) { }
    public abstract void Eject();
}