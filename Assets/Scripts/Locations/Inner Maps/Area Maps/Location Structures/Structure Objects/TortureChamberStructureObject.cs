using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Inner_Maps.Location_Structures {
    public class TortureChamberStructureObject : LocationStructureObject {
        [Header("Specific Objects")]
        [SerializeField] private ParticleSystem _shroudParticles;

        public void ActivateShroudParticles() {
            _shroudParticles.Play();
        }
        public void DeactivateShroudParticles() {
            _shroudParticles.Stop();
        }
    }
}