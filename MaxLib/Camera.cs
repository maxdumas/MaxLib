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
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }

        public Vector3 CameraPosition { get; protected set; }
        Vector3 CameraDirection;
        Vector3 CameraUp;

        MouseState OldMouseState;

        public Camera(Game game, Vector3 position, Vector3 target, Vector3 upDirection)
            : base(game)
        {
            //View = Matrix.CreateLookAt(position, target, upDirection);
            CameraPosition = position;
            CameraDirection = target - position;
            CameraDirection.Normalize();
            CameraUp = upDirection;
            CreateLookAt();

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Game.Window.ClientBounds.Width / Game.Window.ClientBounds.Height, 1, 1000);
        }

        private void CreateLookAt()
        {
            View = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraDirection, CameraUp);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            OldMouseState = Mouse.GetState();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                CameraPosition += CameraDirection * 5.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                CameraPosition -= CameraDirection * 5.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                CameraPosition += Vector3.Cross(CameraUp, CameraDirection) * 5.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                CameraPosition -= Vector3.Cross(CameraUp, CameraDirection) * 5.0f;

            // Yaw
            CameraDirection = Vector3.Transform(CameraDirection, Matrix.CreateFromAxisAngle(CameraUp, (-MathHelper.PiOver4 / 150) * (Mouse.GetState().X - OldMouseState.X)));
            // Roll
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                CameraUp = Vector3.Transform(CameraUp, Matrix.CreateFromAxisAngle(CameraDirection, MathHelper.PiOver4 / 45));
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                CameraUp = Vector3.Transform(CameraUp, Matrix.CreateFromAxisAngle(CameraDirection, -MathHelper.PiOver4 / 45));
            }
            // Pitch
            CameraDirection = Vector3.Transform(CameraDirection, Matrix.CreateFromAxisAngle(Vector3.Cross(CameraUp, CameraDirection), (MathHelper.PiOver4/100) * (Mouse.GetState().Y - OldMouseState.Y)));
            CameraUp = Vector3.Transform(CameraUp, Matrix.CreateFromAxisAngle(Vector3.Cross(CameraUp, CameraDirection), (MathHelper.PiOver4 / 100) * (Mouse.GetState().Y - OldMouseState.Y)));

            OldMouseState = Mouse.GetState();

            CreateLookAt();
            base.Update(gameTime);
        }
    }
}
