using UnityEngine;
namespace Scriptable_Object_Scripts {
    [CreateAssetMenu(fileName = "New Splash Poison Assets", menuName = "Scriptable Objects/Player Spell Assets/Splash Poison Assets", order = 0)]
    public class SplashPoisonAssets : PlayerSkillAssets {
        public AudioClip[] splashSounds;
    }
}