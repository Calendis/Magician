namespace Magician.UI;
using Geo;
using static Geo.Create;

public class RuledAxes
{
    Node axis0;
    Node axis1;
    Node spacers0;
    Node spacers1;
    Node gridLines;

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
            Point(-Runes.Globals.winWidth / 2 - Geo.Ref.Perspective.X, Geo.Ref.Perspective.Y),
            Point(Runes.Globals.winWidth / 2 - Geo.Ref.Perspective.X, -Geo.Ref.Perspective.Y)
        );
        // the vertical spacers along that axis
        spacers0 = new Node().Flagged(DrawMode.INVISIBLE);

        // the vertical axis on a 2d graph
        axis1 = Line(
            Point(-Geo.Ref.Perspective.X, Runes.Globals.winWidth / 2 - Geo.Ref.Perspective.Y),
            Point(-Geo.Ref.Perspective.X, -Runes.Globals.winWidth / 2 - Geo.Ref.Perspective.Y)
        );
        // the horizontal spacers along that axis
        spacers1 = new Node().Flagged(DrawMode.INVISIBLE);

        // The grid lines, which will appear behind the spacers
        gridLines = new Node().Flagged(DrawMode.INVISIBLE);

        // Add spacers to the horizontal axis
        int horizSpacers = (int)(Runes.Globals.winWidth / horizSpacing);
        double vertSubdivSpacing = vertSpacing / vertSubdivs;
        double horizSubdivSpacing = horizSpacing / horizSubdivs;
        for (int i = 0; i < horizSpacers; i++)
        {
            Paint.Text tx = new Paint.Text($"{(int)(i * hSp - Runes.Globals.winWidth / 2)}", Runes.Col.UIDefault.FG, Runes.Globals.fontSize);
            Node horizSpacer = Line(
                Point(i * horizSpacing - Runes.Globals.winWidth / 2, spacerSize / 2 - Geo.Ref.Perspective.Y),
                Point(i * horizSpacing - Runes.Globals.winWidth / 2, -spacerSize / 2 - Geo.Ref.Perspective.Y)
            );
            // If I attach the .Textured(tx.Render()) to the Point call above, the texture comes out null
            // This is because of how Geo.Create.Line works.
            horizSpacer[0].Textured(tx.Render());

            spacers0.Add(horizSpacer);
            tx.Dispose();


            // Add smaller perpendicular subdividers
            for (int j = 0; j < horizSubdivs; j++)
            {
                Node horizSubdiv = Line(
                    Point(i * horizSpacing - Runes.Globals.winWidth / 2 + j * horizSubdivSpacing, subdivSize / 2 - Geo.Ref.Perspective.Y),
                    Point(i * horizSpacing - Runes.Globals.winWidth / 2 + j * horizSubdivSpacing, -subdivSize / 2 - Geo.Ref.Perspective.Y)
                );
                spacers0.Add(horizSubdiv);
            }

            // Vertical grid lines
            if (i % 2 != 0) { continue; }
            gridLines.Add(
                Line(
                    Point(i * horizSpacing - Runes.Globals.winWidth / 2, -Runes.Globals.winWidth),
                    Point(i * horizSpacing - Runes.Globals.winWidth / 2, Runes.Globals.winWidth),
                    Runes.Col.UIDefault[1]
            ));
        }

        // Add spacers to the vertical axis
        int vertSpacers = (int)(Runes.Globals.winHeight / vertSpacing);
        for (int i = 0; i < vertSpacers; i++)
        {
            Node vertSpacer = Line(
                Point(spacerSize / 2 - Geo.Ref.Perspective.X, i * vertSpacing - Runes.Globals.winHeight / 2),
                Point(-spacerSize / 2 - Geo.Ref.Perspective.X, i * vertSpacing - Runes.Globals.winHeight / 2)
            );
            spacers1.Add(vertSpacer);


            // Add smaller perpendicular subdividers
            //double vertSubdivSpacing = vertSpacing / vertSubdivs;
            for (int j = 0; j < vertSubdivs; j++)
            {
                Node vertSubdiv = Line(
                    Point(-subdivSize / 2 - Geo.Ref.Perspective.X, i * vertSpacing + j * vertSubdivSpacing - Runes.Globals.winHeight / 2),
                    Point(subdivSize / 2 - Geo.Ref.Perspective.X, i * vertSpacing + j * vertSubdivSpacing - Runes.Globals.winHeight / 2)
                    );
                spacers1.Add(vertSubdiv);
            }

            // Horizontal grid lines
            if (i % 2 != 0) { continue; }
            gridLines.Add(
                Line(
                    Point(-Runes.Globals.winWidth, i * vertSpacing - Runes.Globals.winHeight / 2),
                    Point(Runes.Globals.winWidth, i * vertSpacing - Runes.Globals.winHeight / 2),
                    Runes.Col.UIDefault[1]
            ));
        }
    }

    // Update the grid according to the UI Perspective
    public RuledAxes Update()
    {
        return new RuledAxes(hSp, hSd, vSp, vSd);
    }

    public Node Render()
    {

        return new Node(gridLines, axis0.Adjoined(spacers0), axis1.Adjoined(spacers1)).Flagged(DrawMode.INVISIBLE)
        ;
    }
}