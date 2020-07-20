using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class CharacterVisuals {
    //Hair HSV Shader Properties
    private static readonly int HairHue = Shader.PropertyToID("_Hue");
    private static readonly int HairSaturation = Shader.PropertyToID("_Saturation");
    private static readonly int HairValue = Shader.PropertyToID("_Value");
    private static readonly int HsvaAdjust = Shader.PropertyToID("_HSVAAdjust");
    
    private Character _owner;
    
    public PortraitSettings portraitSettings { get; private set; }
    public Material hairMaterial { get; private set; }
    public Material hairUIMaterial { get; private set; }
    public Material wholeImageMaterial { get; private set; }
    public Dictionary<string, Sprite> markerAnimations { get; private set; }
    public Sprite defaultSprite { get; private set; }
    public Vector2 selectableSize { get; private set; }
    
    private bool _hasBlood;

    public CharacterVisuals(Character character) {
        _owner = character;
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(character.race, character.gender, character.characterClass.className);
        _hasBlood = true;
        CreateHairMaterial();
        UpdateMarkerAnimations(character);
    }
    public CharacterVisuals(SaveDataCharacter data) {
        portraitSettings = data.portraitSettings;
        _hasBlood = true;
        CreateHairMaterial();
    }
    private void UpdatePortraitSettings(Character character) {
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(character.race, character.gender, character.characterClass.className);
    }
    private void CreateHairMaterial() {
        hairMaterial = Object.Instantiate(CharacterManager.Instance.hsvMaterial);
        hairMaterial.SetFloat(HairHue, portraitSettings.hairColorHue);
        hairMaterial.SetFloat(HairSaturation, portraitSettings.hairColorSaturation);
        hairMaterial.SetFloat(HairValue, portraitSettings.hairColorValue);
        
        hairUIMaterial = Object.Instantiate(CharacterManager.Instance.hairUIMaterial);
        hairUIMaterial.SetVector(HsvaAdjust, new Vector4(portraitSettings.hairColorHue, portraitSettings.hairColorSaturation, portraitSettings.hairColorValue, 0f));
    }
    public void CreateWholeImageMaterial() {
        hairMaterial = Object.Instantiate(CharacterManager.Instance.hsvMaterial);
        hairMaterial.SetFloat(HairHue, portraitSettings.hairColorHue);
        hairMaterial.SetFloat(HairSaturation, portraitSettings.hairColorSaturation);
        hairMaterial.SetFloat(HairValue, portraitSettings.hairColorValue);
        
        hairUIMaterial = Object.Instantiate(CharacterManager.Instance.hairUIMaterial);
        hairUIMaterial.SetVector(HsvaAdjust, new Vector4(portraitSettings.hairColorHue, portraitSettings.hairColorSaturation, portraitSettings.hairColorValue, 0f));
    }

    public void UpdateAllVisuals(Character character) {
        //if (character.isSwitchingAlterEgo) {
        //    return;
        //}
        if (character.characterClass.className == "Zombie") { return; } //if character is a zombie do not update visuals, use default.
        UpdateMarkerAnimations(character);
        UpdatePortraitSettings(character);
        if (character.marker) {
            character.marker.UpdateMarkerVisuals();
        }
    }

    private void UpdateMarkerAnimations(Character character) {
        if (character.traitContainer.HasTrait("Disguised")) {
            character = character.traitContainer.GetNormalTrait<Traits.Disguised>("Disguised").disguisedCharacter;
        }
        CharacterClassAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender, character.characterClass.className);
        defaultSprite = assets.defaultSprite;
        float size = defaultSprite.rect.width / 100f;
        selectableSize = new Vector2(size, size);
        markerAnimations = new Dictionary<string, Sprite>();
        for (int i = 0; i < assets.animationSprites.Count; i++) {
            Sprite currSprite = assets.animationSprites[i];
            markerAnimations.Add(currSprite.name, currSprite);
        }
        if (character.marker != null) {
            character.marker.UpdateName();    
            // character.marker.UpdateNameplatePosition();    
        }
    }
    public void ChangeMarkerAnimationSprite(string key, Sprite sprite) {
        if (markerAnimations.ContainsKey(key)) {
            markerAnimations[key] = sprite;
        }
    }
    public Sprite GetMarkerAnimationSprite(string key) {
        if (markerAnimations.ContainsKey(key)) {
            return markerAnimations[key];
        }
        return null;
    }

    #region Blood
    public bool HasBlood() {
        return _hasBlood;
    }
    public void SetHasBlood(bool state) {
        _hasBlood = state;
    }
    #endregion

    #region UI
    public string GetNameplateName() {
        string name = _owner.firstName + " - " + _owner.raceClassName;
        // if(_owner.isSettlementRuler || _owner.isFactionLeader) {
        //     name = $"{name} {UtilityScripts.Utilities.LeaderIcon()}";
        // }
        return name;
    }
    public string GetThoughtBubble(out Log log) {
        log = null;
        // if (_owner.minion != null) {
        //     return string.Empty;
        // }
        if (_owner.overrideThoughts.Count > 0) {
            return _owner.overrideThoughts[0];
        }
        if (_owner.isDead) {
            if (_owner.deathLog != null) {
                log = _owner.deathLog;
                return UtilityScripts.Utilities.LogReplacer(_owner.deathLog);
            } else {
                return $"<b>{_owner.name}</b> has died.";    
            }
        }
        //Interrupt
        if (_owner.interruptComponent.isInterrupted && _owner.interruptComponent.thoughtBubbleLog != null) {
            log = _owner.interruptComponent.thoughtBubbleLog;
            return UtilityScripts.Utilities.LogReplacer(_owner.interruptComponent.thoughtBubbleLog);
        }

        //Action
        if (_owner.currentActionNode != null) {
            Log currentLog = _owner.currentActionNode.GetCurrentLog();
            log = currentLog;
            return UtilityScripts.Utilities.LogReplacer(currentLog);
        }

        //Character State
        if (_owner.stateComponent.currentState != null) {
            log = _owner.stateComponent.currentState.thoughtBubbleLog;
            return UtilityScripts.Utilities.LogReplacer(_owner.stateComponent.currentState.thoughtBubbleLog);
        }
        //fleeing
        if (_owner.marker && _owner.marker.hasFleePath) {
            return $"<b>{_owner.name}</b> is fleeing.";
        }

        //Travelling
        if (_owner.carryComponent.masterCharacter.avatar.isTravelling) {
            if (_owner.carryComponent.masterCharacter.marker.destinationTile != null) {
                return $"<b>{_owner.name}</b> is going to {_owner.carryComponent.masterCharacter.marker.destinationTile.structure.GetNameRelativeTo(_owner)}.";
            }
        }

        //Default - Do nothing/Idle
        if (_owner.currentStructure != null) {
            return $"<b>{_owner.name}</b> is in {_owner.currentStructure.GetNameRelativeTo(_owner)}.";
        }

        if(_owner.minion != null && !_owner.minion.isSummoned) {
            return $"<b>{_owner.name}</b> is unsummoned.";
        }
        return $"<b>{_owner.name}</b> is in {_owner.currentRegion?.name}.";
    }
    #endregion
}
