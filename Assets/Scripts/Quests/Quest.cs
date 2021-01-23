using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using Tutorial;
using UnityEngine.Assertions;
namespace Quests {
    /// <summary>
    /// Base class for all Quests (Tutorials, Reaction Quests, Special Popups, etc.)
    /// </summary>
    public abstract class Quest {

        #region Properties
        protected List<QuestCriteria> _activationCriteria;
        public string questName { get; private set; }
        public bool isAvailable { get; protected set; }
        /// <summary>
        /// Is this quest activated? In other words, is this quest currently being shown to the player.
        /// </summary>
        public bool isActivated { get; private set; }
        #endregion

        protected Quest(string _questName) {
            questName = _questName;
        }

        protected void ChangeQuestName(string p_newName) {
            questName = p_newName;
        }
        
        #region Availability
        /// <summary>
        /// Make this quest available, this means that this quest is put on the list of available tutorials that the
        /// player can undertake. Usually this is preceded by this quests' criteria being met.  
        /// </summary>
        protected virtual void MakeAvailable() {
            isAvailable = true;
        }
        /// <summary>
        /// Make this tutorial unavailable again. This assumes that this tutorial is currently on wait list.
        /// </summary>
        protected virtual void MakeUnavailable() {
            isAvailable = false;
        }
        #endregion

        #region Activation
        /// <summary>
        /// Activate this tutorial, meaning this quest should be listening for whether its steps are completed.
        /// </summary>
        public virtual void Activate() {
            isActivated = true;
        }
        public virtual void Deactivate() {
            isActivated = false;
            Messenger.Broadcast(PlayerQuestSignals.QUEST_DEACTIVATED, this);
        }
        #endregion

        #region Completion
        protected abstract void CompleteQuest();
        #endregion

        #region Failure
        protected virtual void FailQuest() { }
        #endregion
    }
}