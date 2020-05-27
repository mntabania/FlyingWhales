
using UnityEngine.EventSystems;
namespace Ruinarch.Custom_UI {
    public class RuinarchToggle : UnityEngine.UI.Toggle{
        
        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            Messenger.Broadcast(Signals.TOGGLE_CLICKED, this);    
        }
    }
}