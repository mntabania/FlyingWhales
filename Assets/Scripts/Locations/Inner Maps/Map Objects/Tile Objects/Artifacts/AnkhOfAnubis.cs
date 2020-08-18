using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class AnkhOfAnubis : Artifact {

    public bool isActivated { get; private set; }

    public AnkhOfAnubis() : base(ARTIFACT_TYPE.Ankh_Of_Anubis) {
        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
    }
    //public AnkhOfAnubis(SaveDataArtifact data) : base(data) {
    //}

    #region Overrides
    public override void ActivateTileObject() {
        if(gridTileLocation != null) {
            base.ActivateTileObject();
            isActivated = true;
            traitContainer.RemoveTrait(this, "Treasure");
            GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Ankh_Of_Anubis_Activate);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
        }
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
    }
    public override void OnTileObjectDroppedBy(Character inventoryOwner, LocationGridTile tile) {
        if (inventoryOwner.isDead && inventoryOwner.isNormalCharacter) {
            ActivateTileObject();
        }
    }
    #endregion

    private void OnCharacterDeath(Character characterThatDied) {
        if (isActivated && gridTileLocation != null) {
            if(characterThatDied.isNormalCharacter && currentRegion == characterThatDied.currentRegion && characterThatDied.marker != null && characterThatDied.visuals.HasBlood()) {
                Summon vengefulGhost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Vengeful_Ghost, FactionManager.Instance.undeadFaction, null, currentRegion);
                vengefulGhost.SetName(characterThatDied.name);
                CharacterManager.Instance.PlaceSummon(vengefulGhost, gridTileLocation); //characterThatDied.gridTileLocation

                Log log = new Log(GameManager.Instance.Today(), "Artifact", "Ankh Of Anubis", "spawn_vengeful_ghost");
                log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, characterThatDied.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToInvolvedObjects();
                if(gridTileLocation != null) {
                    PlayerManager.Instance.player.ShowNotificationFrom(gridTileLocation.structure.location, log);
                }
            }
        }
    }
}
