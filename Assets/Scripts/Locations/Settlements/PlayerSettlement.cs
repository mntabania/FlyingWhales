using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Locations.Settlements {
    public class PlayerSettlement : BaseSettlement{
        
        public PlayerSettlement() : base(LOCATION_TYPE.DEMONIC_INTRUSION) { }
        protected PlayerSettlement(SaveDataArea saveDataArea) : base(saveDataArea) { }


        #region Residents
        public override void AssignCharacterToDwellingInArea(Character character, LocationStructure dwellingOverride = null) {
            if (structures == null) {
                Debug.LogWarning(
                    $"{name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
                return;
            }
            if (!character.isFactionless && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
                Debug.LogWarning($"{name} doesn't have any dwellings for {character.name}");
                return;
            }
            if (character.isFactionless) {
                character.SetHomeStructure(null);
                return;
            }
            LocationStructure chosenDwelling = dwellingOverride;
            if (chosenDwelling == null) {
                if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && id == PlayerManager.Instance.player.playerSettlement.id) {
                    chosenDwelling = structures[STRUCTURE_TYPE.DWELLING][0]; //to avoid errors, residents in player npcSettlement will all share the same dwelling
                }
            }
            if (chosenDwelling == null) {
                //if the code reaches here, it means that the npcSettlement could not find a dwelling for the character
                Debug.LogWarning(
                    $"{GameManager.Instance.TodayLogString()}Could not find a dwelling for {character.name} at {name}, setting home to Town Center");
                chosenDwelling = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
            }
            character.ChangeHomeStructure(chosenDwelling);
        }
        protected override bool IsResidentsFull() {
            return false; //resident capacity is never full for player npcSettlement
        }
        #endregion
        
    }
}