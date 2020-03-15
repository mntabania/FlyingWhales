using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SummonsFeature : TileFeature {

	public SummonsFeature() {
        name = "Summons";
        description = "This region is home to various monsters. You can summon the monsters if you build The Kennel here.";
    }

    #region Override
    public override void OnAddFeature(HexTile tile) {
        base.OnAddFeature(tile);
        SUMMON_TYPE chosenSummonType = CharacterManager.Instance.summonsPool[UnityEngine.Random.Range(0, CharacterManager.Instance.summonsPool.Length)];
        int numOfChars = 0;
        if (chosenSummonType == SUMMON_TYPE.Golem) {
            numOfChars = UnityEngine.Random.Range(1, 4);
        } else if (chosenSummonType == SUMMON_TYPE.Wolf) {
            numOfChars = UnityEngine.Random.Range(5, 11);
        } else {
            numOfChars = UnityEngine.Random.Range(1, 4);
        }
        // for (int i = 0; i < numOfChars; i++) {
        //     Summon summon = CharacterManager.Instance.CreateNewSummon(chosenSummonType, FactionManager.Instance.neutralFaction, tile);
        // }
    }
    //public override void Activate(Region region) {
    //    base.Activate(region);
    //    for (int i = 0; i < region.charactersAtLocation.Count; i++) {
    //        Character character = region.charactersAtLocation[i];
    //        if(character is Summon) {
    //            character.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
    //        }
    //    }
    //}
    public virtual void OnRemoveCharacterFromRegion(Region region, Character removedCharacter) {
        bool stillHasSummon = false;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if (character is Summon) {
                stillHasSummon = true;
                break;
            }
        }
        if (!stillHasSummon) {
            // region.RemoveFeature(this);
        }
    }
    #endregion
}
