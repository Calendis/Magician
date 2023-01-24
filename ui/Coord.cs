namespace Magician.UI
{
    public static class Perspective
    {
        public static double x = 0;
        public static double y = 0;
    }

    public class Grid
    {
        Multi axis0;
        Multi axis1;
        Multi spacers0;
        Multi spacers1;
        Multi gridLines;

        int spacerSize = 24;
        int subdivSize = 8;

        double hSp;
        double hSd;
        double vSp;
        double vSd;

        public Grid(double horizSpacing, double horizSubdivs, double vertSpacing, double vertSubdivs)
        {
            hSp = horizSpacing;
            hSd = horizSubdivs;
            vSp = vertSpacing;
            vSd = vertSubdivs;

            // the horizontal axis on a 2d graph
            axis0 = Geo.Line(
                Geo.Point(-Ref.winWidth / 2 - Perspective.x, -Perspective.y),
                Geo.Point(Ref.winWidth / 2 - Perspective.x, -Perspective.y)
            );
            // the vertical spacers along that axis
            spacers0 = new Multi().DrawFlags(DrawMode.INVISIBLE);

            // the vertical axis on a 2d graph
            axis1 = Geo.Line(
                Geo.Point(-Perspective.x, Ref.winWidth / 2 - Perspective.y),
                Geo.Point(-Perspective.x, -Ref.winWidth / 2 - Perspective.y)
            );
            // the horizontal spacers along that axis
            spacers1 = new Multi().DrawFlags(DrawMode.INVISIBLE);

            // The grid lines, which will appear behind the spacers
            gridLines = new Multi().DrawFlags(DrawMode.INVISIBLE);

            // Add spacers to the horizontal axis
            int horizSpacers = (int)(Ref.winWidth / horizSpacing);
            for (int i = 0; i < horizSpacers; i++)
            {
                Multi horizSpacer = Geo.Line(
                    Geo.Point(i * horizSpacing - Ref.winWidth / 2, spacerSize / 2 - Perspective.y),
                    Geo.Point(i * horizSpacing - Ref.winWidth / 2, -spacerSize / 2 - Perspective.y)
                );
                spacers0.Add(horizSpacer
                .Textured(new Renderer.Text($"{(int)(i*hSp - Ref.winWidth/2)}", Ref.UIDefault.FG).Render())
                );


                // Add smaller perpendicular subdividers
                double horizSubdivSpacing = horizSpacing / horizSubdivs;
                for (int j = 0; j < horizSubdivs; j++)
                {
                    Multi horizSubdiv = Geo.Line(
                        Geo.Point(i * horizSpacing - Ref.winWidth / 2 + j * horizSubdivSpacing, subdivSize / 2 - Perspective.y),
                        Geo.Point(i * horizSpacing - Ref.winWidth / 2 + j * horizSubdivSpacing, -subdivSize / 2 - Perspective.y)
                    );
                    spacers0.Add(horizSubdiv);
                }

                // Vertical grid lines
                if (i % 2 != 0) {continue;}
                gridLines.Add(
                    Geo.Line(
                        Geo.Point(i * horizSpacing - Ref.winWidth / 2, -Ref.winHeight),
                        Geo.Point(i * horizSpacing - Ref.winWidth / 2, Ref.winHeight),
                        Ref.UIDefault[1]
                    )
                );
            }

            // Add spacers to the vertical axis
            int vertSpacers = (int)(Ref.winHeight / vertSpacing);
            for (int i = 0; i < vertSpacers; i++)
            {
                Multi vertSpacer = Geo.Line(
                    Geo.Point(spacerSize / 2 - Perspective.x, i * vertSpacing - Ref.winHeight / 2),
                    Geo.Point(-spacerSize / 2 - Perspective.x, i * vertSpacing - Ref.winHeight / 2)
                );
                spacers1.Add(vertSpacer
                //S.Textured(new Renderer.Text($"{-i*vSp + Ref.winHeight/2}", Ref.UIDefault.FG).Render())
                );


                // Add smaller perpendicular subdividers
                double vertSubdivSpacing = vertSpacing / vertSubdivs;
                for (int j = 0; j < vertSubdivs; j++)
                {
                    Multi vertSubdiv = Geo.Line(
                        Geo.Point(-subdivSize / 2 - Perspective.x, i * vertSpacing + j * vertSubdivSpacing - Ref.winHeight / 2),
                        Geo.Point(subdivSize / 2 - Perspective.x, i * vertSpacing + j * vertSubdivSpacing - Ref.winHeight / 2)
                        );
                    spacers1.Add(vertSubdiv);
                }
                
                // Horizontal grid lines
                if (i % 2 != 0) {continue;}
                Multi l = Geo.Line(
                        Geo.Point(-Ref.winWidth, i * vertSpacing - Ref.winHeight / 2),
                        Geo.Point(Ref.winWidth, i * vertSpacing - Ref.winHeight / 2),
                        Ref.UIDefault[1]
                );

                gridLines.Add(
                    l
                );
            }
        }

        // Update the grid according to the UI Perspective
        public Grid Update()
        {
            return new Grid(hSp, hSd, vSp, vSd);
        }

        public Multi Render()
        {
            
            return new Multi(gridLines, axis0.Adjoin(spacers0), axis1.Adjoin(spacers1)).DrawFlags(DrawMode.INVISIBLE)
            ;
        }
    }
}