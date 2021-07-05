using System.Linq;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class CreateBlackmailData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CREATE_BLACKMAIL;
    public override string name => "Create Blackmail";
    public override string description => $"This Ability produces an Intel regarding the state of a Player's Prisoner. It may be used as a blackmail material when shared with the Prisoner's acquaintances.";
    
    public CreateBlackmailData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            Assert.IsTrue(targetCharacter.currentStructure is DemonicStructure);
            ActualGoapNode action = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.IS_IMPRISONED], targetCharacter, targetCharacter, new OtherData[] {
                new LocationStructureOtherData(targetCharacter.currentStructure)
            }, 0);
            action.SetAsIllusion();
            var intel = InteractionManager.Instance.CreateNewIntel(action);
            PlayerManager.Instance.player.AddIntel(intel);
            
            Vector3 pos = targetCharacter.worldPosition;
            pos.z = 0f;
            GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("StoreIntelEffect", pos, Quaternion.identity, InnerMapManager.Instance.transform, true);
            effectGO.transform.position = pos;

            Vector3 intelTabPos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.intelToggle.transform.position);

            Vector3 controlPointA = targetCharacter.worldPosition;
            controlPointA.x += 5f;
		
            Vector3 controlPointB = intelTabPos;
            controlPointB.y -= 5f;

            effectGO.transform.DOPath(new[] {intelTabPos, controlPointA, controlPointB}, 0.7f, PathType.CubicBezier)
                .SetEase(Ease.InSine).OnComplete(() => OnReachIntelTab(effectGO));
            
            base.ActivateAbility(targetPOI);    
        }
    }
    public override void ActivateAbility(LocationStructure targetStructure) {
        if (targetStructure is TortureChambers tortureChambers && tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
            //Character targetCharacter = prisonCell.charactersInRoom.FirstOrDefault(c => CanPerformAbilityTowards(c) && IsValid(c));
            Character targetCharacter = GetFirstCharacterInPrisonCellThatIsValid(prisonCell);
            if (targetCharacter != null) {
                ActivateAbility(targetCharacter);
            }
        }
    }
    private void OnReachIntelTab(GameObject effectGO) {
        PlayerUI.Instance.DoIntelTabPunchEffect();
        ObjectPoolManager.Instance.DestroyObject(effectGO);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                return targetCharacter.isNormalCharacter && !targetCharacter.isDead && targetCharacter.gridTileLocation != null && targetCharacter.currentStructure is TortureChambers && 
                       targetCharacter.gridTileLocation.structure.IsTilePartOfARoom(targetCharacter.gridTileLocation, out var room) && room is PrisonCell;
            } else if (target is TortureChambers tortureChambers) {
                if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && prisonCell.HasValidOccupant()) {
                    return true;
                }
                return false;
            }
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            if (PlayerManager.Instance.player.playerSkillComponent.AlreadyHasBlackmail(targetCharacter)) {
                return false;
            }
            return true;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(LocationStructure targetStructure) {
        bool canPerform = base.CanPerformAbilityTowards(targetStructure);
        if (canPerform) {
            if (targetStructure is TortureChambers tortureChambers) {
                if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && HasCharacterInPrisonCellThatIsValid(prisonCell)/*prisonCell.charactersInRoom.Any(c => CanPerformAbilityTowards(c) && IsValid(c))*/) {
                    return true;
                }
                return false;
            }
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (PlayerManager.Instance.player.playerSkillComponent.AlreadyHasBlackmail(targetCharacter)) {
            reasons += $"Player already has Hostage Blackmail on {targetCharacter.name}.";
        }
        return reasons;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure) {
        string reasons =  base.GetReasonsWhyCannotPerformAbilityTowards(p_targetStructure);
        if (p_targetStructure is TortureChambers tortureChambers) {
            if (tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell && !HasCharacterInPrisonCellThatIsValid(prisonCell)/*!prisonCell.charactersInRoom.Any(c => CanPerformAbilityTowards(c) && IsValid(c))*/) {
                reasons += $"Player already has Hostage Blackmail on all Villagers inside Prison Cell.";
            }
        }
        return reasons;
    }

    private bool HasCharacterInPrisonCellThatIsValid(PrisonCell prisonCell) {
        return GetFirstCharacterInPrisonCellThatIsValid(prisonCell) != null;
    }
    private Character GetFirstCharacterInPrisonCellThatIsValid(PrisonCell prisonCell) {
        for (int i = 0; i < prisonCell.tilesInRoom.Count; i++) {
            LocationGridTile t = prisonCell.tilesInRoom[i];
            for (int j = 0; j < t.charactersHere.Count; j++) {
                Character c = t.charactersHere[j];
                if (CanPerformAbilityTowards(c) && IsValid(c)) {
                    return c;
                }
            }
        }
        return null;
    }
}
