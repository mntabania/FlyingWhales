using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Inner_Maps.Location_Structures {
    public class Beholder : DemonicStructure {
        public EyeWard[] eyeWards { get; private set; }

        #region getters
        public override System.Type serializedData => typeof(SaveDataBeholder);
        #endregion
        public Beholder(Region location) : base(STRUCTURE_TYPE.BEHOLDER, location){
            eyeWards = new EyeWard[3];
        }
        public Beholder(Region location, SaveDataBeholder data) : base(location, data) {
            eyeWards = new EyeWard[data.eyeWards.Length];
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
            PlayerAction spawnEyeWardAction = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
            if(spawnEyeWardAction.maxCharges == -1) {
                spawnEyeWardAction.SetMaxCharges(3);
                spawnEyeWardAction.SetCharges(3);
            } else {
                spawnEyeWardAction.SetMaxCharges(spawnEyeWardAction.maxCharges + 3);
                spawnEyeWardAction.AdjustCharges(3);
            }
            Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);
        }
        protected override void AfterStructureDestruction() {
            base.AfterStructureDestruction();
            PlayerAction spawnEyeWardAction = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
            spawnEyeWardAction.SetMaxCharges(spawnEyeWardAction.maxCharges - 3);

            if(spawnEyeWardAction.charges > spawnEyeWardAction.maxCharges) {
                int chargesToBeDeducted = spawnEyeWardAction.charges - spawnEyeWardAction.maxCharges;
                spawnEyeWardAction.AdjustCharges(-chargesToBeDeducted);
            }
            Messenger.RemoveListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnDestroyTileObject);

            for (int i = 0; i < eyeWards.Length; i++) {
                EyeWard eye = eyeWards[i];
                if (eye != null) {
                    if(eye.gridTileLocation != null) {
                        eye.gridTileLocation.structure.RemovePOI(eye);
                    }
                    eyeWards[i] = null;
                }
            }
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SPAWN_EYE_WARD);
        }
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.y -= 0.2f;
            worldPosition = position;
        }
        #endregion

        #region General
        private void OnDestroyTileObject(TileObject p_tileObject) {
            if(p_tileObject is EyeWard eyeWard) {
                RemoveEyeWard(eyeWard);
            }
        }
        public void SetEyeWard(EyeWard p_eyeWard) {
            for (int i = 0; i < eyeWards.Length; i++) {
                if(eyeWards[i] == null) {
                    eyeWards[i] = p_eyeWard;
                    break;
                }
            }
        }
        public void RemoveEyeWard(EyeWard p_eyeWard) {
            for (int i = 0; i < eyeWards.Length; i++) {
                if (eyeWards[i] == p_eyeWard) {
                    eyeWards[i] = null;
                    break;
                }
            }
        }
        #endregion

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataBeholder data = saveDataLocationStructure as SaveDataBeholder;
            for (int i = 0; i < data.eyeWards.Length; i++) {
                if (!string.IsNullOrEmpty(data.eyeWards[i])) {
                    eyeWards[i] = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.eyeWards[i]) as EyeWard;
                }
            }
        }
        #endregion
    }
}

public class SaveDataBeholder : SaveDataDemonicStructure {
    public string[] eyeWards { get; private set; }

    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Beholder data = structure as Beholder;
        eyeWards = new string[data.eyeWards.Length];
        for (int i = 0; i < data.eyeWards.Length; i++) {
            if(data.eyeWards[i] != null) {
                eyeWards[i] = data.eyeWards[i].persistentID;
            }
        }
    }
}