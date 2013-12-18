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
using LibMax;
using LibMax.Noise;

namespace MaxLibTests
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        Texture2D map;
        Terrain terrain;

        Camera camera;
        VertexPositionNormalTexture[] verts;
        VertexBuffer vertexBuffer;
        IndexBuffer ib;
        int nIndices;
        BasicEffect effect;

        public const int screenWidth = 800, screenHeight = 600;
        bool threeD = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;

            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new Camera(this, new Vector3(0, 0, 5), new Vector3(0, 0, 0), Vector3.Down);
            Components.Add(camera);

            //Components.Add(new CloudManager(this, 0));
            base.Initialize();
        }

        public void Init()
        {
            s.Restart();
            terrain = new Terrain(400, 300, 0.003);
            terrain.Begin();
            //terrain.HeatErosion(2, 0.8, 0.5, 0.2);
            terrain.Normalize();
            terrain.Smooth();
            terrain.RainErosion(2500, 0.07);
            terrain.PropagateMoisture(0.2, 0.3);
            terrain.End(true, 0, 45);
            s.Stop();
            Console.WriteLine("Generation with seed {0} completed in {1} milliseconds", terrain.Seed, s.ElapsedMilliseconds);
            map = terrain.GetTexture(GraphicsDevice);

            verts = new VertexPositionNormalTexture[terrain.Width * terrain.Height];
            for (int i = 0; i < terrain.Width; i++)
            {
                //verts[i] = new VertexPositionColor[terrain.Height];
                for (int j = 0; j < terrain.Height; j++)
                    verts[i + j * terrain.Width] = new VertexPositionNormalTexture(new Vector3(i, (float)(terrain[i, j].Height * -25.0), j), terrain[i, j].Normal, new Vector2(i, j)/*terrain[i, j].TypeColor*/);
            }

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), terrain.Width * terrain.Height, BufferUsage.None);
            vertexBuffer.SetData(verts);

            ib = new IndexBuffer(GraphicsDevice, typeof(int), (terrain.Width - 1) * (terrain.Height - 1) * 6, BufferUsage.WriteOnly);
            int[] indices = new int[(terrain.Width - 1) * (terrain.Height - 1) * 6];
            for (int x = 0; x < terrain.Width - 1; x++)
                for (int y = 0; y < terrain.Height - 1; y++)
                {
                    indices[(x + y * (terrain.Width - 1)) * 6] = (x + 1) + (y + 1) * terrain.Width;
                    indices[(x + y * (terrain.Width - 1)) * 6 + 1] = (x + 1) + y * terrain.Width;
                    indices[(x + y * (terrain.Width - 1)) * 6 + 2] = x + y * terrain.Width;

                    indices[(x + y * (terrain.Width - 1)) * 6 + 3] = (x + 1) + (y + 1) * terrain.Width;
                    indices[(x + y * (terrain.Width - 1)) * 6 + 4] = x + y * terrain.Width;
                    indices[(x + y * (terrain.Width - 1)) * 6 + 5] = x + (y + 1) * terrain.Width;
                }

            ib.SetData<int>(indices);
            nIndices = indices.Length;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            

            Init();

            
            effect = new BasicEffect(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        float time = 0;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) this.Exit();
            else if (Keyboard.GetState().IsKeyDown(Keys.Space)) this.Init();
            else if (Keyboard.GetState().IsKeyDown(Keys.F)) threeD = !threeD;
            
            time += gameTime.ElapsedGameTime.Milliseconds;
            if (time / 40 > 360) time = 0;
            terrain.CastLight(0, time / 40);
            map = terrain.GetTexture(GraphicsDevice);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            if (threeD)
            {
                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.Indices = ib;
                GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

                //effect.World *= Matrix.CreateRotationX(MathHelper.PiOver4 / 60);
                effect.View = camera.View;
                effect.Projection = camera.Projection;
                effect.VertexColorEnabled = false;
                effect.EnableDefaultLighting();

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrain.Width * terrain.Height, 0, nIndices / 2 + 1);
                }
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                spriteBatch.Draw(map, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
