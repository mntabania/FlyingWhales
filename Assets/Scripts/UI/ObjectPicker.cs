using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class ObjectPicker : PopupMenuBase {

    [Header("Object Picker")]
    [SerializeField] private ScrollRect objectPickerScrollView;
    [SerializeField] private GameObject objectPickerCharacterItemPrefab;
    [SerializeField] private GameObject objectPickerAreaItemPrefab;
    [SerializeField] private GameObject objectPickerRegionItemPrefab;
    [SerializeField] private GameObject objectPickerStringItemPrefab;
    [SerializeField] private GameObject objectPickerRaceClassItemPrefab;
    [SerializeField] private GameObject objectPickerEnumItemPrefab;
    [SerializeField] private GameObject objectPickerAttackItemPrefab;
    [SerializeField] private GameObject objectPickerSummonSlotItemPrefab;
    [FormerlySerializedAs("objectPickerArtifactSlotItemPrefab")] [SerializeField] private GameObject objectPickerArtifactItemPrefab;
    [SerializeField] private TextMeshProUGUI titleLbl;
    [SerializeField] private GameObject cover;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private ToggleGroup toggleGroup;

    [Header("Misc")]
    [SerializeField] private UIHoverPosition minionCardPos;

    private bool _isGamePausedBeforeOpeningPicker;

    private object _pickedObj;
    private System.Action<object> onConfirmAction;
    private bool _shouldConfirmOnPick;

    public object pickedObj {
        get { return _pickedObj; }
        set { 
            _pickedObj = value;
            UpdateConfirmBtnState();
        }
    }

    public void ShowClickable<T>(List<T> items, Action<object> onConfirmAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null,
        string title = "", Action<T> onHoverItemAction = null, Action<T> onHoverExitItemAction = null, string identifier = "",
        bool showCover = false, int layer = 9, Func<string, Sprite> portraitGetter = null, bool asButton = false, bool shouldConfirmOnPick = false) {
        UtilityScripts.Utilities.DestroyChildren(objectPickerScrollView.content);

        _shouldConfirmOnPick = shouldConfirmOnPick;
        pickedObj = null;
        this.onConfirmAction = onConfirmAction;

        OrganizeList(items, out var validItems, out var invalidItems, comparer, validityChecker);
        Type type = typeof(T);
        if (type == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
        }
        //else if (type == typeof(NPCSettlement)) {
        //    ShowAreaItems(validItems.Cast<NPCSettlement>().ToList(), invalidItems.Cast<NPCSettlement>().ToList(), onHoverItemAction, onHoverExitItemAction);
        //} 
        else if (type == typeof(Region)) {
            ShowRegionItems(validItems.Cast<Region>().ToList(), invalidItems.Cast<Region>().ToList(), onHoverItemAction, onHoverExitItemAction, asButton);
        } else if (type == typeof(string)) {
            ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
        } else if (type == typeof(SummonSlot)) {
            ShowSummonItems(validItems.Cast<SummonSlot>().ToList(), invalidItems.Cast<SummonSlot>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
        } else if (type == typeof(Artifact)) {
            ShowArtifactItems(validItems.Cast<Artifact>().ToList(), invalidItems.Cast<Artifact>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
        } else if (type.IsEnum) {
            ShowEnumItems(validItems.Cast<Enum>().ToList(), invalidItems.Cast<Enum>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, portraitGetter, asButton);
        } else if (type == typeof(RaceClass)) {
            ShowRaceClassItems(validItems.Cast<RaceClass>().ToList(), invalidItems.Cast<RaceClass>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
        }
        titleLbl.text = title;
        if (!gameObject.activeSelf) {
            base.Open();
            _isGamePausedBeforeOpeningPicker = GameManager.Instance.isPaused;
            GameManager.Instance.SetPausedState(true);
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        cover.SetActive(showCover);
        this.gameObject.transform.SetSiblingIndex(layer);
        // closeBtn.interactable = closable;
        if (_shouldConfirmOnPick) {
            confirmBtn.gameObject.SetActive(false);
        } else {
            confirmBtn.gameObject.SetActive(true);
        }
    }
    public override void Close() {
        if (gameObject.activeSelf) {
            base.Close();
            GameManager.Instance.SetPausedState(_isGamePausedBeforeOpeningPicker);
            UIManager.Instance.SetSpeedTogglesState(true);
        }
    }

    private void OrganizeList<T>(List<T> items, out List<T> validItems, out List<T> invalidItems, IComparer<T> comparer = null, Func<T, bool> validityChecker = null) {
        validItems = new List<T>();
        invalidItems = new List<T>();
        if (validityChecker != null) {
            for (int i = 0; i < items.Count; i++) {
                T currItem = items[i];
                if (validityChecker(currItem)) {
                    validItems.Add(currItem);
                } else {
                    invalidItems.Add(currItem);
                }
            }
        } else {
            validItems.AddRange(items);
        }

        if (comparer != null) {
            validItems.Sort(comparer);
            invalidItems.Sort(comparer);
        }
    }

    #region Instantiators
    private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton) {
        Action<Character> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = Convert(onHoverItemAction);
        }
        Action<Character> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = Convert(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Character currCharacter = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content);
            CharacterNameplateItem characterItem = characterItemGO.GetComponent<CharacterNameplateItem>();
            characterItem.SetObject(currCharacter);
            characterItem.ClearAllOnClickActions();

            characterItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            characterItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            //specific case for minion
            if (currCharacter.minion != null) {
                characterItem.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(currCharacter.minion, minionCardPos));
                characterItem.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
            }
            if (asButton) {
                characterItem.AddOnClickAction(OnPickObject);
                characterItem.SetAsButton();
            } else {
                characterItem.AddOnToggleAction(OnPickObject);
                characterItem.SetAsToggle();
                characterItem.SetToggleGroup(toggleGroup);
            }

            characterItem.SetPortraitInteractableState(false);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Character currCharacter = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content);
            CharacterNameplateItem characterItem = characterItemGO.GetComponent<CharacterNameplateItem>();
            characterItem.SetObject(currCharacter);
            characterItem.ClearAllOnClickActions();

            characterItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            characterItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            //specific case for minion
            if (currCharacter.minion != null) {
                characterItem.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(currCharacter.minion, minionCardPos));
                characterItem.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
            }
            if (asButton) {
                characterItem.SetAsButton();
            } else {
                characterItem.SetAsToggle();
            }
            characterItem.SetInteractableState(false);
            characterItem.SetPortraitInteractableState(true);
        }
    }
    //private void ShowAreaItems<T>(List<NPCSettlement> validItems, List<NPCSettlement> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction) {
    //    Action<NPCSettlement> convertedHoverAction = null;
    //    if (onHoverItemAction != null) {
    //        convertedHoverAction = ConvertToArea(onHoverItemAction);
    //    }
    //    Action<NPCSettlement> convertedHoverExitAction = null;
    //    if (onHoverExitItemAction != null) {
    //        convertedHoverExitAction = ConvertToArea(onHoverExitItemAction);
    //    }
    //    for (int i = 0; i < validItems.Count; i++) {
    //        NPCSettlement currNpcSettlement = validItems[i];
    //        GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
    //        AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
    //        areaItem.SetArea(currNpcSettlement);
    //        areaItem.onClickAction = convertedAction;
    //        areaItem.onHoverEnterAction = convertedHoverAction;
    //        areaItem.onHoverExitAction = convertedHoverExitAction;
    //        areaItem.SetButtonState(true);
    //    }
    //    for (int i = 0; i < invalidItems.Count; i++) {
    //        NPCSettlement currNpcSettlement = invalidItems[i];
    //        GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerAreaItemPrefab.name, objectPickerScrollView.content);
    //        AreaPickerItem areaItem = areaItemGO.GetComponent<AreaPickerItem>();
    //        areaItem.SetArea(currNpcSettlement);
    //        areaItem.onClickAction = null;
    //        areaItem.onHoverEnterAction = convertedHoverAction;
    //        areaItem.onHoverExitAction = convertedHoverExitAction;
    //        areaItem.SetButtonState(false);
    //    }
    //}
    private void ShowRegionItems<T>(List<Region> validItems, List<Region> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, bool asButton) {
        Action<Region> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToRegion(onHoverItemAction);
        }
        Action<Region> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToRegion(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Region currRegion = validItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content);
            RegionNameplateItem item = areaItemGO.GetComponent<RegionNameplateItem>();
            item.SetObject(currRegion);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.AddOnClickAction(OnPickObject);
                item.SetAsButton();
            } else {
                item.AddOnToggleAction(OnPickObject);
                item.SetAsToggle();
                item.SetToggleGroup(toggleGroup);
            }

        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Region currRegion = invalidItems[i];
            GameObject areaItemGO = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content);
            RegionNameplateItem item = areaItemGO.GetComponent<RegionNameplateItem>();
            item.SetObject(currRegion);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.SetAsButton();
            } else {
                item.SetAsToggle();
            }
            item.SetInteractableState(false);
        }
    }
    private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton) {
        Action<string> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToString(onHoverItemAction);
        }
        Action<string> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToString(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            string currString = validItems[i];
            GameObject stringItemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            StringNameplateItem stringItem = stringItemGO.GetComponent<StringNameplateItem>();
            stringItem.SetObject(currString);
            stringItem.SetIdentifier(identifier);
            stringItem.ClearAllOnClickActions();

            stringItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                stringItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            stringItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                stringItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                stringItem.AddOnClickAction(OnPickObject);
                stringItem.SetAsButton();
            } else {
                stringItem.AddOnToggleAction(OnPickObject);
                stringItem.SetAsToggle();
                stringItem.SetToggleGroup(toggleGroup);
            }
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            string currString = invalidItems[i];
            GameObject stringItemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            StringNameplateItem stringItem = stringItemGO.GetComponent<StringNameplateItem>();
            stringItem.SetObject(currString);
            stringItem.SetIdentifier(identifier);
            stringItem.ClearAllOnClickActions();

            stringItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                stringItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            stringItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                stringItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                stringItem.SetAsButton();
            } else {
                stringItem.SetAsToggle();
            }
            stringItem.SetInteractableState(false);
        }
    }
    private void ShowSummonItems<T>(List<SummonSlot> validItems, List<SummonSlot> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton) {
        Action<SummonSlot> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToSummonSlot(onHoverItemAction);
        }
        Action<SummonSlot> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToSummonSlot(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            SummonSlot currSummonSlot = validItems[i];
            GameObject summonSlotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerSummonSlotItemPrefab.name, objectPickerScrollView.content);
            SummonSlotPickerItem item = summonSlotItemGO.GetComponent<SummonSlotPickerItem>();
            item.SetObject(currSummonSlot);

            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.AddOnClickAction(OnPickObject);
                item.SetAsButton();
            } else {
                item.AddOnToggleAction(OnPickObject);
                item.SetAsToggle();
                item.SetToggleGroup(toggleGroup);
            }

        }
        for (int i = 0; i < invalidItems.Count; i++) {
            SummonSlot currSummonSlot = invalidItems[i];
            GameObject summonSlotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerSummonSlotItemPrefab.name, objectPickerScrollView.content);
            SummonSlotPickerItem item = summonSlotItemGO.GetComponent<SummonSlotPickerItem>();
            item.SetObject(currSummonSlot);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.SetAsButton();
            } else {
                item.SetAsToggle();
            }
            item.SetInteractableState(false);
        }
    }
    private void ShowArtifactItems<T>(List<Artifact> validItems, List<Artifact> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton) {
        Action<Artifact> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToArtifact(onHoverItemAction);
        }
        Action<Artifact> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToArtifact(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Artifact currSlot = validItems[i];
            GameObject slotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerArtifactItemPrefab.name, objectPickerScrollView.content);
            ArtifactPickerItem item = slotItemGO.GetComponent<ArtifactPickerItem>();
            item.SetObject(currSlot);

            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.AddOnClickAction(OnPickObject);
                item.SetAsButton();
            } else {
                item.AddOnToggleAction(OnPickObject);
                item.SetAsToggle();
                item.SetToggleGroup(toggleGroup);
            }
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Artifact currSlot = invalidItems[i];
            GameObject slotItemGO = UIManager.Instance.InstantiateUIObject(objectPickerArtifactItemPrefab.name, objectPickerScrollView.content);
            ArtifactPickerItem item = slotItemGO.GetComponent<ArtifactPickerItem>();
            item.SetObject(currSlot);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.SetAsButton();
            } else {
                item.SetAsToggle();
            }
            item.SetInteractableState(false);
        }
    }
    private void ShowEnumItems<T>(List<Enum> validItems, List<Enum> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, Func<string, Sprite> portraitGetter, bool asButton) {
        Action<Enum> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToEnum(onHoverItemAction);
        }
        Action<Enum> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToEnum(onHoverExitItemAction);
        }
        
        for (int i = 0; i < invalidItems.Count; i++) {
            Enum enumerator = invalidItems[i];
            GameObject itemGO = UIManager.Instance.InstantiateUIObject(objectPickerEnumItemPrefab.name, objectPickerScrollView.content);
            EnumNameplateItem item = itemGO.GetComponent<EnumNameplateItem>();
            item.SetObject(enumerator);
            item.ClearAllOnClickActions();

            item.SetPortrait(portraitGetter?.Invoke(enumerator.ToString()));
            
            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.SetAsButton();
            } else {
                item.SetAsToggle();
            }
            item.SetInteractableState(false);
            if (item.isLocked) {
                item.transform.SetAsLastSibling();
            } else {
                item.transform.SetAsFirstSibling();
            }
        }
        
        for (int i = 0; i < validItems.Count; i++) {
            Enum enumerator = validItems[i];
            GameObject itemGO = UIManager.Instance.InstantiateUIObject(objectPickerEnumItemPrefab.name, objectPickerScrollView.content);
            EnumNameplateItem item = itemGO.GetComponent<EnumNameplateItem>();
            item.SetObject(enumerator);
            item.ClearAllOnClickActions();

            item.SetPortrait(portraitGetter?.Invoke(enumerator.ToString()));

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.AddOnClickAction(OnPickObject);
                item.SetAsButton();
            } else {
                item.AddOnToggleAction(OnPickObject);
                item.SetAsToggle();
                item.SetToggleGroup(toggleGroup);
            }
            item.transform.SetAsFirstSibling();
        }
    }
    private void ShowRaceClassItems<T>(List<RaceClass> validItems, List<RaceClass> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton) {
        Action<RaceClass> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToRaceClass(onHoverItemAction);
        }
        Action<RaceClass> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToRaceClass(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            RaceClass obj = validItems[i];
            GameObject itemGO = UIManager.Instance.InstantiateUIObject(objectPickerRaceClassItemPrefab.name, objectPickerScrollView.content);
            RaceClassNameplate item = itemGO.GetComponent<RaceClassNameplate>();
            item.SetObject(obj);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.AddOnClickAction(OnPickObject);
                item.SetAsButton();
            } else {
                item.AddOnToggleAction(OnPickObject);
                item.SetAsToggle();
                item.SetToggleGroup(toggleGroup);
            }
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            RaceClass obj = invalidItems[i];
            GameObject itemGO = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content);
            RaceClassNameplate item = itemGO.GetComponent<RaceClassNameplate>();
            item.SetObject(obj);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            if (asButton) {
                item.SetAsButton();
            } else {
                item.SetAsToggle();
            }
            item.SetInteractableState(false);
        }
    }
    #endregion

    #region Utilities
    private void UpdateConfirmBtnState() {
        confirmBtn.interactable = pickedObj != null;
    }
    public void OnPickObject(object obj, bool isOn) {
        if (isOn) {
            pickedObj = obj;
            if (_shouldConfirmOnPick) {
                OnClickConfirm();
            }
        } else {
            if (pickedObj == obj) {
                pickedObj = null;
            }
        }
    }
    public void OnPickObject(object obj) {
        pickedObj = obj;
        if (pickedObj != null && _shouldConfirmOnPick) {
            OnClickConfirm();
        }
    }
    public void OnPickObject(RaceClass obj) {
        pickedObj = obj;
        if (pickedObj != null && _shouldConfirmOnPick) {
            OnClickConfirm();
        }
    }
    public void OnPickObject(RaceClass obj, bool isOn) {
        if (isOn) {
            pickedObj = obj;
            if (_shouldConfirmOnPick) {
                OnClickConfirm();
            }
        } else {
            if (obj.Equals(pickedObj)) {
                pickedObj = null;
            }
        }
    }
    public void OnClickConfirm() {
        onConfirmAction.Invoke(pickedObj);
    }
    #endregion


    #region Converters
    public Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
    public Action<SummonSlot> ConvertToSummonSlot<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<SummonSlot>(o => myActionT((T)(object)o));
    }
    public Action<Artifact> ConvertToArtifact<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Artifact>(o => myActionT((T)(object)o));
    }
    public Action<Minion> ConvertToMinion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Minion>(o => myActionT((T)(object)o));
    }
    public Action<NPCSettlement> ConvertToArea<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<NPCSettlement>(o => myActionT((T)(object)o));
    }
    public Action<Region> ConvertToRegion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Region>(o => myActionT((T)(object)o));
    }
    public Action<string> ConvertToString<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<string>(o => myActionT((T)(object)o));
    }
    public Action<Enum> ConvertToEnum<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Enum>(o => myActionT((T)(object)o));
    }
    public Action<RaceClass> ConvertToRaceClass<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<RaceClass>(o => myActionT((T)(object)o));
    }
    #endregion
}


