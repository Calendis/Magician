/* 
    The Spell provides the environment for the simulation. It does nothing until you pass it to the
    Spellcaster.Cast method
 */
namespace Magician.Core.Caster;
using Geo;
using Magician.Paint;
using Runes;

public abstract class Spell
{
    public double Time { get; set; }
    public Random RNG = new Random();
    public double RandX => RNG.NextDouble() * Globals.winWidth - Globals.winWidth / 2;
    public double RandY => RNG.NextDouble() * Globals.winHeight - Globals.winHeight / 2;

    // The Origin is the root for a tree of Multis
    public Node Origin { get; set; } = Create.Point(null, 0, 0, Runes.Col.UIDefault.FG)
    .Flagged(DrawMode.INVISIBLE)
    .Tagged("Spell origin")//.Parented(null)  // TODO: bring this back
    ;

    // Sets the initial condirions for the Spell. This is called automatically by the Spellcaster
    public abstract void PreLoop();
    // The main loop of the spell. This is called automatically by the spellcaster
    public abstract void Loop();

    internal void InitRenderCache()
    {
        //
    }
    public void Render()
    {
        // Render all object data
        Ref.Origin.Render(0, 0, 0, 0, 0, 0);
        // Assign buffer ranges before drawing
        Paint.Render.CacheRender();
        // Send stuff to the GPU
        Renderer.Drawables.DrawAll();
        Renderer.Drawables.Todo.ps.Clear();
        Renderer.Drawables.Todo.ls.Clear();
        Renderer.Drawables.Todo.ts.Clear();

        // SAVE FRAME TO IMAGE
        //if (Renderer.RControl.saveFrame && frames != stopFrame)
    }
}