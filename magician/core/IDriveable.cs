namespace Magician
{
    public interface IDriveable
    {
        public virtual void Drive(params double[] x){}
        public abstract void Eject();
    }
}
