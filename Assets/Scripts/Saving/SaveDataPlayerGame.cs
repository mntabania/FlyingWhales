using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayerGame : SaveData<Player> {
    //TODO: Player Faction, Player Settlement
    public string factionID;
    public string settlementID;
    public int mana;

    public int portalTileXCoordinate;
    public int portalTileYCoordinate;

    public PLAYER_ARCHETYPE archetype;

    //TODO: Minions, Summons
    public List<string> minionIDs;
    public List<string> summonIDs;

    //Components
    public SaveDataSeizeComponent seizeComponent;
    public SaveDataThreatComponent threatComponent;
    public SaveDataPlayerSkillComponent playerSkillComponent;

    #region Overrides
    public override void Save() {
        base.Save();
        Player player = PlayerManager.Instance.player;
        factionID = player.playerFaction.persistentID;
        settlementID = player.playerSettlement.persistentID;
        mana = player.mana;
        portalTileXCoordinate = player.portalTile.data.xCoordinate;
        portalTileYCoordinate = player.portalTile.data.yCoordinate;

        archetype = PlayerSkillManager.Instance.selectedArchetype;

        minionIDs = new List<string>();
        for (int i = 0; i < player.minions.Count; i++) {
            Minion minion = player.minions[i];
            minionIDs.Add(minion.character.persistentID);
        }

        summonIDs = new List<string>();
        for (int i = 0; i < player.summons.Count; i++) {
            Summon summon = player.summons[i];
            summonIDs.Add(summon.persistentID);
        }

        seizeComponent = new SaveDataSeizeComponent();
        seizeComponent.Save(player.seizeComponent);

        threatComponent = new SaveDataThreatComponent();
        threatComponent.Save(player.threatComponent);

        playerSkillComponent = new SaveDataPlayerSkillComponent();
        playerSkillComponent.Save(player.playerSkillComponent);
    }
    public override Player Load() {
        Player player = new Player(this);
        return player;
    }
    #endregion
}
