using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DualObjectPicker : PopupMenuBase {

    [Header("Main")]
    [SerializeField] private Button confirmBtn;
    [SerializeField] private TextMeshProUGUI confirmBtnLbl;

    [Header("Column 1")]
    public ScrollRect column1ScrollView;
    public ToggleGroup column1ToggleGroup;
    public TextMeshProUGUI column1TitleLbl;

    [Header("Column 2")]
    public ScrollRect column2ScrollView;
    public ToggleGroup column2ToggleGroup;
    public TextMeshProUGUI column2TitleLbl;

    [Header("Tabs")]
    [SerializeField] private RectTransform tabsParent;
    [SerializeField] private GameObject tabPrefab;
    [SerializeField] private ToggleGroup tabToggleGroup;

    [Header("Misc")]
    [SerializeField] private UIHoverPosition minionCardPos;

    private object _pickedObj1;
    private object _pickedObj2;
    private System.Action<object, object> onConfirmAction;
    private System.Action<object> onPickFirstObjAction;

    private bool isColumn1Optional; //if column 1 is optional, then pickedObj1 can be null
    private bool isColumn2Optional; //if column 2 is optional, then pickedObj2 can be null


    #region getters/setters
    private object pickedObj1 {
        get { return _pickedObj1; }
        set {
            _pickedObj1 = value;
            UpdateConfirmBtnState();
        }
    }
    private object pickedObj2 {
        get { return _pickedObj2; }
        set {
            _pickedObj2 = value;
            UpdateConfirmBtnState();
        }
    }
    private bool needs2Objects { get { return !isColumn1Optional && !isColumn2Optional; } } //if both columns are not optional then 2 objects are needed
    #endregion

    /// <summary>
    /// Show the dual object picker.
    /// </summary>
    /// <typeparam name="T">The type of objects to be shown in column 1.</typeparam>
    /// <typeparam name="U">The type of objects to be shown in column 2.</typeparam>
    /// <param name="column1Items">The list of items to be shown on column 1.</param>
    /// <param name="column2Items">The list of items to be shown on column 2.</param>
    /// <param name="column1Title">The title of column 1.</param>
    /// <param name="column2Title">The title of column 2.</param>
    /// <param name="column1ValidityChecker">Function to check if an item in column 1 should be pickable.</param>
    /// <param name="column2ValidityChecker">Function to check if an item in column 2 should be pickable.</param>
    /// <param name="onClickConfirmAction">Function to execute when the player has picked 2 items from the 2 lists.</param>
    public void ShowDualObjectPicker<T, U>(List<T> column1Items, List<U> column2Items,
        string column1Title, string column2Title,
        Func<T, bool> column1ValidityChecker, Func<U, bool> column2ValidityChecker,
        Action<T> column1ItemHoverEnterAction, Action<U> column2ItemHoverEnterAction,
        Action<T> column1ItemHoverExitAction, Action<U> column2ItemHoverExitAction,
        Action<object, object> onClickConfirmAction,
        string confirmBtnStr, 
        bool column1Optional = false, bool column2Optional = false,
        string column1Identifier = "", string column2Identifier = "") {

        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        //destroy existing items
        UtilityScripts.Utilities.DestroyChildren(column1ScrollView.content);
        UtilityScripts.Utilities.DestroyChildren(column2ScrollView.content);

        //set titles
        column1TitleLbl.text = string.Empty;
        column2TitleLbl.text = string.Empty;

        //reset values
        pickedObj1 = null;
        pickedObj2 = null;

        onConfirmAction = onClickConfirmAction;
        confirmBtnLbl.text = confirmBtnStr;

        onPickFirstObjAction = null;

        isColumn1Optional = column1Optional;
        isColumn2Optional = column2Optional;

        //always allow switch off
        column1ToggleGroup.allowSwitchOff = true;
        column2ToggleGroup.allowSwitchOff = true;

        //populate column 1
        PopulateColumn(column1Items, column1ValidityChecker, column1ItemHoverEnterAction, column1ItemHoverExitAction, column1ScrollView, column1ToggleGroup, column1Title, column1Identifier);

        //populate column 2
        PopulateColumn(column2Items, column2ValidityChecker, column2ItemHoverEnterAction, column2ItemHoverExitAction, column2ScrollView, column2ToggleGroup, column2Title, column2Identifier);

        base.Open();
    }

    /// <summary>
    /// Show dual object picker but initially, only populate the first column.
    /// This is used for cases when the content of the second column is dependent upon the first chosen item.
    /// NOTE: onPickFirstObjAction is the action called once an object in the first column is picked.
    /// </summary>
    public void ShowDualObjectPicker<T>(List<T> column1Items,
        string column1Title,
        Func<T, bool> column1ValidityChecker, 
        Action<T> column1ItemHoverEnterAction, 
        Action<T> column1ItemHoverExitAction,
        Action<object> onPickFirstObjAction,
        Action<object, object> onClickConfirmAction,
        string confirmBtnStr, 
        bool column1Optional = false, bool column2Optional = false) {

        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        //destroy existing items
        UtilityScripts.Utilities.DestroyChildren(column1ScrollView.content);
        UtilityScripts.Utilities.DestroyChildren(column2ScrollView.content);

        //set titles
        column1TitleLbl.text = string.Empty;
        column2TitleLbl.text = string.Empty;

        //reset values
        pickedObj1 = null;
        pickedObj2 = null;

        onConfirmAction = onClickConfirmAction;
        confirmBtnLbl.text = confirmBtnStr;

        this.onPickFirstObjAction = onPickFirstObjAction;

        isColumn1Optional = column1Optional;
        isColumn2Optional = column2Optional;

        //always allow switch off
        column1ToggleGroup.allowSwitchOff = true;
        column2ToggleGroup.allowSwitchOff = true;

        //populate column 1
        PopulateColumn(column1Items, column1ValidityChecker, column1ItemHoverEnterAction, column1ItemHoverExitAction, column1ScrollView, column1ToggleGroup, column1Title);

        base.Open();
    }
    
    public void ShowDualObjectPicker(DualObjectPickerTabSetting[] tabs) {
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        //destroy existing items
        UtilityScripts.Utilities.DestroyChildren(column1ScrollView.content);
        UtilityScripts.Utilities.DestroyChildren(column2ScrollView.content);
        UtilityScripts.Utilities.DestroyChildren(tabsParent);

        for (int i = 0; i < tabs.Length; i++) {
            DualObjectPickerTabSetting currTab = tabs[i];
            GameObject tabGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(tabPrefab.name, Vector3.zero, Quaternion.identity, tabsParent);
            DualObjectPickerTab tab = tabGO.GetComponent<DualObjectPickerTab>();
            tab.Initialize(currTab, tabToggleGroup);
        }
        base.Open();
    }

    public override void Close() {
        UtilityScripts.Utilities.DestroyChildren(tabsParent);
        UIManager.Instance.ResumeLastProgressionSpeed();
        base.Close();
    }

    public void OnClickConfirm() {
        onConfirmAction?.Invoke(pickedObj1, pickedObj2);
        Close();
    }

    public void PopulateColumn<T>(List<T> items, Func<T, bool> validityChecker, Action<T> hoverEnterAction, Action<T> hoverExitAction, ScrollRect column, ToggleGroup toggleGroup, string columnTitle, string identifier = "") {
        UtilityScripts.Utilities.DestroyChildren(column.content);
        List<T> validItems;
        List<T> invalidItems;

        if (column == column1ScrollView) {
            column1TitleLbl.text = columnTitle;
        } else if (column == column2ScrollView) {
            column2TitleLbl.text = columnTitle;
        }

        OrganizeList(items, out validItems, out invalidItems, validityChecker);
        if (typeof(T) == typeof(Character)) {
            ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        } else if (typeof(T) == typeof(string)) {
            ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup, identifier);
        } else if (typeof(T) == typeof(UnsummonedMinionData)) {
            ShowUnsummonedMinionItems(validItems.Cast<UnsummonedMinionData>().ToList(), invalidItems.Cast<UnsummonedMinionData>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        } else if (typeof(T) == typeof(Region)) {
            if (identifier == "WorldEvent") {
                ShowWorldEventItems(validItems.Cast<Region>().ToList(), invalidItems.Cast<Region>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
            }
        } else if (typeof(T) == typeof(Faction)) {
            ShowFactionItems(validItems.Cast<Faction>().ToList(), invalidItems.Cast<Faction>().ToList(), hoverEnterAction, hoverExitAction, column, toggleGroup);
        }
    }

    #region Instantiators
    private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
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
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.characterNameplatePrefab.name, column.content);
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
            characterItem.SetAsToggle();
            characterItem.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
            characterItem.SetToggleGroup(toggleGroup);
            characterItem.SetInteractableState(true);
            characterItem.SetPortraitInteractableState(false);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Character currCharacter = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.characterNameplatePrefab.name, column.content);
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
            characterItem.SetInteractableState(false);
        }
    }
    private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup, string identifier) {
        Action<string> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToString(onHoverItemAction);
        }
        Action<string> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToString(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            string currStr = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.stringNameplatePrefab.name, column.content);
            StringNameplateItem item = characterItemGO.GetComponent<StringNameplateItem>();
            item.SetObject(currStr);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetAsToggle();
            item.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
            item.SetToggleGroup(toggleGroup);
            item.SetInteractableState(true);
            item.SetIdentifier(identifier);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            string currStr = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.stringNameplatePrefab.name, column.content);
            StringNameplateItem item = characterItemGO.GetComponent<StringNameplateItem>();
            item.SetObject(currStr);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetInteractableState(false);
            item.SetIdentifier(identifier);
        }
    }
    private void ShowUnsummonedMinionItems<T>(List<UnsummonedMinionData> validItems, List<UnsummonedMinionData> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
        //Action<UnsummonedMinionData> convertedHoverAction = null;
        //if (onHoverItemAction != null) {
        //    convertedHoverAction = ConvertToUnsummonedMinion(onHoverItemAction);
        //}
        //Action<UnsummonedMinionData> convertedHoverExitAction = null;
        //if (onHoverExitItemAction != null) {
        //    convertedHoverExitAction = ConvertToUnsummonedMinion(onHoverExitItemAction);
        //}
        //for (int i = 0; i < validItems.Count; i++) {
        //    UnsummonedMinionData minion = validItems[i];
        //    GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.unsummonedMinionNameplatePrefab.name, column.content);
        //    UnsummonedMinionNameplateItem item = characterItemGO.GetComponent<UnsummonedMinionNameplateItem>();
        //    item.SetObject(minion);
        //    item.ClearAllOnClickActions();

        //    item.ClearAllHoverEnterActions();
        //    if (convertedHoverAction != null) {
        //        item.AddHoverEnterAction(convertedHoverAction.Invoke);
        //    }

        //    item.ClearAllHoverExitActions();
        //    if (convertedHoverExitAction != null) {
        //        item.AddHoverExitAction(convertedHoverExitAction.Invoke);
        //    }
        //    item.SetAsToggle();
        //    item.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
        //    item.SetToggleGroup(toggleGroup);
        //    item.SetInteractableState(true);
        //    item.AddHoverEnterAction(ShowMinionCardTooltip);
        //    item.AddHoverExitAction(HideMinionCardTooltip);
        //}
        //for (int i = 0; i < invalidItems.Count; i++) {
        //    UnsummonedMinionData minion = invalidItems[i];
        //    GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.unsummonedMinionNameplatePrefab.name, column.content);
        //    UnsummonedMinionNameplateItem characterItem = characterItemGO.GetComponent<UnsummonedMinionNameplateItem>();
        //    characterItem.SetObject(minion);
        //    characterItem.ClearAllOnClickActions();

        //    characterItem.ClearAllHoverEnterActions();
        //    if (convertedHoverAction != null) {
        //        characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
        //    }

        //    characterItem.ClearAllHoverExitActions();
        //    if (convertedHoverExitAction != null) {
        //        characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
        //    }
        //    characterItem.SetInteractableState(false);
        //}
    }
    //private void ShowSummonMinionPlayerSkillItems<T>(List<SPELL_TYPE> validItems, List<SPELL_TYPE> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
    //    Action<SPELL_TYPE> convertedHoverAction = null;
    //    if (onHoverItemAction != null) {
    //        convertedHoverAction = ConvertToSummonMinionPlayerSkill(onHoverItemAction);
    //    }
    //    Action<SPELL_TYPE> convertedHoverExitAction = null;
    //    if (onHoverExitItemAction != null) {
    //        convertedHoverExitAction = ConvertToSummonMinionPlayerSkill(onHoverExitItemAction);
    //    }
    //    for (int i = 0; i < validItems.Count; i++) {
    //        SPELL_TYPE playerSkillType = validItems[i];
    //        GameObject itemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.summonMinionPlayerSkillPrefab.name, column.content);
    //        SummonMinionPlayerSkillNameplateItem item = itemGO.GetComponent<SummonMinionPlayerSkillNameplateItem>();
    //        item.SetObject(playerSkillType);
    //        item.ClearAllOnClickActions();

    //        item.ClearAllHoverEnterActions();
    //        if (convertedHoverAction != null) {
    //            item.AddHoverEnterAction(convertedHoverAction.Invoke);
    //        }

    //        item.ClearAllHoverExitActions();
    //        if (convertedHoverExitAction != null) {
    //            item.AddHoverExitAction(convertedHoverExitAction.Invoke);
    //        }
    //        item.SetAsToggle();
    //        item.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
    //        item.SetToggleGroup(toggleGroup);
    //        item.SetInteractableState(true);
    //    }
    //    for (int i = 0; i < invalidItems.Count; i++) {
    //        SPELL_TYPE minion = invalidItems[i];
    //        GameObject itemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.summonMinionPlayerSkillPrefab.name, column.content);
    //        SummonMinionPlayerSkillNameplateItem characterItem = itemGO.GetComponent<SummonMinionPlayerSkillNameplateItem>();
    //        characterItem.SetObject(minion);
    //        characterItem.ClearAllOnClickActions();

    //        characterItem.ClearAllHoverEnterActions();
    //        if (convertedHoverAction != null) {
    //            characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
    //        }

    //        characterItem.ClearAllHoverExitActions();
    //        if (convertedHoverExitAction != null) {
    //            characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
    //        }
    //        characterItem.SetInteractableState(false);
    //    }
    //}
    private void ShowFactionItems<T>(List<Faction> validItems, List<Faction> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
        Action<Faction> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToFaction(onHoverItemAction);
        }
        Action<Faction> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToFaction(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Faction faction = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.factionNameplatePrefab.name, column.content);
            FactionNameplateItem item = characterItemGO.GetComponent<FactionNameplateItem>();
            item.SetObject(faction);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetAsToggle();
            item.AddOnToggleAction((obj, isOn) => OnToggleItem(obj, isOn, column));
            item.SetToggleGroup(toggleGroup);
            item.SetInteractableState(true);
            //item.AddHoverEnterAction(ShowMinionCardTooltip);
            //item.AddHoverExitAction(HideMinionCardTooltip);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Faction faction = invalidItems[i];
            GameObject itemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.factionNameplatePrefab.name, column.content);
            FactionNameplateItem item = itemGO.GetComponent<FactionNameplateItem>();
            item.SetObject(faction);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetInteractableState(false);
        }
    }
    private void ShowMinionCardTooltip(UnsummonedMinionData data) {
        UIManager.Instance.ShowMinionCardTooltip(data, minionCardPos);
    }
    private void HideMinionCardTooltip(UnsummonedMinionData data) {
        UIManager.Instance.HideMinionCardTooltip();
    }
    private void ShowWorldEventItems<T>(List<Region> validItems, List<Region> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, ScrollRect column, ToggleGroup toggleGroup) {
        Action<Region> convertedHoverAction = null;
        if (onHoverItemAction != null) {
            convertedHoverAction = ConvertToRegion(onHoverItemAction);
        }
        Action<Region> convertedHoverExitAction = null;
        if (onHoverExitItemAction != null) {
            convertedHoverExitAction = ConvertToRegion(onHoverExitItemAction);
        }
        for (int i = 0; i < validItems.Count; i++) {
            Region region = validItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.worldEventNameplatePrefab.name, column.content);
            WorldEventNameplate item = characterItemGO.GetComponent<WorldEventNameplate>();
            item.SetObject(region);
            item.ClearAllOnClickActions();

            item.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                item.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            item.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                item.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            item.SetAsToggle();
            item.AddOnToggleAction((character, isOn) => OnToggleItem(character, isOn, column));
            item.SetToggleGroup(toggleGroup);
            item.SetInteractableState(true);
        }
        for (int i = 0; i < invalidItems.Count; i++) {
            Region region = invalidItems[i];
            GameObject characterItemGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.worldEventNameplatePrefab.name, column.content);
            WorldEventNameplate characterItem = characterItemGO.GetComponent<WorldEventNameplate>();
            characterItem.SetObject(region);
            characterItem.ClearAllOnClickActions();

            characterItem.ClearAllHoverEnterActions();
            if (convertedHoverAction != null) {
                characterItem.AddHoverEnterAction(convertedHoverAction.Invoke);
            }

            characterItem.ClearAllHoverExitActions();
            if (convertedHoverExitAction != null) {
                characterItem.AddHoverExitAction(convertedHoverExitAction.Invoke);
            }
            characterItem.SetInteractableState(false);
        }
    }
    #endregion

    #region Utlities
    private void OrganizeList<T>(List<T> items, out List<T> validItems, out List<T> invalidItems, Func<T, bool> validityChecker) {
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
    }
    private void OnToggleItem(object obj, bool isOn, ScrollRect column) {
        if (isOn) {
            if (column == column1ScrollView) {
                pickedObj1 = obj;
                if (onPickFirstObjAction != null) {
                    onPickFirstObjAction.Invoke(pickedObj1);
                }
            } else {
                pickedObj2 = obj;
            }
        } else {
            if (column == column1ScrollView) {
                pickedObj1 = null;
            } else {
                pickedObj2 = null;
            }
        }
    }
    private void UpdateConfirmBtnState() {
        if (needs2Objects) {
            confirmBtn.interactable = pickedObj1 != null && pickedObj2 != null;
        } else {
            bool satisfiesColumn1 = true;
            if (!isColumn1Optional) {
                satisfiesColumn1 = pickedObj1 != null;
            }

            bool satisfiesColumn2 = true;
            if (!isColumn2Optional) {
                satisfiesColumn2 = pickedObj2 != null;
            }

            confirmBtn.interactable = satisfiesColumn1 && satisfiesColumn2;
        }
        
    }
    #endregion

    #region Converters
    private Action<Character> Convert<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Character>(o => myActionT((T)(object)o));
    }
    public Action<string> ConvertToString<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<string>(o => myActionT((T)(object)o));
    }
    public Action<UnsummonedMinionData> ConvertToUnsummonedMinion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<UnsummonedMinionData>(o => myActionT((T)(object)o));
    }
    public Action<Region> ConvertToRegion<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Region>(o => myActionT((T) (object) o));
    }
    public Action<Faction> ConvertToFaction<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<Faction>(o => myActionT((T) (object) o));
    }
    public Action<SPELL_TYPE> ConvertToSummonMinionPlayerSkill<T>(Action<T> myActionT) {
        if (myActionT == null) return null;
        else return new Action<SPELL_TYPE>(o => myActionT((T) (object) o));
    }
    #endregion
}

public class DualObjectPickerTabSetting {
    public string name;
    public System.Action<bool> onToggleTabAction;
}