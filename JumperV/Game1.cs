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
using Microsoft.Kinect;
using GifAnimation;
using GameStateManagement;

namespace JumperV
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    class SplashScreen : GameScreen
    {
        ContentManager content;
        Texture2D splashTexture;
        //How long should the screen stay fully visible
        const float timeToStayOnScreen = 1.5f;
        //Keep track of how much time has passed
        float timer = 0f;
        public SplashScreen()
        {
            //How long to fade in
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            //How long to fade out
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }
        public void LoadContent()
        {
            //Load a new ContentManager so when we're done
            //showing this screen we can unload the content
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");
            //Splash screen texture
            splashTexture = content.Load<Texture2D>(@"screen");
        }
        public void UnloadContent()
        {
            content.Unload();
        }
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (ScreenState == ScreenState.Active)
            {
                //When this screen is fully active, we want to
                //begin our timer so we know when to fade out
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                timer += elapsed;
                if (timer >= timeToStayOnScreen)
                {
                    //When we've passed the 'timeToStayOnScreen' time,
                    //we call ExitScreen() which will fade out then
                    //kill the screen afterwards
                    ExitScreen();
                }
            }
            else if (ScreenState == ScreenState.TransitionOff)
            {
                if (TransitionPosition == 1)
                {
                    //When 'TransistionPosition' hits 1 then our screen
                    //is fully faded out. Anything in this block of
                    //code is the last thing to be called before this
                    //screen is killed forever so we add the next screen(s)
                }
            }
        }
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            Vector2 center = new Vector2(fullscreen.Center.X, fullscreen.Center.Y);
            spriteBatch.Begin();
            //Draw our logo centered to the screen
            spriteBatch.Draw(splashTexture,
                center,
                null,
                new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha),
                0f,
                new Vector2(splashTexture.Width / 2, splashTexture.Height / 2),
                1f,
                SpriteEffects.None,
                0f);
            spriteBatch.End();
        }
    }



    public class Game1 : Microsoft.Xna.Framework.Game
    {
        interface ISprite
        {
            void Draw(Game1 game);
            void Update(Game1 game);
        }

        class item : Game1.ISprite
        {
            public GifAnimation.GifAnimation itemAnimation;
            //public Texture2D itemTexture;
            public Vector2 itemPosition;
            public Vector2 itemSpeed;
            public bool Burst = false;
            public SoundEffect itemPopSound;
            public Color itemColor;

            static Random rand = new Random();

            public void Draw(Game1 game)
            {
                if (!Burst)
                    game.spriteBatch.Draw(this.itemAnimation.GetTexture(), itemPosition, itemColor);
            }

            public void Update(Game1 game)
            {
                if (Burst) return;

                itemPosition += itemSpeed;

                if (itemPosition.X > game.GraphicsDevice.Viewport.Width)
                {
                    itemPosition.X = -itemAnimation.Width;
                    itemPosition.Y = rand.Next(game.GraphicsDevice.Viewport.Height - itemAnimation.Height);
                }

                if (itemContains(game.PinVectorR) || itemContains(game.PinVectorL))
                {
                    itemPopSound.Play();
                    Burst = true;
                    return;
                }
            }
            public bool itemContains(Vector2 pos)
            {
                if (pos.X < itemPosition.X) return false;
                if (pos.X > itemPosition.X + itemAnimation.Width) return false;
                if (pos.Y < itemPosition.Y) return false;
                if (pos.Y > itemPosition.Y + itemAnimation.Height) return false;
                return true;
            }

            public item(GifAnimation.GifAnimation inAnimation, Vector2 inPosition, Vector2 inSpeed, SoundEffect inPop,Color inColor)
            {
                itemAnimation = inAnimation;
                itemPosition = inPosition;
                itemSpeed = inSpeed;
                itemPopSound = inPop;
                itemColor = inColor;
            }
        }

        public Vector2 PinVectorR, PinVectorL;
        List<ISprite> gameSprite = new List<ISprite>();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KinectSensor sensor = null;
        //GifAnimation.GifAnimation animation;
        GifAnimation.GifAnimation item_Animation1;
        GifAnimation.GifAnimation item_Animation2;
        GifAnimation.GifAnimation item_Animation3;
        GifAnimation.GifAnimation item_Animation4;
        GifAnimation.GifAnimation item_Animation5;
        GifAnimation.GifAnimation item_Animation6;
        GifAnimation.GifAnimation item_Animation_bulb;

        Texture2D KinectVideoTexture;
        Texture2D Background;
        Texture2D bone;
        Texture2D logo;
        Texture2D screen;
        Rectangle videoDisplayRectangle;
        Texture2D lineDot;
        SoundEffect itemPop;
        Rectangle RightRectangle;
        Rectangle LeftRectangle;

        Random rand = new Random();

        int noOfSprite = 0;

        int mAlphaValue = 1;
        int mFadeIncrement = 3;
        double mFadeDelay = .035;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            pinRectangleL = new Rectangle(0, 0, GraphicsDevice.Viewport.Width / 20, GraphicsDevice.Viewport.Height / 20);
            pinRectangleR = new Rectangle(0, 0, GraphicsDevice.Viewport.Width / 20, GraphicsDevice.Viewport.Height / 20);
            videoDisplayRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //this.animation = Content.Load<GifAnimation.GifAnimation>("Assets/mandoo");
            Background = Content.Load<Texture2D>(@"back");
            bone = Content.Load<Texture2D>(@"joint");
            pinTextureR = Content.Load<Texture2D>(@"pin");
            pinTextureL = Content.Load<Texture2D>(@"pin");
            //mask = Content.Load<Texture2D>(@"ironman");
            lineDot = Content.Load<Texture2D>(@"whiteDot");
            itemPop = Content.Load<SoundEffect>("Pop");
            logo = Content.Load<Texture2D>(@"human");
            screen = Content.Load<Texture2D>(@"screen");

            //Texture2D item_Texture = Content.Load<Texture2D>("Assets/2");
            item_Animation1 = Content.Load<GifAnimation.GifAnimation>("Assets/1");
            item_Animation2 = Content.Load<GifAnimation.GifAnimation>("Assets/2");
            item_Animation3 = Content.Load<GifAnimation.GifAnimation>("Assets/3");
            item_Animation4 = Content.Load<GifAnimation.GifAnimation>("Assets/4");
            item_Animation5 = Content.Load<GifAnimation.GifAnimation>("Assets/5");
            item_Animation6 = Content.Load<GifAnimation.GifAnimation>("Assets/6");
            item_Animation_bulb = Content.Load<GifAnimation.GifAnimation>("Assets/bulb04");

            for (int i = 0; i < noOfSprite; i++)
            {
                Vector2 Position1 = new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height));
                Vector2 Position2 = new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height));
                Vector2 Position3 = new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height));
                Vector2 Position4 = new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height));
                Vector2 Position5 = new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height));
                Vector2 Position6 = new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height));

                Vector2 Speed = new Vector2(i / 6f, 0);
                item it1 = new item(item_Animation1, Position1, Speed, itemPop,Color.White);
                item it2 = new item(item_Animation2, Position2, -Speed, itemPop,Color.White);
                item it3 = new item(item_Animation3, Position3, Speed, itemPop,Color.White);
                item it4 = new item(item_Animation4, Position4, -Speed, itemPop,Color.White);
                item it5 = new item(item_Animation5, Position5, -Speed, itemPop,Color.White);
                item it6 = new item(item_Animation6, Position6, Speed, itemPop,Color.White);
                
                gameSprite.Add(it1);
                gameSprite.Add(it2);
                gameSprite.Add(it3);
                gameSprite.Add(it4);
                gameSprite.Add(it5);
                gameSprite.Add(it6);
            }

                // TODO: use this.Content to load your game content here
            sensor = KinectSensor.KinectSensors[0];
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.SkeletonStream.Enable();
            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.Start();
        }

        Skeleton[] Skeletons = null;
        Skeleton activeSkeleton = null;

        int activeSkeletonNumber;

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            byte[] ColorData = null;

            using (ColorImageFrame ImageParam = e.OpenColorImageFrame())
            {
                if (ImageParam != null)
                {
                    if (ColorData == null)
                        ColorData = new byte[ImageParam.Width * ImageParam.Height * 4];
                    ImageParam.CopyPixelDataTo(ColorData);

                    KinectVideoTexture = new Texture2D(GraphicsDevice, ImageParam.Width, ImageParam.Height);

                    Color[] Bitmap = new Color[ImageParam.Width * ImageParam.Height];
                    Bitmap[0] = new Color(ColorData[2], ColorData[1], ColorData[0], 255);

                    int sourceOffset = 0;
                    for(int i = 0 ; i<Bitmap.Length;i++)
                    {
                        Bitmap[i] = new Color(ColorData[sourceOffset +2],ColorData[sourceOffset+1],ColorData[sourceOffset],255);
                        sourceOffset += 4;
                    }
                    KinectVideoTexture.SetData(Bitmap);
                }
            }

            using (SkeletonFrame SkelParam = e.OpenSkeletonFrame())
            {
                if (SkelParam != null)
                {
                    Skeletons = new Skeleton[SkelParam.SkeletonArrayLength];
                    SkelParam.CopySkeletonDataTo(Skeletons);
                }
            }

            activeSkeletonNumber = 0;

            if (Skeletons != null)
            {
                for (int i = 0; i < Skeletons.Length; i++)
                {
                    if (Skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        activeSkeletonNumber = i + 1;
                        activeSkeleton = Skeletons[i];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keystate = Keyboard.GetState();
            if (keystate.IsKeyDown(Keys.Escape))
            {
                sensor.Stop();
                this.Exit();
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            updatePin();
            
            item_Animation1.Update(gameTime.ElapsedGameTime.Ticks);
            item_Animation2.Update(gameTime.ElapsedGameTime.Ticks);
            item_Animation3.Update(gameTime.ElapsedGameTime.Ticks);
            item_Animation4.Update(gameTime.ElapsedGameTime.Ticks);
            item_Animation5.Update(gameTime.ElapsedGameTime.Ticks);
            item_Animation6.Update(gameTime.ElapsedGameTime.Ticks);
            item_Animation_bulb.Update(gameTime.ElapsedGameTime.Ticks);

            // TODO: Add your update logic here
            foreach (ISprite sprite in gameSprite)
            {
                sprite.Update(this);
            }

            //Decrement the delay by the number of seconds that have elapsed since
            //the last time that the Update method was called
            mFadeDelay -= gameTime.ElapsedGameTime.TotalSeconds;

            //If the Fade delays has dropped below zero, then it is time to 
            //fade in/fade out the image a little bit more.
            if (mFadeDelay <= 0)
            {
                //Reset the Fade delay
                mFadeDelay = .035;

                //Increment/Decrement the fade value for the image
                mAlphaValue += mFadeIncrement;

                //If the AlphaValue is equal or above the max Alpha value or
                //has dropped below or equal to the min Alpha value, then 
                //reverse the fade
                if (mAlphaValue >= 255 || mAlphaValue <= 0)
                {
                    mFadeIncrement *= -1;
                }
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (KinectVideoTexture != null)
            {
                //spriteBatch.Draw(KinectVideoTexture, videoDisplayRectangle, Color.White);
                if (Skeletons != null)
                {
                    foreach (Skeleton s in Skeletons)
                    {
                        if (s.TrackingState == SkeletonTrackingState.Tracked)
                            DrawSkeleton(s, Color.Blue);
                       
                    }
                }

                //spriteBatch.Draw(Background, videoDisplayRectangle, Color.White);
            }

            foreach (ISprite sprite in gameSprite)
            {
                sprite.Draw(this);
            }
            //spriteBatch.Draw(this.animation.GetTexture(), new Vector2(0, 0), Color.White);

            //spriteBatch.Draw(pinTextureL, pinRectangleL, Color.Red);
            //spriteBatch.Draw(pinTextureR, pinRectangleR, Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        

        void Drawline(Vector2 v1,Vector2 v2,Color color)
        {
            Vector2 origin = new Vector2(0.5f, 0.0f);
            Vector2 point = v2 - v1;
            float angle;

            Vector2 scale = new Vector2(1.0f, point.Length() / bone.Height);
            angle = (float)(Math.Atan2(point.Y, point.X)) - MathHelper.PiOver2;
            spriteBatch.Draw(lineDot, v1, null, color, angle, origin, scale, SpriteEffects.None, 1.0f);
        }

        void DrawBone(Joint j1,Joint j2,Color color)
        {
            ColorImagePoint j1p = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            Vector2 j1v = new Vector2(j1p.X, j1p.Y);

            ColorImagePoint j2p = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            Vector2 j2v = new Vector2(j2p.X, j2p.Y);

            Drawline(j1v, j2v, color);
        }

        void DrawSkeleton(Skeleton Skeletons,Color color)
        {
            DrawBone(Skeletons.Joints[JointType.Head], Skeletons.Joints[JointType.ShoulderCenter], color);
            DrawBone(Skeletons.Joints[JointType.ShoulderCenter], Skeletons.Joints[JointType.Spine], color);

            DrawBone(Skeletons.Joints[JointType.Spine], Skeletons.Joints[JointType.HipCenter], color);
            DrawBone(Skeletons.Joints[JointType.HipCenter], Skeletons.Joints[JointType.HipRight], color);
            DrawBone(Skeletons.Joints[JointType.HipRight], Skeletons.Joints[JointType.KneeRight], color);
            DrawBone(Skeletons.Joints[JointType.KneeRight], Skeletons.Joints[JointType.AnkleRight], color);
            DrawBone(Skeletons.Joints[JointType.AnkleRight], Skeletons.Joints[JointType.FootRight], color);

            DrawBone(Skeletons.Joints[JointType.HipCenter], Skeletons.Joints[JointType.HipLeft], color);
            DrawBone(Skeletons.Joints[JointType.HipLeft], Skeletons.Joints[JointType.KneeLeft], color);
            DrawBone(Skeletons.Joints[JointType.KneeLeft], Skeletons.Joints[JointType.AnkleLeft], color);
            DrawBone(Skeletons.Joints[JointType.AnkleLeft], Skeletons.Joints[JointType.FootLeft], color);

            DrawBone(Skeletons.Joints[JointType.ShoulderCenter], Skeletons.Joints[JointType.ShoulderRight], color);
            DrawBone(Skeletons.Joints[JointType.ShoulderRight], Skeletons.Joints[JointType.ElbowRight], color);
            DrawBone(Skeletons.Joints[JointType.ElbowRight], Skeletons.Joints[JointType.WristRight], color);
            DrawBone(Skeletons.Joints[JointType.WristRight], Skeletons.Joints[JointType.HandRight], color);

            DrawBone(Skeletons.Joints[JointType.ShoulderCenter], Skeletons.Joints[JointType.ShoulderLeft], color);
            DrawBone(Skeletons.Joints[JointType.ShoulderLeft], Skeletons.Joints[JointType.ElbowLeft], color);
            DrawBone(Skeletons.Joints[JointType.ElbowLeft], Skeletons.Joints[JointType.WristLeft], color);
            DrawBone(Skeletons.Joints[JointType.WristLeft], Skeletons.Joints[JointType.HandLeft], color);

            ColorImagePoint handRightPoint = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(Skeletons.Joints[JointType.HandRight].Position,ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint CenterPoint = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(Skeletons.Joints[JointType.ShoulderCenter].Position, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint handLeftPoint = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(Skeletons.Joints[JointType.HandLeft].Position, ColorImageFormat.RgbResolution640x480Fps30);

            RightRectangle = new Rectangle(handRightPoint.X, handRightPoint.Y, 150, 150);
            LeftRectangle = new Rectangle(handLeftPoint.X, handLeftPoint.Y, 200, 200);

            if (handRightPoint.Y < CenterPoint.Y)
            {
                spriteBatch.Draw(item_Animation_bulb.GetTexture(), new Vector2(handRightPoint.X-50,handRightPoint.Y-70), Color.White);
            }
            if (handLeftPoint.Y < CenterPoint.Y)
            {
                spriteBatch.Draw(logo, new Vector2(handLeftPoint.X - 50, handLeftPoint.Y - 70), Color.White);
            }
            if (Math.Abs(handLeftPoint.Y - handRightPoint.Y) > 10 && Math.Abs(handLeftPoint.X - handRightPoint.X) > 10)
            {
                spriteBatch.Draw(screen, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.Lerp(Color.White, Color.Transparent, mAlphaValue));
            }


            //spriteBatch.Draw(mask, maskRectangle, Color.White);
        }

        Texture2D pinTextureR;
        Texture2D pinTextureL;
        Rectangle pinRectangleR;
        Rectangle pinRectangleL;

        public int pinRX, pinRY;
        public int pinLX, pinLY;

        JointType pinJointR = JointType.HandRight;
        JointType pinJointL = JointType.HandLeft;

        void updatePin()
        {
            if (activeSkeletonNumber == 0)
            {
                pinRX = -100;
                pinRY = -100;
                pinLX = -100;
                pinLY = -100;
            }
            else
            {
                Joint jointR = activeSkeleton.Joints[pinJointR];
                Joint jointL = activeSkeleton.Joints[pinJointL];

                ColorImagePoint pinPointL = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(jointL.Position, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint pinPointR = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(jointR.Position, ColorImageFormat.RgbResolution640x480Fps30);

                pinLX = pinPointL.X;
                pinLY = pinPointL.Y;
                pinRX = pinPointR.X;
                pinRY = pinPointR.Y;
            }

            PinVectorL.X = pinLX;
            PinVectorL.Y = pinLY;

            PinVectorR.X = pinRX;
            PinVectorR.Y = pinRY;

            pinRectangleL.X = pinLX - pinRectangleL.Width / 2;
            pinRectangleL.Y = pinLY - pinRectangleL.Height / 2;

            pinRectangleR.X = pinRX - pinRectangleR.Width / 2;
            pinRectangleR.Y = pinRY - pinRectangleR.Height / 2;
        }

        
    }
}
