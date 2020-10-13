using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inner_Maps.Location_Structures;
using Quests.Steps;
namespace Quests {
    public class DivineIntervention : ReactionQuest {

        private readonly List<Character> _angels;

        #region getters
        public List<Character> angels => _angels;
        public override Type serializedData => typeof(SaveDataDivineIntervention);
        #endregion
        
        public DivineIntervention(List<Character> angels) : base("Divine Intervention") {
            _angels = angels;
        }
        public DivineIntervention(SaveDataDivineIntervention data) : base("Divine Intervention") {
            _angels = SaveUtilities.ConvertIDListToCharacters(data.angelIDs);
        }
        
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new EliminateAngelStep(GetStopCharactersDescription, angels)
                        .SetHoverOverAction(OnHoverEliminateItem)
                        .SetHoverOutAction(() => UIManager.Instance.HideSmallInfo())
                        .SetObjectsToCenter(angels.Where(x => !x.isDead).Select(x => x as ISelectable).ToArray())    
                )
            };
        }

        #region Step Helpers
        private string GetStopCharactersDescription(List<Character> targets, int initialTargetCount) {
            return $"Eliminate {targets.Count.ToString()} Angels.";
        }
        private void OnHoverEliminateItem(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"The Divine has sent down {angels.Count.ToString()} Angels to stop you from further transgressions. " +
                "Eliminate all of them before they destroy your demonic structures.",
                pos: item.hoverPosition
            );
        }
        #endregion
    }
    
    public class SaveDataDivineIntervention : SaveDataReactionQuest {
        public List<string> angelIDs;
        public override void Save(ReactionQuest data) {
            base.Save(data);
            DivineIntervention divineIntervention = data as DivineIntervention;
            Debug.Assert(divineIntervention != null, nameof(divineIntervention) + " != null");
            angelIDs = SaveUtilities.ConvertSavableListToIDs(divineIntervention.angels);
        }
        public override ReactionQuest Load() {
            return new DivineIntervention(this);
        }
    }
}