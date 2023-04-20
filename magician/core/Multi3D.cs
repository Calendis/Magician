using System;

namespace Magician;
// While it's possible to build a 3D Multi out of 2D Multis manually, this approach is impractical.
// The way a 2D Multi is drawn is inherent to the position of its constituent Multis, meaning each
// face of a manually-built 3D Multi needs to be a multi with a number of constituents. This nested-
// -ness makes the 3D Multi extremely impractical to manipulate, so we use a Multi3D instead.
// Multi3Ds have custom drawing behaviour and do not need to be nested. However, faces must be
// defined.
public class Multi3D : Multi
{
    List<List<int>>? faces;
    // Full constructor
    public Multi3D(double x, double y, double z, Color? col = null, DrawMode dm = DrawMode.FULL, params Multi[] points) : base(x, y, z, col, dm, points) { }
    public Multi3D(double x, double y, double z, params Multi[] points) : this(x, y, z, null, DrawMode.FULL, points) { }
    public Multi3D(Multi m) : base(m.x.Evaluate(), m.y.Evaluate(), m.z.Evaluate(), m.Col, m.DrawFlags, m.Constituents.ToArray()) { }

    public override void Render(double xOffset, double yOffset, double zOffset, bool scale3d = true)
    {
        if (faces is null)
            throw Scribe.Error($"Must define faces of Multi3D {this}");
            
        int cc = 0;
        foreach (List<int> face in faces)
        {
            // Do not draw faces behind the camera
            bool occluded = false;
            for (int j = 0; j < face.Count; j++)
            {
                if (csts[face[j]].Z <= Geo.Ref.Perspective.Z)
                {
                    occluded = true;
                    break;
                }
            }
            if (occluded)
            {
                continue;
            }
            if (faces == null)
            {
                throw Scribe.Error($"Faces of {this} were null");
            }
            Multi f = new Multi().Positioned(x.Evaluate(), y.Evaluate(), z.Evaluate()).Colored(new HSLA((cc++ * Math.PI) / Math.PI, 1, 1, 120))
            .WithFlags(drawMode).Tagged($"face{cc}");
            foreach (int idx in face)
            {
                f.Add(csts[idx]);
            }
            f.Render(xOffset, yOffset, zOffset, true);
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
        faces = new List<List<int>>();
        // Tbh, I have no idea how to check to see if 3d points can fit in a certain number of faces...
        /* if (Count % faceSize != 0)
        {
            throw Scribe.Error($"Could not group\n{this}\n into faces of size {faceSize}");
        } */
        List<int> currentFace = new List<int>();
        for (int i = 0; i < fs.Length; i++)
        {
            currentFace.Add(fs[i]);
            if (i % faceSize == faceSize - 1)
            {
                faces.Add(currentFace);
                currentFace = new List<int>();
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