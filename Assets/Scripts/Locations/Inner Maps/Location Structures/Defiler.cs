using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Inner_Maps.Location_Structures {
    public class Defiler : DemonicStructure {
        public bool hasVampirismBefore { get; private set; }
        public bool hasSpawnNecronomiconBefore { get; private set; }

        #region getters
        public override System.Type serializedData => typeof(SaveDataDefiler);
        #endregion

        public Defiler(Region location) : base(STRUCTURE_TYPE.DEFILER, location) {
            SetMaxHPAndReset(3000);
        }
        public Defiler(Region location, SaveDataDefiler data) : base(location, data) {
            hasVampirismBefore = data.hasVampirismBefore;
            hasSpawnNecronomiconBefore = data.hasSpawnNecronomiconBefore;
        }

        #region Overrides
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            SkillData spawnNecronomicon = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON);
            SkillData vampirism = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM);
            hasVampirismBefore = vampirism.isInUse;
            hasSpawnNecronomiconBefore = spawnNecronomicon.isInUse;
            PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(spawnNecronomicon);
            PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(vampirism);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(structureType)) {
                if (!hasVampirismBefore) {
                    PlayerManager.Instance.player.playerSkillComponent.RemovePlayerSkill(PLAYER_SKILL_TYPE.VAMPIRISM);
                }
                if (!hasSpawnNecronomiconBefore) {
                    PlayerManager.Instance.player.playerSkillComponent.RemovePlayerSkill(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON);
                }
            }
        }
        #endregion

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            //position.x += 0.25f;
            worldPosition = position;
        }
        #endregion

        //#region Listeners
        //protected override void SubscribeListeners() {
        //    base.SubscribeListeners();
        //    Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        //}
        //protected override void UnsubscribeListeners() {
        //    base.UnsubscribeListeners();
        //    Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        //}
        //private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        //    if (structure == this && character.isNormalCharacter && IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell defilerRoom && defilerRoom.skeleton == null) {
        //        DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
        //        door?.Close();
        //    }
        //}
        //#endregion

        //#region Rooms
        //protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
        //    return new PrisonCell(tilesInRoom);
        //}
        //#endregion

        //private void StopDrainingCharactersHere() {
        //    for (int i = 0; i < charactersHere.Count; i++) {
        //        Character character = charactersHere[i];
        //        character.traitContainer.RemoveTrait(character, "Being Drained");
        //    }
        //}
    }
}

public class SaveDataDefiler : SaveDataDemonicStructure {
    public bool hasVampirismBefore;
    public bool hasSpawnNecronomiconBefore;

    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Defiler data = structure as Defiler;
        hasVampirismBefore = data.hasVampirismBefore;
        hasSpawnNecronomiconBefore = data.hasSpawnNecronomiconBefore;
    }
}