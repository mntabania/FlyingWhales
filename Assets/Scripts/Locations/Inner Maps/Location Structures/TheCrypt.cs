using System.Collections.Generic;
using Actionables;
using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class TheCrypt : DemonicStructure {
        public override Vector2 selectableSize { get; }
        private Artifact _activatedArtifact;
        
        public TheCrypt(Region location) : base(STRUCTURE_TYPE.THE_CRYPT, location){
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
        #endregion

        #region Activate
        private void AddActivateAction() {
            PlayerAction activate = new PlayerAction(PlayerDB.Activate_Artifact_Action, CanDoActivateArtifactAction, null, OnClickActivateArtifact);
            AddPlayerAction(activate);
        }
        private void RemoveActivateAction() {
            RemovePlayerAction(GetPlayerAction(PlayerDB.Activate_Artifact_Action));
        }
        private bool CanDoActivateArtifactAction() {
            return PlayerManager.Instance.player.mana >= 50;
        }
        private void OnClickActivateArtifact() {
            List<Artifact> artifacts = PlayerManager.Instance.player.artifacts;
            UIManager.Instance.ShowClickableObjectPicker(artifacts, ActivateArtifact, null, CanActivateArtifact, "Activate an Artifact");
        }
        private void ActivateArtifact(object obj) {
            _activatedArtifact?.Deactivate();
            Artifact artifact = obj as Artifact;
            artifact.Activate();
            _activatedArtifact = artifact;
            PlayerManager.Instance.player.AdjustMana(-50);
            UIManager.Instance.HideObjectPicker();
        }
        private bool CanActivateArtifact(Artifact artifact) {
            return artifact.hasBeenActivated == false && artifact.CanGainSomethingNewByActivating();
        }
        #endregion
    }
}