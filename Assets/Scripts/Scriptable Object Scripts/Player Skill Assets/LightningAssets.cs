using UnityEngine;
namespace Scriptable_Object_Scripts {
    [CreateAssetMenu(fileName = "New Lightning Assets", menuName = "Scriptable Objects/Player Spell Assets/Lightning Assets", order = 0)]
    public class LightningAssets : PlayerSkillAssets {
        public AudioClip[] thunderAudioClips;
    }
}