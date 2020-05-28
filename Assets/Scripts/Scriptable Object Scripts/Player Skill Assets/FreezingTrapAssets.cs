using UnityEngine;
namespace Scriptable_Object_Scripts {
    [CreateAssetMenu(fileName = "New Freezing Trap Assets", menuName = "Scriptable Objects/Player Spell Assets/Freezing Trap Assets", order = 0)]
    public class FreezingTrapAssets : PlayerSkillAssets {
        public AudioClip placeTrapSound;
        public AudioClip trapExplosionSound;
    }
}