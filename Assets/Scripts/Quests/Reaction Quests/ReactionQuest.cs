using System;
using Quests.Steps;
namespace Quests {
    public abstract class ReactionQuest : SteppedQuest, ISavable {

        public string persistentID { get; }
        public OBJECT_TYPE objectType => OBJECT_TYPE.Reaction_Quest;
        public abstract Type serializedData { get; }

        protected ReactionQuest(string _questName) : base(_questName) {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        }
        
        public override void Activate() {
            ConstructSteps();
            base.Activate();
        }
        protected override void CompleteQuest() {
            QuestManager.Instance.CompleteQuest(this);
        }
        protected override void FailQuest() {
            base.FailQuest();
            QuestManager.Instance.CompleteQuest(this);
        }
    }

    public abstract class SaveDataReactionQuest : SaveData<ReactionQuest>, ISavableCounterpart {
        public string persistentID { get; private set; }
        public OBJECT_TYPE objectType => OBJECT_TYPE.Reaction_Quest;
        public override void Save(ReactionQuest data) {
            persistentID = data.persistentID;
        }
    }
}