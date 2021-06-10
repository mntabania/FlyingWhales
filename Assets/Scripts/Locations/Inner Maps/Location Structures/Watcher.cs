using UnityEngine;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Inner_Maps.Location_Structures {
    public class Watcher : DemonicStructure {
        public List<DemonEye> eyeWards { get; private set; }
        public int m_defaultEyeCount = 3;
		#region upgradeable data
		private int m_eyesLevel = 0;
        private int m_radiusLevel = 0;
        private int m_eyeWardMaxCount = 3;
        private int m_eyeWardRadius = 7;
		#endregion
		public override string scenarioDescription => "The Watcher allows you to place Eyes on unoccupied tiles in the map. The Eye logs most events and actions that occur around it, allowing the player to store some of them as Intel. Intel can then be used for various purposes. You can share it with other Villagers and watch them react or you may even use one as Blackmail material if you have the Meddler.";
        public override string extraInfo1Header => $"Eyes: Lv{m_eyesLevel+1}";
        public override string extraInfo1Description => $"{eyeWards.Count}/{m_eyeWardMaxCount}";
        public override string extraInfo2Header => $"Radius: Lv{m_radiusLevel+1}";
        public override string extraInfo2Description => $"{m_eyeWardRadius*2}x{m_eyeWardRadius*2}";
        #region getters
        public override System.Type serializedData => typeof(SaveDataWatcher);
        #endregion
        public Watcher(Region location) : base(STRUCTURE_TYPE.WATCHER, location){
            SetMaxHPAndReset(2500);
            eyeWards = new List<DemonEye>();
        }
        public Watcher(Region location, SaveDataWatcher data) : base(location, data) {
            eyeWards = new List<DemonEye>();
        }

        #region Overrides
        //Note: Removed this because of the update that the Beholder will now have max charges upon building instead of 1 charge only
        //https://trello.com/c/t4CezyZO/3805-eye-updates
        //public override void OnBuiltNewStructure() {
        //    base.OnBuiltNewStructure();
        //    //Spawn Eye Ward should start at 1 charge, not max charge
        //    PlayerAction spawnEyeWardAction = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
        //    int chargeToDeduct = spawnEyeWardAction.charges - 1;
        //    if(chargeToDeduct > 0) {
        //        spawnEyeWardAction.AdjustCharges(-chargeToDeduct);
        //    }
        //}
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            UpdateEyeWardCharges(m_defaultEyeCount);
            Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            /*
            PlayerAction spawnEyeWardAction = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
            spawnEyeWardAction.SetMaxCharges(spawnEyeWardAction.maxCharges - m_eyeWardMaxCount);
            */
            /*
            if(spawnEyeWardAction.charges > spawnEyeWardAction.maxCharges) {
                int chargesToBeDeducted = spawnEyeWardAction.charges - spawnEyeWardAction.maxCharges;
                spawnEyeWardAction.AdjustCharges(-chargesToBeDeducted);
            }*/
            Messenger.RemoveListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);

            for (int i = 0; i < eyeWards.Count; i++) {
                DemonEye eye = eyeWards[i];
                if (eye.gridTileLocation != null) {
                    eye.gridTileLocation.structure.RemovePOI(eye);
                }
                if (RemoveEyeWard(eye)) {
                    i--;
                }
            }
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL);
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL);
        }
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion

        #region General
        private void UpdateEyeWardCharges(int p_count, bool p_isFromLevelUp = false) {
            return;
            PlayerAction spawnEyeWardAction = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
            if (spawnEyeWardAction.maxCharges == -1) {
                spawnEyeWardAction.SetMaxCharges(p_count);
                spawnEyeWardAction.SetCharges(p_count);
            } else {
                spawnEyeWardAction.SetMaxCharges(p_count);
                if (!p_isFromLevelUp) {
                    spawnEyeWardAction.AdjustCharges(p_count);
                } else {
                    spawnEyeWardAction.AdjustCharges(1);
                }
            }
        }
        private void OnDestroyTileObject(TileObject p_tileObject) {
            if(p_tileObject is DemonEye eyeWard) {
                RemoveEyeWard(eyeWard);
            }
        }
        public void AddEyeWard(DemonEye p_eyeWard) {
            if (!eyeWards.Contains(p_eyeWard)) {
                eyeWards.Add(p_eyeWard);
                p_eyeWard.SetBeholderOwner(this);
                Messenger.Broadcast(StructureSignals.UPDATE_EYE_WARDS, this);
            }
        }
        public bool RemoveEyeWard(DemonEye p_eyeWard) {
            if (eyeWards.Remove(p_eyeWard)) {
                //broadcast signal
                p_eyeWard.SetBeholderOwner(null);
                Messenger.Broadcast(StructureSignals.UPDATE_EYE_WARDS, this);
                return true;
            }
            return false;
        }

        public void LevelUpEyes() {
            PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-EditableValuesManager.Instance.GetBeholderEyeUpgradeCostPerLevel(m_eyesLevel).processedAmount);
            m_eyesLevel = Mathf.Clamp(++m_eyesLevel, 1, 4);
            m_eyeWardMaxCount = Mathf.Clamp(++m_eyeWardMaxCount, 1, 8);
            UpdateEyeWardCharges(m_eyesLevel + m_defaultEyeCount, true);
        }

        public void LevelUpRadius() {
            PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-EditableValuesManager.Instance.GetBeholderRadiusUpgradeCostPerLevel(m_radiusLevel).processedAmount);
            m_radiusLevel = Mathf.Clamp(++m_radiusLevel, 1, 4);
            m_eyeWardRadius = Mathf.Clamp(++m_eyeWardRadius, 7, 12);
            eyeWards.ForEach((eachEye) => eachEye.UpdateRange());
        }
        #endregion

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataWatcher data = saveDataLocationStructure as SaveDataWatcher;
            if(data.eyeWards != null) {
                for (int i = 0; i < data.eyeWards.Count; i++) {
                    if (!string.IsNullOrEmpty(data.eyeWards[i])) {
                        eyeWards.Add(DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.eyeWards[i]) as DemonEye);
                        eyeWards[i].SetBeholderOwner(this);
                        eyeWards[i].UpdateRange();
                    }
                }
            }
            m_eyesLevel = data.eyesLevel;
            m_radiusLevel = data.radiusLevel;
            m_eyeWardMaxCount = data.eyeWardMaxCount;
            m_eyeWardRadius = data.eyeWardRadius;
            Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);
        }

        public int GetEyeLevel() {
            return m_eyesLevel;
        }

        public int GetRadiusLevel() {
            return m_radiusLevel;
        }

        public int GetEyeWardRadius() {
            return m_eyeWardRadius;
        }

        public int GetCurrentMaxEyeCount() {
            return m_eyeWardMaxCount;
        }
        #endregion
    }
}

public class SaveDataWatcher : SaveDataDemonicStructure {
    public List<string> eyeWards;
    public int eyesLevel;
    public int radiusLevel;
    public int eyeWardMaxCount;
    public int eyeWardRadius;

    public override void Save(LocationStructure structure) {
        base.Save(structure);
		Inner_Maps.Location_Structures.Watcher data = structure as Inner_Maps.Location_Structures.Watcher;
        if(data.eyeWards.Count > 0) {
            eyeWards = new List<string>();
            for (int i = 0; i < data.eyeWards.Count; i++) {
                eyeWards.Add(data.eyeWards[i].persistentID);
            }
        }
        eyesLevel = data.GetEyeLevel();
        radiusLevel = data.GetRadiusLevel();
        eyeWardRadius = data.GetEyeWardRadius();
        eyeWardMaxCount = data.GetCurrentMaxEyeCount();
    }
}