using System;

namespace Renderer
{
    public static class Alg
    {
        // Iterated logarithm
        public static int Logstar(double n)
        {
            int i;
            for (i = 0; n >= 1; i++)
            {
                n = Math.Log2(n);
            }
            return i - 1;
        }

        // TODO: find out what this does
        public static int N(double n, double h)
        {
            double v = n;
            for (int i = 0; i < h; i++)
            {
                v = Math.Log2(v);
            }
            return (int)Math.Ceiling(n / v);
        }

        public static bool GreaterThan(Point_t p0, Point_t p1)
        {
            double x0 = p0.x;
            double y0 = p0.y;
            double x1 = p1.x;
            double y1 = p1.y;
            return y0 > y1 ? true : (y0 < y1 ? false : (x0 > x1 ? true : false));
        }

        public static bool LessThan(Point_t p0, Point_t p1)
        {
            double x0 = p0.x;
            double y0 = p0.y;
            double x1 = p1.x;
            double y1 = p1.y;
            return y0 < y1 ? true : (y0 > y1 ? false : (x0 < x1 ? true : false));
        }

        public static bool GreaterThanEqTo(Point_t p0, Point_t p1)
        {
            double x0 = p0.x;
            double y0 = p0.y;
            double x1 = p1.x;
            double y1 = p1.y;
            return y0 > y1 ? true : (y0 < y1 ? false : (x0 > x1 ? true : true));
        }

        public static bool EqualTo(Point_t p0, Point_t p1)
        {
            return p0.x == p1.x && p0.y == p1.y;
        }

        // Is vertex v left of Segment segs[segnum]? Handles degenerate cases when both of
        // the vertices have the same y-coord., etc.
        public static bool IsLeftOf(int segnum, Point_t v)
        {
            Segment s = Geo.segs[segnum];
            double area;
            if (GreaterThan(s.p1, s.p0))  // Segment going upwards
            {
                if (s.p1.y == v.y)
                {
                    if (v.x < s.p1.x)
                    {
                        area = 1;
                    }
                    else
                    {
                        area = -1;
                    }
                }
                else if (s.p0.y == v.y)
                {
                    if (v.x < s.p0.x)
                    {
                        area = 1;
                    }
                    else
                    {
                        area = -1;
                    }
                }
                else
                {
                    area = Cross(s.p0, s.p1, v);
                }
            }
            else
            {
                if (s.p1.y == v.y)
                {
                    if (v.x < s.p1.x)
                    {
                        area = 1;
                    }
                    else
                    {
                        area = -1;
                    }
                }
                else if (s.p0.y == v.y)
                {
                    if (v.x < s.p0.x)
                    {
                        area = 1;
                    }
                    else
                    {
                        area = -1;
                    }
                }
                else
                {
                    area = Cross(s.p1, s.p0, v);
                }
            }
            return area > 0;
        }

        public static double Cross(Point_t v0, Point_t v1, Point_t v2)
        {
            return (v1.x - v0.x) * (v2.y - v0.y) - (v1.y - v0.y) * (v2.x - v0.x);
        }

        public static double Dot(Point_t v0, Point_t v1)
        {
            return v0.x * v1.x + v0.y * v1.y;
        }

        public static double GetAngle(Point_t vp0, Point_t vpnext, Point_t vp1)
        {
            Point_t v0, v1;

            v0.x = vpnext.x - vp0.x;
            v0.y = vpnext.y - vp0.y;

            v1.x = vp1.x - vp0.x;
            v1.y = vp1.y - vp0.y;

            if (CrossSine(v0, v1) >= 0) /* sine is positive */
                return Dot(v0, v1) / Length(v0) / Length(v1);
            else
                return (-1.0 * Dot(v0, v1) / Length(v0) / Length(v1) - 2);
        }

        public static double CrossSine(Point_t v0, Point_t v1)
        {
            return v0.x * v1.y - v1.x * v0.y;
        }

        public static double Length(Point_t v0)
        {
            return Math.Sqrt(v0.x * v0.x + v0.y * v0.y);
        }

    }

    /*
    *  The big-daddy renderer class. Geo handles importing of data from a Multi
    *  and, using Siedel's algorithm, its division into triangles.
    */
    public static class Geo
    {

        static int MAX_SEGMENTS;
        static int MAX_NODES;
        static int MAX_TRAPEZOIDS;
        public static Node[] query;
        public static Segment[] segs;
        public static Trapezoid[] trapezoids;

        static int q_idx;
        static int tr_idx;
        static int choose_idx = 1;
        public static int segnum = 0;

        ////

        static Vertexchain[] vertices;
        static Monchain[] mchain;
        static int[] mon;
        static bool[] visited;
        static int chain_idx, op_idx, mon_idx;

        const int SP_SIMPLE_LRUP = 1;
        const int SP_SIMPLE_LRDN = 2;
        const int SP_2UP_2DN = 3;
        const int SP_2UP_LEFT = 4;
        const int SP_2UP_RIGHT = 5;
        const int SP_2DN_LEFT = 6;
        const int SP_2DN_RIGHT = 7;
        const int SP_NOSPLIT = -1;

        const int TR_FROM_UP = 1;
        const int TR_FROM_DN = 2;

        const int TRI_LHS = 1;
        const int TRI_RHS = 2;

        // Initialize arrays and read segments from Multi
        public static List<int[]> Triangulate(Magician.Multi m)
        {
            MAX_SEGMENTS = m.Count;
            MAX_NODES = 8 * MAX_SEGMENTS;
            MAX_TRAPEZOIDS = 4 * MAX_SEGMENTS;

            segs = new Segment[MAX_SEGMENTS + 1];
            // nodes set in InitQueryStructure
            // trapezoids set in InitQueryStructure
            // vertices set in MonotonateTrapezoids

            int i = 1;
            int first = i;  // Currently, I only support one contour
            int last = MAX_SEGMENTS;  // Max index. The array is of size MAX_SEGMENTS+1

            for (int j = 0; j < MAX_SEGMENTS; j++)
            {

                double x0 = m.Constituents[i - 1].X.Evaluate();
                double y0 = m.Constituents[i - 1].Y.Evaluate();
                segs[i].p0 = new Point_t();
                segs[i].p0.x = x0;
                segs[i].p0.y = y0;

                if (i == last)
                {
                    segs[i].next = first;
                    segs[i].prev = i - 1;
                    segs[i - 1].p1 = segs[i].p0;
                }
                else if (i == first)
                {
                    segs[i].next = i + 1;
                    segs[i].prev = last;
                    segs[last].p1 = segs[i].p0;
                }
                else
                {
                    segs[i].prev = i - 1;
                    segs[i].next = i + 1;
                    segs[i - 1].p1 = segs[i].p0;
                }
                segs[i].isInserted = false;
                i++;
            }

            // Make sure the root node is instantiated
            // However, it should remain empty
            segs[0].isInserted = false;
            segs[0].next = 0;
            segs[0].prev = 0;
            segs[0].p0 = new Point_t();
            segs[0].p1 = new Point_t();

            int n = i - 1;
            //int[][] op = new int[MAX_SEGMENTS][] {};
            List<int[]> op = new List<int[]>();
            for (int k = 0; k < MAX_TRAPEZOIDS; k++)
            {
                op.Add(new int[3]);
            }
            Initialize(n);
            ConstructTrapezoids(n);
            int nmonopoly = MonotonateTrapezoids(n);
            int ntriangles = TriangulateMonotonePolygons(n, nmonopoly, op);
            //Console.WriteLine("Load successful!");
            /*
            for (int k = 0; k < ntriangles; k++)
            {
                Console.WriteLine($"Triangle #{k}: {op[k][0]},{op[k][1]},{op[k][2]}");
            }
            */
            return op;
        }

        public static void Initialize(int n)
        {
            q_idx = 0;
            tr_idx = 0;
            choose_idx = 0;
            for (int i = 1; i <= n; i++)
            {
                segs[i].isInserted = false;
            }
            Random r = new Random();
            // TODO: ensure this shuffle works
            segs.ToList<Segment>().Sort((s0, s1) => r.Next(2) == 1 ? 1 : -1);
        }

        public static int TriangulateMonotonePolygons(int nvert, int nmonopoly, List<int[]> op)
        {
            int i;
            Point_t ymax, ymin;
            int p, vfirst, posmax, posmin, v;
            int vcount;
            bool processed;

            op_idx = 0;
            for (i = 0; i < nmonopoly; i++)
            {
                vcount = 1;
                processed = false;
                vfirst = mchain[mon[i]].vnum;
                ymax = ymin = vertices[vfirst].pt;
                posmax = posmin = mon[i];
                mchain[mon[i]].marked = false;
                p = mchain[mon[i]].next;
                while ((v = mchain[p].vnum) != vfirst)
                {
                    if (mchain[p].marked)
                    {
                        processed = true;
                        break; /* break from while */
                    }
                    else
                        mchain[p].marked = true;

                    if (Alg.GreaterThan(vertices[v].pt, ymax))
                    {
                        ymax = vertices[v].pt;
                        posmax = p;
                    }
                    if (Alg.LessThan(vertices[v].pt, ymin))
                    {
                        ymin = vertices[v].pt;
                        posmin = p;
                    }
                    p = mchain[p].next;
                    vcount++;
                }

                if (processed) /* Go to next polygon */
                    continue;

                if (vcount == 3) /* already a triangle */
                {
                    op[op_idx] = new int[3];
                    op[op_idx][0] = mchain[p].vnum;
                    op[op_idx][1] = mchain[mchain[p].next].vnum;
                    op[op_idx][2] = mchain[mchain[p].prev].vnum;
                    op_idx++;
                }
                else /* triangulate the polygon */
                {
                    v = mchain[mchain[posmax].next].vnum;
                    if (Alg.EqualTo(vertices[v].pt, ymin))
                    { /* LHS is a single line */
                        TriangulateSinglePolygon(nvert, posmax, TRI_LHS, op);
                    }
                    else
                        TriangulateSinglePolygon(nvert, posmax, TRI_RHS, op);
                }
            }
            return op_idx;
        }

        static int TriangulateSinglePolygon(int nvert, int posmax, int side, List<int[]> op)
        {
            int v;
            int[] rc = new int[MAX_SEGMENTS];
            int ri = 0;
            int endv, tmp, vpos;

            // RHS is a single segment
            if (side == TRI_RHS)
            {
                rc[0] = mchain[posmax].vnum;
                tmp = mchain[posmax].next;
                rc[1] = mchain[tmp].vnum;
                ri = 1;

                vpos = mchain[tmp].next;
                v = mchain[vpos].vnum;

                if ((endv = mchain[mchain[posmax].prev].vnum) == 0)
                    endv = nvert;
            }
            // LHS is a single segment
            else
            {
                tmp = mchain[posmax].next;
                rc[0] = mchain[tmp].vnum;
                tmp = mchain[tmp].next;
                rc[1] = mchain[tmp].vnum;
                ri = 1;

                vpos = mchain[tmp].next;
                v = mchain[vpos].vnum;

                endv = mchain[posmax].vnum;
            }

            while (v != endv || ri > 1)
            {
                if (ri > 0) /* reflex chain is non-empty */
                {
                    if (Alg.Cross(vertices[v].pt, vertices[rc[ri - 1]].pt, vertices[rc[ri]].pt) >
                        0)
                    { /* convex corner: cut if off */
                        //op[op_idx] = new int[3];
                        op[op_idx][0] = rc[ri - 1];
                        op[op_idx][1] = rc[ri];
                        op[op_idx][2] = v;
                        op_idx++;
                        ri--;
                    }
                    else /* non-convex */
                    {      /* add v to the chain */
                        ri++;
                        Console.WriteLine($"ri: {rc[0]}, {rc[1]}, {rc[2]}");
                        rc[ri] = v;
                        vpos = mchain[vpos].next;
                        v = mchain[vpos].vnum;
                    }
                }
                else /* reflex-chain empty: add v to the */
                {      /* reflex chain and advance it  */
                    rc[++ri] = v;
                    vpos = mchain[vpos].next;
                    v = mchain[vpos].vnum;
                }
            }

            /* reached the bottom vertex. Add in the triangle formed */
            op[op_idx][0] = rc[ri - 1];
            op[op_idx][1] = rc[ri];
            op[op_idx][2] = v;
            op_idx++;
            ri--;

            return 0;
        }

        public static int MonotonateTrapezoids(int n)
        {
            int i;
            int tr_start;
            vertices = new Vertexchain[MAX_SEGMENTS + 1];
            mchain = new Monchain[MAX_TRAPEZOIDS + 1];
            mon = new int[MAX_SEGMENTS + 1];
            visited = new bool[MAX_TRAPEZOIDS + 1];
            //chain_idx, op_idx, mon_idx

            /* First locate a trapezoid which lies inside the polygon */
            /* and which is triangular */
            for (i = 0; i < MAX_TRAPEZOIDS + 1; i++)
                if (InsidePolygon(trapezoids[i]))
                    break;
            tr_start = i - 1;

            /* Initialise the mon data-structure and start spanning all the */
            /* trapezoids within the polygon */

            for (i = 1; i <= n; i++)
            {
                mchain[i].prev = segs[i].prev;
                mchain[i].next = segs[i].next;
                mchain[i].vnum = i;
                vertices[i].pt = segs[i].p0;
                unsafe
                {
                    vertices[i].vnext[0] = segs[i].next; /* next vertex */
                    vertices[i].vpos[0] = i;            /* locn. of next vertex */
                }
                vertices[i].nextfree = 1;
            }

            chain_idx = n;
            mon_idx = 0;
            mon[0] = 1; /* position of any vertex in the first */
            /* chain  */
            //

            /* traverse the polygon */
            if (trapezoids[tr_start].u0 > 0)
                TraversePolygon(0, tr_start, trapezoids[tr_start].u0, TR_FROM_UP);
            else if (trapezoids[tr_start].d0 > 0)
                TraversePolygon(0, tr_start, trapezoids[tr_start].d0, TR_FROM_DN);

            /* return the number of polygons created */
            return newmon();
        }

        static int newmon()
        {
            return ++mon_idx;
        }

        static int TraversePolygon(int mcur, int trnum, int from, int dir)
        {
            Trapezoid t = trapezoids[trnum];
            int howsplit, mnew;
            int v0, v1, v0next, v1next;
            int retval = 0, tmp;
            bool do_switch = false;

            if ((trnum <= 0) || visited[trnum])
                return 0;

            visited[trnum] = true;

            /* We have much more information available here. */
            /* rseg: goes upwards   */
            /* lseg: goes downwards */

            /* Initially assume that dir = TR_FROM_DN (from the left) */
            /* Switch v0 and v1 if necessary afterwards */

            /* special cases for triangles with cusps at the opposite ends. */
            /* take care of this first */
            if ((t.u0 <= 0) && (t.u1 <= 0))
            {
                //
                if ((t.d0 > 0) && (t.d1 > 0)) /* downward opening triangle */
                {
                    v0 = trapezoids[t.d1].lSeg;
                    v1 = t.lSeg;
                    if (from == t.d1)
                    {
                        do_switch = true;
                        mnew = MakeNewMonotonePoly(mcur, v1, v0);
                        TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                        TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                    }
                    else
                    {
                        mnew = MakeNewMonotonePoly(mcur, v0, v1);
                        TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                        TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                    }
                }
                else
                {
                    retval = SP_NOSPLIT; /* Just traverse all neighbours */
                    TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                    TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                    TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                    TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                }
            }
            else if ((t.d0 <= 0) && (t.d1 <= 0))
            {
                if ((t.u0 > 0) && (t.u1 > 0)) /* upward opening triangle */
                {
                    v0 = t.rSeg;
                    v1 = trapezoids[t.u0].rSeg;
                    if (from == t.u1)
                    {
                        do_switch = true;
                        mnew = MakeNewMonotonePoly(mcur, v1, v0);
                        TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                        TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                    }
                    else
                    {
                        mnew = MakeNewMonotonePoly(mcur, v0, v1);
                        TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                        TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                    }
                }
                else
                {
                    retval = SP_NOSPLIT; /* Just traverse all neighbours */
                    TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                    TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                    TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                    TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                }
            }
            else if ((t.u0 > 0) && (t.u1 > 0))
            {
                if ((t.d0 > 0) && (t.d1 > 0)) /* downward + upward cusps */
                {
                    v0 = trapezoids[t.d1].lSeg;
                    v1 = trapezoids[t.u0].rSeg;
                    retval = SP_2UP_2DN;
                    if (((dir == TR_FROM_DN) && (t.d1 == from)) ||
                        ((dir == TR_FROM_UP) && (t.u1 == from)))
                    {
                        do_switch = true;
                        mnew = MakeNewMonotonePoly(mcur, v1, v0);
                        TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                        TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                        TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                        TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                    }
                    else
                    {
                        mnew = MakeNewMonotonePoly(mcur, v0, v1);
                        TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                        TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                        TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                        TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                    }
                }
                else /* only downward cusp */
                {
                    if (Alg.EqualTo(t.lo, segs[t.lSeg].p1))
                    {
                        v0 = trapezoids[t.u0].rSeg;
                        v1 = segs[t.lSeg].next;

                        retval = SP_2UP_LEFT;
                        if ((dir == TR_FROM_UP) && (t.u0 == from))
                        {
                            do_switch = true;
                            mnew = MakeNewMonotonePoly(mcur, v1, v0);
                            TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                        }
                        else
                        {
                            mnew = MakeNewMonotonePoly(mcur, v0, v1);
                            TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                        }
                    }
                    else
                    {
                        v0 = t.rSeg;
                        v1 = trapezoids[t.u0].rSeg;
                        retval = SP_2UP_RIGHT;
                        if ((dir == TR_FROM_UP) && (t.u1 == from))
                        {
                            do_switch = true;
                            mnew = MakeNewMonotonePoly(mcur, v1, v0);
                            TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                        }
                        else
                        {
                            mnew = MakeNewMonotonePoly(mcur, v0, v1);
                            TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                        }
                    }
                }
            }
            else if ((t.u0 > 0) || (t.u1 > 0))
            {
                if ((t.d0 > 0) && (t.d1 > 0)) /* only upward cusp */
                {

                    int mlsg = t.lSeg == -1 ? 0 : t.lSeg;
                    if (Alg.EqualTo(t.hi, segs[t.lSeg].p0))
                    {
                        v0 = trapezoids[t.d1].lSeg;
                        v1 = t.lSeg;
                        retval = SP_2DN_LEFT;
                        if (!((dir == TR_FROM_DN) && (t.d0 == from)))
                        {
                            do_switch = true;
                            mnew = MakeNewMonotonePoly(mcur, v1, v0);
                            TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                        }
                        else
                        {
                            mnew = MakeNewMonotonePoly(mcur, v0, v1);
                            TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                        }
                    }
                    else
                    {
                        v0 = trapezoids[t.d1].lSeg;
                        v1 = segs[t.rSeg].next;

                        retval = SP_2DN_RIGHT;
                        if ((dir == TR_FROM_DN) && (t.d1 == from))
                        {
                            do_switch = true;
                            mnew = MakeNewMonotonePoly(mcur, v1, v0);
                            TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                        }
                        else
                        {
                            mnew = MakeNewMonotonePoly(mcur, v0, v1);
                            TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                        }
                    }
                }
                else
                {
                    int mlsg = t.lSeg == -1 ? 0 : t.lSeg;
                    if (Alg.EqualTo(t.hi, segs[t.lSeg].p0) &&
                        Alg.EqualTo(t.lo, segs[t.rSeg].p0))
                    {
                        v0 = t.rSeg;
                        v1 = t.lSeg;
                        retval = SP_SIMPLE_LRDN;
                        if (dir == TR_FROM_UP)
                        {
                            do_switch = true;
                            mnew = MakeNewMonotonePoly(mcur, v1, v0);
                            TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                        }
                        else
                        {
                            mnew = MakeNewMonotonePoly(mcur, v0, v1);
                            TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                        }
                    }
                    else if (Alg.EqualTo(t.hi, segs[t.rSeg].p1) &&
                             Alg.EqualTo(t.lo, segs[t.lSeg].p1))
                    {
                        v0 = segs[t.rSeg].next;
                        v1 = segs[t.lSeg].next;

                        retval = SP_SIMPLE_LRUP;
                        if (dir == TR_FROM_UP)
                        {
                            do_switch = true;
                            mnew = MakeNewMonotonePoly(mcur, v1, v0);
                            TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.d0, trnum, TR_FROM_UP);
                        }
                        else
                        {
                            mnew = MakeNewMonotonePoly(mcur, v0, v1);
                            TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                            TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                            TraversePolygon(mnew, t.u0, trnum, TR_FROM_DN);
                            TraversePolygon(mnew, t.u1, trnum, TR_FROM_DN);
                        }
                    }
                    else /* no split possible */
                    {
                        retval = SP_NOSPLIT;
                        TraversePolygon(mcur, t.u0, trnum, TR_FROM_DN);
                        TraversePolygon(mcur, t.d0, trnum, TR_FROM_UP);
                        TraversePolygon(mcur, t.u1, trnum, TR_FROM_DN);
                        TraversePolygon(mcur, t.d1, trnum, TR_FROM_UP);
                    }
                }
            }

            return retval;
        }

        static int MakeNewMonotonePoly(int mcur, int v0, int v1)
        {
            int p, q, ip, iq;
            int mnew = newmon();
            int i, j, nf0, nf1;
            Vertexchain vp0, vp1;

            vp0 = vertices[v0];
            vp1 = vertices[v1];

            GetVertexPositions(v0, v1, out ip, out iq);

            unsafe
            {
                p = vp0.vpos[ip];
                q = vp1.vpos[iq];
            }

            /* At this stage, we have got the positions of v0 and v1 in the */
            /* desired chain. Now modify the linked lists */

            i = NewChainElement(); /* for the new list */
            j = NewChainElement();

            mchain[i].vnum = v0;
            mchain[j].vnum = v1;

            mchain[i].next = mchain[p].next;
            mchain[mchain[p].next].prev = i;
            mchain[i].prev = j;
            mchain[j].next = i;
            mchain[j].prev = mchain[q].prev;
            mchain[mchain[q].prev].next = j;

            mchain[p].next = q;
            mchain[q].prev = p;

            nf0 = vp0.nextfree;
            nf1 = vp1.nextfree;

            unsafe
            {
                vp0.vnext[ip] = v1;
                vp0.vpos[nf0] = i;
                vp0.vnext[nf0] = mchain[mchain[i].next].vnum;
                vp1.vpos[nf1] = j;
                vp1.vnext[nf1] = v0;
            }
            vp0.nextfree++;
            vp1.nextfree++;

            mon[mcur] = p;
            mon[mnew] = i;
            return mnew;
        }

        /* (v0, v1) is the new diagonal to be added to the polygon. Find which */
        /* chain to use and return the positions of v0 and v1 in p and q */
        static int GetVertexPositions(int v0, int v1, out int ip, out int iq)
        {
            Vertexchain vp0, vp1;
            int i;
            double angle, temp;
            int tp = 0, tq = 0;

            vp0 = vertices[v0];
            vp1 = vertices[v1];

            /* p is identified as follows. Scan from (v0, v1) rightwards till */
            /* you hit the first segment starting from v0. That chain is the */
            /* chain of our interest */

            angle = -4.0;
            for (i = 0; i < 4; i++)
            {
                unsafe
                {
                    if (vp0.vnext[i] <= 0)
                        continue;
                    if ((temp = Alg.GetAngle(vp0.pt, (vertices[vp0.vnext[i]].pt), vp1.pt)) >
                        angle)
                    {
                        angle = temp;
                        tp = i;
                    }
                }
            }
            ip = tp;

            /* Do similar actions for q */

            angle = -4.0;
            for (i = 0; i < 4; i++)
            {
                unsafe
                {
                    if (vp1.vnext[i] <= 0)
                        continue;
                    if ((temp = Alg.GetAngle(vp1.pt, (vertices[vp1.vnext[i]].pt), vp0.pt)) >
                        angle)
                    {
                        angle = temp;
                        tq = i;
                    }
                }
            }
            iq = tq;
            return 0;
        }

        static int NewChainElement()
        {
            return ++chain_idx;
        }

        /* Is a trapezoid inside the polygon? */
        static bool InsidePolygon(Trapezoid t)
        {
            int rseg = t.rSeg;
            if (t.state == false)
                return false;

            if (t.lSeg <= 0 || t.rSeg <= 0)
                return false;

            if (((t.u0 <= 0) && (t.u1 <= 0)) ||
                ((t.d0 <= 0) && (t.d1 <= 0)))
                return Alg.GreaterThan(segs[rseg].p1, segs[rseg].p0);

            return false;

        }

        /////////

        // Increments q_idx and returns for use as an indexer
        public static int NewNode()
        {
            return q_idx++;
        }

        // Increments and returns tr_idx
        public static int NewTrapezoid()
        {
            trapezoids[tr_idx].lSeg = -1;
            trapezoids[tr_idx].rSeg = -1;
            trapezoids[tr_idx].state = true;
            return tr_idx++;
        }


        // Increments and returns choose_idx
        public static int ChooseSegment()
        {
            return choose_idx++;
        }

        public static int InitialQueryStructure(int segnum)
        {
            int i1, i2, i3, i4, i5, i6, i7, root;
            int t1, t2, t3, t4;
            q_idx = tr_idx = 1;
            Segment s = segs[segnum];

            // Define the initial 7 nodes, with the first being the root
            query = new Node[MAX_NODES + 1];
            trapezoids = new Trapezoid[MAX_TRAPEZOIDS + 1];
            // Initialize all trapezoids
            foreach (Trapezoid tr in trapezoids)
                for (int k = 0; k < trapezoids.Length; k++)
                {
                    trapezoids[k] = new Trapezoid();
                }

            i1 = NewNode();
            query[i1].kind = NodeKind.Y_NODE;
            //query[i1].pY = Alg.GreaterThan(s.p0, s.p1) ? s.p0 : (s.p0.x > s.p1.x ? s.p0 : s.p1);
            query[i1].pY = Alg.GreaterThan(s.p0, s.p1) ? s.p0 : s.p1;
            root = i1;

            query[i1].right = i2 = NewNode();
            query[i2].kind = Renderer.NodeKind.S_NODE;
            query[i2].parent = i1;

            query[i1].left = i3 = NewNode();
            query[i3].kind = NodeKind.Y_NODE;
            query[i3].pY = Alg.GreaterThan(s.p0, s.p1) ? s.p1 : s.p0;
            query[i3].parent = i1;

            query[i3].left = i4 = NewNode();
            query[i4].kind = Renderer.NodeKind.S_NODE;
            query[i4].parent = i3;

            query[i3].right = i5 = NewNode();
            query[i5].kind = NodeKind.X_NODE;
            query[i5].segnum = segnum;
            query[i5].parent = i3;

            query[i5].left = i6 = NewNode();
            query[i6].kind = Renderer.NodeKind.S_NODE;
            query[i6].parent = i5;

            query[i5].right = i7 = NewNode();
            query[i7].kind = Renderer.NodeKind.S_NODE;
            query[i7].parent = i5;

            // Define the initial 4 trapezoids in the trapezoidation
            // The initial trapezoids:
            /*
            * 0: middle left
            * 1: middle right
            * 2: bottommost
            * 3: topmost
            */
            t1 = NewTrapezoid();
            t2 = NewTrapezoid();
            t3 = NewTrapezoid();
            t4 = NewTrapezoid();

            trapezoids[t1].hi = trapezoids[t2].hi = trapezoids[t4].lo = query[i1].pY;
            trapezoids[t1].lo = trapezoids[t2].lo = trapezoids[t3].hi = query[i3].pY;
            trapezoids[t4].hi.y = Double.PositiveInfinity;
            trapezoids[t4].hi.x = Double.PositiveInfinity;
            trapezoids[t3].lo.y = Double.NegativeInfinity;
            trapezoids[t3].lo.x = Double.NegativeInfinity;
            trapezoids[t1].rSeg = trapezoids[t2].lSeg = segnum;
            trapezoids[t1].u0 = trapezoids[t2].u0 = t4;
            trapezoids[t1].d0 = trapezoids[t2].d0 = t3;
            trapezoids[t4].d0 = trapezoids[t3].u0 = t1;
            trapezoids[t4].d1 = trapezoids[t3].u1 = t2;

            trapezoids[t1].sink = i6;
            trapezoids[t2].sink = i7;
            trapezoids[t3].sink = i4;
            trapezoids[t4].sink = i2;

            trapezoids[t1].state = trapezoids[t2].state = true;
            trapezoids[t3].state = trapezoids[t4].state = true;

            query[i2].trnum = t4;
            query[i4].trnum = t3;
            query[i6].trnum = t1;
            query[i7].trnum = t2;

            // Congrats, you just added the first segment of the trapezoidation!
            s.isInserted = true;
            return root;
        }

        public static void ConstructTrapezoids(int nseg)
        {

            /*
            * Initialize Trapezoidation tree / query structure
            */
            int root = Renderer.Geo.InitialQueryStructure(ChooseSegment());

            /*
            * Set the roots of all segments to the initial root
            */
            for (int i = 1; i <= nseg; i++)
            {
                segs[i].root0 = segs[i].root1 = root;
            }

            /*
            * Add the remaining segments into the trapezoidation
            */
            int logstarN = Renderer.Alg.Logstar(nseg);
            // Idk
            for (int h = 1; h <= logstarN; h++)
            {
                for (int i = Renderer.Alg.N(nseg, h - 1) + 1; i <= Renderer.Alg.N(nseg, h); i++)
                {
                    AddSegment(ChooseSegment());
                }

                // Find a new root for the segment endpoints
                for (int i = 1; i <= nseg; i++)
                {
                    FindNewRoots(i);
                }
            }

            for (int i = Alg.N(nseg, Alg.Logstar(nseg)) + 1; i <= nseg; i++)
            {
                AddSegment(ChooseSegment());
            }
        }

        static void AddSegment(int segnum)
        {
            Segment s = segs[segnum];
            int tu, tl, sk, tfirst, tlast, tnext;
            int tfirstr = 0, tlastr = 0;
            int tfirstl, tlastl;
            int i1, i2, t, t1, t2, tn;
            Point_t tpt;  // Top point?
            int tritop = 0, tribot = 0;
            bool isSwapped = false;
            int tmpTriSeg;

            if (Alg.GreaterThan(s.p1, s.p0))
            {
                // Swap points
                tpt = s.p0;
                s.p0 = s.p1;
                s.p1 = tpt;

                // Swap roots
                int tmp;
                tmp = s.root0;
                s.root0 = s.root1;
                s.root1 = tmp;
                isSwapped = true;
            }

            if (isSwapped ? !IsInserted(segnum, 2) : !IsInserted(segnum, 1))
            {
                int tmp_d;
                tu = LocateEndpoint(s.p0, s.p1, s.root0);
                tl = NewTrapezoid();

                trapezoids[tl].state = true;
                trapezoids[tl] = trapezoids[tu];
                trapezoids[tu].lo.y = trapezoids[tl].hi.y = s.p0.y;
                trapezoids[tu].lo.x = trapezoids[tl].hi.x = s.p0.x;
                trapezoids[tu].d0 = tl;
                trapezoids[tu].d1 = 0;
                trapezoids[tl].u0 = tu;
                trapezoids[tl].u1 = 0;

                if (((tmp_d = trapezoids[tl].d0) > 0) && (trapezoids[tmp_d].u0 == tu))
                {
                    trapezoids[tmp_d].u0 = tl;
                }
                if (((tmp_d = trapezoids[tl].d0) > 0) && (trapezoids[tmp_d].u1 == tu))
                {
                    trapezoids[tmp_d].u1 = tl;
                }
                if (((tmp_d = trapezoids[tl].d1) > 0) && (trapezoids[tmp_d].u0 == tu))
                {
                    trapezoids[tmp_d].u0 = tl;
                }
                if (((tmp_d = trapezoids[tl].d1) > 0) && (trapezoids[tmp_d].u1 == tu))
                {
                    trapezoids[tmp_d].u1 = tl;
                }

                // Update the query and obtain the sinks for the trapezoids
                i1 = NewNode();
                i2 = NewNode();
                sk = trapezoids[tu].sink;

                query[sk].kind = NodeKind.Y_NODE;
                query[sk].pY = s.p0;
                query[sk].segnum = segnum;  // not necessary according to original?
                query[sk].left = i2;
                query[sk].right = i1;

                query[i1].kind = NodeKind.S_NODE;
                query[i1].trnum = tu;
                query[i1].parent = sk;

                query[i2].kind = NodeKind.S_NODE;
                query[i2].trnum = tl;
                query[i2].parent = sk;

                trapezoids[tu].sink = i1;
                trapezoids[tl].sink = i2;
                tfirst = tl;
            }

            // p0 already present in existing segment
            // Get the topmost intersecting trapezoid
            else
            {
                tfirst = LocateEndpoint(s.p0, s.p1, s.root0);
                tritop = 1;
            }

            if (isSwapped ? !IsInserted(segnum, 1) : !IsInserted(segnum, 2))
            {
                int tmp_d;
                tu = LocateEndpoint(s.p1, s.p0, s.root1);
                tl = NewTrapezoid();
                trapezoids[tl].state = true;
                trapezoids[tl] = trapezoids[tu];
                trapezoids[tu].lo.y = trapezoids[tl].hi.y = s.p1.y;
                trapezoids[tu].lo.x = trapezoids[tl].hi.x = s.p1.x;
                trapezoids[tu].d0 = tl;
                trapezoids[tu].d1 = 0;
                trapezoids[tl].u0 = tu;
                trapezoids[tl].u1 = 0;

                if (((tmp_d = trapezoids[tl].d0) > 0) && (trapezoids[tmp_d].u0 == tu))
                    trapezoids[tmp_d].u0 = tl;
                if (((tmp_d = trapezoids[tl].d0) > 0) && (trapezoids[tmp_d].u1 == tu))
                    trapezoids[tmp_d].u1 = tl;

                if (((tmp_d = trapezoids[tl].d1) > 0) && (trapezoids[tmp_d].u0 == tu))
                    trapezoids[tmp_d].u0 = tl;
                if (((tmp_d = trapezoids[tl].d1) > 0) && (trapezoids[tmp_d].u1 == tu))
                    trapezoids[tmp_d].u1 = tl;

                i1 = NewNode();
                i2 = NewNode();
                sk = trapezoids[tu].sink;

                query[sk].kind = NodeKind.Y_NODE;
                query[sk].pY = s.p1;
                query[sk].segnum = segnum; /* not really reqd ... maybe later */
                query[sk].left = i2;
                query[sk].right = i1;

                query[i1].kind = NodeKind.S_NODE;
                query[i1].trnum = tu;
                query[i1].parent = sk;

                query[i2].kind = NodeKind.S_NODE;
                query[i2].trnum = tl;
                query[i2].parent = sk;

                trapezoids[tu].sink = i1;
                trapezoids[tl].sink = i2;
                tlast = tu;
            }
            // p1 already present in existing segment
            // get bottommost intersecting trapezoid
            else
            {
                tlast = LocateEndpoint(s.p1, s.p0, s.root1);
                tribot = 1;
            }

            // While start
            t = tfirst;
            while (t > 0 && Alg.GreaterThanEqTo(trapezoids[t].lo, trapezoids[tlast].lo))
            {
                int t_sav, tn_sav;
                sk = trapezoids[t].sink;
                i1 = NewNode();
                i2 = NewNode();

                query[sk].kind = NodeKind.X_NODE;
                query[sk].segnum = segnum;
                query[sk].left = i1;
                query[sk].right = i2;

                query[i1].kind = NodeKind.S_NODE; /* left trapezoid (use existing one) */
                query[i1].trnum = t;
                query[i1].parent = sk;

                query[i2].kind = NodeKind.S_NODE; /* right trapezoid (allocate new) */
                query[i2].trnum = tn = NewTrapezoid();
                //tn = query[i2].trnum;  // TODO: delete?
                trapezoids[tn].state = true;
                query[i2].parent = sk;

                if (t == tfirst)
                    tfirstr = tn;
                if (Alg.EqualTo(trapezoids[t].lo, trapezoids[tlast].lo))
                    tlastr = tn;

                trapezoids[tn] = trapezoids[t];
                trapezoids[t].sink = i1;
                trapezoids[tn].sink = i2;
                t_sav = t;
                tn_sav = tn;

                // Impossible case
                if (trapezoids[t].d0 <= 0 && trapezoids[t].d1 <= 0)
                {
                    throw new InvalidDataException("Horror in AddSegment!");
                }
                // Only one trapezoid below. Partition t into 2 and take the two
                // resulting trapezoids t and tn as the upper neighbours of the
                // sole lower trapezoid
                else if ((trapezoids[t].d0 > 0) && (trapezoids[t].d1 <= 0))
                { /* Only one trapezoid below */
                    if ((trapezoids[t].u0 > 0) &&
                        (trapezoids[t].u1 > 0))
                    {  /* continuation of a chain from abv. */
                        if (trapezoids[t].usave > 0) /* three upper neighbours */
                        {
                            if (trapezoids[t].uside == 1)
                            {
                                trapezoids[tn].u0 = trapezoids[t].u1;
                                trapezoids[t].u1 = -1;
                                trapezoids[tn].u1 = trapezoids[t].usave;

                                trapezoids[trapezoids[t].u0].d0 = t;
                                trapezoids[trapezoids[tn].u0].d0 = tn;
                                trapezoids[trapezoids[tn].u1].d0 = tn;
                            }
                            else /* intersects in the right */
                            {
                                trapezoids[tn].u1 = -1;
                                trapezoids[tn].u0 = trapezoids[t].u1;
                                trapezoids[t].u1 = trapezoids[t].u0;
                                trapezoids[t].u0 = trapezoids[t].usave;

                                trapezoids[trapezoids[t].u0].d0 = t;
                                trapezoids[trapezoids[t].u1].d0 = t;
                                trapezoids[trapezoids[tn].u0].d0 = tn;
                            }

                            trapezoids[t].usave = trapezoids[tn].usave = 0;
                        }
                        else /* No usave.... simple case */
                        {
                            trapezoids[tn].u0 = trapezoids[t].u1;
                            trapezoids[t].u1 = trapezoids[tn].u1 = -1;
                            trapezoids[trapezoids[tn].u0].d0 = tn;
                        }
                    }
                    else
                    { /* fresh seg. or upward cusp */
                        int tmp_u = trapezoids[t].u0;
                        int td0, td1;
                        if (((td0 = trapezoids[tmp_u].d0) > 0) &&
                            ((td1 = trapezoids[tmp_u].d1) > 0))
                        { /* upward cusp */
                            if ((trapezoids[td0].rSeg > 0) && !Alg.IsLeftOf(trapezoids[td0].rSeg, s.p1))
                            {
                                trapezoids[t].u0 = trapezoids[t].u1 = trapezoids[tn].u1 = -1;
                                trapezoids[trapezoids[tn].u0].d1 = tn;
                            }
                            else /* cusp going leftwards */
                            {
                                trapezoids[tn].u0 = trapezoids[tn].u1 = trapezoids[t].u1 = -1;
                                trapezoids[trapezoids[t].u0].d0 = t;
                            }
                        }
                        else /* fresh segment */
                        {
                            trapezoids[trapezoids[t].u0].d0 = t;
                            trapezoids[trapezoids[t].u0].d1 = tn;
                        }
                    }

                    if ((trapezoids[t].lo.y == trapezoids[tlast].lo.y) &&
                        (trapezoids[t].lo.x == trapezoids[tlast].lo.x) &&
                        tribot == 1)
                    { /* bottom forms a triangle */

                        if (isSwapped)
                            tmpTriSeg = segs[segnum].prev;
                        else
                            tmpTriSeg = segs[segnum].next;

                        if ((tmpTriSeg > 0) && Alg.IsLeftOf(tmpTriSeg, s.p0))
                        {
                            /* L-R downward cusp */
                            trapezoids[trapezoids[t].d0].u0 = t;
                            trapezoids[tn].d0 = trapezoids[tn].d1 = -1;
                        }
                        else
                        {
                            /* R-L downward cusp */
                            trapezoids[trapezoids[tn].d0].u1 = tn;
                            trapezoids[t].d0 = trapezoids[t].d1 = -1;
                        }
                    }
                    else
                    {
                        if ((trapezoids[trapezoids[t].d0].u0 > 0) && (trapezoids[trapezoids[t].d0].u1 > 0))
                        {
                            if (trapezoids[trapezoids[t].d0].u0 == t) /* passes thru LHS */
                            {
                                trapezoids[trapezoids[t].d0].usave = trapezoids[trapezoids[t].d0].u1;
                                trapezoids[trapezoids[t].d0].uside = 1;
                            }
                            else
                            {
                                trapezoids[trapezoids[t].d0].usave = trapezoids[trapezoids[t].d0].u0;
                                trapezoids[trapezoids[t].d0].uside = 2;
                            }
                        }
                        trapezoids[trapezoids[t].d0].u0 = t;
                        trapezoids[trapezoids[t].d0].u1 = tn;
                    }

                    t = trapezoids[t].d0;
                }

                else if ((trapezoids[t].d0 <= 0) && (trapezoids[t].d1 > 0))
                { /* Only one trapezoid below */
                    if ((trapezoids[t].u0 > 0) &&
                        (trapezoids[t].u1 > 0))
                    {  /* continuation of a chain from abv. */
                        if (trapezoids[t].usave > 0) /* three upper neighbours */
                        {
                            if (trapezoids[t].uside == 1)
                            {
                                trapezoids[tn].u0 = trapezoids[t].u1;
                                trapezoids[t].u1 = -1;
                                trapezoids[tn].u1 = trapezoids[t].usave;

                                trapezoids[trapezoids[t].u0].d0 = t;
                                trapezoids[trapezoids[tn].u0].d0 = tn;
                                trapezoids[trapezoids[tn].u1].d0 = tn;
                            }
                            else /* intersects in the right */
                            {
                                trapezoids[tn].u1 = -1;
                                trapezoids[tn].u0 = trapezoids[t].u1;
                                trapezoids[t].u1 = trapezoids[t].u0;
                                trapezoids[t].u0 = trapezoids[t].usave;

                                trapezoids[trapezoids[t].u0].d0 = t;
                                trapezoids[trapezoids[t].u1].d0 = t;
                                trapezoids[trapezoids[tn].u0].d0 = tn;
                            }

                            trapezoids[t].usave = trapezoids[tn].usave = 0;
                        }
                        else /* No usave.... simple case */
                        {
                            trapezoids[tn].u0 = trapezoids[t].u1;
                            trapezoids[t].u1 = trapezoids[tn].u1 = -1;
                            trapezoids[trapezoids[tn].u0].d0 = tn;
                        }
                    }
                    else
                    { /* fresh seg. or upward cusp */
                        int tmp_u = trapezoids[t].u0;
                        int td0, td1;
                        if (((td0 = trapezoids[tmp_u].d0) > 0) &&
                            ((td1 = trapezoids[tmp_u].d1) > 0))
                        { /* upward cusp */
                            if ((trapezoids[td0].rSeg > 0) && !Alg.IsLeftOf(trapezoids[td0].rSeg, s.p1))
                            {
                                trapezoids[t].u0 = trapezoids[t].u1 = trapezoids[tn].u1 = -1;
                                trapezoids[trapezoids[tn].u0].d1 = tn;
                            }
                            else
                            {
                                trapezoids[tn].u0 = trapezoids[tn].u1 = trapezoids[t].u1 = -1;
                                trapezoids[trapezoids[t].u0].d0 = t;
                            }
                        }
                        else /* fresh segment */
                        {
                            trapezoids[trapezoids[t].u0].d0 = t;
                            trapezoids[trapezoids[t].u0].d1 = tn;
                        }
                    }

                    if (trapezoids[t].lo.y == trapezoids[tlast].lo.y &&
                        trapezoids[t].lo.x == trapezoids[tlast].lo.x &&
                        tribot == 1)
                    { /* bottom forms a triangle */
                        int tmpseg;

                        // TODO: try both of these, since the source code is wrong
                        //tmpseg = trapezoids[trapezoids[t].d0].rSeg;
                        tmpseg = 0;

                        if (isSwapped)
                            tmpTriSeg = segs[segnum].prev;
                        else
                            tmpTriSeg = segs[segnum].next;

                        if ((tmpseg > 0) && Alg.IsLeftOf(tmpseg, s.p0))
                        {
                            /* L-R downward cusp */
                            trapezoids[trapezoids[t].d1].u0 = t;
                            trapezoids[tn].d0 = trapezoids[tn].d1 = -1;
                        }
                        else
                        {
                            /* R-L downward cusp */
                            trapezoids[trapezoids[tn].d1].u1 = tn;
                            trapezoids[t].d0 = trapezoids[t].d1 = -1;
                        }
                    }
                    else
                    {
                        if ((trapezoids[trapezoids[t].d1].u0 > 0) && (trapezoids[trapezoids[t].d1].u1 > 0))
                        {
                            if (trapezoids[trapezoids[t].d1].u0 == t) /* passes thru LHS */
                            {
                                trapezoids[trapezoids[t].d1].usave = trapezoids[trapezoids[t].d1].u1;
                                trapezoids[trapezoids[t].d1].uside = 1;
                            }
                            else
                            {
                                trapezoids[trapezoids[t].d1].usave = trapezoids[trapezoids[t].d1].u0;
                                trapezoids[trapezoids[t].d1].uside = 2;
                            }
                        }
                        trapezoids[trapezoids[t].d1].u0 = t;
                        trapezoids[trapezoids[t].d1].u1 = tn;
                    }

                    t = trapezoids[t].d1;
                }
                // Two trapezoids below
                else
                {
                    int tmpseg = trapezoids[trapezoids[t].d0].rSeg;
                    double y0, yt;
                    Point_t tmppt = new Point_t();
                    //int tnext, i_d0, i_d1;
                    int i_d0, i_d1;

                    i_d0 = i_d1 = 0;
                    if (trapezoids[t].lo.y == s.p0.y)
                    {
                        if (trapezoids[t].lo.x > s.p0.x)
                            i_d0 = 1;
                        else
                            i_d1 = 1;
                    }
                    else
                    {
                        tmppt.y = y0 = trapezoids[t].lo.y;
                        yt = (y0 - s.p0.y) / (s.p1.y - s.p0.y);
                        tmppt.x = s.p0.x + yt * (s.p1.x - s.p0.x);

                        if (!Alg.GreaterThanEqTo(tmppt, trapezoids[t].lo))
                            i_d0 = 1;
                        else
                            i_d1 = 1;
                    }

                    /* check continuity from the top so that the lower-neighbour */
                    /* values are properly filled for the upper trapezoid */

                    if ((trapezoids[t].u0 > 0) &&
                        (trapezoids[t].u1 > 0))
                    {  /* continuation of a chain from abv. */
                        if (trapezoids[t].usave > 0) /* three upper neighbours */
                        {
                            if (trapezoids[t].uside == 1)
                            {
                                trapezoids[tn].u0 = trapezoids[t].u1;
                                trapezoids[t].u1 = -1;
                                trapezoids[tn].u1 = trapezoids[t].usave;

                                trapezoids[trapezoids[t].u0].d0 = t;
                                trapezoids[trapezoids[tn].u0].d0 = tn;
                                trapezoids[trapezoids[tn].u1].d0 = tn;
                            }
                            else /* intersects in the right */
                            {
                                trapezoids[tn].u1 = -1;
                                trapezoids[tn].u0 = trapezoids[t].u1;
                                trapezoids[t].u1 = trapezoids[t].u0;
                                trapezoids[t].u0 = trapezoids[t].usave;

                                trapezoids[trapezoids[t].u0].d0 = t;
                                trapezoids[trapezoids[t].u1].d0 = t;
                                trapezoids[trapezoids[tn].u0].d0 = tn;
                            }

                            trapezoids[t].usave = trapezoids[tn].usave = 0;
                        }
                        else /* No usave.... simple case */
                        {
                            trapezoids[tn].u0 = trapezoids[t].u1;
                            trapezoids[tn].u1 = -1;
                            trapezoids[t].u1 = -1;
                            trapezoids[trapezoids[tn].u0].d0 = tn;
                        }
                    }
                    else
                    { /* fresh seg. or upward cusp */
                        int tmp_u = trapezoids[t].u0;
                        int td0, td1;
                        if (((td0 = trapezoids[tmp_u].d0) > 0) &&
                            ((td1 = trapezoids[tmp_u].d1) > 0))
                        { /* upward cusp */
                            if ((trapezoids[td0].rSeg > 0) && !Alg.IsLeftOf(trapezoids[td0].rSeg, s.p1))
                            {
                                trapezoids[t].u0 = trapezoids[t].u1 = trapezoids[tn].u1 = -1;
                                trapezoids[trapezoids[tn].u0].d1 = tn;
                            }
                            else
                            {
                                trapezoids[tn].u0 = trapezoids[tn].u1 = trapezoids[t].u1 = -1;
                                trapezoids[trapezoids[t].u0].d0 = t;
                            }
                        }
                        else /* fresh segment */
                        {
                            trapezoids[trapezoids[t].u0].d0 = t;
                            trapezoids[trapezoids[t].u0].d1 = tn;
                        }
                    }

                    if (trapezoids[t].lo.y == trapezoids[tlast].lo.y &&
                        trapezoids[t].lo.x == trapezoids[tlast].lo.x && tribot == 1)
                    {
                        /* this case arises only at the lowest trapezoid.. i.e.
                            tlast, if the lower endpoint of the segment is
                            already inserted in the structure */

                        trapezoids[trapezoids[t].d0].u0 = t;
                        trapezoids[trapezoids[t].d0].u1 = -1;
                        trapezoids[trapezoids[t].d1].u0 = tn;
                        trapezoids[trapezoids[t].d1].u1 = -1;

                        trapezoids[tn].d0 = trapezoids[t].d1;
                        trapezoids[t].d1 = trapezoids[tn].d1 = -1;

                        tnext = trapezoids[t].d1;
                    }
                    else if (i_d0 == 1)
                    /* intersecting d0 */
                    {
                        trapezoids[trapezoids[t].d0].u0 = t;
                        trapezoids[trapezoids[t].d0].u1 = tn;
                        trapezoids[trapezoids[t].d1].u0 = tn;
                        trapezoids[trapezoids[t].d1].u1 = -1;

                        /* new code to determine the bottom neighbours of the */
                        /* newly partitioned trapezoid */

                        trapezoids[t].d1 = -1;

                        tnext = trapezoids[t].d0;
                    }
                    else /* intersecting d1 */
                    {
                        trapezoids[trapezoids[t].d0].u0 = t;
                        trapezoids[trapezoids[t].d0].u1 = -1;
                        trapezoids[trapezoids[t].d1].u0 = t;
                        trapezoids[trapezoids[t].d1].u1 = tn;

                        /* new code to determine the bottom neighbours of the */
                        /* newly partitioned trapezoid */

                        trapezoids[tn].d0 = trapezoids[t].d1;
                        trapezoids[tn].d1 = -1;

                        tnext = trapezoids[t].d1;
                    }

                    t = tnext;
                }
                trapezoids[t_sav].rSeg = trapezoids[tn_sav].lSeg = segnum;
            }
            // while end
            tfirstl = tfirst;
            tlastl = tlast;
            MergeTrapezoids(segnum, tfirstl, tlastl, 1);
            MergeTrapezoids(segnum, tfirstr, tlastr, 2);
            segs[segnum].isInserted = true;
        }

        public static void MergeTrapezoids(int segnum, int tfirst, int tlast, int side)
        {
            int t, tnext;
            bool cond;
            int ptnext;

            t = tfirst;
            while (t > 0 && Alg.GreaterThanEqTo(trapezoids[t].lo, trapezoids[tlast].lo))
            {
                if (side == 1)
                    cond = ((((tnext = trapezoids[t].d0) > 0) && (trapezoids[tnext].rSeg == segnum)) ||
                            (((tnext = trapezoids[t].d1) > 0) && (trapezoids[tnext].rSeg == segnum)));
                else
                    cond = ((((tnext = trapezoids[t].d0) > 0) && (trapezoids[tnext].lSeg == segnum)) ||
                            (((tnext = trapezoids[t].d1) > 0) && (trapezoids[tnext].lSeg == segnum)));

                if (cond)
                {
                    if (trapezoids[t].lSeg == trapezoids[tnext].lSeg &&
                        trapezoids[t].rSeg == trapezoids[tnext].rSeg)
                    {
                        ptnext = query[trapezoids[tnext].sink].parent;
                        if (query[ptnext].left == trapezoids[tnext].sink)
                            query[ptnext].left = trapezoids[t].sink;
                        else
                            query[ptnext].right = trapezoids[t].sink;


                        if ((trapezoids[t].d0 = trapezoids[tnext].d0) > 0)
                            if (trapezoids[trapezoids[t].d0].u0 == tnext)
                                trapezoids[trapezoids[t].d0].u0 = t;
                            else if (trapezoids[trapezoids[t].d0].u1 == tnext)
                                trapezoids[trapezoids[t].d0].u1 = t;

                        if ((trapezoids[t].d1 = trapezoids[tnext].d1) > 0)
                            if (trapezoids[trapezoids[t].d1].u0 == tnext)
                                trapezoids[trapezoids[t].d1].u0 = t;
                            else if (trapezoids[trapezoids[t].d1].u1 == tnext)
                                trapezoids[trapezoids[t].d1].u1 = t;

                        trapezoids[t].lo = trapezoids[tnext].lo;
                        trapezoids[tnext].state = false;
                    }
                    else
                    {
                        t = tnext;
                    }
                }
                else
                {
                    t = tnext;
                }
            }
            // while end
        }

        static bool IsInserted(int segnum, int whichP)
        {
            if (whichP == 1)
            {
                return segs[segs[segnum].prev].isInserted;
            }
            else
            {
                return segs[segs[segnum].next].isInserted;
            }
        }

        static int LocateEndpoint(Point_t v, Point_t vo, int r)
        {
            //Console.WriteLine("  LocateEndpoint!");
            Node rNode = query[r];

            switch (rNode.kind)
            {
                case NodeKind.S_NODE:
                    return rNode.trnum;
                case NodeKind.Y_NODE:
                    if (Alg.GreaterThan(v, rNode.pY))  // Above
                    {
                        return LocateEndpoint(v, vo, rNode.right);
                    }
                    else if (Alg.EqualTo(v, rNode.pY))  // Already inserted
                    {
                        if (Alg.GreaterThan(vo, rNode.pY))
                        {
                            return LocateEndpoint(v, vo, rNode.right);  // Above
                        }
                        else
                        {
                            return LocateEndpoint(v, vo, rNode.left);  // Below
                        }
                    }
                    else
                    {
                        return LocateEndpoint(v, vo, rNode.left);  // Below
                    }

                case NodeKind.X_NODE:
                    if (Alg.EqualTo(v, segs[rNode.segnum].p0) ||
                        Alg.EqualTo(v, segs[rNode.segnum].p1))
                    {
                        if (v.y == vo.y)  // Horizontal segment
                        {
                            if (vo.x < v.x)
                            {
                                return LocateEndpoint(v, vo, rNode.left);  // Left
                            }
                            else
                            {
                                return LocateEndpoint(v, vo, rNode.right);  // Right
                            }
                        }
                        else if (Alg.IsLeftOf(rNode.segnum, vo))
                        {
                            return LocateEndpoint(v, vo, rNode.left);
                        }
                        else
                        {
                            return LocateEndpoint(v, vo, rNode.right);
                        }
                    }
                    else if (Alg.IsLeftOf(rNode.segnum, v))
                    {
                        return LocateEndpoint(v, vo, rNode.left);
                    }
                    else
                    {
                        return LocateEndpoint(v, vo, rNode.right);
                    }

                default:
                    throw new InvalidDataException("Horror in LocateEndpoint!");
            }
        }
        static void FindNewRoots(int segnum)
        {
            Segment s = segs[segnum];
            if (s.isInserted)
            {
                return;
            }
            s.root0 = LocateEndpoint(s.p0, s.p1, s.root0);
            s.root0 = trapezoids[s.root0].sink;

            s.root1 = LocateEndpoint(s.p1, s.p0, s.root1);
            s.root1 = trapezoids[s.root1].sink;
        }
    }

    public enum NodeKind
    {
        X_NODE,
        Y_NODE,
        S_NODE
    }

    public struct Node
    {
        public NodeKind kind;
        public int segnum;
        public int trnum;
        public Point_t pY;
        public int parent;
        public int left, right;
    }

    // Trapezoids will be arranged in a tree structure
    public struct Trapezoid
    {
        public int rSeg, lSeg;
        public Point_t hi, lo;
        public int u0, u1;  // Up to two adjacent trapezoids above
        public int d0, d1;  // Up to two adjacent trapezoids below
        public int sink;
        public int usave, uside;  // Unknown
        public bool state;  // Inside or outside?

        public Trapezoid()
        {
            rSeg = 0;
            lSeg = 0;
            hi = new Point_t();
            lo = new Point_t();
            u0 = 0;
            u1 = 0;
            d0 = 0;
            d1 = 0;
            sink = 0;
            usave = 0;
            uside = 0;
            state = false;
        }
    }

    public struct Segment
    {
        public Point_t p0, p1;  // Start and end points
        public bool isInserted;
        public int root0, root1;
        public int next;
        public int prev;
    }

    public struct Point_t
    {
        public double x, y;
        public Point_t()
        {
            x = 0;
            y = 0;
        }
    }

    public struct Vertexchain
    {
        public Point_t pt;
        public unsafe fixed int vnext[4];
        public unsafe fixed int vpos[4];
        public int nextfree;
        public Vertexchain()
        {
            pt = new Point_t();
            unsafe
            {
                vnext[0] = 0;
                vnext[1] = 0;
            }
            nextfree = 0;
        }
    }

    public struct Monchain
    {
        public int vnum;
        public int next;
        public int prev;
        public bool marked;
        public Monchain()
        {
            vnum = 0;
            next = 0;
            prev = 0;
            marked = false;
        }
    }
}
