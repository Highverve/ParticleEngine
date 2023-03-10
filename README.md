# ParticleEngine

This was an old library I wrote to help my gamedev cycle. I migrated the code to work with the latest versions of Visual Studio and MonoGame. At a glance, I already see areas of improvement—modifying particles by type, cleaner code consistency, different built-in particle types, etc.—although, I doubt I'll rework this anytime soon. Unfortunately, I had to remove the connected particles (seen below) due to needing a modified .dll of the MonoGame framework. It was an easy enough change to make in the SpriteBatch class, just add an override that accepts four vectors. Then you just link a particle's top vectors to a referenced particle's bottom vectors, and make sure they pull towards each other.


https://user-images.githubusercontent.com/119130949/215289037-684c01c4-9f26-42cd-abba-bd048b908300.mp4


### Setting up

It's fairly straightforward. Just make sure Initialize(), Load(), Update(), and DrawGroup() are called in their respective methods.

This is the example code to add particles:
```csharp
//pm = ParticleManager
//Parameters: string particleGroup, int quantity, Func<Particle> (the created particle), Action<Particle> on spawn.
pm.Add("World", 20, () => new Firework(ms.Position.ToVector2()), (p) =>
{
    Vector2 direction = new Vector2(random.Next(-10, 10), random.Next(-10, 10));
    if (direction != Vector2.Zero)
        direction.Normalize();
    p.Velocity = (direction * random.Next(-3000, 3000)) + (ms.Position - lastMs.Position).ToVector2() * 20;

    p.Color = Color.Lerp(Color.Crimson, Color.Gold, (float)random.NextDouble());
    p.SetTime(random.Next(1000, 2000));
});
```

Specific groups of particles can be influenced with the ParticleManager.CallGroup() method. It takes string group and Action<Particle>. The following code will all particles in the "World" group toward the player's mouse.
```csharp
if (ms.RightButton == ButtonState.Pressed)
    emitter.CallGroup("World", (p) => p.MoveTo(ms.Position.ToVector2(), 2000f * (float)gameTime.ElapsedGameTime.TotalSeconds));
```

And lastly, here's what the Firework particle code looks like:
```csharp
    public class Firework : Particle
    {
        public Firework(Vector2 Position) : base()
        {
            position = Position;

        }

        public override void Initialize()
        {
            Scale = new Vector2(1 + (float)random.NextDouble() * 2);
        }
        public override void Load(ContentManager cm)
        {
            Texture = cm.Load<Texture2D>("firework");

            FrameSize = new Point(16, 16);
            SheetSize = new Point(8, 1);
            FrameSpeed = 50;

            Origin = new Vector2(FrameSize.X / 2, FrameSize.Y / 2);
        }
        public override void Update(GameTime gt)
        {
            Angle += random.Next(1, 5) * .5f * (float)gt.ElapsedGameTime.TotalSeconds;

            if (CurrentTime >= 500 && CurrentTime <= 750)
                Scale += new Vector2(2.5f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime <= 500)
                Scale -= Scale * 5f * (float)gt.ElapsedGameTime.TotalSeconds;

            Velocity -= Velocity * 5f * (float)gt.ElapsedGameTime.TotalSeconds;
            velocity.Y += 500f * (float)gt.ElapsedGameTime.TotalSeconds;

            base.Update(gt);
        }
    }
```

The Firework uses a spritesheet texture, an all-white animation of a firework spark.


https://user-images.githubusercontent.com/119130949/215289464-d423ab8b-21de-44d8-9606-89cb937a9029.mp4
