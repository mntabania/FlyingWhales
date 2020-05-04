using UnityEngine;
namespace Quests.Steps {
    public class LookAroundStep : QuestStep {

        private bool hasMovedUp;
        private bool hasMovedDown;
        private bool hasMovedLeft;
        private bool hasMovedRight;
        
        public LookAroundStep(string stepDescription = "Look Around") : base(stepDescription) { }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<Vector3>(Signals.CAMERA_MOVED_BY_PLAYER, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Vector3>(Signals.CAMERA_MOVED_BY_PLAYER, CheckForCompletion);
        }


        private void CheckForCompletion(Vector3 movement) {
            if (movement.x < 0f) {
                hasMovedLeft = true;
            } else if (movement.x > 0f) {
                hasMovedRight = true;
            }

            if (movement.y > 0f) {
                hasMovedUp = true;
            } else if (movement.y < 0f) {
                hasMovedDown = true;
            }

            if (hasMovedUp && hasMovedDown && hasMovedLeft && hasMovedRight) {
                Complete();
            }
            
        }
    }
}