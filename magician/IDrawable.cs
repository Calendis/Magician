// This interface provides common features for Multis and Textures (Geometry and Images)
using Magician.Renderer;

namespace Magician
{
    public interface IDrawable
    {
        public abstract void Draw(double xOffset=0, double yOffset=0);
        public abstract Color Col {get; set;}

        // Setter accessors for colour
        public void SetR(double r)
        {
            Col.R = r;
        }
        public void SetG(double g)
        {
            Col.G = g;
        }
        public void SetB(double b)
        {
            Col.B = b;
        }
        public void SetA(double a)
        {
            Col.A = a;
        }
        public void SetH(double h)
        {
            Col.H = h;
        }
        public void SetS(double s)
        {
            Col.S = s;
        }
        public void SetL(double l)
        {
            Col.L = l;
        }

        // Incrementor accessors for colour
        public void IncrR(double r)
        {
            Col.R += r;
        }
        public void IncrG(double g)
        {
            Col.G += g;
        }
        public void IncrB(double b)
        {
            Col.B += b;
        }
        public void IncrA(double a)
        {
            Col.A += a;
        }
        public void IncrH(double h)
        {
            Col.H += h;
        }
        public void IncrS(double s)
        {
            Col.S += s;
        }
        public void IncrL(double l)
        {
            Col.L += l;
        }
    }
}