using UnityEngine;

public class BuildStructureParticleEffect : BaseParticleEffect {
    [SerializeField] private ParticleSystem _particles;

    public override void SetSize(Vector2Int p_size) {
        ParticleSystem.ShapeModule shapeModule = _particles.shape; 
        shapeModule.scale = new Vector3(p_size.x, p_size.y, 1f);
        ParticleSystem.MainModule mainModule = _particles.main;
        int maxParticles = p_size.x * p_size.y + 5;
        mainModule.maxParticles = maxParticles;
        ParticleSystem.EmissionModule emissionModule = _particles.emission;
        emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve((float)maxParticles / 5);
    }
}
