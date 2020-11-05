﻿using System;
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using UtilityScripts;
using Traits;
using Object = UnityEngine.Object;

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
    public string classToUseForVisuals {
        get {
            if (_owner.characterClass.className == "Zombie") {
                //if character class is a zombie, then use previous class for any visuals to use
                //this is so we do not need to create special sprites and special cases for every race that can become a zombie
                if (!string.IsNullOrEmpty(_owner.previousClassName)) {
                    return _owner.previousClassName;    
                } else {
                    return "Peasant"; //just a failsafe
                }
            }
            return _owner.characterClass.className;
        }
    }
    
    private bool _hasBlood;

    public CharacterVisuals(Character character) {
        _owner = character;
        _hasBlood = true;
        markerAnimations = new Dictionary<string, Sprite>();
    }
   
    public CharacterVisuals(Character character, SaveDataCharacter data) {
        _owner = character;
        _hasBlood = true;
        portraitSettings = data.portraitSettings;
        CreateHairMaterial(portraitSettings);
        markerAnimations = new Dictionary<string, Sprite>();
    }

    #region Initialization
    public void Initialize() {
        portraitSettings = CharacterManager.Instance.GeneratePortrait(_owner);
        CreateHairMaterial(portraitSettings);
        UpdateMarkerAnimations(_owner);
    }
    #endregion

    #region Listeners
    public void SubscribeListeners() {
        Messenger.AddListener<Character, ILeader>(Signals.ON_SET_AS_FACTION_LEADER, OnSetAsFactionLeader);
        Messenger.AddListener<Character, Character>(Signals.ON_SET_AS_SETTLEMENT_RULER, OnSetAsSettlementRuler);
    }
    public void UnsubscribeListeners() {
        Messenger.RemoveListener<Character, ILeader>(Signals.ON_SET_AS_FACTION_LEADER, OnSetAsFactionLeader);
        Messenger.RemoveListener<Character, Character>(Signals.ON_SET_AS_SETTLEMENT_RULER, OnSetAsSettlementRuler);
    }
    private void OnSetAsFactionLeader(Character character, ILeader previousLeader) {
        if (character == _owner) {
            UpdatePortrait(character);
        }
    }
    private void OnSetAsSettlementRuler(Character character, Character previousRuler) {
        if (character == _owner) {
            UpdatePortrait(character);
        }
    }
    #endregion
    
    #region Hair
    private void CreateHairMaterial(PortraitSettings ps) {
        hairMaterial = Object.Instantiate(CharacterManager.Instance.hsvMaterial);
        hairMaterial.SetFloat(HairHue, ps.hairColorHue);
        hairMaterial.SetFloat(HairSaturation, ps.hairColorSaturation);
        hairMaterial.SetFloat(HairValue, ps.hairColorValue);
        
        hairUIMaterial = Object.Instantiate(CharacterManager.Instance.hairUIMaterial);
        hairUIMaterial.SetVector(HsvaAdjust, new Vector4(ps.hairColorHue, ps.hairColorSaturation, ps.hairColorValue, 0f));
    }
    public void CreateWholeImageMaterial(PortraitSettings ps) {
        hairMaterial = Object.Instantiate(CharacterManager.Instance.hsvMaterial);
        hairMaterial.SetFloat(HairHue, ps.hairColorHue);
        hairMaterial.SetFloat(HairSaturation, ps.hairColorSaturation);
        hairMaterial.SetFloat(HairValue, ps.hairColorValue);
        
        hairUIMaterial = Object.Instantiate(CharacterManager.Instance.hairUIMaterial);
        hairUIMaterial.SetVector(HsvaAdjust, new Vector4(ps.hairColorHue, ps.hairColorSaturation, ps.hairColorValue, 0f));
    }
    #endregion

    #region Utilities
    public void UpdateAllVisuals(Character character, bool regeneratePortrait = false) {
        UpdateMarkerAnimations(character);
        if (regeneratePortrait) {
            RegeneratePortrait(character);
        } else {
            UpdatePortrait(character);
        }
        if (character.marker) {
            character.marker.UpdateMarkerVisuals();
        }
    }
    private void UpdatePortrait(Character character) {
        portraitSettings = CharacterManager.Instance.UpdatePortraitSettings(character);
    }
    private void RegeneratePortrait(Character character) {
        portraitSettings = CharacterManager.Instance.GeneratePortrait(character);
    }
    #endregion

    #region Animations
    private void UpdateMarkerAnimations(Character character) {
        bool isInBatForm = character.isInVampireBatForm;
        bool isInWerewolfForm = character.isInWerewolfForm;
        if (character.reactionComponent.disguisedCharacter != null) {
            character = character.reactionComponent.disguisedCharacter;
            //If a character is only disguising, do not copy bat form of the original character in case the original one is currently in bat form
            isInBatForm = false;
            isInWerewolfForm = false;
        }
        CharacterClassAsset assets = null;
        if (!isInBatForm) {
            if (!isInWerewolfForm) {
                assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender, character.visuals.classToUseForVisuals);
            } else {
                assets = CharacterManager.Instance.GetAdditionalMarkerAsset("Werewolf");
            }
        } else {
            assets = CharacterManager.Instance.GetAdditionalMarkerAsset("Vampire Bat");
        }
        defaultSprite = assets.defaultSprite;

        float size = defaultSprite.rect.width / 100f;
        if (character is Troll) {
            size = 0.8f;
        } else if (character is Dragon) {
            size = 2.56f;
        } 
        selectableSize = new Vector2(size, size);

        if(markerAnimations == null) {
            markerAnimations = new Dictionary<string, Sprite>();
        } else {
            markerAnimations.Clear();
        }
        for (int i = 0; i < assets.animationSprites.Count; i++) {
            Sprite currSprite = assets.animationSprites[i];
            markerAnimations.Add(currSprite.name, currSprite);
        }
        if (character.marker) {
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
    #endregion

    #region Blood
    public bool HasBlood() {
        return _hasBlood;
    }
    public void SetHasBlood(bool state) {
        _hasBlood = state;
    }
    #endregion

    #region UI
    public string GetThoughtBubble() {
        if (_owner.isDead) {
            if (_owner.deathLog.hasValue) {
                return _owner.deathLog.logText;
            } else {
                return $"<b>{GetCharacterNameWithIconAndColor()}</b> has died.";    
            }
        }
        //Interrupt
        if (_owner.interruptComponent.isInterrupted && _owner.interruptComponent.thoughtBubbleLog.hasValue) {
            return _owner.interruptComponent.thoughtBubbleLog.logText;
        }

        //Action
        if (_owner.currentActionNode != null) {
            Log currentLog = _owner.currentActionNode.GetCurrentLog();
            return currentLog.logText;
        }

        //Character State
        if (_owner.stateComponent.currentState != null) {
            return _owner.stateComponent.currentState.thoughtBubbleLog.logText;
        }
        //fleeing
        if (_owner.marker && _owner.marker.hasFleePath) {
            return $"<b>{GetCharacterNameWithIconAndColor()}</b> is fleeing.";
        }

        //Travelling
        Character masterCharacter = _owner.carryComponent.masterCharacter;
        if (masterCharacter.marker && masterCharacter.marker.destinationTile != null && masterCharacter.marker.isMoving && masterCharacter.marker.pathfindingAI.currentPath != null) {
            return $"<b>{GetCharacterNameWithIconAndColor()}</b> is going to {_owner.carryComponent.masterCharacter.marker.destinationTile.structure.GetNameRelativeTo(_owner)}.";
        }

        //Default - Do nothing/Idle
        if (_owner.currentStructure != null) {
            return $"<b>{GetCharacterNameWithIconAndColor()}</b> is in {_owner.currentStructure.GetNameRelativeTo(_owner)}.";
        }

        if(_owner.minion != null && !_owner.minion.isSummoned) {
            return $"<b>{GetCharacterNameWithIconAndColor()}</b> is unsummoned.";
        }
        return $"<b>{GetCharacterNameWithIconAndColor()}</b> is in {_owner.currentRegion?.name}.";
        
    }
    public string GetCharacterStringIcon() {
        if (!_owner.isNormalCharacter) {
            if (_owner.characterClass.className == "Necromancer") {
                return UtilityScripts.Utilities.VillagerIcon();        
            } else if (_owner.faction != null && _owner.faction.isPlayerFaction) {
                return UtilityScripts.Utilities.DemonIcon();
            } else if (_owner.faction != null && _owner.faction.factionType.type == FACTION_TYPE.Undead) {
                return UtilityScripts.Utilities.UndeadIcon();
            }
            return UtilityScripts.Utilities.MonsterIcon();
        } else if (_owner.isAlliedWithPlayer) {
            return UtilityScripts.Utilities.CultistIcon();
        }
        return UtilityScripts.Utilities.VillagerIcon();
    }
    public string GetCharacterNameWithIconAndColor() {
        string icon = GetCharacterStringIcon();
        string characterName = UtilityScripts.Utilities.ColorizeName(_owner.name, CharacterManager.Instance.GetCharacterNameColorHexForLogs(_owner));
        return $"{icon}{characterName}";
    }
    public string GetRelationshipSummary(Character character) {
        if (_owner.relationshipContainer.HasRelationshipWith(character)) {
            string relationshipName = _owner.relationshipContainer.GetRelationshipNameWith(character);
            int opinionOfOwner = _owner.relationshipContainer.GetTotalOpinion(character);
            int opinionOfTarget = character.relationshipContainer.GetTotalOpinion(_owner);
            string opinionOfOwnerStr = $"<color={BaseRelationshipContainer.OpinionColor(opinionOfOwner)}>{GetOpinionText(opinionOfOwner)}";
            string opinionOfTargetStr = $"<color={BaseRelationshipContainer.OpinionColor(opinionOfTarget)}>{GetOpinionText(opinionOfTarget)}";
            return $"{relationshipName}  {character.visuals.GetCharacterNameWithIconAndColor()}  {opinionOfOwnerStr}({opinionOfTargetStr})";
            // return $"{character.visuals.GetCharacterNameWithIconAndColor()} - {relationshipName} of {_owner.visuals.GetCharacterNameWithIconAndColor()}\n";
        } else {
            return $"{_owner.visuals.GetCharacterNameWithIconAndColor()} doesn't have a relationship with {character.visuals.GetCharacterNameWithIconAndColor()}\n";
        }
        return string.Empty;
    }
    private string GetOpinionText(int number) {
        if (number < 0) {
            return $"{number.ToString()}";
        }
        return $"+{number.ToString()}";
    }
    public string GetBothWayRelationshipSummary(Character otherCharacter) {
        string relationship1 = GetRelationshipSummary(otherCharacter);
        string relationship2 = otherCharacter.visuals.GetRelationshipSummary(_owner);
        string relationshipSummary = string.Empty;
        if (!string.IsNullOrEmpty(relationship1)) {
            relationshipSummary += $"{relationship1}\n";
        }
        if (!string.IsNullOrEmpty(relationship2)) {
            relationshipSummary += $"{relationship2}\n";
        }
        return relationshipSummary;
    }
    #endregion

    public void CleanUp() {
        if (hairMaterial != null) {
            Object.Destroy(hairMaterial);    
        }
        if (hairUIMaterial != null) {
            Object.Destroy(hairUIMaterial);    
        }
        if (wholeImageMaterial != null) {
            Object.Destroy(wholeImageMaterial);    
        }
        markerAnimations?.Clear();
        defaultSprite = null;
    }
}
