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
    public class ParticleManager
    {
        protected List<Particle> particles = new List<Particle>();
        public List<Particle> Particles { get { return particles; } }

        protected ContentManager content;
        protected Random random;

        public ParticleManager()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public virtual void Initialize() { }
        public virtual void Load(ContentManager cm) { content = cm; }

        public virtual void Update(GameTime gt)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].IsActive == true)
                    particles[i].Update(gt);
                else
                {
                    onParticleDeath?.Invoke(particles[i]);
                    particles.Remove(particles[i]);
                    i--;
                }
            }
        }
        public virtual void DrawGroup(SpriteBatch sb, string groupID)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].GroupID == groupID.ToUpper())
                    particles[i].Draw(sb);
            }
        }

        #region Methods

        public void Add(string groupID, Particle particle)
        {
            particle.GroupID = groupID.ToUpper();
            particle.Particles = this;

            particle.Initialize();
            particle.Load(content);

            particles.Add(particle);
            onAddParticle?.Invoke(particle);
        }
        public void Add(string groupID, int quantity, Func<Particle> particle, Action<Particle> onSpawn)
        {
            for (int i = 0; i < quantity; i++)
            {
                Particle p = particle?.Invoke();
                Add(groupID, p);
                onSpawn?.Invoke(p);
            }
        }
        public void Add(string groupID, int quantity, Func<Particle> particle) { Add(groupID, quantity, particle, null); }

        public void Clear()
        {
            Particle.AdjustParticleCount(-particles.Count);
            particles.Clear();
        }
        public void Clear(string groupID)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].GroupID == groupID.ToUpper())
                {
                    particles.RemoveAt(i);
                    Particle.AdjustParticleCount(-1);

                    i--;
                }
            }
        }

        public Particle FromIndex(int index)
        {
            if (particles.Count >= index)
                return particles[index];
            return null;
        }
        /// <summary>
        /// Returns the first particle of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Particle First<T>() where T : Particle
        {
            return particles.OfType<T>().FirstOrDefault();
        }
        /// <summary>
        /// Returns the last particle of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Particle Last<T>() where T : Particle
        {
            return particles.OfType<T>().LastOrDefault();
        }

        /// <summary>
        /// Applies the action to all particles of the same group ID.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="action"></param>
        public void CallGroup(string groupID, Action<Particle> action)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].GroupID == groupID.ToUpper())
                    action?.Invoke(particles[i]);
            }
        }
        public void CallID(string id, Action<Particle> action)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].ID == id.ToUpper())
                    action?.Invoke(particles[i]);
            }
        }
        public void Call(Action<Particle> action)
        {
            for (int i = 0; i < particles.Count; i++)
                action?.Invoke(particles[i]);
        }

        #endregion

        #region Events

        private event Action<Particle> onAddParticle, onParticleDeath;
        public event Action<Particle> OnAddParticle { add { onAddParticle += value; } remove { onAddParticle -= value; } }
        public event Action<Particle> OnParticleDeath { add { onParticleDeath += value; } remove { onParticleDeath -= value; } }

        #endregion

        /*public void FreeRecycleMemory(GameTime gt, int maxInactivityTime = 60000, int delay = 100)
        {
            inactiveTimer += gt.ElapsedGameTime.Milliseconds;

            if (inactiveTimer >= maxInactivityTime)
            {
                if (recycled.Count > 0)
                {
                    recycled[0] = null;
                    recycled.RemoveAt(0);
                }

                inactiveTimer -= delay;
            }
        }
        public void ForceRecycleAll()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].ResetParticle(); //Recycle the particle's variables
                recycled.Add(particles[i]); //Add the particle to the recycle list
                particles.Remove(particles[i]); //Remove from current particle list
            }
        }*/
    }
}
