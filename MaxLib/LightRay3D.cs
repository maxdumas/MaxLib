using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LibMax
{
    public class LightRay3D
    {
        private const double H = 150;

        private Vector3 StartPosition, Position, Direction;
        private float Altitude, Azimuth;
        private TerrainUnit[][] Terrainmap;

        public LightRay3D(ref TerrainUnit[][] terrainmap, float altitude, float azimuth)
        {
            Terrainmap = terrainmap;
            Altitude = MathHelper.ToRadians(altitude);
            Azimuth = MathHelper.ToRadians(azimuth);
            double K = (H / Math.Sin(Altitude)) * Math.Cos(Altitude);
            double x = K * Math.Cos(Azimuth);
            double y = K * Math.Sin(Azimuth);
            double z = H;

            StartPosition = Position = new Vector3((float)x, (float)y, (float)z);
            Direction = new Vector3((float)(Math.Cos(Altitude) * Math.Cos(Azimuth)), (float)(Math.Cos(Altitude) * Math.Sin(Azimuth)), (float)Math.Sin(Altitude));
            Direction.Normalize();
        }

        public void CastTo(int u, int v)
        {
            Position = StartPosition;
            Position.X += u;
            Position.Y += v;
            double w = 100.0 * Terrainmap[u][v].Height;
            if (w < -7.0) w = -7.0;
            //Direction = new Vector3(Position.X - u, Position.Y - v, Position.Z - (float)w);

            double heightAtLocation = w;

            while (inBounds() && Position.Z > heightAtLocation)
            {
                heightAtLocation = 100.0 * Terrainmap[(int)Position.X][(int)Position.Y].Height;
                Position += Direction;
            }

            if (Math.Abs(Position.X - u) > 3 && Math.Abs(Position.Y - v) > 3)
            {
                float d = Vector3.Dot(Direction, Terrainmap[u][v].Normal);
                Terrainmap[u][v].RealColor = Color.Lerp(Terrainmap[u][v].RealColor, Color.Black, 1);
            }
        }

        private bool inBounds()
        {
            return Position.X >= 0 && Position.X < Terrainmap.GetLength(0) && Position.Y >= 0 && Position.Y < Terrainmap[0].Length;
        }

        public void CastFrom(int x, int y)
        {
            //if (terrainmap[x][y].type != TerrainType.Ocean)
            //            	if(!terrainmap[x][y].inShadow)
            //				{
            //					Vector3f pos = new Vector3f(x, y, terrainmap[x][y].height * 100f);
            //					float h = pos.z;
            //
            //					while (inBounds(pos.x, pos.y) && pos.z >= h)
            //					{
            //						int i = (int)clamp(pos.x, 0f, width - 1);
            //						int j = (int)clamp(pos.y, 0f, height - 1);
            //						//if (terrainmap[(int) pos.x][(int) pos.y].height > TerrainType.Ocean.getMaxHeight())
            //							h = 100f * terrainmap[i][j].height;
            //						//else h = TerrainType.Ocean.getMaxHeight() * 100f;
            //						terrainmap[i][j].realColor = lerp(Color.BLACK, terrainmap[i][j].typeColor, 0.3f);
            //						terrainmap[i][j].inShadow = true;
            //						Vector3f.add(pos, s, pos);
            //					}
            //				}
        }
    }
}
