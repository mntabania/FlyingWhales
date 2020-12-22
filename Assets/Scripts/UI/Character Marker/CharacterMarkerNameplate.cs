using System;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using Inner_Maps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMarkerNameplate : PooledObject {

    [SerializeField] private RectTransform thisRect;
    [SerializeField] private GameObject visualsParent;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private Image actionIcon;

    [Header("Thoughts")] 
    [SerializeField] private GameObject thoughtGO;
    [SerializeField] private TextMeshProUGUI thoughtLbl;
    [SerializeField] private RectTransform thoughtsRectTransform;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    
    [Header("Intel Helper")]
    [SerializeField] private TextMeshProUGUI intelHelperLbl;
    [SerializeField] private GameObject intelHelperGO;
    [SerializeField] private GameObject highlightGO;
    private CharacterMarker _parentMarker;

    private const float DefaultSize = 80f;

    public void Initialize(CharacterMarker characterMarker) {
        name = $"{characterMarker.character.name} Marker Nameplate";
        _parentMarker = characterMarker;
        UpdateName();
        UpdateSizeBasedOnZoom();
        Messenger.AddListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
        Messenger.AddListener<Character, Region>(RegionSignals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.AddListener<Character, Region>(RegionSignals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener(UISignals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
    }

    #region Listeners
    private void OnCameraZoomChanged(Camera camera, float amount) {
        if (camera == InnerMapCameraMove.Instance.camera) {
            UpdateSizeBasedOnZoom();
        }
    }
    private void OnLocationMapClosed(Region location) {
        if (location == _parentMarker.character.currentRegion) {
            SetGameObjectActiveState(false);
        }        
    }
    private void OnLocationMapOpened(Region location) {
        if (location == _parentMarker.character.currentRegion) {
            SetGameObjectActiveState(true);
        }        
    }
    private void OnCharacterExitedRegion(Character character, Region region) {
        if (character == _parentMarker.character && character.isDead == false) { //added checking for isDead so nameplate is not deactivated when a character dies.
            SetGameObjectActiveState(false);
        }
    }
    private void OnCharacterEnteredRegion(Character character, Region region) {
        if (character == _parentMarker.character && InnerMapManager.Instance.currentlyShowingLocation == region) {
            SetGameObjectActiveState(true);
        }
    }
    #endregion

    #region Monobehaviours
    private void Update() {
        if (thoughtGO.activeSelf) {
            UpdateThoughtText();
        }
    }
    private void LateUpdate() {
        Vector3 markerScreenPosition =
            InnerMapCameraMove.Instance.camera.WorldToScreenPoint(_parentMarker.transform.position);
        markerScreenPosition.z = 0f;
        if (_parentMarker.character != null && _parentMarker.character.grave != null) {
            markerScreenPosition.y += 15f;
        }
        transform.position = markerScreenPosition;
    }
    #endregion

    #region Name
    public void UpdateName() {
        string icon = _parentMarker.character.visuals.GetCharacterStringIcon();
        string characterName = _parentMarker.character.firstNameWithColor; //UtilityScripts.Utilities.ColorizeName(_parentMarker.character.name);
        nameLbl.text = $"{icon}{characterName}";
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        HideThoughtsAndNameplate();
        HideIntelHelper();
        SetHighlighterState(false);
        _parentMarker = null;
        Messenger.RemoveListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
        Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
        Messenger.RemoveListener<Character, Region>(RegionSignals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.RemoveListener<Character, Region>(RegionSignals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.RemoveListener(UISignals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
    }
    #endregion

    #region Utilities
    public void UpdateActiveState() {
        SetGameObjectActiveState(InnerMapManager.Instance.currentlyShowingLocation == _parentMarker.character.currentRegion);
    }
    /// <summary>
    /// Set the active state of this game object.
    /// </summary>
    private void SetGameObjectActiveState(bool state) {
        gameObject.SetActive(state);
    }
    /// <summary>
    /// Only Set the active state of all objects under the visuals parent. 
    /// </summary>
    public void SetVisualsState(bool state) {
        visualsParent.gameObject.SetActive(state);
    }
    public void SetNameState(bool state) {
        nameLbl.gameObject.SetActive(state);
    }
    private void UpdateSizeBasedOnZoom() {
        float fovDiff = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
        float spriteSize = _parentMarker.character.visuals.selectableSize.y * 100f;
        if (_parentMarker.character.grave != null) {
            spriteSize = DefaultSize - (fovDiff * 4f);
        } else {
            if (_parentMarker.character is Dragon) {
                spriteSize -= (12f * fovDiff);  
            } else {
                spriteSize -= (4f * fovDiff);
            }
        }
        float size = spriteSize; //- (fovDiff * 4f);
        thisRect.sizeDelta = new Vector2(size, size);
    }
    public void UpdateElementsStateBasedOnActiveCharacter() {
        Character shownCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
        if (UIManager.Instance.gameObject.activeSelf) {
            //if UI is shown
            if (shownCharacter == _parentMarker.character) {
                SetNameState(true);
                ShowThoughtsAndNameplate();
            } else {
                SetNameState(true);
                HideThoughtsAndNameplate();
            }
        } else {
            //if UI is not shown
            if (shownCharacter == _parentMarker.character) {
                SetNameState(true);
                ShowThoughtsAndNameplate();
            } else {
                SetNameState(false);
                HideThoughtsAndNameplate();
            }    
        }
        
    }
    #endregion

    #region Action Icon
    public void UpdateActionIcon() {
        if (_parentMarker == null) {
            return;
        }
        Character character = _parentMarker.character;
        if (character == null) {
            return;
        }
        if (character.isDead) {
            actionIcon.gameObject.SetActive(false);
            return;
        }
        if (character.isConversing && !character.combatComponent.isInCombat) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Social_Icon];
            actionIcon.gameObject.SetActive(true);
            return;
        }
        
        if (character.interruptComponent.isInterrupted) {
            if (character.interruptComponent.currentInterrupt.interrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.currentInterrupt.interrupt.interruptIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
            return;
        } else if (character.interruptComponent.hasTriggeredSimultaneousInterrupt) {
            if (character.interruptComponent.triggeredSimultaneousInterrupt.interrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.triggeredSimultaneousInterrupt.interrupt.interruptIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
            return;
        } else {
            actionIcon.gameObject.SetActive(false);
        }
        
        if (character.currentActionNode != null) {
            string actionIconString = character.currentActionNode.action.GetActionIconString(character.currentActionNode);
            if (actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[actionIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
        } else if (character.stateComponent.currentState != null) {
            string actionIconString = character.stateComponent.currentState.actionIconString;
            if (actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[actionIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
        } else if (_parentMarker.hasFleePath) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Flee_Icon];
            actionIcon.gameObject.SetActive(true);
        } else {
            //no action or state
            actionIcon.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Thoughts
    public void ShowThoughtsAndNameplate() {
        //nameLbl.gameObject.SetActive(true);
        thoughtGO.SetActive(true);
        UpdateThoughtText();
    }
    public void HideThoughtsAndNameplate() {
        //nameLbl.gameObject.SetActive(false);
        thoughtGO.SetActive(false);
        thoughtLbl.text = string.Empty;
    }
    private void UpdateThoughtText() {
        string thoughts = _parentMarker.character.visuals.GetThoughtBubble();
        if (thoughtLbl.text.Equals(thoughts) == false) {
            //thoughts changed
            thoughtLbl.text = thoughts;
            LayoutRebuilder.ForceRebuildLayoutImmediate(thoughtsRectTransform);
            // contentSizeFitter.SetLayoutVertical();
            // contentSizeFitter.SetLayoutHorizontal();
        }
        
    }
    #endregion

    #region Intel Helper
    public void ShowIntelHelper(string text) {
        intelHelperLbl.text = text;
        intelHelperGO.SetActive(true);
    }
    public void HideIntelHelper() {
        intelHelperGO.SetActive(false);
    }
    #endregion

    #region Highlighter
    public void SetHighlighterState(bool state) {
        highlightGO.SetActive(state);
    }
    #endregion
}
