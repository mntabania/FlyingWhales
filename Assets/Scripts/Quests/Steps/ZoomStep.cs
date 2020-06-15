using System;
using UnityEngine;
namespace Quests.Steps {
    public class ZoomStep : QuestStep {

        private bool _hasZoomedIn;
        private bool _hasZoomedOut;
        
        public ZoomStep(string stepDescription = "Zoom in and out") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoom);
        }
        
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoom);
        }
        
        private void OnCameraZoom(Camera camera, float amount) {
            if (amount < 0) {
                _hasZoomedOut = true;
            } else if (amount > 0) {
                _hasZoomedIn = true;
            }

            if (_hasZoomedIn && _hasZoomedOut) {
                Complete();
            }
        }
    }
}