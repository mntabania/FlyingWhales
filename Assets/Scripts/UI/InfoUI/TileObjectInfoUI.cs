using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class TileObjectInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI quantityLbl;
    [SerializeField] private TextMeshProUGUI generalDescription;
    [SerializeField] private TextMeshProUGUI ownerLbl;
    [SerializeField] private EventLabel ownerEventLbl;
    [SerializeField] private TextMeshProUGUI carriedByLbl;
    [SerializeField] private EventLabel carriedByEventLbl;
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private GameObject equipBonusGO;
    [SerializeField] private TextMeshProUGUI equipBonusLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;
    [SerializeField] private TileObjectPortrait tileObjectPortrait;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private Toggle charactersToggle;
    [SerializeField] private TextMeshProUGUI charactersToggleLbl;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;

    [Space(10)]
    [Header("Item Effects")]
    [SerializeField] private GameObject itemEffectParent;
    [SerializeField] private TextMeshProUGUI itemEffectsLbl;

    [Space(10)] [Header("Logs")] 
    [SerializeField] private LogsWindow logsWindow;
    
    [Space(10)]
    [Header("Store Target")] 
    [SerializeField] private StoreTargetButton btnStoreTarget;

    public TileObject activeTileObject { get; private set; }

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateLogsFromSignal);
        Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateLogsFromSignal);
        Messenger.AddListener<Character>(UISignals.LOG_MENTIONING_CHARACTER_UPDATED, OnLogMentioningCharacterUpdated);
        Messenger.AddListener<TileObject, Character>(TileObjectSignals.ADD_TILE_OBJECT_USER, UpdateUsersFromSignal);
        Messenger.AddListener<TileObject, Character>(TileObjectSignals.REMOVE_TILE_OBJECT_USER, UpdateUsersFromSignal);
        Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<TileObject, Trait>(TileObjectSignals.TILE_OBJECT_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);

        ownerEventLbl.SetOnLeftClickAction(OnLeftClickOwner);
        ownerEventLbl.SetOnRightClickAction(OnRightClickOwner);
        
        carriedByEventLbl.SetOnLeftClickAction(OnLeftClickLocation);
        carriedByEventLbl.SetOnRightClickAction(OnRightClickLocation);
        tileObjectPortrait.SetRightClickAction(OnRightClickPortrait);

        logsWindow.Initialize();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        if(activeTileObject != null) {
            if (activeTileObject.mapVisual != null) {
                // activeTileObject.mapVisual.UnlockHoverObject();
                // activeTileObject.mapVisual.SetHoverObjectState(false);
                activeTileObject.mapVisual.UpdateSortingOrders(activeTileObject);
                if (InnerMapCameraMove.Instance.target == activeTileObject.mapObjectVisual.transform) {
                    InnerMapCameraMove.Instance.CenterCameraOn(null);
                }
            }
            if (activeTileObject is DemonEye eyeWard) {
                //Show eye ward highlight of current eye ward
                eyeWard.HideEyeWardHighlight();
            }
        }
        activeTileObject = null;
        btnStoreTarget.SetTarget(null);
    }
    public override void OpenMenu() {
        TileObject previousTileObject = activeTileObject;
        // if (previousTileObject != null && previousTileObject.mapVisual != null) {
        //     // previousTileObject.mapVisual.UnlockHoverObject();
        //     // previousTileObject.mapVisual.SetHoverObjectState(false);
        //     Selector.Instance.Deselect();
        // }
        
        activeTileObject = _data as TileObject;
        if (previousTileObject != null) {
            if(previousTileObject.mapVisual != null) {
                previousTileObject.mapVisual.UpdateSortingOrders(previousTileObject);
            }
            if(previousTileObject is DemonEye previousEyeWard) {
                //Hide eye ward highlight of previous eye ward
                previousEyeWard.HideEyeWardHighlight();
            }
        }
        if(activeTileObject.gridTileLocation != null && activeTileObject.mapObjectVisual != null) {
            bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(activeTileObject.currentRegion);
            InnerMapCameraMove.Instance.CenterCameraOn(activeTileObject.mapObjectVisual.gameObject, instantCenter);
        }
        // activeTileObject.mapVisual.SetHoverObjectState(true);
        // activeTileObject.mapVisual.LockHoverObject();
        base.OpenMenu();
        if (activeTileObject.mapObjectVisual != null) {
            Selector.Instance.Select(activeTileObject, activeTileObject.mapObjectVisual.transform);    
            activeTileObject.mapVisual.UpdateSortingOrders(activeTileObject);
        }
        if (activeTileObject is DemonEye eyeWard) {
            //Show eye ward highlight of current eye ward
            eyeWard.ShowEyeWardHighlight();
        }
        btnStoreTarget.SetTarget(activeTileObject);
        UIManager.Instance.HideObjectPicker();
        UpdateTabs();
        UpdateBasicInfo();
        UpdateInfo();
        UpdateTraits();
        UpdateUsers();
        logsWindow.OnParentMenuOpened(activeTileObject.persistentID);
        UpdateLogs();
        LoadActions(activeTileObject);
    }
    protected override void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PLAYER_SKILL_TYPE skillType = target.actions[i];
            if(skillType == PLAYER_SKILL_TYPE.DESTROY_EYE_WARD) {
                PlayerAction action = PlayerSkillManager.Instance.GetPlayerActionData(skillType);
                if (action.IsValid(target) && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(action.type)) {
                    ActionItem actionItem = AddNewAction(action, target);
                    actionItem.SetInteractable(action.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
                    actionItem.ForceUpdateCooldown();
                }
            }
        }
    }
    #endregion

    private void OnReceiveKeyCodeSignal(KeyCode p_key) {
        if (p_key == KeyCode.Mouse1) {
            CloseMenu();
        }
    }

    #region General
    public void UpdateTileObjectInfo() {
        if(activeTileObject == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateInfo();
    }
    private void UpdateTabs() {
        //Do not show Characters tab is Tile Object cannot have users/characters
        // if(activeTileObject.tileObjectType == TILE_OBJECT_TYPE.BED || activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TABLE || activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOMBSTONE) {
        if (activeTileObject.users != null) {
            charactersToggle.interactable = true;
            charactersToggleLbl.gameObject.SetActive(true);
        } else {
            charactersToggle.isOn = false;
            charactersToggle.interactable = false;
            charactersToggleLbl.gameObject.SetActive(false);
        }
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeTileObject.nameplateName;
    }
    private void UpdateInfo() {
        hpLbl.text = $"{activeTileObject.currentHP}/{activeTileObject.maxHP}";

        int quantity = 1;
        if(activeTileObject is ResourcePile) {
            quantity = (activeTileObject as ResourcePile).resourceInPile;
        } else if (activeTileObject is Table) {
            quantity = activeTileObject.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD);
        }
        quantityLbl.text = $"{quantity}";
        if (activeTileObject.tileObjectType.IsTileObjectWithCount()) {
            quantityLbl.text = GetCountQuantityBaseOnType(activeTileObject).ToString();
        }
        //if(activeTileObject.tileObjectType.)
        generalDescription.text = activeTileObject.description;
        ownerLbl.text = activeTileObject.characterOwner != null ? $"<link=\"1\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeTileObject.characterOwner.name)}</link>" : "None";
        UpdateLocationInfo();
        tileObjectPortrait.SetTileObject(activeTileObject);
    }

    public int GetCountQuantityBaseOnType(TileObject p_object) {
        switch (p_object.tileObjectType) {
            case TILE_OBJECT_TYPE.ROCK:
            return (p_object as Rock).count;
            case TILE_OBJECT_TYPE.ORE:
            return (p_object as Ore).count;
            case TILE_OBJECT_TYPE.SMALL_TREE_OBJECT:
            case TILE_OBJECT_TYPE.BIG_TREE_OBJECT:
            return (p_object as TreeObject).count;
        }

        return 1;
    }
    private void OnRightClickPortrait(TileObject p_tileObject) {
        UIManager.Instance.ShowPlayerActionContextMenu(p_tileObject, Input.mousePosition, true);
    }
    private void UpdateLocationInfo() {
        if (activeTileObject.isBeingCarriedBy != null) {
            carriedByLbl.text = $"<link=\"1\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeTileObject.isBeingCarriedBy.name)}</link>";
        } else if (activeTileObject.gridTileLocation != null) {
            if (activeTileObject.gridTileLocation.structure is Wilderness) {
                carriedByLbl.text = "Wilderness";
            }
            else {
                carriedByLbl.text = $"<link=\"1\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeTileObject.gridTileLocation.structure.name)}</link>";
            }
        } else {
            carriedByLbl.text = "None";
        }
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;
        string equipTraits = string.Empty;

        for (int i = 0; i < activeTileObject.traitContainer.statuses.Count; i++) {
            Status currStatus = activeTileObject.traitContainer.statuses[i];
            if (currStatus.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (!string.IsNullOrEmpty(statusTraits)) {
                statusTraits = $"{statusTraits}, ";
            }
            statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currStatus.GetNameInUI(activeTileObject)}</link></color></b>";
        }
        for (int i = 0; i < activeTileObject.traitContainer.traits.Count; i++) {
            Trait currTrait = activeTileObject.traitContainer.traits[i];
            if (currTrait.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (currTrait.type == TRAIT_TYPE.BUFF) {
                color = UIManager.buffTextColor;
            } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                color = UIManager.flawTextColor;
            }
            if (!string.IsNullOrEmpty(normalTraits)) {
                normalTraits = $"{normalTraits}, ";
            }
            normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(activeTileObject)}</link></color></b>";
        }

        statusTraitsLbl.text = string.Empty;
        if (activeTileObject is EquipmentItem equip) {
            itemEffectParent.gameObject.SetActive(true);
            equipBonusGO.gameObject.SetActive(false);
            equipTraits += "\n" + equip.GetBonusDescription();
            if (string.IsNullOrEmpty(equipTraits) == false) {
                //character has status traits
                equipBonusLbl.text = equipTraits;
            }
            
            itemEffectsLbl.text = equip.GetBonusDescription();
        } else {
            itemEffectParent.gameObject.SetActive(false);
            equipBonusGO.gameObject.SetActive(false);
        }
        if (string.IsNullOrEmpty(statusTraits) == false) {
            //character has status traits
            statusTraitsLbl.text = statusTraits;
        }
        normalTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(normalTraits) == false) {
            //character has normal traits
            normalTraitsLbl.text = normalTraits;
        }
    }
    private void UpdateUsers() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        Character[] users = activeTileObject.users;
        if (users != null && users.Length > 0) {
            for (int i = 0; i < users.Length; i++) {
                Character character = users[i];
                if(character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
    public void UpdateLogs() {
        logsWindow.UpdateAllHistoryInfo();
    }
    #endregion

    #region Listeners
    private void UpdateLogsFromSignal(Log log) {
        if(isShowing && log.IsInvolved(activeTileObject)) {
            UpdateLogs();
        }
    }
    private void OnLogMentioningCharacterUpdated(Character character) {
        if (isShowing) {
            //update history regardless of character because updated character might be referenced in this objects logs
            UpdateLogs();
        }
    }
    private void UpdateUsersFromSignal(TileObject tileObject, Character user) {
        if(isShowing && activeTileObject == tileObject) {
            UpdateUsers();
        }
    }
    private void UpdateTraitsFromSignal(TileObject tileObject, Trait trait) {
        if (isShowing && activeTileObject == tileObject) {
            UpdateTraits();
        }
    }
    #endregion

    #region Event Labels
    private void OnLeftClickOwner(object obj) {
        if(activeTileObject.characterOwner != null) {
            UIManager.Instance.ShowCharacterInfo(activeTileObject.characterOwner, true);
        }
    }
    private void OnRightClickOwner(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }
    private void OnLeftClickLocation(object obj) {
        if (activeTileObject.isBeingCarriedBy != null) {
            UIManager.Instance.ShowCharacterInfo(activeTileObject.isBeingCarriedBy, true);
        } else if (activeTileObject.gridTileLocation != null) {
            if (!(activeTileObject.gridTileLocation.structure is Wilderness)) {
                UIManager.Instance.ShowStructureInfo(activeTileObject.gridTileLocation.structure);
            }
        }
    }
    private void OnRightClickLocation(object obj) {
        if (obj is IPlayerActionTarget playerActionTarget) {
            if (playerActionTarget is Character character) {
                if(character.isLycanthrope) {
                    playerActionTarget = character.lycanData.activeForm;
                }
            }
            UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, Input.mousePosition, true);
        }
    }

    public void OnClickItem() {
        if(activeTileObject == null) {
            return;
		}
        if (activeTileObject.worldObject == null && activeTileObject.isBeingCarriedBy != null) {
            InnerMapCameraMove.Instance.CenterCameraOn(activeTileObject.isBeingCarriedBy.worldObject.gameObject);
        } else if (activeTileObject.worldObject != null && activeTileObject.isBeingCarriedBy == null) {
            InnerMapCameraMove.Instance.CenterCameraOn(activeTileObject.worldObject.gameObject);
        } else if (activeTileObject.worldObject != null && activeTileObject.isBeingCarriedBy != null) {
            InnerMapCameraMove.Instance.CenterCameraOn(activeTileObject.worldObject.gameObject); 
        } else {
            return;
        }
    }

    public void OnHoverTrait(object obj) {
        if (obj is string) {
            string text = (string) obj;
            int index = int.Parse(text);
            Trait trait = activeTileObject.traitContainer.traits[index];
            UIManager.Instance.ShowSmallInfo(trait.descriptionInUI);
        }
    }
    public void OnHoverStatus(object obj) {
        if (obj is string) {
            string text = (string) obj;
            int index = int.Parse(text);
            Trait trait = activeTileObject.traitContainer.statuses[index];
            UIManager.Instance.ShowSmallInfo(trait.descriptionInUI);
        }
    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
