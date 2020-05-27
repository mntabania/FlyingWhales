using UnityEngine.EventSystems;
namespace Ruinarch.Custom_UI {
    public class RuinarchButton : UnityEngine.UI.Button {
        
        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            if (!IsInteractable())
                return;
            Messenger.Broadcast(Signals.BUTTON_CLICKED, this);
        }
    }
}