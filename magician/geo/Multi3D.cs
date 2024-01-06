using System;

namespace Magician.Geo;
// While it's possible to build a 3D Multi out of 2D Multis manually, this approach is impractical.
// The way a 2D Multi is drawn is inherent to the position of its constituent Multis, meaning each
// face of a manually-built 3D Multi needs to be a multi with a number of constituents. This nested-
// -ness makes the 3D Multi extremely impractical to manipulate, so we use a Multi3D instead.
// Multi3Ds have custom drawing behaviour and do not need to be nested. However, faces must be
// defined.
public class Multi3D : Multi
{
    List<int[]>? faces;
    public List<int[]>? Faces => faces;
    // Full constructor
    public Multi3D(double x, double y, double z, Color? col = null, DrawMode dm = DrawMode.FULL, params Multi[] points) : base(x, y, z, col, dm, points) { }
    public Multi3D(double x, double y, double z, params Multi[] points) : this(x, y, z, null, DrawMode.FULL, points) { }
    public Multi3D(Multi m) : base(m.x.Get(), m.y.Get(), m.z.Get(), m.Col, m.DrawFlags, m.Constituents.ToArray()) { }

    public override void Render(double xOffset, double yOffset, double zOffset)
    {
        if (faces is null)
            throw Scribe.Error($"Must define faces of Multi3D {this}");
            
        int cc = 0;
        foreach (int[] face in faces)
        {
            Multi f = new Multi().To(x.Get(), y.Get(), z.Get())
            .Flagged(drawMode).Tagged($"face{cc}");
            foreach (int idx in face)
            {
                // Hack to stop crashes for plot debugging
                if (idx >= Count)
                {
                    //Scribe.Warn($"{idx} / {Count}");
                    //continue;
                }
                f.Add(constituents[idx]);
                f.Colored(constituents[idx].Col);
                // TODO: remove this faux-lighting
                f.Col.L = 1-(((float)idx)/4000);
            }
            f.Render(xOffset, yOffset, zOffset);
        }
    }

    public override Multi3D Copy()
    {
        Multi3D c = new Multi3D(base.Copy());
        c.faces = faces;
        return c;
    }

    // Methods to generate faces
    public void FacesNearest(int n)
    {
    }

    // Arrange faces in a tetrahedral pattern
    public Multi3D FacesSimplex()
    {
        return FacesGrouped(3,
            0, 1, 2,
            0, 1, 3,
            1, 2, 3,
            0, 2, 3
        );
    }
    // Arrange faces in a cubic pattern
    public Multi3D FacesCube()
    {
        return FacesGrouped(4,
        0, 1, 2, 3,
        4, 5, 6, 7,
        0, 4, 5, 1,
        3, 7, 6, 2,
        1, 5, 6, 2,
        0, 4, 7, 3
        );
    }

    public Multi3D FacesGrouped(int faceSize, params int[] fs)
    {
        faces = new();
        List<int> currentFace = new();
        for (int i = 0; i < fs.Length; i++)
        {
            currentFace.Add(fs[i]);
            if (i % faceSize == faceSize - 1)
            {
                faces.Add(currentFace.ToArray());
                currentFace = new List<int>();
            }
        }
        return this;
    }

    public Multi3D SetFaces(List<int[]> newFaces)
    {
        faces = new();
        for (int i = 0; i < newFaces.Count; i++)
        {
            int[] face = newFaces[i];
            faces.Add(new int[face.Length]);
            for (int j = 0; j < face.Length; j++)
            {
                faces[i][j] = face[j];
            }
        }
        return this;
    }

    /* public void FacesGrouped(params int[] n)
    {
        //
    }

    public void FacesGrouped(IMap im)
    {
        //
    } */
}