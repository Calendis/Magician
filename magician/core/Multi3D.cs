using System;

namespace Magician
{
    // While it's possible to build a 3D Multi out of 2D Multis manually, this approach is impractical
    // The way a 2D Multi is drawn is inherent to the position of its constituent Multis, meaning each
    // face of a manually-built 3D Multi needs to be a multi with a number of constituents. This nested-
    // -ness makes the 3D Multi extremely impractical to manipulate, so we use a Multi3D instead.
    // Multi3Ds have custom drawing behaviour and do not need to be nested. However, faces must be
    // defined.
    public class Multi3D : Multi
    {
        List<List<int>>? faces;
        // Full constructor
        public Multi3D(double x, double y, double z, int n = 0, Color? col = null, DrawMode dm = DrawMode.FULL, params Multi[] points) : base(x, y, z, col, dm, points)
        {
            if (n > 0)
            {
                FacesSimplex(n);
            }
        }
        public Multi3D(double x, double y, double z, int n = 0, params Multi[] points) : this(x, y, z, n, null, DrawMode.FULL, points) { }

        public override void Draw(double xOffset, double yOffset)
        {
            //base.Draw(xOffset, yOffset);
            int cc = 0;
            foreach (List<int> face in faces)
            {
                Multi f = new Multi().Positioned(X, Y, Z).Colored(new HSLA((cc++*Math.PI)/Math.PI, 1, 1, 120)).DrawFlags(drawMode);
                foreach (int idx in face)
                {
                    f.Add(csts[idx]);
                }
                f.Draw(xOffset, yOffset);
            }
        }

        // Methods to generate faces
        public void FacesNearest(int n)
        {
        }

        // For testing
        public void FacesSimplex(int n)
        {
            faces = new List<List<int>>();
            faces.Add(new List<int> {0, 1, 2});
            faces.Add(new List<int> {0, 1, 3});
            faces.Add(new List<int> {1, 2, 3});
            faces.Add(new List<int> {0, 2, 3});
        }

        // Groups the Multi into n equal faces. Assumes the constituents are grouped by faces
        public void FacesGrouped(int n)
        {
            faces = new List<List<int>>();
            if (Count % (n) != 0)
            {
                throw Scribe.Error($"Could not group {Count} points into {n} equal faces!");
            }
            List<int> newFace = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                newFace.Add(i);
                if (i % n == n-1)
                {
                    faces.Add(newFace);
                    newFace = new List<int>();
                }
            }
        }

        public void FacesGrouped(params int[] n)
        {
            //
        }

        public void FacesGrouped(IMap im)
        {
            //
        }
    }
}
