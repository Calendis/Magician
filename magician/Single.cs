/*
    A single is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/

namespace Magician {
    public abstract class Single
    {
        protected double[] pos;
        public double X {
            get => pos[0];
        }
        public double Y {
            get => pos[1];
        }

        public abstract void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0);
    }
}