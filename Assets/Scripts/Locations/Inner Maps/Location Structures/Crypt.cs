using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Inner_Maps.Location_Structures {
    public class Crypt : DemonicStructure {
        public override Vector2 selectableSize { get; }
        private Artifact _activatedArtifact;
        
        public Crypt(Region location) : base(STRUCTURE_TYPE.CRYPT, location){
            selectableSize = new Vector2(10f, 10f);
        }

        #region Overrides
        public override void Initialize() {
            base.Initialize();
            AddActivateAction();
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            RemoveActivateAction();
        }
        public override void OnBuiltStructure() {
            base.OnBuiltStructure();
            List<SaveDataTileObject> tileobjectsData = SaveManager.Instance.currentSaveDataPlayer.cryptTileObjects;
            if(tileobjectsData != null && tileobjectsData.Count > 0) {
                for (int i = 0; i < tileobjectsData.Count; i++) {
                    SaveDataTileObject data = tileobjectsData[i];
                    TileObject obj = null;
                    if (data.isArtifact) {
                        obj = InnerMapManager.Instance.CreateNewArtifact(data.artifactType);
                    } else {
                        obj = InnerMapManager.Instance.CreateNewTileObject<TileObject>(data.tileObjectType);
                    }
                    obj.SetIsSaved(true);
                    obj.traitContainer.RemoveAllTraits(obj);
                    if (data.traitNames != null && data.traitNames.Length > 0) {
                        for (int j = 0; j < data.traitNames.Length; j++) {
                            obj.traitContainer.AddTrait(obj, data.traitNames[j]);
                        }
                    }
                    //if (data.statusNames != null && data.statusNames.Length > 0) {
                    //    for (int j = 0; j < data.statusNames.Length; j++) {
                    //        int stacks = data.statusStacks[j];
                    //        for (int k = 0; k < stacks; k++) {
                    //            obj.traitContainer.AddTrait(obj, data.statusNames[j]);
                    //        }
                    //    }
                    //}
                    if (data.storedResourcesTypes != null && data.storedResourcesTypes.Length > 0) {
                        for (int j = 0; j < data.storedResourcesTypes.Length; j++) {
                            obj.SetResource(data.storedResourcesTypes[j], data.storedResourcesAmount[j]);
                        }
                    }
                    LocationGridTile tileLocation = GetRandomUnoccupiedTile();
                    if(tileLocation != null) {
                        AddPOI(obj, tileLocation);
                    }
                }
            }
        }
        #endregion

        #region Activate
        private void AddActivateAction() {
            //PlayerAction activate = new PlayerAction(PlayerDB.Activate_Artifact_Action, CanDoActivateArtifactAction, null, OnClickActivateArtifact);
            AddPlayerAction(SPELL_TYPE.ACTIVATE_TILE_OBJECT);
        }
        private void RemoveActivateAction() {
            //RemovePlayerAction(GetPlayerAction(PlayerDB.Activate_Artifact_Action));
            RemovePlayerAction(SPELL_TYPE.ACTIVATE_TILE_OBJECT);
        }
        private bool CanDoActivateArtifactAction() {
            return PlayerManager.Instance.player.mana >= 50;
        }
        private void OnClickActivateArtifact() {
            List<Artifact> artifacts = PlayerManager.Instance.player.artifacts;
            UIManager.Instance.ShowClickableObjectPicker(artifacts, ActivateArtifact, null, CanActivateArtifact, "Activate an Artifact");
        }
        private void ActivateArtifact(object obj) {
            //_activatedArtifact?.Deactivate();
            //Artifact artifact = obj as Artifact;
            //artifact.Activate();
            //_activatedArtifact = artifact;
            //PlayerManager.Instance.player.AdjustMana(-50);
            //UIManager.Instance.HideObjectPicker();
        }
        private bool CanActivateArtifact(Artifact artifact) {
            //return artifact.hasBeenActivated == false && artifact.CanGainSomethingNewByActivating();
            return false;
        }
        #endregion
    }
}