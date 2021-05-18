using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using System.Collections.Generic;

namespace Plague.Death_Effect {
    public class HauntedSpirits : PlagueDeathEffect {
        
        public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Haunted_Spirits;

        private TILE_OBJECT_TYPE[] _spirits = new TILE_OBJECT_TYPE[] { TILE_OBJECT_TYPE.RAVENOUS_SPIRIT, TILE_OBJECT_TYPE.FEEBLE_SPIRIT, TILE_OBJECT_TYPE.FORLORN_SPIRIT };

        protected override void ActivateEffect(Character p_character) {
            CreateSpirits(_level, p_character);
            //switch (_level) {
            //    case 1:
            //        CreateSpirits(1, p_character);
            //        break;
            //    case 2:
            //        CreateSpirits(2, p_character);
            //        break;
            //    case 3:
            //        CreateSpirits(3, p_character);
            //        break;
            //}
#if DEBUG_LOG
            Debug.Log("Activated Haunted Spirits Effect");
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
                    return "1 Random Spirit";
                case 2:
                    return "2 Random Spirits";
                case 3:
                    return "3 Random Spirits";
                default:
                    return string.Empty;
            }
        }
        public override void OnDeath(Character p_character) {
            ActivateEffectOn(p_character);
        }

        private void CreateSpirits(int amount, Character p_character) {
            LocationGridTile targetTile = p_character.gridTileLocation;
            if (targetTile != null) {
                for (int i = 0; i < amount; i++) {
                    TILE_OBJECT_TYPE chosenSpiritType = _spirits[GameUtilities.RandomBetweenTwoNumbers(0, _spirits.Length - 1)];
                    TileObject spirit = InnerMapManager.Instance.CreateNewTileObject<TileObject>(chosenSpiritType);
                    spirit.SetGridTileLocation(targetTile);
                    spirit.OnPlacePOI();
                }
            }
        }
    }
}