using System.Collections.Generic;
using Ruinarch;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Quests.Steps {
    public abstract class QuestStep {

        public string stepDescription => GetStepDescription(); 
        public bool isCompleted { get; private set; }
        private System.Action onCompleteAction { get; set; }
        public System.Action<QuestStepItem> onHoverOverAction { get; private set; }
        public System.Action onHoverOutAction { get; private set; }
        /// <summary>
        /// List of selectables to cycle through when center button is clicked.
        /// </summary>
        public List<ISelectable> objectsToCenter { get; private set; }

        private readonly string _stepDescription;
        
        #region getters
        public bool hasHoverAction => onHoverOverAction != null || onHoverOutAction != null;
        #endregion

        protected QuestStep(string stepDescription) {
            _stepDescription = stepDescription;
            isCompleted = false;
        }

        #region Initialization
        /// <summary>
        /// Activate this quest step. This means that this step will start listening for its completion.
        /// </summary>
        public void Activate() {
            SubscribeListeners();
        }
        #endregion

        #region Listeners
        protected abstract void SubscribeListeners();
        protected abstract void UnSubscribeListeners();
        #endregion

        #region Completion
        protected void Complete() {
            if (isCompleted) { return; }
            isCompleted = true;
            Messenger.Broadcast(Signals.QUEST_STEP_COMPLETED, this);
            onCompleteAction?.Invoke();
        }
        public QuestStep SetCompleteAction(System.Action onCompleteAction) {
            this.onCompleteAction = onCompleteAction;
            return this;
        }
        #endregion

        #region Fail
        public void FailStep() {
            Messenger.Broadcast(Signals.QUEST_STEP_FAILED, this);
        }
        #endregion

        #region Hover Actions
        public QuestStep SetHoverOverAction(System.Action<QuestStepItem> onHoverOverAction) {
            this.onHoverOverAction = onHoverOverAction;
            return this;
        }
        public QuestStep SetHoverOutAction(System.Action onHoverOutAction) {
            this.onHoverOutAction = onHoverOutAction;
            return this;
        }
        #endregion
        
        #region Cleanup
        public void Cleanup() {
            UnSubscribeListeners();
        }
        #endregion

        #region Center Actions
        public QuestStep SetObjectsToCenter(List<ISelectable> objectsToCenter) {
            this.objectsToCenter = objectsToCenter;
            return this;
        }
        public QuestStep SetObjectsToCenter(params ISelectable[] objectsToCenter) {
            Assert.IsNotNull(objectsToCenter, $"Passed objects to center for {this} is null!");
            Assert.IsTrue(objectsToCenter.Length > 0, $"Passed objects to center for {this} is empty!");
            this.objectsToCenter = new List<ISelectable>(objectsToCenter);
            return this;
        }
        public void CenterCycle() {
            ISelectable objToSelect = null;
            for (int i = 0; i < objectsToCenter.Count; i++) {
                ISelectable currentSelectable = objectsToCenter[i];
                if (currentSelectable.IsCurrentlySelected()) {
                    //set next selectable in list to be selected.
                    objToSelect = CollectionUtilities.GetNextElementCyclic(objectsToCenter, i);
                    break;
                }
            }
            if (objToSelect == null) {
                objToSelect = objectsToCenter[0];
            }
            if (objToSelect != null) {
                InputManager.Instance.Select(objToSelect);
            }
        }
        #endregion

        #region Description
        protected virtual string GetStepDescription() {
            return _stepDescription;
        }
        #endregion
    }
}