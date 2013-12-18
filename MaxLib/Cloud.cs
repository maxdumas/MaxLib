using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using LibMax.Noise;

namespace LibMax
{
    class Cloud
    {
        private Texture2D texture;
        public Vector2 position;

        public Cloud(GraphicsDevice gfx, int width, int height, Vector2 pos)
        {
            SimplexNoise s = new SimplexNoise();

            texture = new Texture2D(gfx, width, height);
            Color[] col = new Color[width * height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    col[x + width * y] = Color.Lerp(Color.White, Color.Transparent, (0.5f * (float)s[x, y]) + 0.5f);

            texture.SetData<Color>(col);

            position = pos;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, Color.White);
            spriteBatch.End();
        }
    }
}
