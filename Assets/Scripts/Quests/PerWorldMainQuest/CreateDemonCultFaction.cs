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

        public CreateDemonCultFaction() : base($"Grow your own Cult") { }
        protected override void ConstructSteps() {
            QuestStep turnVillagerToPsychopathStep = new CreateDemonFactionStep(GetCreateCultFactionDescription).SetHoverOverAction(OnHoverOverStep1)
                .SetHoverOutAction(UIManager.Instance.HideSmallInfo); ;
            _eliminateVillagerStep = new RecruitFifteenMembersDemonCultStep(GetRecruitFifteenCultist);   
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(turnVillagerToPsychopathStep),
                new QuestStepCollection(_eliminateVillagerStep),
            };
        }

        private void OnHoverOverStep1(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                "HINT: After recruiting enough cultists, one may eventually become a Cult Leader. You can directly order a Cult Leader to start its own Demon Cult faction. Transform villagers into cultists by brainwashing them in your Prison.",
                stepItem.hoverPosition, "Cult Faction"
            );
        }

        #region Step Helpers
        private string GetCreateCultFactionDescription() {
            return $"Start a Demon Cult"; // /{totalCharactersToEliminate.ToString()}
        }

        private string GetRecruitFifteenCultist(int p_cultistCount) {
            return $"Demon Cult Members: " + p_cultistCount + "/12"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDataCreateDemonCultFaction : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new CreateDemonCultFaction();
        }
    }
}