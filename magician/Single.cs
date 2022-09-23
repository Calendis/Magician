/*
    A single is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/

namespace Magician {
    public abstract class Single
    {
        protected double[] pos;
        
        // Convert to Cartesian coordinates
        public double X
        {
            get => Globals.winWidth / 2 + pos[0];
            set
            {
                pos[0] = value;
            }
        }
        public double Y
        {
            get => Globals.winHeight / 2 - pos[1];
            set
            {
                pos[1] = value;
            }
        }

        public double Phase
        {
            get => Math.Atan2(pos[1], pos[0]);
        }
        public double Magnitude
        {
            get => Math.Sqrt(X*X + Y*Y);
        }

        // Raw screen coordinates
        public double WindowX {
            get => pos[0];
        }
        public double WindowY {
            get => pos[1];
        }

        public abstract void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0);
    }
}