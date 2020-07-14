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
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    
    private CharacterMarker _parentMarker;

    private const float DefaultSize = 80f;

    public void Initialize(CharacterMarker characterMarker) {
        name = $"{characterMarker.character.name} Marker Nameplate";
        _parentMarker = characterMarker;
        UpdateName();
        UpdateSizeBasedOnZoom();
        Messenger.AddListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnLocationMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnLocationMapClosed);
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener(Signals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
    }

    #region Listeners
    private void OnCameraZoomChanged(Camera camera, float amount) {
        if (camera == InnerMapCameraMove.Instance.innerMapsCamera) {
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
            InnerMapCameraMove.Instance.innerMapsCamera.WorldToScreenPoint(_parentMarker.transform.position);
        markerScreenPosition.z = 0f;
        if (_parentMarker.character != null && _parentMarker.character.grave != null) {
            markerScreenPosition.y += 15f;
        }
        transform.position = markerScreenPosition;
    }
    #endregion

    #region Name
    public void UpdateName() {
        string icon = UtilityScripts.Utilities.VillagerIcon();
        string characterName = UtilityScripts.Utilities.ColorizeName(_parentMarker.character.name);
        if (_parentMarker.character.isNormalCharacter == false) {
            icon = UtilityScripts.Utilities.MonsterIcon();
            characterName = _parentMarker.character.name;
            //name = $"<color=#820000>{_parentMarker.character.name}</color>";
        } else if (_parentMarker.character.isAlliedWithPlayer) {
            icon = UtilityScripts.Utilities.CultistIcon();
        }
        nameLbl.text = $"{icon}{characterName}";
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        _parentMarker = null;
        Messenger.RemoveListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_OPENED, OnLocationMapOpened);
        Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_CLOSED, OnLocationMapClosed);
        Messenger.RemoveListener<Character, Region>(Signals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.RemoveListener<Character, Region>(Signals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.RemoveListener(Signals.UI_STATE_SET, UpdateElementsStateBasedOnActiveCharacter);
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
        float spriteSize = (_parentMarker.character.visuals.defaultSprite.rect.width - 20f);
        if (_parentMarker.character.grave != null) {
            spriteSize = DefaultSize;
        }
        float size = spriteSize - (fovDiff * 4f);
        thisRect.sizeDelta = new Vector2(size, size);
    }
    public void UpdateElementsStateBasedOnActiveCharacter() {
        Character shownCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
        if (UIManager.Instance.gameObject.activeSelf) {
            //if UI is shown
            if (shownCharacter == _parentMarker.character) {
                SetNameState(true);
                ShowThoughts();
            } else {
                SetNameState(true);
                HideThoughts();
            }
        } else {
            //if UI is not shown
            if (shownCharacter == _parentMarker.character) {
                SetNameState(true);
                ShowThoughts();
            } else {
                SetNameState(false);
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
            if (character.currentActionNode.action.actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.currentActionNode.action.actionIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
        } else if (character.stateComponent.currentState != null) {
            if (character.stateComponent.currentState.actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.stateComponent.currentState.actionIconString];
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
    public void ShowThoughts() {
        thoughtGO.SetActive(true);
        UpdateThoughtText();
    }
    public void HideThoughts() {
        thoughtGO.SetActive(false);
        thoughtLbl.text = string.Empty;
    }
    private void UpdateThoughtText() {
        string thoughts = _parentMarker.character.visuals.GetThoughtBubble(out var log);
        if (thoughtLbl.text.Equals(thoughts) == false) {
            //thoughts changed
            thoughtLbl.text = thoughts;
            contentSizeFitter.SetLayoutVertical();
            contentSizeFitter.SetLayoutHorizontal();
        }
        
    }
    #endregion
}
