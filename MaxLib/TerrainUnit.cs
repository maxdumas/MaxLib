using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LibMax
{
    public class TerrainUnit
    {
        public TerrainType Type;
        public Color TypeColor;
        public Color RealColor;
        public double Height;
        public double Moisture;
        public Vector3 Normal;

        public TerrainUnit(double height = 0, double moisture = 0)
        {
            Height = height;
            Moisture = moisture;
        }

        public void determineType()
        {
            if (Type != TerrainType.River)
            {
                if (Height <= -0.07) Type = TerrainType.Ocean;
                else if (Height <= -0.02) Type = TerrainType.Beach;
                else if (Height <= 0.1975)
                {
                    if (Moisture <= -0.66) Type = TerrainType.SubtropicalDesert;
                    else if (Moisture <= -0.2475) Type = TerrainType.Grassland;
                    else if (Moisture <= 0.33) Type = TerrainType.TropicalSeasonalForest;
                    else if (Moisture <= 1.0) Type = TerrainType.TropicalRainForest;
                }
                else if (Height <= 0.465)
                {
                    if (Moisture <= -0.5825) Type = TerrainType.TemperateDesert;
                    else if (Moisture <= 0.0) Type = TerrainType.Grassland;
                    else if (Moisture <= 0.5825) Type = TerrainType.TemperateDeciduousForest;
                    else if (Moisture <= 1.0) Type = TerrainType.TemperateRainForest;
                }
                else if (Height <= 0.7325)
                {
                    if (Moisture <= -0.33) Type = TerrainType.TemperateDesert;
                    else if (Moisture <= 0.33) Type = TerrainType.Shrubland;
                    else if (Moisture <= 1.0) Type = TerrainType.Taiga;
                }
                else if (Height <= 1.0)
                {
                    if (Moisture <= -0.5) Type = TerrainType.Scorched;
                    else if (Moisture <= 0.5) Type = TerrainType.Tundra;
                    else if (Moisture <= 1.0) Type = TerrainType.Snow;
                }
            }
        }

        public void determineTypeColor(Gradient[] grad)
        {
            Color c;
            if (Type == TerrainType.River) c = new Color(64, 96, 192);
            else if (Height < -0.02) c = grad[0][Height];
            else
            {
                double[] t = { 0.02, 0.1975, 0.465, 0.7325, 1.0 };
                int i;
                for (i = 1; i < t.Length - 1; i++) if (Height < t[i]) break;
                int i0 = (i - 1< 1) ? 1 : (i - 1);
                if (i0 == i) c = grad[i0][Moisture];
                else
                {
                    double a = (Height - t[i0]) / (t[i] - t[i0]);
                    c = Color.Lerp(grad[i0][Moisture], grad[i][Moisture], (float)a);
                }
            }

            //c = new Color(c.R + r.Next(-2, 2), c.G + r.Next(-2, 2), c.B + r.Next(-2, 2));
            TypeColor = c;
        }
    }


    public enum TerrainType
    {
        Ocean, Beach, River,
        Snow, Tundra, Bare, Scorched,
        Taiga, Shrubland, TemperateDesert,
        TemperateRainForest, TemperateDeciduousForest, Grassland,
        TropicalRainForest, TropicalSeasonalForest, SubtropicalDesert,
        ERROR
    }
}
