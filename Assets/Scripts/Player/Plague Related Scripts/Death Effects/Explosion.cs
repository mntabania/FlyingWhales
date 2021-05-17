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
#if DEBUG_LOG
            Debug.Log("Activated Explosion Death Effect");
#endif
        }
        protected override int GetNextLevelUpgradeCost() {
            switch (_level) {
                case 1:
                    return 50;
                case 2:
                    return 75;
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
            ActivateEffectOn(p_character);
        }

        private void Ignite(Character p_character) {
            if (p_character.marker) {
                BurningSource bs = new BurningSource();
                Burning burning = TraitManager.Instance.CreateNewInstancedTraitClass<Burning>("Burning");
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
                CharacterManager.Instance.PlaceSummonInitially(summon, chosenTile);
                summon.SetTerritory(chosenTile.area, false);
            }
        }
        private void Meteor(Character p_character) {
            if(p_character.gridTileLocation != null) {
                p_character.gridTileLocation.AddMeteor();
            }
        }
    }
}