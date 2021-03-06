﻿// EXTEND LOOPS TO OTHER DIMENSIONS OF SIMPLEX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace LibMax.Noise
{
    public class SimplexNoise // Simplex Noise in 2D, 3D and 4D
    {
        #region Tables
        private static int[][] grad3 = {new int[] {1,1,0},new int[] {-1,1,0},new int[] {1,-1,0},new int[] {-1,-1,0},
new int[] {1,0,1},new int[] {-1,0,1},new int[] {1,0,-1},new int[] {-1,0,-1},
new int[] {0,1,1},new int[] {0,-1,1},new int[] {0,1,-1},new int[] {0,-1,-1}};
        private static int[][] grad4 = {new int[] {0,1,1,1}, new int[] {0,1,1,-1}, new int[] {0,1,-1,1}, new int[] {0,1,-1,-1},
new int[] {0,-1,1,1}, new int[] {0,-1,1,-1}, new int[] {0,-1,-1,1}, new int[] {0,-1,-1,-1},
new int[] {1,0,1,1}, new int[] {1,0,1,-1}, new int[] {1,0,-1,1}, new int[] {1,0,-1,-1},
new int[] {-1,0,1,1}, new int[] {-1,0,1,-1}, new int[] {-1,0,-1,1}, new int[] {-1,0,-1,-1},
new int[] {1,1,0,1}, new int[] {1,1,0,-1}, new int[] {1,-1,0,1}, new int[] {1,-1,0,-1},
new int[] {-1,1,0,1}, new int[] {-1,1,0,-1}, new int[] {-1,-1,0,1}, new int[] {-1,-1,0,-1},
new int[] {1,1,1,0}, new int[] {1,1,-1,0}, new int[] {1,-1,1,0}, new int[] {1,-1,-1,0},
new int[] {-1,1,1,0}, new int[] {-1,1,-1,0}, new int[] {-1,-1,1,0}, new int[] {-1,-1,-1,0}};
        // A lookup table to traverse the simplex around a given point in 4D.
        // Details can be found where this table is used, in the 4D Noise method.
        private static int[,] simplex = {
{0,1,2,3},{0,1,3,2},{0,0,0,0},{0,2,3,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,2,3,0},
{0,2,1,3},{0,0,0,0},{0,3,1,2},{0,3,2,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,3,2,0},
{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
{1,2,0,3},{0,0,0,0},{1,3,0,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,3,0,1},{2,3,1,0},
{1,0,2,3},{1,0,3,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,0,3,1},{0,0,0,0},{2,1,3,0},
{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
{2,0,1,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,0,1,2},{3,0,2,1},{0,0,0,0},{3,1,2,0},
{2,1,0,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,1,0,2},{0,0,0,0},{3,2,0,1},{3,2,1,0}};
#endregion

        private int Seed, Octaves;
        private double Amplitude, Persistence, Frequency;
        private Random r;

        // To remove the need for index wrapping, double the permutation table length
        private static int[] perm = new int[512];
        //static SimplexNoise() { for (int i = 0; i < 512; i++) perm[i] = p[i & 255]; }
        public SimplexNoise()
        {
            Seed = (int)DateTime.Now.Ticks;
            r = new Random(Seed);            
            SeedPerm();
            this.Octaves = 8;
            this.Amplitude = 1.0;
            this.Persistence = 0.5;
            this.Frequency = 0.0015;
        }
        public SimplexNoise(int seed, int octaves = 8, double amplitude = 1.0, double persistence = 0.5, double frequency = 0.0015)
        {
            this.Seed = seed;
            r = new Random(Seed);
            SeedPerm();
            this.Octaves = octaves;
            this.Amplitude = amplitude;
            this.Persistence = persistence;
            this.Frequency = frequency;
        }

        public double this[double x, double y]
        {
            get { return Amplitude * Total(x,y); }
            private set { }
        }

        public double this[double x, double y, double z]
        {
            get { return Amplitude * Total(x, y, z); }
            private set { }
        }

        public double this[double x, double y, double z, double w]
        {
            get { return Amplitude * Total(x, y, z, w); }
            private set { }
        }

        private void SeedPerm()
        {
            bool[] slotFilled = new bool[256];
            int availToSet, availCounter, slotsToFill = 256;
            while (slotsToFill > 0)
            {
                availToSet = r.Next(slotsToFill--);
                availCounter = -1;
                for (int i = 0; i < slotFilled.Length; i++)
                {
                    if (slotFilled[i] || ++availCounter != availToSet) continue;
                    perm[i] = slotsToFill;
                    slotFilled[i] = true;
                }
            }
            for (int i = 0; i < 512; i++) perm[i] = perm[i & 255];
        }

        #region Utilities

        // This method is a *lot* faster than using (int)Math.floor(x)
        private static int FastFloor(double x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }
        private static double Dot(int[] g, double x, double y)
        {
            return g[0] * x + g[1] * y;
        }
        private static double Dot(int[] g, double x, double y, double z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }
        private static double Dot(int[] g, double x, double y, double z, double w)
        {
            return g[0] * x + g[1] * y + g[2] * z + g[3] * w;
        }

        #endregion

        #region Octave Summators

        private double Total(double x, double y)
        {
            double total = 0.0;
            double freq = Frequency, amp = Amplitude;
            for (int i = 0; i < Octaves; ++i)
            {
                total += Noise(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= Persistence;
            }
            return total;
        }

        private double Total(double x, double y, double z)
        {
            double total = 0.0;
            double freq = Frequency, amp = Amplitude;
            for (int i = 0; i < Octaves; ++i)
            {
                total = total + Noise(x * freq, y * freq, z * freq) * amp;
                freq *= 2;
                amp *= Persistence;
            }
            return (total < -1) ? -1 : (total > 1) ? 1 : total;
        }

        private double Total(double x, double y, double z, double w)
        {
            double total = 0.0;
            double freq = Frequency, amp = Amplitude;
            for (int i = 0; i < Octaves; ++i)
            {
                total = total + Noise(x * freq, y * freq, z * freq, w * freq) * amp;
                freq *= 2;
                amp *= Persistence;
            }
            return (total < -1) ? -1 : (total > 1) ? 1 : total;
        }

        #endregion

        #region Noise Functions

        // 2D simplex Noise
        private double Noise(double x, double y)
        {
            double[] n = new double[3]; // Noise contributions from the three corners
            // Skew the input space to determine which simplex cell we're in
            const double F2 = 0.36602540378443864676372317075294; // 0.5*(Math.Sqrt(3.0)-1.0)
            double s = (x + y) * F2; // Hairy factor for 2D
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            const double G2 = 0.21132486540518711774542560974902; // (3.0-Math.Sqrt(3.0))/6.0;
            double k = (i + j) * G2;
            double X0 = i - k; // Unskew the cell origin back to (x,y) space
            double Y0 = j - k;
            double[] xx = new double[3], yy = new double[3];
            xx[0] = x - X0; // The x,y distances from the cell origin
            yy[0] = y - Y0;
            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (xx[0] > yy[0]) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; } // upper triangle, YX order: (0,0)->(0,1)->(1,1)
            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6
            xx[1] = xx[0] - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            yy[1] = yy[0] - j1 + G2;
            xx[2] = xx[0] - 1.0 + 2.0 * G2; // Offsets for last corner in (x,y) unskewed coords
            yy[2] = yy[0]- 1.0 + 2.0 * G2;
            // Work out the hashed gradient indices of the three simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int[] gi = { perm[ii + perm[jj]] % 12, perm[ii + i1 + perm[jj + j1]] % 12, perm[ii + 1 + perm[jj + 1]] % 12 };
            // Calculate the contribution from the three corners
            double[] t = new double[3];
            for(int q=0;q<t.Length;q++) {
                t[q] = 0.5 - xx[q] * xx[q] - yy[q] * yy[q];
                if(t[q] < 0) n[q] = 0.0;
                else {
                    t[q] *= t[q];
                    n[q] = t[q] * t[q] * Dot(grad3[gi[q]],xx[q],yy[q]);
                }
            }
            // Add contributions from each corner to get the final Noise value.
            // The result is scaled to return values in the interval [-1,1].
            //return 70.0 * (n[0] + n[1] + n[2]);
            return 60.0 * (n[0] + n[1] + n[2]);
        }
        // 3D simplex Noise
        public double Noise(double x, double y, double z)
        {
            double n0, n1, n2, n3; // Noise contributions from the four corners
            // Skew the input space to determine which simplex cell we're in
            const double F3 = 0.33333333333333333333333333333333; // 1.0/3.0
            double s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            int k = FastFloor(z + s);
            const double G3 = 0.16666666666666666666666666666667; // 1.0/6.0; // Very nice and simple unskew factor, too
            double t = (i + j + k) * G3;
            double X0 = i - t; // Unskew the cell origin back to (x,y,z) space
            double Y0 = j - t;
            double Z0 = k - t;
            double x0 = x - X0; // The x,y,z distances from the cell origin
            double y0 = y - Y0;
            double z0 = z - Z0;
            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
            }
            else
            { // x0<y0
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
            }
            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.
            double x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
            double y1 = y0 - j1 + G3;
            double z1 = z0 - k1 + G3;
            double x2 = x0 - i2 + 2.0 * G3; // Offsets for third corner in (x,y,z) coords
            double y2 = y0 - j2 + 2.0 * G3;
            double z2 = z0 - k2 + 2.0 * G3;
            double x3 = x0 - 1.0 + 3.0 * G3; // Offsets for last corner in (x,y,z) coords
            double y3 = y0 - 1.0 + 3.0 * G3;
            double z3 = z0 - 1.0 + 3.0 * G3;
            // Work out the hashed gradient indices of the four simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
            int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]] % 12;
            int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]] % 12;
            int gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;
            // Calculate the contribution from the four corners
            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(grad3[gi0], x0, y0, z0);
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(grad3[gi1], x1, y1, z1);
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(grad3[gi2], x2, y2, z2);
            }
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Dot(grad3[gi3], x3, y3, z3);
            }
            // Add contributions from each corner to get the final Noise value.
            // The result is scaled to stay just inside [-1,1]
            return 32.0 * (n0 + n1 + n2 + n3);
        }
        // 4D simplex Noise
        double Noise(double x, double y, double z, double w)
        {
            // The skewing and unskewing factors are hairy again for the 4D case
            const double F4 = 0.30901699437494742410229341718282; // (Math.Sqrt(5.0)-1.0)/4.0;
            const double G4 = 0.13819660112501051517954131656344; // (5.0-Math.Sqrt(5.0))/20.0;
            double n0, n1, n2, n3, n4; // Noise contributions from the five corners
            // Skew the (x,y,z,w) space to determine which cell of 24 simplices we're in
            double s = (x + y + z + w) * F4; // Factor for 4D skewing
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            int k = FastFloor(z + s);
            int l = FastFloor(w + s);
            double t = (i + j + k + l) * G4; // Factor for 4D unskewing
            double X0 = i - t; // Unskew the cell origin back to (x,y,z,w) space
            double Y0 = j - t;
            double Z0 = k - t;
            double W0 = l - t;
            double x0 = x - X0; // The x,y,z,w distances from the cell origin
            double y0 = y - Y0;
            double z0 = z - Z0;
            double w0 = w - W0;
            // For the 4D case, the simplex is a 4D shape I won't even try to describe.
            // To find out which of the 24 possible simplices we're in, we need to
            // determine the magnitude ordering of x0, y0, z0 and w0.
            // The method below is a good way of finding the ordering of x,y,z,w and
            // then find the correct traversal order for the simplex we’re in.
            // First, six pair-wise comparisons are performed between each possible pair
            // of the four coordinates, and the results are used to add up binary bits
            // for an integer index.
            int c1 = (x0 > y0) ? 32 : 0;
            int c2 = (x0 > z0) ? 16 : 0;
            int c3 = (y0 > z0) ? 8 : 0;
            int c4 = (x0 > w0) ? 4 : 0;
            int c5 = (y0 > w0) ? 2 : 0;
            int c6 = (z0 > w0) ? 1 : 0;
            int c = c1 + c2 + c3 + c4 + c5 + c6;
            int i1, j1, k1, l1; // The integer offsets for the second simplex corner
            int i2, j2, k2, l2; // The integer offsets for the third simplex corner
            int i3, j3, k3, l3; // The integer offsets for the fourth simplex corner
            // simplex[c] is a 4-vector with the numbers 0, 1, 2 and 3 in some order.
            // Many values of c will never occur, since e.g. x>y>z>w makes x<z, y<w and x<w
            // impossible. Only the 24 indices which have non-zero entries make any sense.
            // We use a thresholding to set the coordinates in turn from the largest magnitude.
            // The number 3 in the "simplex" array is at the position of the largest coordinate.
            i1 = simplex[c, 0] >= 3 ? 1 : 0;
            j1 = simplex[c, 1] >= 3 ? 1 : 0;
            k1 = simplex[c, 2] >= 3 ? 1 : 0;
            l1 = simplex[c, 3] >= 3 ? 1 : 0;
            // The number 2 in the "simplex" array is at the second largest coordinate.
            i2 = simplex[c, 0] >= 2 ? 1 : 0;
            j2 = simplex[c, 1] >= 2 ? 1 : 0;
            k2 = simplex[c, 2] >= 2 ? 1 : 0;
            l2 = simplex[c, 3] >= 2 ? 1 : 0;
            // The number 1 in the "simplex" array is at the second smallest coordinate.
            i3 = simplex[c, 0] >= 1 ? 1 : 0;
            j3 = simplex[c, 1] >= 1 ? 1 : 0;
            k3 = simplex[c, 2] >= 1 ? 1 : 0;
            l3 = simplex[c, 3] >= 1 ? 1 : 0;
            // The fifth corner has all coordinate offsets = 1, so no need to look that up.
            double x1 = x0 - i1 + G4; // Offsets for second corner in (x,y,z,w) coords
            double y1 = y0 - j1 + G4;
            double z1 = z0 - k1 + G4;
            double w1 = w0 - l1 + G4;
            double x2 = x0 - i2 + 2.0 * G4; // Offsets for third corner in (x,y,z,w) coords
            double y2 = y0 - j2 + 2.0 * G4;
            double z2 = z0 - k2 + 2.0 * G4;
            double w2 = w0 - l2 + 2.0 * G4;
            double x3 = x0 - i3 + 3.0 * G4; // Offsets for fourth corner in (x,y,z,w) coords
            double y3 = y0 - j3 + 3.0 * G4;
            double z3 = z0 - k3 + 3.0 * G4;
            double w3 = w0 - l3 + 3.0 * G4;
            double x4 = x0 - 1.0 + 4.0 * G4; // Offsets for last corner in (x,y,z,w) coords
            double y4 = y0 - 1.0 + 4.0 * G4;
            double z4 = z0 - 1.0 + 4.0 * G4;
            double w4 = w0 - 1.0 + 4.0 * G4;
            // Work out the hashed gradient indices of the five simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int ll = l & 255;
            int gi0 = perm[ii + perm[jj + perm[kk + perm[ll]]]] % 32;
            int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1 + perm[ll + l1]]]] % 32;
            int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2 + perm[ll + l2]]]] % 32;
            int gi3 = perm[ii + i3 + perm[jj + j3 + perm[kk + k3 + perm[ll + l3]]]] % 32;
            int gi4 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1 + perm[ll + 1]]]] % 32;
            // Calculate the contribution from the five corners
            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(grad4[gi0], x0, y0, z0, w0);
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(grad4[gi1], x1, y1, z1, w1);
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(grad4[gi2], x2, y2, z2, w2);
            }
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
            if (t3 < 0) n3 = 0.0;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Dot(grad4[gi3], x3, y3, z3, w3);
            }
            double t4 = 0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
            if (t4 < 0) n4 = 0.0;
            else
            {
                t4 *= t4;
                n4 = t4 * t4 * Dot(grad4[gi4], x4, y4, z4, w4);
            }
            // Sum up and scale the result to cover the range [-1,1]
            return 27.0 * (n0 + n1 + n2 + n3 + n4);
        }

        #endregion
    }
}
