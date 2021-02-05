﻿using System.Collections.Generic;
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
        /// Getter for list of selectables to center, this is for when objects to center are determined after this step was made.
        /// </summary>
        private System.Func<List<ISelectable>> _selectablesToCenterGetter;
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
        /// <summary>
        /// Action to perform when this step is set as the top most incomplete one
        /// </summary>
        private System.Action _setAsTopmostIncompleteStepAction;
        /// <summary>
        /// Action to perform when this step is no longer the top most incomplete one
        /// </summary>
        private System.Action _noLongerTopmostIncompleteStepAction;
        
        
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
        public virtual void Activate() {
            SubscribeListeners();
            Messenger.Broadcast(PlayerQuestSignals.QUEST_STEP_ACTIVATED, this);
        }
        public void TryToCompleteStep() {
            if (CheckIfStepIsAlreadyCompleted()) {
                Complete();
            }
        }
        protected virtual bool CheckIfStepIsAlreadyCompleted() {
            return false;
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
            onCompleteAction?.Invoke();
            Messenger.Broadcast(PlayerQuestSignals.QUEST_STEP_COMPLETED, this);
        }
        public QuestStep SetCompleteAction(System.Action onCompleteAction) {
            this.onCompleteAction = onCompleteAction;
            return this;
        }
        #endregion

        #region Fail
        public void FailStep() {
            Messenger.Broadcast(PlayerQuestSignals.QUEST_STEP_FAILED, this);
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
            Messenger.Broadcast(PlayerQuestSignals.QUEST_STEP_HOVERED, this);
        }
        public void ExecuteHoverOutAction() {
            onHoverOutAction?.Invoke();
            Messenger.Broadcast(PlayerQuestSignals.QUEST_STEP_HOVERED_OUT, this);
        }
        #endregion
        
        #region Cleanup
        public void Deactivate() {
            UnSubscribeListeners();
        }
        #endregion

        #region Center Actions
        public bool HasObjectsToCenter() {
            return (objectsToCenter != null && objectsToCenter.Count > 0) || _objectsToCenterGetter != null || _selectablesToCenterGetter != null;
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
        public QuestStep SetObjectsToCenter([NotNull]System.Func<List<ISelectable>> objectsToCenterGetter) {
            _selectablesToCenterGetter = objectsToCenterGetter;
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
                //normal objects to center
                ISelectable objToSelect = GetNextObjectToCenter(objectsToCenter);
                if (objToSelect != null) {
                    InputManager.Instance.Select(objToSelect);
                }    
            } else if (_objectsToCenterGetter != null) {
                //game objects to center getter
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
            } else if (_selectablesToCenterGetter != null) {
                //selectables to center getter
                List<ISelectable> selectables = _selectablesToCenterGetter.Invoke();
                ISelectable objToSelect = GetNextObjectToCenter(selectables);
                if (objToSelect != null) {
                    InputManager.Instance.Select(objToSelect);
                }    
            }
        }
        private ISelectable GetNextObjectToCenter(List<ISelectable> selectables) {
            ISelectable objToSelect = null;
            for (int i = 0; i < selectables.Count; i++) {
                ISelectable currentSelectable = selectables[i];
                if (currentSelectable.IsCurrentlySelected()) {
                    //set next selectable in list to be selected.
                    objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                    break;
                }
            }
            if (objToSelect == null) {
                objToSelect = selectables[0];
            }
            return objToSelect;
        }
        #endregion

        #region Description
        protected virtual string GetStepDescription() {
            return _stepDescription;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Set this step as the top most incomplete step in the quest checklist.
        /// </summary>
        public void SetAsTopMostIncompleteStep() {
            _setAsTopmostIncompleteStepAction?.Invoke();   
        }
        /// <summary>
        /// This is called when this step was previously the topmost incomplete one, but is not anymore. 
        /// </summary>
        public void NoLongerTopMostIncompleteStep() {
            _noLongerTopmostIncompleteStepAction?.Invoke();
        }
        public QuestStep SetOnTopmostActions(System.Action setAsTopmostAction, System.Action noLongerTopMostAction) {
            _setAsTopmostIncompleteStepAction = setAsTopmostAction;
            _noLongerTopmostIncompleteStepAction = noLongerTopMostAction;
            return this;
        }
        #endregion
        
    }
}