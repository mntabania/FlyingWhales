using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayerGame : SaveData<Player> {
    public string factionID;
    public string settlementID;
    public int mana;
    public int spiritEnergy;

    public int portalTileXCoordinate;
    public int portalTileYCoordinate;

    public PLAYER_ARCHETYPE archetype;

    public bool hasAlreadyWon;
    //public List<string> minionIDs;
    //public List<string> summonIDs;

    public List<SaveDataActionIntel> actionIntels;
    public List<SaveDataInterruptIntel> interruptIntels;

    public List<SaveDataNotification> allNotifs;
    public List<SaveDataChaosOrb> allChaosOrbs;
    public Dictionary<SUMMON_TYPE, MonsterCapacity> monsterCharges;
    public List<string> charactersThatHaveReportedDemonicStructure;
    
    //Components
    public SaveDataSeizeComponent seizeComponent;
    public SaveDataThreatComponent threatComponent;
    public SaveDataPlayerSkillComponent playerSkillComponent;
    public SaveDataPlagueComponent plagueComponent;
    public SaveDataPlayerUnderlingsComponent underlingsComponent;
    public SaveDataPlayerTileObjectComponent tileObjectComponent;
    public SaveDataStoredTargetsComponent storedTargetsComponent;
    public SaveDataBookmarkComponent bookmarkComponent;
    public SaveDataSummonMeterComponent summonMeterComponent;
    public SaveDataPlayerDamageAccumulator damageAccumulator;
    public SaveDataPlayerRetaliationComponent retaliationComponent;
    public SaveDataManaRegenComponent manaRegenComponent;

    #region Overrides
    public override void Save() {
        base.Save();
        Player player = PlayerManager.Instance.player;
        factionID = player.playerFaction.persistentID;
        settlementID = player.playerSettlement.persistentID;
        mana = player.mana;
        spiritEnergy = player.spiritEnergy;
        portalTileXCoordinate = player.portalArea.areaData.xCoordinate;
        portalTileYCoordinate = player.portalArea.areaData.yCoordinate;
        hasAlreadyWon = player.hasAlreadyWon;
        archetype = PlayerSkillManager.Instance.selectedArchetype;

        //minionIDs = new List<string>();
        //for (int i = 0; i < player.minions.Count; i++) {
        //    Minion minion = player.minions[i];
        //    minionIDs.Add(minion.character.persistentID);
        //}

        //summonIDs = new List<string>();
        //for (int i = 0; i < player.summons.Count; i++) {
        //    Summon summon = player.summons[i];
        //    summonIDs.Add(summon.persistentID);
        //}

        actionIntels = new List<SaveDataActionIntel>();
        interruptIntels = new List<SaveDataInterruptIntel>();
        for (int i = 0; i < player.allIntel.Count; i++) {
            IIntel intel = player.allIntel[i];
            if(intel is ActionIntel actionIntel) {
                SaveDataActionIntel saveIntel = new SaveDataActionIntel();
                saveIntel.Save(actionIntel);
                actionIntels.Add(saveIntel);
            } else if (intel is InterruptIntel interruptIntel) {
                SaveDataInterruptIntel saveIntel = new SaveDataInterruptIntel();
                saveIntel.Save(interruptIntel);
                interruptIntels.Add(saveIntel);
            }
        }

        charactersThatHaveReportedDemonicStructure = player.charactersThatHaveReportedDemonicStructure;

        allNotifs = new List<SaveDataNotification>();
        for (int i = 0; i < UIManager.Instance.activeNotifications.Count; i++) {
            PlayerNotificationItem notifItem = UIManager.Instance.activeNotifications[i];
            SaveDataNotification notif = new SaveDataNotification();
            notif.Save(notifItem);
            allNotifs.Add(notif);
        }

        allChaosOrbs = new List<SaveDataChaosOrb>();
        for (int i = 0; i < PlayerManager.Instance.availableChaosOrbs.Count; i++) {
            ChaosOrb orb = PlayerManager.Instance.availableChaosOrbs[i];
            if(orb.location != null) {
                SaveDataChaosOrb saveOrb = new SaveDataChaosOrb();
                saveOrb.Save(orb);
                allChaosOrbs.Add(saveOrb);
            }
        }
        
        seizeComponent = new SaveDataSeizeComponent();
        seizeComponent.Save(player.seizeComponent);

        threatComponent = new SaveDataThreatComponent();
        threatComponent.Save(player.threatComponent);

        playerSkillComponent = new SaveDataPlayerSkillComponent();
        playerSkillComponent.Save(player.playerSkillComponent);
        
        plagueComponent = new SaveDataPlagueComponent();
        plagueComponent.Save(player.plagueComponent);

        underlingsComponent = new SaveDataPlayerUnderlingsComponent();
        underlingsComponent.Save(player.underlingsComponent);

        tileObjectComponent = new SaveDataPlayerTileObjectComponent();
        tileObjectComponent.Save(player.tileObjectComponent);
        
        storedTargetsComponent = new SaveDataStoredTargetsComponent();
        storedTargetsComponent.Save(player.storedTargetsComponent);

        bookmarkComponent = new SaveDataBookmarkComponent();
        bookmarkComponent.Save(player.bookmarkComponent);

        summonMeterComponent = new SaveDataSummonMeterComponent();
        summonMeterComponent.Save(player.summonMeterComponent);

        damageAccumulator = new SaveDataPlayerDamageAccumulator();
        damageAccumulator.Save(player.damageAccumulator);

        retaliationComponent = new SaveDataPlayerRetaliationComponent();
        retaliationComponent.Save(player.retaliationComponent);

        manaRegenComponent = new SaveDataManaRegenComponent();
        manaRegenComponent.Save(player.manaRegenComponent);
    }
    public override Player Load() {
        Player player = new Player(this);
        return player;
    }
    #endregion

    #region Clean Up
    public void CleanUp() {
        //minionIDs.Clear();
        //summonIDs.Clear();
        actionIntels.Clear();
        interruptIntels.Clear();
        allNotifs.Clear();
    }
    #endregion
}
