namespace Magician
{
    public interface Drawable
    {
        public abstract void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0);

        public abstract Drawable Parent();
        public abstract void SetX(double offset);
        public abstract void SetY(double offset);
        public abstract Drawable Copy();
        public void IncrX(double x)
        {
            SetX(x + XAbsolute(0));
        }
        public void IncrY(double x)
        {
            SetY(x + YAbsolute(0));
        }
        public void SetPhase(double x)
        {
            double parentOffset = 0;
            if (Parent() is not null)
            {
                parentOffset = 0*Parent().Phase;
            }
            double m = Magnitude;
            SetX(m*Math.Cos(x - parentOffset));
            SetY(m*Math.Sin(x - parentOffset));
        }
        public void IncrPhase(double x)
        {
            SetPhase(Phase + x);
        }
        public void SetMagnitude(double x)
        {
            double parentOffset = 0;
            if (Parent() is not null)
            {
                parentOffset = Parent().Phase;
            }
            double p = Phase;
            SetX(x*Math.Cos(p - parentOffset));
            SetY(x*Math.Sin(p - parentOffset));
        }
        public void IncrMagnitude(double x)
        {
            SetMagnitude(Magnitude + x);
        }

        public double Phase
        {
            get
            {
                double p = Math.Atan2(YAbsolute(0), XAbsolute(0));
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        public double Magnitude
        {
            get => Math.Sqrt(XAbsolute(0) * XAbsolute(0) + YAbsolute(0) * YAbsolute(0));
        }
        public double XCartesian(double offset)
        {
            return Globals.winWidth / 2 + XAbsolute(0) + offset;
        }
        public abstract double XAbsolute(double offset);
        public double YCartesian(double offset)
        {
            return Globals.winHeight / 2 - YAbsolute(0) - offset;
        }
        public abstract double YAbsolute(double offset);
        public Point GetPoint()
        {
            return new Point(XAbsolute(0), YAbsolute(0));
        }

        public Color Col {get; set;}

        // Moves the Drawable towards a point by a certain amount
        public Drawable Towards(Drawable d, double howMuch)
        {
            IncrX((-XAbsolute(0) + d.XAbsolute(0)) * howMuch);
            IncrY((-YAbsolute(0) + d.YAbsolute(0)) * howMuch);
            return this;
        }

        // These setters are sensitive to whether or not the colour is HSL or RGB
        // I write setters here instead of using properties, because I may want to pass these into a Driver
        public void SetCol0(double d)
        {
            if (!Col.IsHSL)
            {
                Col.R = (byte)d;
            }
            else
            {
                Col = new Color(0, Col.Saturation, Col.Lightness, Col.A, (float)d, disambiguationBool: true);
            }
        }
        public void IncrCol0(double d)
        {
            if (!Col.IsHSL)
            {
                Col.R += (byte)d;
            }
            else
            {
                // TODO: saturation and lightness getters
                Console.WriteLine("HEY LAZY: \n    IMPLEMENT SATURATION AND LIGHTNESS GETTERS!!!");
                Console.WriteLine("    HSL SUPPORT IS NOT FINISHED UNTIL YOU DO THIS!!!");
                
                Col = new Color(Col.Hue, Col.Saturation, Col.Lightness, Col.A,
                (float)d + Col.FloatingPart0, Col.FloatingPart1, Col.FloatingPart2, disambiguationBool: true);
            }
        }

        public void SetCol1(double d)
        {
            if (!Col.IsHSL)
            {
                Col.G = (byte)d;
            }
            else
            {
                Col.HexCol = Color.HSLToRGBHex(Col.Hue, (float)d, Col.Lightness, Col.A);
            }
        }
        public void IncrCol1(double d)
        {
            if (!Col.IsHSL)
            {
                Col.G += (byte)d;
            }
            else
            {
                // TODO: incrCol1
            }
        }

        public void SetCol2(double d)
        {
            if (!Col.IsHSL)
            {
                Col.B = (byte)d;
            }
            else
            {
                Col.HexCol = Color.HSLToRGBHex(Col.Hue, Col.Saturation, (float)d, Col.A);
            }
        }
        public void IncrCol2(double d)
        {
            if (!Col.IsHSL)
            {
                Col.B += (byte)d;
            }
            else
            {
                // Todo incrcol2
            }
        }

        public void SetAlpha(double d)
        {
            Col.A = (byte)d;
        }
        public void IncrAlpha(double d)
        {
            Col.A += (byte)d;
        }

        /////
        public abstract void Scale(double x);

        
    }
}
