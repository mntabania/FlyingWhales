using UnityEngine;
using UnityEngine.Video;
namespace Scriptable_Object_Scripts {
    [CreateAssetMenu(fileName = "New Player Spell Assets", menuName = "Scriptable Objects/Player Spell Assets/Default Player Skill Asset", order = 0)]
    public class PlayerSkillAssets : ScriptableObject {
        public VideoClip tooltipVideoClip;
        public Texture tooltipImage;
    }
}