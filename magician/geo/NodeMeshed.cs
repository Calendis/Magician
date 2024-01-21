/* TODO: remove this class. Node will have parity, with an optional Mesh
         remember to update Node's Copy method
*/

namespace Magician.Geo;
// While it's possible to build a 3D Multi out of 2D Multis manually, this approach is impractical.
// The way a 2D Multi is drawn is inherent to the position of its constituent Multis, meaning each
// face of a manually-built 3D Multi needs to be a multi with a number of constituents. This nested-
// -ness makes the 3D Multi extremely impractical to manipulate, so we use a Multi3D instead.
// Multi3Ds have custom drawing behaviour and do not need to be nested. However, faces must be
// defined.
public class NodeMeshed : Node
{
    //List<int[]>? faces;
    //public List<int[]>? Faces => faces;
    // Full constructor
    protected Mesh? faces;
    public NodeMeshed(double x, double y, double z, Mesh? mesh=null, Color? col = null, DrawMode dm = DrawMode.FULL, params Node[] points) : base(x, y, z, col, dm, points)
    {
        faces = mesh;
    }
    public NodeMeshed(Node m, Mesh mesh) : this(m.x.Get(), m.y.Get(), m.z.Get(), mesh, m.Col, m.DrawFlags, m.Constituents.ToArray()) { }
    public NodeMeshed(double x, double y, double z, Mesh mesh, params Node[] points) : this(x, y, z, mesh, null, DrawMode.FULL, points) { }

    public override void Render(double xOffset, double yOffset, double zOffset)
    {
        if (faces is null)
            throw Scribe.Error($"Must define faces of Multi3D {this}");
            
        foreach (int[] face in faces.Faces)
        {
            List<double[]> projected = Paint.Render.Project(face.Select(i => this[i]), xOffset+x.Get(), yOffset+y.Get(), zOffset+z.Get());
            List<double[]> culled = Paint.Render.Cull(this, xOffset, yOffset, zOffset, projected, face);
            List<Color> cols = face.Select(i => this[i].Col).ToList();
            Paint.Render.Polygon(culled.ToArray(), drawMode, cols, this);
            
            // Old slow rendering method
            //Node f = new Node().To(x.Get(), y.Get(), z.Get()).Flagged(DrawMode.OUTER);
            //foreach (int idx in face)
            //{
            //    f.Add(this[idx]);
            //    f.Colored(this[idx].Col);
            //    // TODO: remove this faux-lighting
            //    f.Col.L = 1-(((float)idx)/4000);
            //}
            //f.Render(xOffset, yOffset, zOffset);
        }
    }

    public override NodeMeshed Copy()
    {
        NodeMeshed c = new NodeMeshed(base.Copy(), faces);
        c.faces = faces;
        return c;
    }
}