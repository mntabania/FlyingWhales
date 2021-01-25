using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Ruinarch.Custom_UI {
    public class RuinarchDropdown : TMP_Dropdown {

        private Canvas m_mainDropdownCanvas;

        private bool isBlockerActive;
        
        protected override void Awake() {
            base.Awake();
            m_mainDropdownCanvas = GetOrAddComponent<Canvas>(gameObject);
        }
        protected override GameObject CreateBlocker(Canvas rootCanvas) {
            GameObject created = base.CreateBlocker(rootCanvas);
            if (created != null) {
                isBlockerActive = true;
                m_mainDropdownCanvas.overrideSorting = true;
                m_mainDropdownCanvas.sortingOrder = 30000;
            }
            return created;
        }
        protected override void DestroyBlocker(GameObject blocker) {
            base.DestroyBlocker(blocker);
            m_mainDropdownCanvas.overrideSorting = false;
            isBlockerActive = false;
        }
        public override void OnPointerClick(PointerEventData eventData) {
            // base.OnPointerClick(eventData);
            if (isBlockerActive) {
                Hide();
            } else {
                Show();
            }
        }
        private static T GetOrAddComponent<T>(GameObject go) where T : Component {
            T comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }
    }
}