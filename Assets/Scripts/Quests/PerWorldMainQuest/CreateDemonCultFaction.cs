using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class CreateDemonCultFaction : ReactionQuest {


        private RecruitFifteenMembersDemonCultStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveDataCreateDemonCultFaction);
        #endregion

        public CreateDemonCultFaction() : base($"Create Demon Cult Faction and have 15 members") { }
        protected override void ConstructSteps() {
            QuestStep turnVillagerToPsychopathStep = new CreateDemonFactionStep(GetCreateCultFactionDescription);
            _eliminateVillagerStep = new RecruitFifteenMembersDemonCultStep(GetRecruitFifteenCultist);   
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(turnVillagerToPsychopathStep),
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        #region Step Helpers
        private string GetCreateCultFactionDescription() {
            return $"Create a demon cult faction"; // /{totalCharactersToEliminate.ToString()}
        }

        private string GetRecruitFifteenCultist(int p_remainingCultistcount) {
            return $"Remaing cultist to recruit: " + (15 - p_remainingCultistcount); // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDataCreateDemonCultFaction : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new CreateDemonCultFaction();
        }
    }
}