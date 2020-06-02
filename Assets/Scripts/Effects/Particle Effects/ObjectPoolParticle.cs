using EZObjectPools;
using UnityEngine;
namespace Particle_Effects {
    public class ObjectPoolParticle: PooledObject {
        [SerializeField] private ParticleSystem[] particles;
        
        public override void Reset() {
            base.Reset();
            for (int i = 0; i < particles.Length; i++) {
                ParticleSystem p = particles[i];
                p.Clear();
            }
        }
    }
}