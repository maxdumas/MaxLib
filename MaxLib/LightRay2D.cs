using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LibMax
{
    public class LightRay2D
    {
        // Needs to have a positive Y and a negative X
        private Vector2 Position, safePosition;
        private float Angle, Darkness;
        private int CenterX, Width;
        private TerrainUnit[][] Terrainmap;

        public LightRay2D(ref TerrainUnit[][] terrainmap, float angle)
        {
            this.Angle = MathHelper.ToRadians(angle);
            this.Darkness = (float)Math.Abs(Math.Cos(Angle));
            this.CenterX = (this.Width = terrainmap.GetLength(0))/2;
            // Give a starting position for the ray that it can always refer to.
            this.safePosition = new Vector2(Width * angle/180, 100);
            this.Position = Vector2.Zero;
            this.Terrainmap = terrainmap;
        }

        public void CastTo(int x, int y)
        {
            // Find the exagerated height for the target.
            float targetHeight = 100 * (float)Terrainmap[x][y].Height;
            // If the target is in the water, then clamp its height to the water threshold, so as to make the water shadows appear as a flat plane, as they should.
            if (targetHeight < -7) targetHeight = -7f;
            //double r = 100.0f * (Math.Cos(Math.Atan2(targetHeight, x) - MathHelper.ToRadians(Angle))) / x;
            //Position.Y = (float)(r * Math.Sin(Angle));
            //Position.X = (float)(r * Math.Cos(Angle));
            //if (x == 399 && y == 299) Console.WriteLine(Position);
            // The X position is the dx of the line from the calculated X from the angle to the center of the screen, offset by x
            Position.X = safePosition.X - CenterX + x;
            // The Y position is whatever we wanted it to be, offset by the target height.
            Position.Y = safePosition.Y + targetHeight;
            float dx = Math.Abs(x - Position.X);
            float dy = Math.Abs(targetHeight - Position.Y);
            int sx = (Position.X < x) ? 1 : -1;
            int sy = (Position.Y < targetHeight) ? 1 : -1; // Should never happen
            float error = dx - dy;
            float? H = null;
            // Depending on which way things are facing, adjust the checks.
            while ((Angle < 90) ? (x >= (int)Position.X) : (x <= (int)Position.X) && targetHeight < Position.Y)
            {
                // All this mess does is make sure the grid unit H is check is within in the bounds.
                H = 100 * (float)Terrainmap[((int)Position.X < 0) ? 0 : (((int)Position.X > Width-1) ? Width-1 : (int)Position.X)][y].Height;
                // Same as what is done with the targetHeight
                if (H < -7f) H = -7f;
                float error2 = 2 * error;
                // If we've collided with another part of the terrain, bail!
                if (H > Position.Y) break;
                // If it's time to change y, do so.
                if (error2 > -dy)
                {
                    error -= dy;
                    Position.X += sx;
                }
                // If it's time to change x, do so.
                if (error2 < dx)
                {
                    error += dx;
                    Position.Y += sy;
                }
            }

            //Vector3 s1 = new Vector3((float)-Math.Cos(Angle), 0, (float)-Math.Sin(Angle));
            //s1.Normalize();

            //float d = Vector3.Dot(s1, Terrainmap[x][y].Normal);

            if (Math.Abs(Position.X - x) > 3.0)
            {
                Terrainmap[x][y].TypeColor = Color.Lerp(Terrainmap[x][y].TypeColor, Color.Black, 0.4f * Darkness);
                //if (Terrainmap[x][y].Height > -0.07) Terrainmap[x][y].TypeColor = Color.Lerp(Terrainmap[x][y].TypeColor, Color.Black, 0.25f * ((Angle < 90) ? (1.0f - d) : d));
            }
            //else if (Terrainmap[x][y].Height > -0.07)
            //{
            //    Terrainmap[x][y].TypeColor = Color.Lerp(Terrainmap[x][y].TypeColor, Color.Black, 0.25f * ((Angle < 90) ? (1.0f - d) : d));
            //}
        }
    }
}