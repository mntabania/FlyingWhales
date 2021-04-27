using Coffee.UIExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ruinarch.Custom_UI {
    public class RuinarchButton : UnityEngine.UI.Button {
        
        private UIShiny shineEffect;
        private System.Action _onHoverOverAction;
        private System.Action _onHoverOutAction;
        private TextMeshProUGUI _lblBtnName;

        private bool _isUnavailable;

        #region Monobehaviours
        protected override void Awake() {
            base.Awake();
            if (Application.isPlaying) {
                shineEffect = GetComponent<UIShiny>();
                if (shineEffect == null) {
                    shineEffect = targetGraphic.gameObject.GetComponent<UIShiny>();
                }
                if (shineEffect != null) {
                    if(targetGraphic.gameObject.GetComponent<PlayShineEffectOnAwake>() == null) {
                        shineEffect.Stop();
                    } else {
                        shineEffect.Play();
                    }
                }
                _lblBtnName = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        protected override void OnEnable() {
            base.OnEnable();
            if (Application.isPlaying) {
                Messenger.AddListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
                Messenger.AddListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
                Messenger.AddListener<string>(UISignals.HOTKEY_CLICK, OnReceiveHotKeyClick);
                Messenger.Broadcast(UISignals.BUTTON_SHOWN, this);
                if (InputManager.Instance != null && InputManager.Instance.ShouldBeHighlighted(this)) {
                    StartGlow();
                }
            }
        }
        protected override void OnDisable() {
            base.OnDisable();
            if (Application.isPlaying) {
                Messenger.RemoveListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
                Messenger.RemoveListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
                Messenger.RemoveListener<string>(UISignals.HOTKEY_CLICK, OnReceiveHotKeyClick);
                HideGlow();
            }
        }
        #endregion

        public void MakeAvailable() {
            _isUnavailable = false;
        }
        public void MakeUnavailable() {
            _isUnavailable = true;
        }

        #region Pointer Clicks
        public override void OnPointerClick(PointerEventData eventData) {
			if (_isUnavailable) {
                AudioManager.Instance.OnErrorSoundPlay();
                //play error sound here
                return;
			}
            if (!IsInteractable())
                return;
            Messenger.Broadcast(UISignals.BUTTON_CLICKED, this);
            base.OnPointerClick(eventData);
            if (!IsActive() || !IsInteractable())
                return;
            _onHoverOverAction?.Invoke();
        }
        #endregion

        #region Hover
        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            if (!IsActive() || !IsInteractable())
                return;
            _onHoverOverAction?.Invoke();
        }
        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            if (!IsActive() || !IsInteractable())
                return;
            _onHoverOutAction?.Invoke();
        }
        public void AddHoverOverAction(System.Action p_hoverOverAction) {
            _onHoverOverAction += p_hoverOverAction;
        }
        public void AddHoverOutAction(System.Action p_hoverOutAction) {
            _onHoverOutAction += p_hoverOutAction;
        }
        public void RemoveHoverOverAction(System.Action p_hoverOverAction) {
            _onHoverOverAction -= p_hoverOverAction;
        }
        public void RemoveHoverOutAction(System.Action p_hoverOutAction) {
            _onHoverOutAction -= p_hoverOutAction;
        }
        #endregion

        #region Shine
        public void StartGlow() {
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
        public void ForceUpdateGlow() {
            if (InputManager.Instance != null && InputManager.Instance.ShouldBeHighlighted(this)) {
                StartGlow();
            }
        }
        #endregion

        #region Label
        public void SetButtonLabelName(string p_name) {
            if (_lblBtnName != null) {
                _lblBtnName.text = p_name;    
            }
        }
        #endregion
        
        private void OnReceiveHotKeyClick(string buttonName) {
            if (name == buttonName) {
                if (IsInteractable()) {
                    onClick?.Invoke();
                    Messenger.Broadcast(UISignals.BUTTON_CLICKED, this); 
                }
            }
        }
    }
}