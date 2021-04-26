using UnityEngine;
namespace UtilityScripts {
    [System.Serializable]
    public class RuinarchBasicProgress : RuinarchProgressable {

        public override string progressableName => name;
        public override BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Progress_Bar;

        [SerializeField] private string name;
        
        public RuinarchBasicProgress(string p_name) {
            name = p_name;
        }

        public void Initialize(int p_minValue, int p_maxValue) {
            Setup(p_minValue, p_maxValue);
        }
        public void SetName(string p_name) {
            name = p_name;
            bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
        }
    }
}