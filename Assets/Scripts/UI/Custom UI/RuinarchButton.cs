using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Ruinarch.Custom_UI {
    public class RuinarchButton : UnityEngine.UI.Button {
        
        private UIShiny shineEffect;

        #region Monobehaviours
        protected override void Awake() {
            base.Awake();
            shineEffect = GetComponent<UIShiny>();
            if (shineEffect != null) {
                shineEffect.Stop();
            }
        }
        protected override void OnEnable() {
            base.OnEnable();
            if (Application.isPlaying) {
                Messenger.AddListener<string>(Signals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
                Messenger.AddListener<string>(Signals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
                if (InputManager.Instance.ShouldBeHighlighted(this)) {
                    StartGlow();
                }
            }
        }
        protected override void OnDisable() {
            base.OnDisable();
            if (Application.isPlaying) {
                Messenger.RemoveListener<string>(Signals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
                Messenger.RemoveListener<string>(Signals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
                HideGlow();
            }
        }
        #endregion

        #region Pointer Clicks
        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            if (!IsInteractable())
                return;
            Messenger.Broadcast(Signals.BUTTON_CLICKED, this);
        }
        #endregion

        #region Shine
        private void StartGlow() {
            if (shineEffect != null) {
                shineEffect.Play();
            }
        }
        private void HideGlow() {
            if (shineEffect != null) {
                shineEffect.Stop();
            }
        }
        private void OnReceiveShowGlowSignal(string buttonName) {
            if (name == buttonName) {
                StartGlow();
            }
        }
        private void OnReceiveHideGlowSignal(string buttonName) {
            if (name == buttonName) {
                HideGlow();
            }
        }
        #endregion
        
        
    }
}