using System;
using UnityEngine;
namespace UI.UI_Utilities {
    public class RectTransformEventDispatcher : MonoBehaviour {
        
        
        private System.Action<RectTransform> onRectTransformDimensionsChange;
        [SerializeField] private RectTransform _rectTransform;
        
        private void Awake() {
            if (_rectTransform == null) {
                _rectTransform = transform as RectTransform;
            }
        }
        private void OnRectTransformDimensionsChange() {
            onRectTransformDimensionsChange?.Invoke(_rectTransform);
        }

        public void Subscribe(System.Action<RectTransform> p_listener) {
            onRectTransformDimensionsChange += p_listener;
        }
        public void Unsubscribe(System.Action<RectTransform> p_listener) {
            onRectTransformDimensionsChange -= p_listener;
        }
        
        private void OnDestroy() {
            onRectTransformDimensionsChange = null;
        }
    }
}