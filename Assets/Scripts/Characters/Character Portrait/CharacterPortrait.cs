using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;


public class CharacterPortrait : PooledObject, IPointerClickHandler {

    private Character _character;
    private PortraitSettings _portraitSettings;
    private Sprite _portraitSprite;

    public bool ignoreInteractions = false;

    private PointerEventData.InputButton interactionBtn = PointerEventData.InputButton.Left;

    [Header("BG")]
    [SerializeField] private Image baseBG;
    [SerializeField] private TextMeshProUGUI lvlTxt;
    [SerializeField] private GameObject lvlGO;

    [Header("Face")]
    [SerializeField] private Image head;
    [SerializeField] private Image brows;
    [SerializeField] private Image eyes;
    [SerializeField] private Image mouth;
    [SerializeField] private Image nose;
    [SerializeField] private Image hair;
    [SerializeField] private Image mustache;
    [SerializeField] private Image beard;
    [SerializeField] private Image ears;
    [SerializeField] private Image wholeImage;

    [Header("Other")]
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private GameObject hoverObj;
    [SerializeField] private GameObject leaderIcon;

    private System.Action _onClickAction;
    private bool _isSubscribedToListeners;
    
    #region getters/setters
    public Character character => _character;
    #endregion

    private bool isPixelPerfect;

    private void OnEnable() {
        Messenger.AddListener(CharacterSignals.CHARACTER_INFO_REVEALED, UpdateLeaderIcon);
        SubscribeListeners();
        UpdateLeaderIcon();
    }
    public void GeneratePortrait(PortraitSettings portraitSettings, bool makePixelPerfect = true) {
        _portraitSettings = portraitSettings;
        UpdatePortrait(makePixelPerfect);
        UpdateLeaderIcon();
    }
    public void GeneratePortrait(Character character, bool makePixelPerfect = true) {
        _character = character;
        _portraitSettings = character.visuals.portraitSettings;
        _portraitSprite = null;
        UpdatePortrait(makePixelPerfect);
        UpdateLeaderIcon();
    }
    public void GeneratePortrait(SUMMON_TYPE p_monsterType, bool makePixelPerfect = true) {
        _portraitSprite = CharacterManager.Instance.GetOrCreateCharacterClassData(CharacterManager.Instance.GetSummonSettings(p_monsterType).className)?.portraitSprite;
        UpdatePortrait(makePixelPerfect);
        UpdateLeaderIcon();
    }
    public void GeneratePortrait(MINION_TYPE p_demonType, bool makePixelPerfect = true) {
        _portraitSprite = CharacterManager.Instance.GetOrCreateCharacterClassData(CharacterManager.Instance.GetMinionSettings(p_demonType).className)?.portraitSprite;
        UpdatePortrait(makePixelPerfect);
        UpdateLeaderIcon();
    }

    private void UpdatePortrait(bool makePixelPerfect) {
        isPixelPerfect = makePixelPerfect;
        if(!string.IsNullOrEmpty(_portraitSettings.className)) {
            _portraitSprite = CharacterManager.Instance.GetOrCreateCharacterClassData(_portraitSettings.className)?.portraitSprite;
        }
        if (_portraitSprite != null) {
            //use portrait sprite directly
            //use whole image
            SetWholeImageSprite(_portraitSprite);
            if (character != null) {
                SetWholeImageMaterial(character.visuals.wholeImageMaterial);
            }
            SetWholeImageState(true);
            SetFaceObjectStates(false);
        } else {
            SetWholeImageSprite(null);
            SetWholeImageState(false);

            if(character != null) {
                SetHairMaterial(character.visuals.hairUIMaterial);
            }

            SetPortraitAsset("head", _portraitSettings.head, _portraitSettings.race, _portraitSettings.gender, head);
            SetPortraitAsset("brows", _portraitSettings.brows, _portraitSettings.race, _portraitSettings.gender, brows);
            SetPortraitAsset("eyes", _portraitSettings.eyes, _portraitSettings.race, _portraitSettings.gender, eyes);
            SetPortraitAsset("mouth", _portraitSettings.mouth, _portraitSettings.race, _portraitSettings.gender, mouth);
            SetPortraitAsset("nose", _portraitSettings.nose, _portraitSettings.race, _portraitSettings.gender, nose);
            SetPortraitAsset("hair", _portraitSettings.hair, _portraitSettings.race, _portraitSettings.gender, hair);
            SetPortraitAsset("mustache", _portraitSettings.mustache, _portraitSettings.race, _portraitSettings.gender, mustache);
            SetPortraitAsset("beard", _portraitSettings.beard, _portraitSettings.race, _portraitSettings.gender, beard);
            SetPortraitAsset("ears", _portraitSettings.ears, _portraitSettings.race, _portraitSettings.gender, ears);

            // if (makePixelPerfect) {
            //     head.SetNativeSize();
            //     brows.SetNativeSize();
            //     eyes.SetNativeSize();
            //     mouth.SetNativeSize();
            //     nose.SetNativeSize();
            //     hair.SetNativeSize();
            //     mustache.SetNativeSize();
            //     beard.SetNativeSize();
            //     ears.SetNativeSize();
            //
            //     head.rectTransform.anchorMin = Vector2.zero;
            //     head.rectTransform.anchorMax = Vector2.one;
            //
            //     // head.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // brows.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // eyes.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // mouth.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // nose.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // hair.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // mustache.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // beard.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            //     // ears.rectTransform.anchoredPosition = new Vector2(55f, 55f);
            // }
        }
        // UpdateFrame();
        UpdateFactionEmblem();

        wholeImage.rectTransform.SetSiblingIndex(0);
        head.rectTransform.SetSiblingIndex(1);
        brows.rectTransform.SetSiblingIndex(2);
        eyes.rectTransform.SetSiblingIndex(3);
        hair.rectTransform.SetSiblingIndex(4);
        ears.rectTransform.SetSiblingIndex(5);
        beard.rectTransform.SetSiblingIndex(6);
        mouth.rectTransform.SetSiblingIndex(7);
        nose.rectTransform.SetSiblingIndex(8);
        mustache.rectTransform.SetSiblingIndex(9);
        lvlGO.SetActive(false);
    }

    #region Utilities
    private void SetWholeImageSprite(Sprite sprite) {
        wholeImage.sprite = sprite;
    }
    public void SetAsDefaultMinion() {
        SetWholeImageSprite(CharacterManager.Instance.GetOrCreateCharacterClassData("Wrath").portraitSprite);
        SetWholeImageState(true);
        SetFaceObjectStates(false);
        lvlGO.SetActive(false);
        factionEmblem.SetFaction(PlayerManager.Instance.player.playerFaction);
        leaderIcon.SetActive(false);
    }
    private void SetWholeImageState(bool state) {
        wholeImage.gameObject.SetActive(state);
    }
    public Color GetHairColor() {
        return hair.color;
    }
    private void UpdateFrame() {
        if (_character != null) {
            PortraitFrame frame = null;
            if (_character.isFactionLeader || _character.isSettlementRuler) {
                frame = CharacterManager.Instance.GetPortraitFrame(CHARACTER_ROLE.LEADER);
            } else { //if(character)
                frame = CharacterManager.Instance.GetPortraitFrame(CHARACTER_ROLE.SOLDIER);
                // frame = CharacterManager.Instance.GetPortraitFrame(_character.role.roleType);
            }
            baseBG.sprite = frame.baseBG;

            SetBaseBGState(true);
        }
    }
    public void SetBaseBGState(bool state) {
        baseBG.gameObject.SetActive(state);
    }
    public void ShowCharacterInfo() {
        if(_character != null) {
            UIManager.Instance.ShowSmallInfo(_character.name);
        }
    }
    public void HideCharacterInfo() {
        if (_character != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    public void SetImageRaycastTargetState(bool state) {
        Image[] targets = this.GetComponentsInChildren<Image>();
        for (int i = 0; i < targets.Length; i++) {
            Image currImage = targets[i];
            currImage.raycastTarget = state;
        }
    }
    #endregion

    #region Pointer Actions
    public void AddPointerClickAction(System.Action p_action) {
        _onClickAction += p_action;
    }
    public void OnPointerClick(PointerEventData eventData) {
        if (ignoreInteractions) {
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left) {
            OnLeftClick();
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            OnRightClick();
        }
    }
    public void OnClick(BaseEventData eventData) {
        if (ignoreInteractions || !gameObject.activeSelf) {
            return;
        }
        OnPointerClick(eventData as PointerEventData);
    }
    public void OnLeftClick() {
        if (_onClickAction != null) {
            _onClickAction?.Invoke();
        } else {
            ShowCharacterMenu();    
        }
    }
    private void OnRightClick() {
        if (_character != null) {
            UIManager.Instance.ShowPlayerActionContextMenu(_character.isLycanthrope ? _character.lycanData.activeForm : _character, Input.mousePosition, true);
        }
    }
    public void SetHoverHighlightState(bool state) {
        hoverObj.SetActive(state);
    }
    private void ShowCharacterMenu() {
        if (_character != null) {
            UIManager.Instance.ShowCharacterInfo(_character, true);
        }
    }
    public void OnHoverEnterSuccessor() {
        if (ignoreInteractions) {
            return;
        }
        SetHoverHighlightState(true);
        if(character != null && character.faction != null) {
            int totalWeights = character.faction.successionComponent.GetTotalWeightsOfSuccessors();
            int weight = character.faction.successionComponent.GetWeightOfSuccessor(character);
            float chance = (weight / (float) totalWeights) * 100f;
            string text = $"{chance.ToString("N1")}% chance to be the next Faction Leader";
            UIManager.Instance.ShowSmallInfo(text, header: character.visuals.GetCharacterNameWithIconAndColor(), autoReplaceText: false);
        }
    }
    public void OnHoverExitSuccessor() {
        if (ignoreInteractions) {
            return;
        }
        SetHoverHighlightState(false);
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Body Parts
    private void SetPortraitAsset(string identifier, int index, RACE race, GENDER gender, Image renderer) {
        if (CharacterManager.Instance.TryGetPortraitSprite(identifier, index, race, gender, out var sprite)) {
            renderer.sprite = sprite;
            renderer.gameObject.SetActive(true);
        } else {
            renderer.gameObject.SetActive(false);    
        }
    }
    private void SetFaceObjectStates(bool state) {
        head.gameObject.SetActive(state);
        brows.gameObject.SetActive(state);
        eyes.gameObject.SetActive(state);
        mouth.gameObject.SetActive(state);
        nose.gameObject.SetActive(state);
        hair.gameObject.SetActive(state);
        mustache.gameObject.SetActive(state);
        beard.gameObject.SetActive(state);
        ears.gameObject.SetActive(state);
    }
    #endregion

    #region Listeners
    private void SubscribeListeners() {
        if (_isSubscribedToListeners) { return; }
        _isSubscribedToListeners = true;
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, OnFactionSet);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(CharacterSignals.ROLE_CHANGED, OnCharacterChangedRole);
        Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
        Messenger.AddListener<Character, Character>(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
        Messenger.AddListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.AddListener<NPCSettlement, Character>(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, OnSettlementRulerRemoved);
    }
    private void RemoveListeners() {
        _isSubscribedToListeners = false;
        Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, OnFactionSet);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.RemoveListener<Character>(CharacterSignals.ROLE_CHANGED, OnCharacterChangedRole);
        Messenger.RemoveListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
        Messenger.RemoveListener<Character, Character>(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
        Messenger.RemoveListener<Faction, ILeader>(CharacterSignals.ON_FACTION_LEADER_REMOVED, OnFactionLeaderRemoved);
        Messenger.RemoveListener<NPCSettlement, Character>(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, OnSettlementRulerRemoved);
    }
    #endregion

    #region Pooled Object
    public override void Reset() {
        base.Reset();
        _character = null;
        _onClickAction = null;
        ignoreInteractions = false;
        _portraitSprite = null;
        RemoveListeners();
    }
    #endregion

    #region Faction
    public void OnFactionSet(Character character) {
        if (_character != null && _character.id == character.id) {
            UpdateFactionEmblem();
        }
    }
    private void UpdateFactionEmblem() {
        if (_character != null) {
            factionEmblem.SetFaction(_character.faction);
            // factionEmblem.gameObject.SetActive(true);
        } else {
            factionEmblem.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Shader
    private void SetHairMaterial(Material material) {
        hair.material = material;
        mustache.material = material;
        beard.material = material;
    }
    private void SetWholeImageMaterial(Material material) {
        wholeImage.material = material;
    }
    #endregion

    #region Leader Icon
    private void UpdateLeaderIcon() {
        if (character != null) {
            leaderIcon.SetActive(character.isFactionLeader || character.isSettlementRuler);
        } else {
            leaderIcon.SetActive(false);
        }
    }
    public void OnHoverLeaderIcon() {
        if (character != null) {
            string message = string.Empty;
            if (character.isSettlementRuler) {
                message = $"<b>{character.name}</b> is the Settlement Ruler of <b>{character.homeSettlement.name}</b>\n";
            } 
            if (character.isFactionLeader) {
                message += $"<b>{character.name}</b> is the Faction Leader of <b>{character.faction.name}</b>";
            }
            UIManager.Instance.ShowSmallInfo(message);    
        }
    }
    public void OnHoverExitLeaderIcon() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    public void OnCharacterChangedRace(Character character) {
        if (_character != null && _character.id == character.id) {
            GeneratePortrait(character, isPixelPerfect);
        }
    }
    private void OnCharacterChangedRole(Character character) {
        if (_character != null && _character.id == character.id) {
            GeneratePortrait(character, isPixelPerfect);
        }
    }
    private void OnCharacterSetAsFactionLeader(Character character, ILeader previousLeader) {
        if (_character != null && _character == character) {
            // UpdateFrame();
            GeneratePortrait(character, isPixelPerfect);
        }
    }
    private void OnCharacterSetAsSettlementRuler(Character character, Character previousRuler) {
        if (_character != null && _character == character) {
            // UpdateFrame();
            GeneratePortrait(character, isPixelPerfect);
        }
    }
    private void OnFactionLeaderRemoved(Faction faction, ILeader newLeader) {
        if (_character != null && _character == newLeader) {
            UpdateLeaderIcon();
        }
    }
    private void OnSettlementRulerRemoved(NPCSettlement settlement, Character previousLeader) {
        if (previousLeader == character) {
            UpdateLeaderIcon();
        }
    }
}
