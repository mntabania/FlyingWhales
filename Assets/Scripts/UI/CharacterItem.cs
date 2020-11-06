
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterItem : PooledObject {

    public Character character { get; private set; }

    public CharacterPortrait portrait;
    [SerializeField] private RectTransform thisTrans;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private GameObject coverGO;
    [SerializeField] private Button itemBtn;
    [SerializeField] private Toggle toggle;
    [SerializeField] private EventTrigger toggleEventTrigger;
    public HoverHandler hoverHandler;

    private List<System.Action> _toggleClickActions;

    public List<System.Action> toggleClickActions {
        get {
            if (_toggleClickActions == null) {
                _toggleClickActions = new List<System.Action>();
            }
            return _toggleClickActions;
        }
    }
    public bool coverState {
        get { return coverGO.activeSelf; }
    }

    public virtual void SetCharacter(Character character) {
        this.character = character;
        UpdateInfo();
        AddOnClickAction(ShowCharacterInfo, true);
       
    }
    private void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }
    protected virtual void UpdateInfo() {
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = character.characterClass.className;       
    }

    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        UIManager.Instance.ShowSmallInfo(character.name);
        //if (character.currentParty.characters.Count > 1) {
        //    UIManager.Instance.ShowSmallInfo(character.currentParty.name);
        //} else {
        //    UIManager.Instance.ShowSmallInfo(character.name);
        //}
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        ResetToggle();
        ClearClickActions();
    }

    public void SetCoverState(bool state, bool blockRaycasts = false) {
        coverGO.SetActive(state);
        if (state) { //only block raycasts if active
            coverGO.GetComponent<Image>().raycastTarget = blockRaycasts;
        }
    }

    #region Click Action
    public void AddOnClickAction(System.Action onClick, bool clearAllOtherActions = false) {
        if (clearAllOtherActions) {
            itemBtn.onClick.RemoveAllListeners();
        }
        itemBtn.onClick.AddListener(onClick.Invoke);
    }
    public void ClearClickActions() {
        itemBtn.onClick.RemoveAllListeners();
    }
    public void AddOnToggleAction(System.Action onClick, bool clearAllOtherActions = false) {
        if (clearAllOtherActions) {
            toggleClickActions.Clear();
        }
        toggleClickActions.Add(onClick);
    }
    public void OnToggleClick() {
        for (int i = 0; i < _toggleClickActions.Count; i++) {
            _toggleClickActions[i].Invoke();
        }
    }
    #endregion

    public void SetAsToggle(ToggleGroup group) {
        itemBtn.gameObject.SetActive(false);
        toggle.gameObject.SetActive(true);
        toggle.group = group;
    }
    public void ResetToggle() {
        toggle.group = null;
        toggle.isOn = false;
    }
    public void SetAsButton() {
        itemBtn.gameObject.SetActive(true);
        toggle.gameObject.SetActive(false);
    }
    public void SetToggleState(bool state) {
        toggle.isOn = state;
    }
}
