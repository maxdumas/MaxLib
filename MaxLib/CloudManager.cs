using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace LibMax
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CloudManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private float WindDirection;
        private Vector2 WindVector;
        private List<Cloud> Clouds;
        private Random r;
        private float time = 0;
        private int timeToSpawn = 0;
        private SpriteBatch spriteBatch;

        public CloudManager(Game game, float windDir)
            : base(game)
        {
            WindDirection = windDir;
            WindVector = new Vector2((float)Math.Cos(windDir), (float)Math.Sin(windDir));
            Clouds = new List<Cloud>();
            r = new Random();
            timeToSpawn = r.Next(5000);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.Milliseconds;

            if (time > timeToSpawn)
            {
                time = 0;
                timeToSpawn = r.Next(5000);
                Clouds.Add(new Cloud(GraphicsDevice, r.Next(150, 300), r.Next(150, 300), new Vector2(r.Next(800), r.Next(600))));
            }

            foreach (Cloud c in Clouds) c.position += WindVector;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (Cloud c in Clouds) c.Draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}
