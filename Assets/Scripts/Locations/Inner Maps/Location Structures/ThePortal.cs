using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class ThePortal : DemonicStructure {
        public override Vector2 selectableSize { get; }

        private List<Character> validMinions;

        public ThePortal(Region location) : base(STRUCTURE_TYPE.THE_PORTAL, location){
            selectableSize = new Vector2(10f, 10f);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            validMinions = new List<Character>();
            //PlayerAction summonMinion = new PlayerAction(PlayerDB.Summon_Minion_Action, 
            //    () => PlayerManager.Instance.player.mana >= EditableValuesManager.Instance.summonMinionManaCost, null,
            //    SummonMinion);
            AddPlayerAction(SPELL_TYPE.SUMMON_MINION);
        }
        public void SummonMinion() {
            validMinions.Clear();
            for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
                Minion minion = PlayerManager.Instance.player.minions[i];
                if(minion.character.currentHP >= minion.character.maxHP && minion.character.gridTileLocation == null) {
                    if (PlayerManager.Instance.player.archetype.CanSummonMinion(minion)) {
                        validMinions.Add(minion.character);
                    }
                }
            }
            //List<Character> validMinions = new List<Character>(PlayerManager.Instance.player.minions
            //    .Where(x => x.character.currentHP >= x.character.maxHP && x.character.gridTileLocation == null)
            //    .Select(x => x.character));
            UIManager.Instance.ShowClickableObjectPicker(validMinions,
                OnSelectMinionConfirmation, null, CanSummonMinion, 
                "Choose Minion to Summon", showCover: true, shouldConfirmOnPick: true, asButton: true);
            //PlayerManager.Instance.player.minions.Select(x => x.character).ToList()
        }
        private bool CanSummonMinion(Character character) {
            return character.currentHP >= character.maxHP && character.gridTileLocation == null;
        }
        private void OnSelectMinionConfirmation(object obj) {
            Character character = obj as Character;
            UIManager.Instance.ShowYesNoConfirmation("Summon Minion Confirmation", "Are you sure you want to summon " + character.name + "?", () => OnSelectMinion(character));
        }
        private void OnSelectMinion(Character character) {
            //Character character = obj as Character;
            character.minion.Summon(this);
            UIManager.Instance.HideObjectPicker();
            PlayerManager.Instance.GetPlayerActionData(SPELL_TYPE.SUMMON_MINION).OnExecuteSpellActionAffliction();
        }
    }
}