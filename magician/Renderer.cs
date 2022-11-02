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

        public static bool GreaterThan(double[] p0, double[] p1)
        {
            double x0 = p0[0];
            double y0 = p0[1];
            double x1 = p1[0];
            double y1 = p1[1];
            return y0 > y1 ? true : (y0 < y1 ? false : (x0 > x1 ? true : false));
        }

        public static bool GreaterThanEqTo(double[] p0, double[] p1)
        {
            double x0 = p0[0];
            double y0 = p0[1];
            double x1 = p1[0];
            double y1 = p1[1];
            return y0 > y1 ? true : (y0 < y1 ? false : (x0 > x1 ? true : true));
        }

        public static bool EqualTo(double[] p0, double[] p1)
        {
            return (p0[0] == p1[0] && p0[1] == p1[1]);
        }

        // Is vertex v left of Segment segs[segnum]? Handles degenerate cases when both of
        // the vertices have the same y-coord., etc.
        public static bool IsLeftOf(int segnum, double[] v)
        {
            Segment s = Geo.segs[segnum];
            double area;
            if (GreaterThan(s.p1, s.p0))  // Segment going upwards
            {
                if (s.p1[1] == v[1])
                {
                    if (v[0] < s.p1[0])
                    {
                        area = 1;
                    }
                    else
                    {
                        area = -1;
                    }
                }
                else if (s.p0[1] == v[1])
                {
                    if (v[0] < s.p0[0])
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
                if (s.p1[1] == v[1])
                {
                    if (v[0] < s.p1[0])
                    {
                        area = 1;
                    }
                    else
                    {
                        area = -1;
                    }
                }
                else if (s.p0[1] == v[1])
                {
                    if (v[0] < s.p0[0])
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

        public static double Cross(double[] v0, double[] v1, double[] v2)
        {
            return (v1[0] - v0[0]) * (v2[1] - v0[1]) - (v1[1] - v0[1]) * (v2[0] - v0[0]);
        }
    }

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

        // Initialize arrays and read segments from Multi
        public static void Load(Magician.Multi m)
        {
            MAX_SEGMENTS = m.Count;
            MAX_NODES = 8 * MAX_SEGMENTS;
            MAX_TRAPEZOIDS = 4 * MAX_SEGMENTS;

            segs = new Segment[MAX_SEGMENTS];
            // nodes set in InitQueryStructure
            // trapezoids set in InitQueryStructure

            List<Magician.Multi> edges = m.Edges();
            int i = 1;  // The original indexed its segments at 1 for some reason
            int last = MAX_SEGMENTS;
            foreach (Magician.Multi seg in edges)
            {
                // Sort the edge top-to-bottom, with ties broken left-to-right (lexicographic sort)
                // TODO: test if this sort is even necessary
                seg.Constituents.Sort(new Comparison<Magician.Multi>((m1, m2) =>
                {
                    return m1.Y.Evaluate() > m2.Y.Evaluate() ? 1 : (m1.Y.Evaluate() < m2.Y.Evaluate() ? -1 : (m1.X.Evaluate() < m2.X.Evaluate() ? 1 : -1));
                }));

                segs[i].next = i + 1;
                segs[i].prev = last;
                segs[last].p1 = segs[i].p0;
                segs[i].isInserted = false;

                i++;
            }
            Initialize();
            ConstructTrapezoids();
            //MonotonateTrapezoids();
            //TriangulateMonotonePolygons();

        }

        public static void Initialize()
        {
            for (int i = 0; i < segs.Length; i++)
            {
                segs[i].isInserted = false;
            }
            Random r = new Random();
            // TODO: ensure this shuffle works
            segs.ToList<Segment>().Sort((s0, s1) => r.Next(1) == 1 ? 1 : -1);
        }

        // Increments q_idx and returns for use as an indexer
        public static int NewNode()
        {
            return q_idx++;
        }

        // Increments and returns tr_idx
        public static int NewTrapezoid()
        {
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

            // The initial trapezoids:
            /*
            * 0: middle left
            * 1: middle right
            * 2: bottommost
            * 3: topmost
            */

            // Define the initial 7 nodes, with the first being the root
            i1 = NewNode();
            query[i1].kind = NodeKind.Y_NODE;
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
            t1 = NewTrapezoid();
            t2 = NewTrapezoid();
            t3 = NewTrapezoid();
            t4 = NewTrapezoid();

            trapezoids[t1].hi = trapezoids[t2].hi = trapezoids[t4].lo = query[i1].pY;
            trapezoids[t1].lo = trapezoids[t2].lo = trapezoids[t3].hi = query[i3].pY;
            trapezoids[t4].hi[1] = Double.PositiveInfinity;
            trapezoids[t4].hi[0] = Double.PositiveInfinity;
            trapezoids[t3].lo[1] = Double.NegativeInfinity;
            trapezoids[t3].lo[0] = Double.NegativeInfinity;
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

        public static void ConstructTrapezoids()
        {

            /*
            * Initialize Trapezoidation tree / query structure
            */
            int root = Renderer.Geo.InitialQueryStructure(ChooseSegment());

            /*
            * Set the roots of all segments to the initial root
            */
            for (int i = 0; i < segs.Length; i++)
            {
                segs[i].root0 = segs[1].root1 = root;
            }

            /*
            * Add the remaining segments into the trapezoidation
            */
            int logstarN = Renderer.Alg.Logstar(segs.Length);
            for (int h = 1; h < logstarN; h++)
            {
                // Idk
                for (int i = Renderer.Alg.N(segs.Length, h - 1) + 1; i <= Renderer.Alg.N(segs.Length, h); i++)
                {
                    AddSegment(ChooseSegment());
                }

                // Find a new root for the segment endpoints
                for (int i = 1; i <= segs.Length; i++)
                {
                    FindNewRoots(i);
                }
            }

            for (int i = Alg.N(segs.Length, Alg.Logstar(segs.Length)) + 1; i <= segs.Length; i++)
            {
                AddSegment(ChooseSegment());
            }


            /*
            * End of trapezoidation code
            */
        }

        static void AddSegment(int segnum)
        {
            Segment s = segs[segnum];
            int tu, tl, sk, tfirst, tlast, tnext;
            int tfirstr=0, tlastr=0;
            int tfirstl, tlastl;
            int i1, i2, t, t1, t2, tn;
            double[] tpt;  // Top point?
            int tritop = 0, tribot = 0;
            bool isSwapped = false;
            int tmpTriSeg;

            if (Alg.GreaterThan(s.p0, s.p1))
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
                trapezoids[tu].lo[1] = trapezoids[tl].hi[1] = s.p0[1];
                trapezoids[tu].lo[0] = trapezoids[tl].hi[0] = s.p0[0];
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

                // Update the query and obtain the sinks for the trapezoids
                i1 = NewNode();
                i2 = NewNode();
                sk = trapezoids[tu].sink;
                query[sk].kind = NodeKind.Y_NODE;
                query[sk].pY = s.p0;
                query[sk].segnum = segnum;
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
                trapezoids[tu].lo[1] = trapezoids[tl].hi[1] = s.p1[1];
                trapezoids[tu].lo[0] = trapezoids[tl].hi[0] = s.p1[0];
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

            t = tfirst;
            while (t > 0 && Alg.GreaterThanEqTo(trapezoids[t].lo, trapezoids[tlast].lo))
            {
                int t_sav, tn_sav;
                sk = Geo.trapezoids[t].sink;
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
                tn = query[i2].trnum;
                Geo.trapezoids[tn].state = true;
                query[i2].parent = sk;

                if (t == tfirst)
                    tfirstr = tn;
                if (Geo.trapezoids[t].lo == Geo.trapezoids[tlast].lo)
                    tlastr = tn;
                Geo.trapezoids[tn] = Geo.trapezoids[t];
                Geo.trapezoids[t].sink = i1;
                Geo.trapezoids[tn].sink = i2;
                t_sav = t;
                tn_sav = tn;

                // Impossible case
                if (Geo.trapezoids[t].d0 <= 0 && Geo.trapezoids[t].d1 <= 0)
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

                    if ((trapezoids[t].lo[1] == trapezoids[tlast].lo[1]) &&
                        (trapezoids[t].lo[0] == trapezoids[tlast].lo[0]) &&
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

                    if (trapezoids[t].lo[1] == trapezoids[tlast].lo[1] &&
                        trapezoids[t].lo[0] == trapezoids[tlast].lo[0] &&
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
                    double[] tmppt = { 0, 0 };
                    //int tnext, i_d0, i_d1;
                    int i_d0, i_d1;

                    i_d0 = i_d1 = 0;
                    if (trapezoids[t].lo[1] == s.p0[1])
                    {
                        if (trapezoids[t].lo[0] > s.p0[0])
                            i_d0 = 1;
                        else
                            i_d1 = 1;
                    }
                    else
                    {
                        tmppt[1] = y0 = trapezoids[t].lo[1];
                        yt = (y0 - s.p0[1]) / (s.p1[1] - s.p0[1]);
                        tmppt[0] = s.p0[0] + yt * (s.p1[0] - s.p0[0]);

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

                    if (trapezoids[t].lo[1] == trapezoids[tlast].lo[1] &&
                        trapezoids[t].lo[0] == trapezoids[tlast].lo[0] && tribot == 1)
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

        static int LocateEndpoint(double[] v, double[] vo, int r)
        {
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
                    if (Alg.EqualTo(v, segs[rNode.segnum].p0) || Alg.EqualTo(v, segs[rNode.segnum].p1))
                    {
                        if (v[1] == vo[1])  // Horizontal segment
                        {
                            if (vo[0] < v[0])
                            {
                                return LocateEndpoint(v, vo, rNode.left);
                            }
                            else
                            {
                                return LocateEndpoint(v, vo, rNode.right);
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
        static int FindNewRoots(int segnum)
        {
            Segment s = segs[segnum];
            if (s.isInserted)
            {
                return 0;
            }
            s.root0 = LocateEndpoint(s.p0, s.p1, s.root0);
            s.root0 = trapezoids[s.root0].sink;

            s.root1 = LocateEndpoint(s.p1, s.p0, s.root1);
            s.root1 = trapezoids[s.root1].sink;
            
            return 0;
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
        public double[] pY;
        public int parent;
        public int left, right;
    }

    // Trapezoids will be arranged in a tree structure
    public struct Trapezoid
    {
        public int rSeg, lSeg;
        public double[] hi, lo;
        public int u0, u1;  // Up to two adjacent trapezoids above
        public int d0, d1;  // Up to two adjacent trapezoids below
        public int sink;
        public int usave, uside;  // Unknown
        public bool state;  // Inside or outside?
    }

    public struct Segment
    {
        public double[] p0, p1;  // Start and end points
        public bool isInserted;
        public int root0, root1;
        public int next;
        public int prev;
    }
}
