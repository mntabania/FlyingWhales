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

    private System.Action onClickAction;
    private bool _isSubscribedToListeners;
    #region getters/setters
    public Character character => _character;
    #endregion

    private bool isPixelPerfect;

    private void OnEnable() {
        //Messenger.AddListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        SubscribeListeners();
    }
    public void GeneratePortrait(PortraitSettings portraitSettings, bool makePixelPerfect = true) {
        _portraitSettings = portraitSettings;
        UpdatePortrait(makePixelPerfect);
    }
    public void GeneratePortrait(Character character, bool makePixelPerfect = true) {
        _character = character;
        _portraitSettings = character.visuals.portraitSettings;
        UpdatePortrait(makePixelPerfect);
    }

    private void UpdatePortrait(bool makePixelPerfect) {
        isPixelPerfect = makePixelPerfect;

        if (string.IsNullOrEmpty(_portraitSettings.wholeImage) == false) {
            //use whole image
            SetWholeImageSprite(CharacterManager.Instance.GetWholeImagePortraitSprite(_portraitSettings.wholeImage));
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
        SetWholeImageSprite(CharacterManager.Instance.GetWholeImagePortraitSprite("Wrath"));
        SetWholeImageState(true);
        SetFaceObjectStates(false);
        lvlGO.SetActive(false);
        factionEmblem.SetFaction(PlayerManager.Instance.player.playerFaction);
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
    public void OnPointerClick(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        if (ignoreInteractions) {
            return;
        }
        if (eventData.button == interactionBtn) {
            OnClick();
        }
        
#endif
    }
    public void OnClick(BaseEventData eventData) {
        if (ignoreInteractions || !gameObject.activeSelf) {
            return;
        }
        OnPointerClick(eventData as PointerEventData);
    }
    public void OnClick() {
        ShowCharacterMenu();
    }
    public void SetHoverHighlightState(bool state) {
        hoverObj.SetActive(state);
    }
    private void ShowCharacterMenu() {
        if (_character != null) {
            UIManager.Instance.ShowCharacterInfo(_character, true);
        }
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
        Messenger.AddListener<Character>(Signals.FACTION_SET, OnFactionSet);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character>(Signals.ROLE_CHANGED, OnCharacterChangedRole);
        Messenger.AddListener<Character>(Signals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
        Messenger.AddListener<Character>(Signals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
    }
    private void RemoveListeners() {
        _isSubscribedToListeners = false;
        Messenger.RemoveListener<Character>(Signals.FACTION_SET, OnFactionSet);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.RemoveListener<Character>(Signals.ROLE_CHANGED, OnCharacterChangedRole);
        Messenger.RemoveListener<Character>(Signals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
        Messenger.RemoveListener<Character>(Signals.ON_SET_AS_SETTLEMENT_RULER, OnCharacterSetAsSettlementRuler);
    }
    #endregion

    #region Pooled Object
    public override void Reset() {
        base.Reset();
        _character = null;
        ignoreInteractions = false;
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
    private void OnCharacterSetAsFactionLeader(Character character) {
        if (_character != null && _character == character) {
            // UpdateFrame();
        }
    }
    private void OnCharacterSetAsSettlementRuler(Character character) {
        if (_character != null && _character == character) {
            // UpdateFrame();
        }
    }
}
