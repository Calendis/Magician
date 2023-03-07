using static Magician.Geo.Create;

namespace Magician.UI
{
    public static class Perspective
    {
        public static Quantity x = new Quantity(0);
        public static Quantity y = new Quantity(0);
    }

    public class RuledAxes
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

        public RuledAxes(double horizSpacing, double horizSubdivs, double vertSpacing, double vertSubdivs)
        {
            hSp = horizSpacing;
            hSd = horizSubdivs;
            vSp = vertSpacing;
            vSd = vertSubdivs;

            // the horizontal axis on a 2d graph
            axis0 = Line(
                Point(-Data.Globals.winWidth / 2 - Perspective.x.Evaluate(), -Perspective.y.Evaluate()),
                Point(Data.Globals.winWidth / 2 - Perspective.x.Evaluate(), -Perspective.y.Evaluate())
            );
            // the vertical spacers along that axis
            spacers0 = new Multi().SetDraw(DrawMode.INVISIBLE);

            // the vertical axis on a 2d graph
            axis1 = Line(
                Point(-Perspective.x.Evaluate(), Data.Globals.winWidth / 2 - Perspective.y.Evaluate()),
                Point(-Perspective.x.Evaluate(), -Data.Globals.winWidth / 2 - Perspective.y.Evaluate())
            );
            // the horizontal spacers along that axis
            spacers1 = new Multi().SetDraw(DrawMode.INVISIBLE);

            // The grid lines, which will appear behind the spacers
            gridLines = new Multi().SetDraw(DrawMode.INVISIBLE);

            // Add spacers to the horizontal axis
            int horizSpacers = (int)(Data.Globals.winWidth / horizSpacing);
            for (int i = 0; i < horizSpacers; i++)
            {
                Renderer.Text tx = new Renderer.Text($"{(int)(i * hSp - Data.Globals.winWidth / 2)}", Data.Col.UIDefault.FG, Data.Globals.fontSize);
                Multi horizSpacer = Line(
                    Point(i * horizSpacing - Data.Globals.winWidth / 2, spacerSize / 2 - Perspective.y.Evaluate()),
                    Point(i * horizSpacing - Data.Globals.winWidth / 2, -spacerSize / 2 - Perspective.y.Evaluate())
                );
                // If I attach the .Textured(tx.Render()) to the Point call above, the texture comes out null
                // This is because of how Geo.Create.Line works.
                horizSpacer[0].Textured(tx.Render());

                spacers0.Add(horizSpacer);
                tx.Dispose();


                // Add smaller perpendicular subdividers
                double horizSubdivSpacing = horizSpacing / horizSubdivs;
                for (int j = 0; j < horizSubdivs; j++)
                {
                    Multi horizSubdiv = Line(
                        Point(i * horizSpacing - Data.Globals.winWidth / 2 + j * horizSubdivSpacing, subdivSize / 2 - Perspective.y.Evaluate()),
                        Point(i * horizSpacing - Data.Globals.winWidth / 2 + j * horizSubdivSpacing, -subdivSize / 2 - Perspective.y.Evaluate())
                    );
                    spacers0.Add(horizSubdiv);
                }

                // Vertical grid lines
                if (i % 2 != 0) { continue; }
                gridLines.Add(
                    Line(
                        Point(i * horizSpacing - Data.Globals.winWidth / 2, -Data.Globals.winHeight),
                        Point(i * horizSpacing - Data.Globals.winWidth / 2, Data.Globals.winHeight),
                        Data.Col.UIDefault[1]
                    )
                );
            }

            // Add spacers to the vertical axis
            int vertSpacers = (int)(Data.Globals.winHeight / vertSpacing);
            for (int i = 0; i < vertSpacers; i++)
            {
                Multi vertSpacer = Line(
                    Point(spacerSize / 2 - Perspective.x.Evaluate(), i * vertSpacing - Data.Globals.winHeight / 2),
                    Point(-spacerSize / 2 - Perspective.x.Evaluate(), i * vertSpacing - Data.Globals.winHeight / 2)
                );
                spacers1.Add(vertSpacer);


                // Add smaller perpendicular subdividers
                double vertSubdivSpacing = vertSpacing / vertSubdivs;
                for (int j = 0; j < vertSubdivs; j++)
                {
                    Multi vertSubdiv = Line(
                        Point(-subdivSize / 2 - Perspective.x.Evaluate(), i * vertSpacing + j * vertSubdivSpacing - Data.Globals.winHeight / 2),
                        Point(subdivSize / 2 - Perspective.x.Evaluate(), i * vertSpacing + j * vertSubdivSpacing - Data.Globals.winHeight / 2)
                        );
                    spacers1.Add(vertSubdiv);
                }

                // Horizontal grid lines
                if (i % 2 != 0) { continue; }
                Multi l = Line(
                        Point(-Data.Globals.winWidth, i * vertSpacing - Data.Globals.winHeight / 2),
                        Point(Data.Globals.winWidth, i * vertSpacing - Data.Globals.winHeight / 2),
                        Data.Col.UIDefault[1]
                );

                gridLines.Add(
                    l
                );
            }
        }

        // Update the grid according to the UI Perspective
        public RuledAxes Update()
        {
            return new RuledAxes(hSp, hSd, vSp, vSd);
        }

        public Multi Render()
        {

            return new Multi(gridLines, axis0.Adjoined(spacers0), axis1.Adjoined(spacers1)).SetDraw(DrawMode.INVISIBLE)
            ;
        }
    }
}