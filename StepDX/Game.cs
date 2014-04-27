using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public partial class Game : Form
    {
        /// <summary>
        /// The DirectX device we will draw on
        /// </summary>
        private Device device = null;

        /// <summary> 
        /// Height of our playing area (meters)
        /// </summary>
        private float playingH = 4;

        /// <summary>
        /// Width of our playing area (meters)
        /// </summary>
        private float playingW = 7.2f;

        /// <summary>
        /// Vertex buffer for our drawing
        /// </summary>
        private VertexBuffer vertices = null;

        /// <summary>
        /// The background image class
        /// </summary>
        private Background background = null;

        /// <summary>
        /// All of the enemies in the game
        /// </summary>
        List<Polygon> enemies = new List<Polygon>();

        /// <summary>
        /// All of the lasers in play
        /// </summary>
        List<Polygon> lasers = new List<Polygon>();

        /// <summary>
        /// All of the explosions
        /// </summary>
        List<Polygon> explosions = new List<Polygon>();

        /// <summary>
        /// Our player sprite
        /// </summary>
        GameSprite player = new GameSprite();

        /// <summary>
        /// The collision testing subsystem
        /// </summary>
        Collision collision = new Collision();

        /// <summary>
        /// The sounds
        /// </summary>
        private GameSounds sounds;
        
        /// <summary>
        /// What the last time reading was
        /// </summary>
        private long lastTime;

        private long lastShot;

        private long lastSpawn;

        public float difficulty = 0;

        private bool gameOver = false;

        private int lives = 3;

        private bool tookLife = false;

        private float waitTime = 0;

        Random random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// A stopwatch to use to keep track of time
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// Font variable to display score
        /// </summary>
        private Microsoft.DirectX.Direct3D.Font font;
        private Microsoft.DirectX.Direct3D.Font gameOverFont;
        private Microsoft.DirectX.Direct3D.Font endScoreFont;
        int score = 0;
        
        /// <summary>
        /// Initialize the Direct3D device for rendering
        /// </summary>
        /// <returns>true if successful</returns>
        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        //TODO: Add up and down buttons for movement
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (!gameOver)
            {
                if (e.KeyCode == Keys.Escape)
                    this.Close(); // Esc was pressed
                else if (e.KeyCode == Keys.Right)
                {
                    Vector2 v = player.V;
                    v.X = 2.5f;
                    player.V = v;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    Vector2 v = player.V;
                    v.X = -2.5f;
                    player.V = v;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    Vector2 v = player.V;
                    v.Y = 2.5f;
                    player.V = v;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    Vector2 v = player.V;
                    v.Y = -2.5f;
                    player.V = v;
                }
                else if (e.KeyCode == Keys.Space && stopwatch.ElapsedMilliseconds > lastShot + 400)
                {
                    //TODO: Make the player shoot
                    AddLaser(player.P);
                    sounds.Shoot();
                    lastShot = stopwatch.ElapsedMilliseconds;
                }
            }
            else if (e.KeyCode == Keys.Escape)
                this.Close(); // Exit
        }

        //TODO: Make this work for up and down for the player as well
        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (!gameOver)
            {
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
                {
                    Vector2 v = player.V;
                    v.X = 0;
                    player.V = v;
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    Vector2 v = player.V;
                    v.Y = 0;
                    player.V = v;
                }
            }
        }

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D())
                return;

            vertices = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
                                        4,      // How many
                                        device, // What device
                                        0,      // No special usage
                                        CustomVertex.PositionColored.Format,
                                        Pool.Managed);

            background = new Background(device, playingW+2, playingH);
            sounds = new GameSounds(this);

            // Determine the last time
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;

            if (!gameOver)
            {
                Texture spritetexture = TextureLoader.FromFile(device, "../../ship.bmp");
                player.Tex = spritetexture;

                float L = 0.225f;
                float Y = 0.05f;

                player.AddVertex(new Vector2(-L, L));
                player.AddTex(new Vector2(Y, 0));

                player.AddVertex(new Vector2(0, L));
                player.AddTex(new Vector2(0.5f, 0));

                player.AddVertex(new Vector2(L, 0.005f));
                player.AddTex(new Vector2(1.0f - Y, 0.5f - 0.005f));

                player.AddVertex(new Vector2(L, -0.005f));
                player.AddTex(new Vector2(1.0f - Y, 0.5f + 0.005f));

                player.AddVertex(new Vector2(0, -L));
                player.AddTex(new Vector2(0.5f, 1));

                player.AddVertex(new Vector2(-L, -L));
                player.AddTex(new Vector2(Y, 1));

                player.Color = Color.Transparent;
                player.Transparent = true;
                player.P = new Vector2(2f, 2);
                player.T = GameSprite.SpriteType.Player;
            }

            //setting up the font
            font = new Microsoft.DirectX.Direct3D.Font(device,  // Device we are drawing on
                20,         // Font height in pixels
                0,          // Font width in pixels or zero to match height
                FontWeight.Bold,    // Font weight (Normal, Bold, etc.)
                0,          // mip levels (0 for default)
                false,      // italics?
                CharacterSet.Default,   // Character set to use
                Precision.Default,      // The font precision, try some of them...
                FontQuality.Default,    // Quality?
                PitchAndFamily.FamilyDoNotCare,     // Pitch and family, we don't care
                "Terminal");               // And the name of the font

            gameOverFont = new Microsoft.DirectX.Direct3D.Font(device, 100, 0, FontWeight.Bold, 0, false, CharacterSet.Default,
                Precision.Default, FontQuality.Default, PitchAndFamily.FamilyDoNotCare, "Terminal");
            endScoreFont = new Microsoft.DirectX.Direct3D.Font(device, 30, 0, FontWeight.Bold, 0, false, CharacterSet.Default,
                Precision.Default, FontQuality.Default, PitchAndFamily.FamilyDoNotCare, "Terminal");
        }


        //TODO: Need to render enemies at certain times. Don't know when, but probably need to put that here or in Advance
        public void Render()
        {
            //This is the soundtrack. This can be changed if necessary
            //sounds.Soundtrack();
            if (device == null)
                return;

            device.Clear(ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);

            int wid = Width;                            // Width of our display window
            int hit = Height;                           // Height of our display window.
            float aspect = (float)wid / (float)hit;     // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false;   // We'll not use this feature
            device.RenderState.Lighting = false;        // Or this one...
            device.RenderState.CullMode = Cull.None;    // Or this one...

            float widP = playingH * aspect;         // Total width of window
            
            float winCenter = player.P.X;
            winCenter = widP / 2;

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP / 2,
                                                                  winCenter + widP / 2,
                                                                  0, playingH, 0, 1);

            //Begin the scene
            device.BeginScene();

            // Render the background
            background.Render();

            //game over
            if (gameOver)
            {
                //210 and 100 are rough offsets for the the length of 'game over'
                gameOverFont.DrawText(null, "GAME OVER", new Point((int)wid/2 - 210, (int)hit/2 - 100), Color.WhiteSmoke);
                endScoreFont.DrawText(null, "Final Score: " + score, new Point((int)wid / 2 - 115, (int)hit / 2 + 30), Color.WhiteSmoke);
            }

            else
            {
                //score display
                font.DrawText(null,     // Because I say so
                            "Score: " + score,  // Text to draw
                            new Point(25, 15),  // Location on the display (pixels with 0,0 as upper left)
                            Color.WhiteSmoke);   // Font color

                //lives display
                font.DrawText(null, "Lives: " + lives, new Point(25, 40), Color.WhiteSmoke);

                foreach (Polygon p in explosions)
                    p.Render(device);

                foreach (Polygon p in lasers)
                {
                    p.Render(device);
                }

                foreach (Polygon p in enemies)
                    p.Render(device);

                if (!tookLife) player.Render(device);
                //flash player sprite when hit by enemy
                else if (waitTime % 10 == 0) player.Render(device);
            }

            //End the scene
            device.EndScene();
            device.Present();
        }

        /// <summary>
        /// Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            long time = stopwatch.ElapsedMilliseconds;
            float delta = (time - lastTime) * 0.001f;       // Delta time in milliseconds
            lastTime = time;

            //invincible for a second or so
            if (tookLife && waitTime < 80) waitTime += 1;
            else
            {
                waitTime = 0;
                tookLife = false;
            }

            List<Polygon> keepLaser = new List<Polygon>();
            List<Polygon> keepEnemy = new List<Polygon>();
            List<Polygon> keepExp = new List<Polygon>();

            foreach (GameSprite p in lasers)
            {
                if(p.P.X < 7.6)
                    keepLaser.Add(p);
            }

            lasers = keepLaser;

            foreach (GameSprite p in enemies)
            {
                if(p.P.X > 0)
                    keepEnemy.Add(p);
            }

            enemies = keepEnemy;

            foreach (Explosion p in explosions)
            {
                if (!p.getRidOf)
                    keepExp.Add(p);
            }

            explosions = keepExp;

            Vector2 q = player.V;
            Vector2 r = player.P;

            difficulty+=delta/150;

            if (stopwatch.ElapsedMilliseconds > (lastSpawn + 750-difficulty*50))
            {
                float randomNumber = random.Next(50,400);
                int randomType = random.Next(1, 5);

                AddEnemy(new Vector2(7, randomNumber/100), randomType);

                lastSpawn = stopwatch.ElapsedMilliseconds;
            }

            //These added borders, is there a better way to do this?
            if (player.P.X < 0.1f)
            {
                q.X = 0;
                r.X = 0.1f;
            }
            else if (player.P.X > playingW - 0.25f)
            {
                q.X = 0;
                r.X = playingW-0.25f;
            }
            if (player.P.Y < 0.1f)
            {
                q.Y = 0;
                r.Y = 0.1f;
            }
            else if (player.P.Y > playingH - 0.1f)
            {
                q.Y = 0;
                r.Y = playingH - 0.1f;
            }

            player.P = r;
            player.V = q;
            player.Advance(0);

            while (delta > 0)
            {
                float step = delta;
                if (step > 0.05f)
                    step = 0.05f;

                float maxspeed = Math.Max(Math.Abs(player.V.X), Math.Abs(player.V.Y));
                if (maxspeed > 0)
                {
                    step = (float)Math.Min(step, 0.05 / maxspeed);
                }

                player.Advance(step);

                foreach (Polygon p in lasers)
                    p.Advance(step);

                foreach (Polygon p in enemies)
                    p.Advance(step);

                foreach (Polygon p in explosions)
                    p.Advance(step);

                foreach (Polygon p in enemies)
                {
                    if (collision.Test(player, p))
                    {
                        //player hit an enemy, take a life if not invincible
                        if (!tookLife)
                        {
                            lives -= 1;
                            tookLife = true;
                            sounds.Explosion();
                        }
                        //you died
                        if(lives < 1) gameOver = true;
                        player.Advance(0);
                    }
                }


                List<Polygon> newlasers = new List<Polygon>();
                List<Polygon> tempenemies = enemies;
                bool hit = false;

                foreach (Polygon f in lasers)
                {
                    List<Polygon> newenemies = new List<Polygon>();
                    hit = false;
                    if (tempenemies.Count() > 0)
                    {
                        foreach (GameSprite p in tempenemies)
                        {
                            if (collision.Test(f, p))
                            {
                                // Score a collision with p
                                // and we won't need this laser anymore.
                                switch (p.T)
                                {
                                    case GameSprite.SpriteType.One:
                                        score += 100;
                                        Explosion(p.P);
                                        break;
                                    case GameSprite.SpriteType.Two:
                                        score += 100;
                                        Explosion(p.P);
                                        break;
                                    case GameSprite.SpriteType.Three:
                                        score += 250;
                                        Explosion(p.P);
                                        break;
                                    case GameSprite.SpriteType.Four:
                                        if (p.health <= 1)
                                        {
                                            score += 300;
                                            Explosion(p.P);
                                            break;
                                        }
                                        else
                                        {
                                            p.health -= 1;
                                            newenemies.Add(p);
                                            break;
                                        }
                                    default:
                                        score += 0;
                                        break;
                                }
                                hit = true;
                                sounds.Explosion();
                            }
                            else
                            {
                                // Otherwise, we still need the enemy
                                newenemies.Add(p);
                            }
                        }
                    }
                    if (!hit)
                        newlasers.Add(f);
                    tempenemies = newenemies;
                }

                lasers = newlasers;
                enemies = tempenemies;

                
                delta -= step;
            }
        }


        public void AddLaser(Vector2 p)
        {
            float left = 0;
            float right = 0.1f;
            float top = 0.1f;
            float bottom = 0;
            GameSprite obs = new GameSprite();

            Texture lasertexture = TextureLoader.FromFile(device, "../../redLaser.bmp");
            obs.Tex = lasertexture;

            obs.AddVertex(new Vector2(left, top));
            obs.AddTex(new Vector2(0, 0));
            obs.AddVertex(new Vector2(right, top));
            obs.AddTex(new Vector2(1, 0));
            obs.AddVertex(new Vector2(right, bottom));
            obs.AddTex(new Vector2(1, 1));
            obs.AddVertex(new Vector2(left, bottom));
            obs.AddTex(new Vector2(0, 1));
            obs.Color = Color.Transparent;
            obs.Transparent = true;
            Vector2 q = p;
            q.Y -= 0.05f;
            q.X += 0.25f;
            obs.P = q;
            obs.V = new Vector2(3, 0);
            lasers.Add(obs);
        }


        public void AddEnemy(Vector2 p, int type)
        {
            GameSprite enemy = new GameSprite();

            Texture enemytexture;
            switch (type)
            {
                case 2:
                    enemy.V = new Vector2(-1.3f - difficulty, 0);
                    enemy.AddVertex(new Vector2(-0.2f, 0.09f));
                    enemy.AddVertex(new Vector2(-0.065f, 0.15f));
                    enemy.AddVertex(new Vector2(0.065f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0.09f));
                    enemy.AddVertex(new Vector2(0.2f, -0.15f));
                    enemy.AddVertex(new Vector2(-0.2f, -0.15f));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy2.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0.2f));
                    enemy.AddTex(new Vector2(0.32f, 0.1f));
                    enemy.AddTex(new Vector2(0.64f, 0.1f));
                    enemy.AddTex(new Vector2(1, 0.2f));
                    enemy.AddTex(new Vector2(1, 1));
                    enemy.AddTex(new Vector2(0, 1));
                    enemy.T = GameSprite.SpriteType.Two;
                    break;
                case 3:
                    enemy.V = new Vector2(-0.8f - difficulty, 0);
                    enemy.AddVertex(new Vector2(-0.2f, 0.07f));
                    enemy.AddVertex(new Vector2(-0.07f, 0.2f));
                    enemy.AddVertex(new Vector2(0.07f, 0.2f));
                    enemy.AddVertex(new Vector2(0.2f, 0.07f));
                    enemy.AddVertex(new Vector2(0.2f, -0.2f));
                    enemy.AddVertex(new Vector2(-0.2f, -0.2f));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy3.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0.36f));
                    enemy.AddTex(new Vector2(0.36f, 0.06f));
                    enemy.AddTex(new Vector2(0.64f, 0.06f));
                    enemy.AddTex(new Vector2(1, 0.36f));
                    enemy.AddTex(new Vector2(1, 1));
                    enemy.AddTex(new Vector2(0, 1));
                    enemy.T = GameSprite.SpriteType.Three;
                    break;
                case 4:
                    enemy.V = new Vector2(-0.5f - difficulty, 0);
                    enemy.AddVertex(new Vector2(-0.2f, 0));
                    enemy.AddVertex(new Vector2(-0.05f, 0.15f));
                    enemy.AddVertex(new Vector2(0.05f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0));
                    enemy.AddVertex(new Vector2(0.15f, -0.05f));
                    enemy.AddVertex(new Vector2(-0.15f, -0.05f));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy4.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0.55f));
                    enemy.AddTex(new Vector2(0.32f, 0.14f));
                    enemy.AddTex(new Vector2(0.68f, 0.14f));
                    enemy.AddTex(new Vector2(1, 0.55f));
                    enemy.AddTex(new Vector2(1, 0.8f));
                    enemy.AddTex(new Vector2(0, 0.8f));
                    enemy.T = GameSprite.SpriteType.Four;
                    break;
                default:
                    float newDifficulty = (float)difficulty * 1.5f;
                    enemy.V = new Vector2(-3.5f - newDifficulty, 0);
                    enemy.AddVertex(new Vector2(-0.2f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0));
                    enemy.AddVertex(new Vector2(0.15f, -0.15f));
                    enemy.AddVertex(new Vector2(-0.15f, -0.15f));
                    enemy.AddVertex(new Vector2(-0.2f, 0));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy1.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0));
                    enemy.AddTex(new Vector2(1, 0));
                    enemy.AddTex(new Vector2(1, 0.5f));
                    enemy.AddTex(new Vector2(0.86f, 0.93f));
                    enemy.AddTex(new Vector2(0.14f, 0.93f));
                    enemy.AddTex(new Vector2(0, 0.5f));
                    enemy.T = GameSprite.SpriteType.One;
                    break;
            }

            enemy.Color = Color.Transparent;
            enemy.Transparent = true;
            enemy.P = p;
            enemies.Add(enemy);
        }

        public void Explosion(Vector2 p)
        {
            float left = -0.2f;
            float right = 0.2f;
            float top = 0.2f;
            float bottom = -0.2f;
            Explosion exp = new Explosion();

            Texture exptexture = TextureLoader.FromFile(device, "../../explosion.bmp");
            exp.Tex = exptexture;

            exp.AddVertex(new Vector2(left, bottom));
            exp.AddVertex(new Vector2(left, top));
            exp.AddVertex(new Vector2(right, top));
            exp.AddVertex(new Vector2(right, bottom)); 
            exp.AddTex(new Vector2(0, 1));
            exp.AddTex(new Vector2(0, 0));
            exp.AddTex(new Vector2(1, 0));
            exp.AddTex(new Vector2(1, 1));
            exp.Color = Color.Transparent;
            exp.Transparent = true;
            exp.P = p;
            explosions.Add(exp);
        }
    }
}
