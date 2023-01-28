using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleEngine
{
    public class Particle
    {
        public string ID { get; set; }
        public string GroupID { get; set; }
        public ParticleManager Particles { get; set; }

        protected Random random;

        #region Fields

        protected Vector2 position, velocity, origin, scale;
        protected float angle, depth;
        protected Color color, baseColor;
        protected Point currentFrame, frameSize, sheetSize;

        private bool isActive;

        #endregion

        #region Properties

        public Texture2D Texture { get; set; }

        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 Velocity { get { return velocity; } set { velocity = value; } }
        public Vector2 Origin { get { return origin; } set { origin = value; } }

        public Vector2 Scale { get { return scale; } set { scale = new Vector2(MathHelper.Max(value.X, 0), MathHelper.Max(value.Y, 0)); } }
        public float Angle { get { return angle; } set { angle = value; } }
        public float Depth { get { return depth; } set { depth = MathHelper.Clamp(value, 0, 1); } }

        public Color Color { get { return color; } set { color = value; } }
        public Color BaseColor { get { return baseColor; } set { baseColor = value; } }

        #endregion

        #region Life Variables

        public int CurrentTime { get; set; }
        public int MaxTime { get; set; }
        public void SetTime(int time) { CurrentTime = MaxTime = time; }

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value == true)
                    AdjustParticleCount(+1);
                else
                    AdjustParticleCount(-1);

                isActive = value;
            }
        }

        #endregion

        #region Animation

        private int animationTime;

        public bool IsPaused { get; set; } = false;

        public Point CurrentFrame { get { return currentFrame; } set { currentFrame = value; } }
        public int FrameSpeed { get; set; }
        public Point FrameSize { get { return frameSize; } set { frameSize = value; } }
        public Point SheetSize { get { return sheetSize; } set { sheetSize = value; } }

        public void SetCurrentFrame(Point frame)
        {
            currentFrame.X = MathHelper.Clamp(frame.X, 0, sheetSize.X - 1);
            currentFrame.Y = MathHelper.Clamp(frame.Y, 0, sheetSize.Y - 1);
        }
        public Rectangle Source { get; set; }

        #endregion

        #region Collision

        public float Radius { get; set; }

        public void MoveTo(Vector2 position, float speed)
        {
            Vector2 direction = position - this.position;
            if (direction != Vector2.Zero)
                direction.Normalize();

            velocity += direction * speed;
        }
        public bool Contains(Vector2 position)
        {
            return (Vector2.Distance(position, this.position) < Radius);
        }
        public bool Contains(Point position)
        {
            return (Vector2.Distance(position.ToVector2(), this.position) < Radius);
        }
        public bool Intersects(Vector2 position, float radius)
        {
            return (Radius + radius) > Vector2.Distance(this.position, position);
        }
        public bool Intersects(Point position, float radius)
        {
            return (Radius + radius) > Vector2.Distance(this.position, position.ToVector2());
        }

        #endregion

        #region Events

        private event Action onInitialize;
        private event Action<ContentManager> onLoad;
        private event Action<GameTime> onUpdate;

        public event Action OnInitialize { add { onInitialize += value; } remove { onInitialize -= value; } }
        public event Action<ContentManager> OnLoad { add { onLoad += value; } remove { onLoad -= value; } }
        public event Action<GameTime> OnUpdate { add { onUpdate += value; } remove { onUpdate -= value; } }

        #endregion

        #region Static Count

        private static readonly object particleLock = new object();
        private static int CurrentParticles;
        public static void AdjustParticleCount(int value)
        {
            lock (particleLock)
            {
                CurrentParticles += value;
            }
        }

        #endregion

        public Particle()
        {
            isActive = true;
            random = new Random(Guid.NewGuid().GetHashCode());
        }
        public Particle(Texture2D Texture, Vector2 Position, Vector2 Velocity, Vector2 Scale, float Angle, float Depth, Color Color, int LifeTime, Point FrameSize)
        {
            this.Texture = Texture;
            position = Position;
            velocity = Velocity;

            scale = Scale;
            angle = Angle;
            depth = Depth;

            color = Color;
            baseColor = Color;

            CurrentTime = LifeTime;
            MaxTime = LifeTime;

            IsActive = true;

            currentFrame = new Point(0, 0);
            frameSize = FrameSize;

            if (Texture != null)
            {
                if (frameSize == null || frameSize == Point.Zero)
                    frameSize = Texture.Bounds.Size;

                sheetSize = new Point(Texture.Width / frameSize.X, Texture.Height / frameSize.Y);

                Origin = new Vector2(frameSize.X / 2, frameSize.Y / 2);
                Radius = frameSize.X / 2;
            }
        }

        public virtual void Initialize() { onInitialize?.Invoke(); }
        public virtual void Load(ContentManager cm) { onLoad?.Invoke(cm); }

        public virtual void Update(GameTime gt)
        {
            UpdatePosition(gt);
            UpdateTime(gt);
            UpdateAnimation(gt);

            onUpdate?.Invoke(gt);
        }
        protected virtual void UpdatePosition(GameTime gt)
        {
            Position += Velocity * (float)gt.ElapsedGameTime.TotalSeconds;
        }
        protected virtual void UpdateTime(GameTime gt)
        {
            CurrentTime -= gt.ElapsedGameTime.Milliseconds;

            if (CurrentTime <= 0)
                IsActive = false;
        }
        protected virtual void UpdateAnimation(GameTime gt)
        {
            if (IsPaused == false)
            {
                if (sheetSize.X > 0 || sheetSize.Y > 0)
                {
                    animationTime += gt.ElapsedGameTime.Milliseconds;

                    if (animationTime > FrameSpeed)
                    {
                        currentFrame.X++;

                        if (currentFrame.X >= sheetSize.X)
                        {
                            currentFrame.X = 0;
                            currentFrame.Y++;

                            if (currentFrame.Y >= sheetSize.Y)
                                currentFrame.Y = 0;
                        }

                        animationTime = 0;
                        Source = new Rectangle(currentFrame.X * frameSize.X,
                                               currentFrame.Y * frameSize.Y,
                                               frameSize.X, frameSize.Y);
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch sb)
        {
            if (Texture != null)
                sb.Draw(Texture, Position, Source, Color, Angle, Origin, Scale, SpriteEffects.None, Depth);
        }

        public virtual Particle Copy()
        {
            Particle copy = (Particle)MemberwiseClone();

            copy.random = new Random(Guid.NewGuid().GetHashCode());

            return copy;
        }
    }
}
