using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using System.Collections.Generic;

namespace Plague.Death_Effect {
    public class Explosion : PlagueDeathEffect {
        
        public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Explosion;

        protected override void ActivateEffect(Character p_character) {
            switch (_level) {
                case 1:
                    FireBlast(p_character);
                    break;
                case 2:
                    FireBlastAndFireElementals(p_character);
                    break;
                case 3:
                    Meteor(p_character);
                    break;
            }
            Debug.Log("Activated Explosion Death Effect");
        }
        public override int GetNextLevelUpgradeCost() {
            switch (_level) {
                case 1:
                    return 15;
                case 2:
                    return 30;
                default:
                    return -1; //Max Level
            }
        }
        public override string GetCurrentEffectDescription() {
            switch (_level) {
                case 1:
                    return "Fire Blast";
                case 2:
                    return "Fire Elemental";
                case 3:
                    return "Meteor";
                default:
                    return string.Empty;
            }
        }
      
        public override void OnDeath(Character p_character) {
            ActivateEffect(p_character);
        }

        private void Ignite(Character p_character) {
            if (p_character.marker) {
                BurningSource bs = new BurningSource();
                Burning burning = new Burning();
                burning.InitializeInstancedTrait();
                burning.SetSourceOfBurning(bs, p_character);
                p_character.traitContainer.AddTrait(p_character, burning, bypassElementalChance: true);
            }
        }
        private void FireBlast(Character p_character) {
            LocationGridTile targetTile = p_character.gridTileLocation;
            if (targetTile != null) {
                for (int i = 0; i < targetTile.neighbourList.Count; i++) {
                    LocationGridTile tile = targetTile.neighbourList[i];
                    tile.PerformActionOnTraitables((traitable) => FireBlastEffect(traitable));
                }
            }
        }
        private void FireBlastEffect(ITraitable traitable) {
            if (traitable.gridTileLocation == null) { return; }
            BurningSource burningSource = null;
            traitable.AdjustHP(-150, ELEMENTAL_TYPE.Fire, true, elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource), showHPBar: true);
        }
        private void FireBlastAndFireElementals(Character p_character) {
            FireBlast(p_character);
            LocationGridTile chosenTile = p_character.gridTileLocation; 
            if (chosenTile != null) {
                Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Fire_Elemental, FactionManager.Instance.neutralFaction, null, chosenTile.parentMap.region);
                CharacterManager.Instance.PlaceSummon(summon, chosenTile);
                summon.SetTerritory(chosenTile.collectionOwner.partOfHextile.hexTileOwner, false);
            }
        }
        private void Meteor(Character p_character) {
            if(p_character.gridTileLocation != null) {
                p_character.gridTileLocation.AddMeteor();
            }
        }
    }
}