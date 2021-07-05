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
        //Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
        //Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
        //Messenger.AddListener<Character, Region>(RegionSignals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        //Messenger.AddListener<Character, Region>(RegionSignals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener(UISignals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, OnCharacterSetFaction);
    }

    #region Listeners
    private void OnCharacterSetFaction(Character p_character) {
        if (p_character == _parentMarker.character) {
            UpdateName();
        }
    }
    private void OnCameraZoomChanged(Camera camera, float amount) {
        if (camera == InnerMapCameraMove.Instance.camera) {
            UpdateSizeBasedOnZoom();
        }
    }
    //private void OnLocationMapClosed(Region location) {
    //    if (location == _parentMarker.character.currentRegion) {
    //        SetGameObjectActiveState(false);
    //    }        
    //}
    //private void OnLocationMapOpened(Region location) {
    //    if (location == _parentMarker.character.currentRegion) {
    //        UpdateActiveState();
    //    }        
    //}
    //private void OnCharacterExitedRegion(Character character, Region region) {
    //    if (character == _parentMarker.character && character.isDead == false) { //added checking for isDead so nameplate is not deactivated when a character dies.
    //        SetGameObjectActiveState(false);
    //    }
    //}
    //private void OnCharacterEnteredRegion(Character character, Region region) {
    //    if (character == _parentMarker.character && InnerMapManager.Instance.currentlyShowingLocation == region) {
    //        UpdateActiveState();
    //    }
    //}
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
        HideThoughts();
        HideIntelHelper();
        SetHighlighterState(false);
        _parentMarker = null;
        Messenger.RemoveListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        //Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
        //Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
        //Messenger.RemoveListener<Character, Region>(RegionSignals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        //Messenger.RemoveListener<Character, Region>(RegionSignals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.RemoveListener(UISignals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
        Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, OnCharacterSetFaction);
    }
    #endregion

    #region Utilities
    public void UpdateNameActiveState() {
        if (_parentMarker != null && _parentMarker.character != null && CharacterManager.Instance != null && InnerMapManager.Instance != null) {
            SetNameActiveState(CharacterManager.Instance.toggleCharacterMarkerName
                               || (_parentMarker != null && _parentMarker.character != null && (_parentMarker.character.isStoredAsTarget || InnerMapManager.Instance.IsPOIConsideredTheCurrentHoveredPOI(_parentMarker.character))));    
        }
    }
    public void SetNameActiveState(bool state) {
        nameLbl.gameObject.SetActive(state);
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
                //Removed this because nameplate state is now controlled by pressing Left Alt button
                //SetNameState(true);
                ShowThoughts();
            } else {
                //SetNameState(true);
                HideThoughts();
            }
        } else {
            //if UI is not shown
            if (shownCharacter == _parentMarker.character) {
                //SetNameState(true);
                ShowThoughts();
            } else {
                //SetNameState(false);
                HideThoughts();
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
            SetActionIconState(false);
            return;
        }
        if (character.isConversing && !character.combatComponent.isInCombat) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Social_Icon];
            SetActionIconState(true);
            return;
        }
        
        if (character.interruptComponent.isInterrupted) {
            if (character.interruptComponent.currentInterrupt.interrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.currentInterrupt.interrupt.interruptIconString];
                SetActionIconState(true);
            } else {
                SetActionIconState(false);
            }
            return;
        } else if (character.interruptComponent.hasTriggeredSimultaneousInterrupt) {
            if (character.interruptComponent.triggeredSimultaneousInterrupt.interrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.triggeredSimultaneousInterrupt.interrupt.interruptIconString];
                SetActionIconState(true);
            } else {
                SetActionIconState(false);
            }
            return;
        } else {
            SetActionIconState(false);
        }

        if (character.currentActionNode != null) {
            string actionIconString = character.currentActionNode.action.GetActionIconString(character.currentActionNode);
            if (actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[actionIconString];
                SetActionIconState(true);
            } else {
                SetActionIconState(false);
            }
        } else if (_parentMarker.hasFleePath) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Flee_Icon];
            SetActionIconState(true);
        } else if (character.combatComponent.isInActualCombat) {
            //Once the character is actually in combat, do not show thought bubble action icon so that the damage numbers can be seen
            SetActionIconState(false);
        } else if (character.stateComponent.currentState != null) {
            string actionIconString = character.stateComponent.currentState.actionIconString;
            if (actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[actionIconString];
                SetActionIconState(true);
            } else {
                SetActionIconState(false);
            }
        } else {
            //no action or state
            SetActionIconState(false);
        }
    }
    private void SetActionIconState(bool state) {
        if (actionIcon.gameObject.activeSelf != state) {
            actionIcon.gameObject.SetActive(state);    
        }
    }
    #endregion

    #region Thoughts
    public void ShowThoughts() {
        //nameLbl.gameObject.SetActive(true);
        thoughtGO.SetActive(true);
        UpdateThoughtText();
    }
    public void HideThoughts() {
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
