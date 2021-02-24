using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class LetGoData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LET_GO;
    public override string name => "Let Go";
    public override string description => "This Action teleports a character outside the Demonic Structure.";
    public LetGoData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                return targetCharacter.currentStructure is Kennel || targetCharacter.currentStructure is TortureChambers;
            }
            return false;
        }
        return false;
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            return false;
        }
        if (targetCharacter.interruptComponent.isInterrupted) {
            if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed ||
                targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                //do not allow characters being tortured or brainwashed to be seized
                return false;
            }
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Being Drained")) {
            reasons += "Characters being drained cannot be Let Go.";
        }
        if (targetCharacter.interruptComponent.isInterrupted) {
            if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed) {
                reasons += "Character is currently being Brainwashed.";
            }else if (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured) {
                reasons += "Character is currently being Tortured.";
            }
        }
        return reasons;
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            //Make character dazed (if not summon) and teleport him/her on a random spot outside
            List<LocationGridTile> allTilesOutside = RuinarchListPool<LocationGridTile>.Claim();
            List<LocationGridTile> passableTilesOutside = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < targetCharacter.currentStructure.tiles.Count; i++) {
                LocationGridTile tileInStructure = targetCharacter.currentStructure.tiles.ElementAt(i);
                for (int j = 0; j < tileInStructure.neighbourList.Count; j++) {
                    LocationGridTile neighbour = tileInStructure.neighbourList[j];
                    if (neighbour.structure is Wilderness && !allTilesOutside.Contains(neighbour)) {
                        allTilesOutside.Add(neighbour);
                        if (neighbour.IsPassable()) {
                            passableTilesOutside.Add(neighbour);
                        }
                    }
                }
            }
            Assert.IsTrue(allTilesOutside.Count > 0);
            var targetTile = CollectionUtilities.GetRandomElement(passableTilesOutside.Count > 0 ? passableTilesOutside : allTilesOutside);
            if (targetCharacter is Summon == false) {
                targetCharacter.traitContainer.AddTrait(targetCharacter, "Dazed");    
            }
            CharacterManager.Instance.Teleport(targetCharacter, targetTile);
            GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);
            RuinarchListPool<LocationGridTile>.Release(allTilesOutside);
            base.ActivateAbility(targetPOI);    
        }
    }
}
