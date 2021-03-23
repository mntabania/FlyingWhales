using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class SummonTheDemon : ReactionQuest {


        private SummonTheDemonStep _summonTheDemon;

        #region getters
        public override Type serializedData => typeof(SaveDataSummonTheDemon);
        #endregion

        public SummonTheDemon() : base($"Summon the almighty demon") { }
        protected override void ConstructSteps() {
            _summonTheDemon = new SummonTheDemonStep(GetSummonTheDemonDescription);
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_summonTheDemon),
            };
        }

        #region Step Helpers
        private string GetSummonTheDemonDescription(int currentSummonPoints, int totalsummonPoints) {
            return $"Summon meter: {currentSummonPoints.ToString()}/{totalsummonPoints.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }

    public class SaveDataSummonTheDemon : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new SummonTheDemon();
        }
    }
}