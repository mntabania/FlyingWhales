﻿using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ruinarch.Custom_UI {
    public class RuinarchButton : UnityEngine.UI.Button {
        
        private UIShiny shineEffect;

        #region Monobehaviours
        protected override void Awake() {
            base.Awake();
            if (Application.isPlaying) {
                shineEffect = GetComponent<UIShiny>();
                if (shineEffect == null) {
                    shineEffect = targetGraphic.gameObject.GetComponent<UIShiny>();
                }
                if (shineEffect != null) {
                    shineEffect.Stop();
                }
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

        #region Pointer Clicks
        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            if (!IsInteractable())
                return;
            Messenger.Broadcast(UISignals.BUTTON_CLICKED, this);
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