using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMax.Noise
{
    public class PerlinNoise
    {
        public int Seed, Octaves;
        public double Amplitude, Persistence, Frequency;

        public PerlinNoise()
        {
            Random r = new Random();
            this.Seed = r.Next();
            this.Octaves = 8;
            this.Amplitude = 1.0;
            this.Frequency = 0.005;
            this.Persistence = 0.5;
        }

        public PerlinNoise(int seed, int octaves = 8, double amplitude = 1.0, double persistence = 0.5, double frequency = 0.0015)
        {
            this.Seed = seed;
            this.Octaves = octaves;
            this.Amplitude = amplitude;
            this.Persistence = persistence;
            this.Frequency = frequency;
        }

        public PerlinNoise(int seed)
        {
            this.Seed = seed;
            this.Octaves = 8;
            this.Amplitude = 1.0;
            this.Frequency = 0.0015;
            this.Persistence = 0.5;
        }

        public double this[double x, double y] {
            get { return Total(x, y); }
            private set { }
        }

        private double Total(double x)
        {
            //returns -1 to 1
            double total = 0.0;
            double freq = Frequency, amp = Amplitude;
            for (int i = 0; i < Octaves; ++i)
            {
                total = total + Smooth(x * freq) * amp;
                freq *= 2;
                amp *= Persistence;
            }
            if (total < -2.4) total = -2.4;
            else if (total > 2.4) total = 2.4;

            return Amplitude * (total / 2.4);
        }

        private double Total(double x, double y)
        {
            //returns -1 to 1
            double total = 0.0;
            double freq = Frequency, amp = Amplitude;
            for (int i = 0; i < Octaves; ++i)
            {
                total = total + Smooth(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= Persistence;
            }
            if (total < -2.4) total = -2.4;
            else if (total > 2.4) total = 2.4;

            return Amplitude * (total / 2.4);
        }

        private double Noise(int x)
        {
            int n = x * 57;
            n = (n << 13) ^ n;

            return (1.0 - ((n * (n * n * 15731 + 789221) + Seed) & 0x7fffffff) / 1073741824.0);
        }

        private double Noise(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;

            return (1.0 - ((n * (n * n * 15731 + 789221) + Seed) & 0x7fffffff) / 1073741824.0);
        }

        private double Smooth(double x)
        {
            double n1 = Noise((int)x);
            double n2 = Noise((int)x + 1);

            return Interpolate(n1, n2, x - (int)x);
        }

        private double Smooth(double x, double y)
        {
            double n1 = Noise((int)x, (int)y);
            double n2 = Noise((int)x + 1, (int)y);
            double n3 = Noise((int)x, (int)y + 1);
            double n4 = Noise((int)x + 1, (int)y + 1);

            double i1 = Interpolate(n1, n2, x - (int)x);
            double i2 = Interpolate(n3, n4, x - (int)x);

            return Interpolate(i1, i2, y - (int)y);
        }

        private double Interpolate(double x, double y, double a)
        {
            double value = (1 - Math.Cos(a * Math.PI)) * 0.5;
            return x * (1 - value) + y * value;
        }

    }
}
