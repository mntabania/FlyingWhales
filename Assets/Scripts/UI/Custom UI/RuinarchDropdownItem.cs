using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Ruinarch.Custom_UI {
    [RequireComponent(typeof(HoverHandler))]
    public class RuinarchDropdownItem : MonoBehaviour {
        [SerializeField] private HoverHandler _hoverHandler;
        [SerializeField] private TMP_Dropdown _parentDropdown;
        private void Awake() {
            _hoverHandler.AddOnHoverOverAction(OnHoverOver);
            _hoverHandler.AddOnHoverOutAction(OnHoverOut);
        }
        private void OnDestroy() {
            _hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
            _hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
        }

        private void OnHoverOver() {
            int index = transform.GetSiblingIndex() - 1;
            if (index != -1) {
                Messenger.Broadcast(UISignals.DROPDOWN_ITEM_HOVERED_OVER, _parentDropdown, index); //-1 since 0 is occupied by the dropdown template item    
            }
        }
        private void OnHoverOut() {
            int index = transform.GetSiblingIndex() - 1;
            if (index != -1) {
                Messenger.Broadcast(UISignals.DROPDOWN_ITEM_HOVERED_OUT, _parentDropdown, index); //-1 since 0 is occupied by the dropdown template item    
            }
        }
    }
}