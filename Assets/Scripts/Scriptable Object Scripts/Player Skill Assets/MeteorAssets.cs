using UnityEngine;
namespace Scriptable_Object_Scripts {
    [CreateAssetMenu(fileName = "New Meteor Assets", menuName = "Scriptable Objects/Player Spell Assets/Meteor Assets", order = 0)]
    public class MeteorAssets : PlayerSkillAssets {
        public AudioClip[] impactSounds;
    }
}