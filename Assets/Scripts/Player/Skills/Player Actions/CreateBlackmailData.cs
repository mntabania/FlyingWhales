﻿using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class CreateBlackmailData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CREATE_BLACKMAIL;
    public override string name => "Create Blackmail";
    public override string description => $"Create blackmail for use in Schemes.";
    
    public CreateBlackmailData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            Assert.IsTrue(targetCharacter.currentStructure is DemonicStructure);
            ActualGoapNode action = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.IS_IMPRISONED], targetCharacter, targetCharacter, new OtherData[] {
                new LocationStructureOtherData(targetCharacter.currentStructure)
            }, 0);
            action.SetAsIllusion();
            var intel = InteractionManager.Instance.CreateNewIntel(action);

            Vector3 pos = targetCharacter.worldPosition;
            pos.z = 0f;
            GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("StoreIntelEffect", pos, Quaternion.identity, InnerMapManager.Instance.transform, true);
            effectGO.transform.position = pos;

            Vector3 intelTabPos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.intelToggle.transform.position);

            Vector3 controlPointA = targetCharacter.worldPosition;
            controlPointA.x += 5f;
		
            Vector3 controlPointB = intelTabPos;
            controlPointB.y -= 5f;

            IIntel storedIntel = intel;
            effectGO.transform.DOPath(new[] {intelTabPos, controlPointA, controlPointB}, 0.7f, PathType.CubicBezier)
                .SetEase(Ease.InSine).OnComplete(() => OnReachIntelTab(effectGO, storedIntel));
            
            PlayerManager.Instance.player.playerSkillComponent.AddCharacterToBlackmailList(targetCharacter);
            
            base.ActivateAbility(targetPOI);    
        }
    }
    private void OnReachIntelTab(GameObject effectGO, IIntel intel) {
        PlayerUI.Instance.DoIntelTabPunchEffect();
        ObjectPoolManager.Instance.DestroyObject(effectGO);
        PlayerManager.Instance.player.AddIntel(intel);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                return targetCharacter.isNormalCharacter;
            }
            return false;
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
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (PlayerManager.Instance.player.playerSkillComponent.AlreadyHasBlackmail(targetCharacter)) {
            reasons += $"Player already has Hostage Blackmail on {targetCharacter.name}.";
        }
        return reasons;
    }
}