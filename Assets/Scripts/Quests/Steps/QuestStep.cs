using System.Collections.Generic;
using JetBrains.Annotations;
using Ruinarch;
using UnityEngine;
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
        /// <summary>
        /// Getter for list of objects to center, if <see cref="objectsToCenter"/> is null, then this will be checked.
        /// </summary>
        private System.Func<List<GameObject>> _objectsToCenterGetter;
        /// <summary>
        /// Helper function for <see cref="_objectsToCenterGetter"/>, this is used to determine if a game object in the getter list is selected or not.
        /// </summary>
        private System.Func<GameObject, bool> _isGameObjectSelected;
        /// <summary>
        /// Helper function for <see cref="_objectsToCenterGetter"/>, this is used to dictate how the selected object will be centered
        /// </summary>
        private System.Action<GameObject> _centerGameObjectAction;

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
        public void ExecuteHoverAction(QuestStepItem item) {
            onHoverOverAction?.Invoke(item);
            Messenger.Broadcast(Signals.QUEST_STEP_HOVERED, this);
        }
        public void ExecuteHoverOutAction() {
            onHoverOutAction?.Invoke();
            Messenger.Broadcast(Signals.QUEST_STEP_HOVERED_OUT, this);
        }
        #endregion
        
        #region Cleanup
        public void Cleanup() {
            UnSubscribeListeners();
        }
        #endregion

        #region Center Actions
        public bool HasObjectsToCenter() {
            return (objectsToCenter != null && objectsToCenter.Count > 0) || _objectsToCenterGetter != null;
        }
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
        public QuestStep SetObjectsToCenter([NotNull]System.Func<List<GameObject>> objectsToCenterGetter, 
            [NotNull]System.Func<GameObject, bool> isGameObjectSelected, [NotNull]System.Action<GameObject> centerGameObjectAction) {
            _objectsToCenterGetter = objectsToCenterGetter;
            _isGameObjectSelected = isGameObjectSelected;
            _centerGameObjectAction = centerGameObjectAction;
            return this;
        }
        public void CenterCycle() {
            if (objectsToCenter != null) {
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
            } else if (_objectsToCenterGetter != null) {
                GameObject objToSelect = null;
                List<GameObject> gameObjects = _objectsToCenterGetter.Invoke();
                for (int i = 0; i < gameObjects.Count; i++) {
                    GameObject currentGameObject = gameObjects[i];
                    if (_isGameObjectSelected.Invoke(currentGameObject)) {
                        //set next selectable in list to be selected.
                        objToSelect = CollectionUtilities.GetNextElementCyclic(gameObjects, i);
                        break;
                    }
                }
                if (objToSelect == null) {
                    objToSelect = gameObjects[0];
                }
                if (objToSelect != null) {
                    _centerGameObjectAction.Invoke(objToSelect);
                }    
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