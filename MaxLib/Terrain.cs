using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LibMax.Noise;

namespace LibMax
{
    public class Terrain
    {
        public int Width, Height, Seed;
        private double Scale;
        //private float LightAngle;
        private TerrainUnit[][] Terrainmap;
        private Gradient[] Grad;
        private Random r;
        private SimplexNoise noise;
        private Thread[] Threads;

        public Terrain(int width, int height, double scale, int seed = -1)
        {
            r = new Random();
            this.Width = width;
            this.Height = height;
            this.Scale = scale;

            #region Gradient Initializer (Big & Ugly)

            // This first gradient is height based, and used exclusively for water. This convention is expected to be followed
            // (First gradient is for water, and height-based, all succeeding gradients are moisture-based and for land)
            Grad = new Gradient[5];
            List<KeyValuePair<double, Color>> colorList = new List<KeyValuePair<double, Color>>();

            colorList.Add(new KeyValuePair<double, Color>(-1.0, new Color(0, 0, 128)));
            colorList.Add(new KeyValuePair<double, Color>(-0.2, new Color(32, 64, 128)));
            colorList.Add(new KeyValuePair<double, Color>(-0.07, new Color(64, 96, 192)));
            colorList.Add(new KeyValuePair<double, Color>(-0.035, new Color(192, 192, 128)));

            Grad[0] = new Gradient(colorList);

            // This gradient and all following it are for land, and are moisture-based.
            colorList = new List<KeyValuePair<double, Color>>();
            colorList.Add(new KeyValuePair<double, Color>(-1.0, new Color(128, 128, 128))); // Threshold values between these types:
            colorList.Add(new KeyValuePair<double, Color>(0.0, new Color(255, 255, 255))); // -0.5
            colorList.Add(new KeyValuePair<double, Color>(1.0, new Color(255, 255, 255))); // 0.5

            Grad[4] = new Gradient(colorList);

            colorList = new List<KeyValuePair<double, Color>>();

            colorList.Add(new KeyValuePair<double, Color>(-0.66, new Color(202, 213, 162)));
            colorList.Add(new KeyValuePair<double, Color>(0.0, new Color(151, 165, 139))); // -0.33
            colorList.Add(new KeyValuePair<double, Color>(0.66, new Color(163, 178, 139))); // 0.33

            Grad[3] = new Gradient(colorList);

            colorList = new List<KeyValuePair<double, Color>>();

            colorList.Add(new KeyValuePair<double, Color>(-0.835, new Color(202, 213, 162)));
            colorList.Add(new KeyValuePair<double, Color>(-0.33, new Color(150, 179, 117))); // -0.5825
            colorList.Add(new KeyValuePair<double, Color>(0.33, new Color(129, 161, 115))); // 0.0
            colorList.Add(new KeyValuePair<double, Color>(0.835, new Color(109, 153, 114))); // 0.5825

            Grad[2] = new Gradient(colorList);

            colorList = new List<KeyValuePair<double, Color>>();

            colorList.Add(new KeyValuePair<double, Color>(-0.835, new Color(210, 193, 157)));
            colorList.Add(new KeyValuePair<double, Color>(-0.495, new Color(150, 179, 117))); // -0.66
            colorList.Add(new KeyValuePair<double, Color>(0.0, new Color(115, 166, 109))); // -0.2475
            colorList.Add(new KeyValuePair<double, Color>(0.66, new Color(100, 139, 114))); // 0.33

            Grad[1] = new Gradient(colorList);

            //colorList = new List<KeyValuePair<double, Color>>();
            //colorList.Add(new KeyValuePair<double, Color>(-1.0, new Color(0, 0, 128)));
            //colorList.Add(new KeyValuePair<double, Color>(-0.2, new Color(32, 64, 128)));
            //colorList.Add(new KeyValuePair<double, Color>(-0.07, new Color(64, 96, 192)));
            //colorList.Add(new KeyValuePair<double, Color>(-0.035, new Color(192, 192, 128)));

            //WaterGrad = new Gradient(colorList);
            #endregion

            this.Seed = (seed == -1) ? r.Next() : seed;
            noise = new SimplexNoise(Seed, frequency: Scale);
            //CoastalIndices = new List<Point>();

            Threads = new Thread[Environment.ProcessorCount];
        }

        public TerrainUnit this[int x, int y]
        {
            get { return Terrainmap[x][y]; }
        }

        /// <summary>
        /// Begins Terrain Generation, creating a base heightmap. Enclose in between Begin() and End() all heightmap operations.
        /// <param name="useMoisture">Signifies whether or not to cast moisture from a random direction</param>
        /// </summary>
        public void Begin()
        {
            Terrainmap = new TerrainUnit[Width][];
            for (int i = 0; i < Width; i++) Terrainmap[i] = new TerrainUnit[Height];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Terrainmap[x][y] = new TerrainUnit(noise[x, y], 0);
        }

        private double dist2(int x1, int y1, int x2, int y2)
        {
            return Math.Abs((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public void RainErosion(int nbDrops, double dropMoistureValue)
        {
            // While there are still drops to simulate
            while (nbDrops > 0)
            {
                double erosionCoef = 0.1, agregationCoef = 0.1;
                // Choose a random point in the heightmap
                int x = r.Next(0, Width - 1);
                int y = r.Next(0, Height - 1);
                //if (Terrainmap[x][y] < 0) continue;
                // Create a set of derivative coordinates
                int[] d = { -1, 0, 1 };
                double slope = 0.0f;
                double sediment = 0.0f;
                do
                {
                    int nx = 0, ny = 0;
                    // Get the height value of the rent point
                    double v = Terrainmap[x][y].Height;
                    /* calculate slope at x,y */
                    slope = 0.0f;
                    foreach (int i in d)
                        foreach (int j in d)
                        {
                            // Check all the points around the point
                            int dx = x + i;
                            int dy = y + j;
                            // Make sure the derivative point is on the heightmap
                            if (dx >= 0 && dx < Width && dy >= 0 && dy < Height)
                            {
                                // Find the slope, and if that slope is greater than the
                                // previous greatest slope, take note of it
                                double nslope = v - Terrainmap[dx][dy].Height;
                                if (nslope > slope)
                                {
                                    slope = nslope;
                                    nx = dx;
                                    ny = dy;
                                }
                            }
                        }
                    if (Terrainmap[x][y].Height > -0.035) /*Terrainmap[x][y].Type = TerrainType.River;*/ Terrainmap[x][y].Moisture += dropMoistureValue;
                    // If a slope was greater than zero, then erosion would be
                    // occurring

                    if (slope > 0.0f)
                    {
                        Terrainmap[x][y].Height -= erosionCoef * slope;
                        DigHill(x, y, 4 * r.NextDouble() * (1.0 - Terrainmap[x][y].Height), -erosionCoef * 4.0 * slope);
                        x = nx;
                        y = ny;
                        sediment += slope;
                    }
                    // If there was no slope greater than zero, then aggregation
                    // would be occurring
                    else Terrainmap[x][y].Height += agregationCoef * sediment;
                }
                // Continue to do this as long as there is a slope
                while (slope > 0.0f);
                nbDrops--;
            }
        }

        public void HeatErosion(int nbPass, double minSlope, double erosionCoef, double agregationCoef)
        {
            while (nbPass > 0)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        double v = Terrainmap[x][y].Height;
                        //if (v < 0) continue;
                        int[] d = { -1, 0, 1 };
                        int nextx = x, nexty = x;
                        /* calculate slope at x,y */
                        double slope = 0.0f;
                        foreach (int i in d)
                            foreach (int j in d)
                            {
                                int nx = x + i;
                                int ny = y + j;
                                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                                {
                                    double nslope = v - Terrainmap[nx][ny].Height;
                                    if (nslope > slope)
                                    {
                                        slope = nslope;
                                        nextx = nx;
                                        nexty = ny;
                                    }
                                }
                            }
                        if (slope > minSlope)
                        {
                            Terrainmap[x][y].Height -= erosionCoef * (slope - minSlope);
                            //AddHill(x, y, 2 * r.NextDouble(), -erosionCoef * (slope - minSlope));
                            Terrainmap[nextx][nexty].Height += agregationCoef * (slope - minSlope);
                            //DigHill(x, y, 2 * r.NextDouble(), agregationCoef * (slope - minSlope));
                        }
                        else if (slope < minSlope)
                        {
                            Terrainmap[x][y].Height += erosionCoef * (slope - minSlope);
                            //AddHill(x, y, 2 * r.NextDouble(), -erosionCoef * (slope - minSlope));
                            Terrainmap[nextx][nexty].Height -= agregationCoef * (slope - minSlope);
                            //DigHill(x, y, 2 * r.NextDouble(), agregationCoef * (slope - minSlope));
                        }
                    }
                }
                nbPass--;
            }
        }

        private void DigHill(double hx, double hy, double hradius, double hheight)
        {
            int x, y;
            double hradius2 = hradius * hradius;
            double coef = hheight / hradius2;
            int minx = (int)Math.Max(0, hx - hradius);
            int maxx = (int)Math.Min(Width, hx + hradius);
            int miny = (int)Math.Max(0, hy - hradius);
            int maxy = (int)Math.Min(Height, hy + hradius);
            for (x = minx; x < maxx; x++)
            {
                double xdist = (x - hx) * (x - hx);
                for (y = miny; y < maxy; y++)
                {
                    double dist = xdist + (y - hy) * (y - hy);
                    if (dist < hradius2)
                    {
                        double z = (hradius2 - dist) * coef;
                        if (hheight > 0.0)
                        {
                            if (Terrainmap[x][y].Height < z)
                                Terrainmap[x][y].Height -= z;
                        }
                        else
                        {
                            if (Terrainmap[x][y].Height > z)
                                Terrainmap[x][y].Height += z;
                        }
                        Terrainmap[x][y].Moisture += 0.005;
                    }
                }
            }
        }

        private void AddHill(double hx, double hy, double hradius, double hheight)
        {
            int x, y;
            double hradius2 = hradius * hradius;
            double coef = hheight / hradius2;
            int minx = (int)Math.Max(0, hx - hradius);
            int maxx = (int)Math.Min(Width, hx + hradius);
            int miny = (int)Math.Max(0, hy - hradius);
            int maxy = (int)Math.Min(Height, hy + hradius);
            for (x = minx; x < maxx; x++)
            {
                double xdist = (x - hx) * (x - hx);
                for (y = miny; y < maxy; y++)
                {
                    double z = hradius2 - xdist - (y - hy) * (y - hy);
                    if (z > 0.0) Terrainmap[x][y].Height += z * coef;
                }
            }
        }

        /// <summary>
        /// Sends moisture from rivers out across the rest of the terrain.
        /// </summary>
        /// <param name="seedThreshold">The minimum moisture a point must have to be able to propagate it.</param>
        /// <param name="falloffExponent">The amount by which to multiply the moisture per level of propagation. Must be greater than 0.</param>
        public void PropagateMoisture(double seedThreshold, double falloffExponent)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Terrainmap[x][y].Height > -0.07 && Terrainmap[x][y].Moisture > seedThreshold)
                        Terrainmap[x][y].Type = TerrainType.River;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Terrainmap[x][y].Type == TerrainType.River)
                        SendMoistureToNeighbors(x, y, 1, falloffExponent);
        }

        private void SendMoistureToNeighbors(int x, int y, int level, double fe)
        {
            double moisture = (1.0 - Math.Abs(Terrainmap[x][y].Height)) * Terrainmap[x][y].Moisture / Math.Pow(level, fe);
            if (moisture < 0.05) return;
            int[] d = { -1, 0, 1 };

            if (x - 1 >= 0 && x + 1 < Width && y - 1 >= 0 && y + 1 < Height)
            {
                foreach (int i in d)
                    foreach (int j in d)
                        if (Terrainmap[x + i][y + j].Type != TerrainType.River && Terrainmap[x + i][y + j].Moisture < moisture && (i != 0 || j != 0))
                        {
                            Terrainmap[x + i][y + j].Moisture += moisture;
                            Terrainmap[x + i][y + j].TypeColor = new Color((float)moisture, 0f, 0f);
                            SendMoistureToNeighbors(x + i, y + j, level + 1, fe);
                        }
            }
        }

        public void Smooth()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    double value = 0.0;
                    double cellAverage = 1.0;
                    int[] d = { -1, 0, 1 };

                    if (x - 1 >= 0 && x + 1 < Width && y - 1 >= 0 && y + 1 < Height)
                    {
                        foreach (int i in d)
                            foreach (int j in d)
                            {
                                int xi = x + i, yi = y + j;
                                value += Terrainmap[xi][yi].Height;
                                ++cellAverage;
                            }
                        Terrainmap[x][y].Height = value / cellAverage;
                    }
                }
        }

        public void Normalize()
        {
            double low = 0.0, high = 0.0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (Terrainmap[x][y].Height > high) high = Terrainmap[x][y].Height;
                    else if (Terrainmap[x][y].Height < low) low = Terrainmap[x][y].Height;
                }
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Terrainmap[x][y].Height = 3 * ((Terrainmap[x][y].Height - low) / (high - low)) - 1.5;
        }
        /// <summary>
        /// Ends Terrain Generation, finalizing the complete Terrainmap. After this is called you can get textures and values.
        /// </summary>
        /// <param name="lighting">Whether or not to use simple raycasting to shade the map</param>
        /// <param name="azimuth">If using lighting, this represents the azimuth of the sun relative the to north.</param>
        /// <param name="altitude">If using lighting, this represents the angle of the sun relative to the horizon.</param>
        public void End(bool lighting, float azimuth = 0, float altitude = 0)
        {
            double high = 0.0, low = 0.0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Terrainmap[x][y].determineType();
                    Terrainmap[x][y].determineTypeColor(Grad);

                    if (x - 1 >= 0 && x + 1 < Width && y - 1 >= 0 && y + 1 < Height)
                    {
                        Vector3 n1 = Vector3.Cross(new Vector3(0, 1, 100.0f * (float)(Terrainmap[x][y + 1].Height - Terrainmap[x][y].Height)),
                                                    new Vector3(-1, 0, 100.0f * (float)(Terrainmap[x - 1][y].Height - Terrainmap[x][y].Height)));
                        Vector3 n2 = Vector3.Cross(new Vector3(1, 0, 100.0f * (float)(Terrainmap[x + 1][y].Height - Terrainmap[x][y].Height)),
                                                    new Vector3(0, 1, 100.0f * (float)(Terrainmap[x][y + 1].Height - Terrainmap[x][y].Height)));
                        Vector3 n3 = Vector3.Cross(new Vector3(0, -1, 100.0f * (float)(Terrainmap[x][y - 1].Height - Terrainmap[x][y].Height)),
                                                    new Vector3(1, 0, 100.0f * (float)(Terrainmap[x + 1][y].Height - Terrainmap[x][y].Height)));
                        Vector3 n4 = Vector3.Cross(new Vector3(-1, 0, 100.0f * (float)(Terrainmap[x - 1][y].Height - Terrainmap[x][y].Height)),
                                                    new Vector3(0, -1, 100.0f * (float)(Terrainmap[x][y - 1].Height - Terrainmap[x][y].Height)));

                        Terrainmap[x][y].Normal = n1 + n2 + n3 + n4;
                        //Terrainmap[x][y].Normal /= 4;
                        Terrainmap[x][y].Normal.Normalize();
                    }
                    else Terrainmap[x][y].Normal = Vector3.UnitZ;

                    if (Terrainmap[x][y].Height < low) low = Terrainmap[x][y].Height;
                    else if (Terrainmap[x][y].Height > high) high = Terrainmap[x][y].Height;
                }
            if (lighting)
            {
                CastLight(azimuth, altitude);

                //for (int k = 0, i = 0; k < Width; k++, i++)
                //{
                //    if (i >= Environment.ProcessorCount) i = 0;

                //    Threads[i] = new Thread(new ParameterizedThreadStart(CastSunRays));
                //    Threads[i].SetApartmentState(ApartmentState.MTA);

                //    Threads[i].Start(k);
                //}
                //foreach (Thread thread in Threads) if (thread != null) thread.Join();
            }
            else
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        Terrainmap[x][y].RealColor = Terrainmap[x][y].TypeColor;

            Console.WriteLine("Highest point was {0}. Lowest point was {1}", high, low);
        }

        //private void CastSunRays(object offset)
        //{
        //    LightRay2D ray = new LightRay2D(ref Terrainmap, LightAngle);
        //    int o = (int)offset;

        //    for (int y = 0; y < Height; y++)
        //        ray.CastTo(o, y);
        //}

        public void CastLight(float azimuth, float altitude)
        {
            float al = MathHelper.ToRadians(altitude);
            float az = MathHelper.ToRadians(90 + azimuth);
            Vector3 s = new Vector3((float)(Math.Cos(al) * Math.Cos(az)), (float)(Math.Cos(al) * Math.Sin(az)), (float)Math.Sin(al));
            s.Normalize();
            LightRay3D ray = new LightRay3D(ref Terrainmap, altitude, azimuth);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    //if (Terrainmap[x][y].Height < -0.07) Terrainmap[x][y].RealColor = Terrainmap[x][y].TypeColor;
                    //else
                    //{
                    float d = Vector3.Dot(s, Terrainmap[x][y].Normal);
                    Terrainmap[x][y].RealColor = Color.Lerp(Terrainmap[x][y].TypeColor, Color.Black, 0.3f * d);
                    //}
                    ray.CastTo(x, y);
                }
        }

        public Texture2D GetTexture(GraphicsDevice graphicsDevice)
        {
            if (Terrainmap == null) return GetGrayscale(graphicsDevice);
            Color[] col = new Color[Width * Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    col[x + Width * y] = Terrainmap[x][y].RealColor;
            Texture2D tex = new Texture2D(graphicsDevice, Width, Height);
            tex.SetData<Color>(col);
            return tex;
        }

        public Texture2D GetGrayscale(GraphicsDevice graphicsDevice)
        {
            if (Terrainmap == null) return new Texture2D(graphicsDevice, 0, 0);
            Color[] col = new Color[Width * Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    float h = (float)(255 * (0.5 * Terrainmap[x][y].Height + 0.5));
                    col[x + Width * y] = new Color(h, h, h);
                }
            Texture2D tex = new Texture2D(graphicsDevice, Width, Height);
            tex.SetData<Color>(col);
            return tex;
        }
    }
}
