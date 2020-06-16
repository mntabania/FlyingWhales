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
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private Image actionIcon;

    [Header("Thoughts")] 
    [SerializeField] private GameObject thoughtGO;
    [SerializeField] private TextMeshProUGUI thoughtLbl;
    
    private CharacterMarker _parentMarker;

    private const float DefaultSize = 80f;

    public void Initialize(CharacterMarker characterMarker) {
        name = $"{characterMarker.character.name} Marker Nameplate";
        _parentMarker = characterMarker;
        UpdateName();
        UpdateSizeBasedOnZoom();
        Messenger.AddListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
    }

    #region Listeners
    private void OnCameraZoomChanged(Camera camera, float amount) {
        if (camera == InnerMapCameraMove.Instance.innerMapsCamera) {
            UpdateSizeBasedOnZoom();
        }
    }
    #endregion

    #region Monobehaviours
    private void LateUpdate() {
        Vector3 markerScreenPosition =
            InnerMapCameraMove.Instance.innerMapsCamera.WorldToScreenPoint(_parentMarker.transform.position);
        markerScreenPosition.z = 0f;
        if (_parentMarker.character != null && _parentMarker.character.grave != null) {
            markerScreenPosition.y += 15f;
        }
        transform.position = markerScreenPosition;
        if (thoughtGO.activeSelf) {
            UpdateThoughtText();
        }
    }
    #endregion

    #region Name
    public void UpdateName() {
        string icon = UtilityScripts.Utilities.VillagerIcon();
        string name = UtilityScripts.Utilities.ColorizeName(_parentMarker.character.name);
        if (_parentMarker.character.isNormalCharacter == false) {
            icon = UtilityScripts.Utilities.MonsterIcon();
            name = $"<color=#820000>{_parentMarker.character.name}</color>";
        }
        nameLbl.SetText($"{icon} {name}");
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        _parentMarker = null;
        Messenger.RemoveListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
    }
    #endregion

    #region Utilities
    public void SetActiveState(bool state) {
        gameObject.SetActive(state);
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
        if (character.isConversing && !character.isInCombat) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Social_Icon];
            actionIcon.gameObject.SetActive(true);
            return;
        }
        
        if (character.interruptComponent.isInterrupted) {
            if (character.interruptComponent.currentInterrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.currentInterrupt.interruptIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
            return;
        } else if (character.interruptComponent.hasTriggeredSimultaneousInterrupt) {
            if (character.interruptComponent.triggeredSimultaneousInterrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.triggeredSimultaneousInterrupt.interruptIconString];
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
    }
    private void UpdateThoughtText() {
        thoughtLbl.text = _parentMarker.character.visuals.GetThoughtBubble(out var log);
    }
    #endregion
}
