using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class TheGoader : DemonicStructure {
        public override Vector2 selectableSize { get; }
        public TheGoader(Region location) : base(STRUCTURE_TYPE.THE_GOADER, location){
            selectableSize = new Vector2(10f, 10f);
        }

        #region Initialization
        public override void Initialize() {
            base.Initialize();
            AddInterfereAction();
        }
        #endregion

        #region Overrides
        protected override void DestroyStructure() {
            base.DestroyStructure();
            RemoveInterfereAction();
        }
        #endregion
        
        #region Interfere
        public void ShowInterfereUI() {
            DualObjectPickerTabSetting[] tabs = new[] {
                new DualObjectPickerTabSetting() {
                    name = "Join Faction",
                    onToggleTabAction = OnClickJoinFaction
                },
                new DualObjectPickerTabSetting() {
                    name = "Leave Faction",
                    onToggleTabAction = OnClickLeaveFaction
                }
            };
            UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(tabs);
        }
        private void AddInterfereAction() {
            //PlayerAction interfere = new PlayerAction(PlayerDB.Interfere_Action, () => true, null, ShowInterfereUI);
            AddPlayerAction(SPELL_TYPE.INTERFERE);
        }
        private void RemoveInterfereAction() {
            //RemovePlayerAction(GetPlayerAction(PlayerDB.Interfere_Action));
            RemovePlayerAction(SPELL_TYPE.INTERFERE);
        }
        #endregion

        #region Join Faction
        private void OnClickJoinFaction(bool isOn) {
            List<Character> viableCharacters = new List<Character>();
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character character = CharacterManager.Instance.allCharacters[i];
                if (character.isFriendlyFactionless && character.isStillConsideredAlive && character.IsAble()) {
                    viableCharacters.Add(character);
                }
            }
            viableCharacters = viableCharacters.OrderBy(x => x.faction.name).ToList();
            UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(viableCharacters, "Choose Character", 
                CanChooseCharacterToJoin, OnHoverJoinCharacter, OnHoverExitJoinCharacter, OnPickCharacter, 
                ConfirmJoin, "Join Faction");
        }
        private bool CanChooseCharacterToJoin(Character character) {
            return character.traitContainer.GetNormalTrait<Trait>("Blessed") == null 
                   && PlayerManager.Instance.player.mana >= GetJoinManaCostForCharacter(character);
        }
        private void OnPickCharacter(object obj) {
            Character character = obj as Character;
            List<Faction> viableFactions = new List<Faction>();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer && faction.name != "Undead") {
                    viableFactions.Add(faction);
                }
            }
            UIManager.Instance.dualObjectPicker.PopulateColumn(viableFactions, (faction) => 
                CanChooseFactionToJoin(faction, character), null, null, 
                UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, 
                "Choose Faction");
        }
        private bool CanChooseFactionToJoin(Faction faction, Character character) {
            return !faction.isDestroyed && faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(character);
        }
        private void ConfirmJoin(object obj1, object obj2) {
            Character character = obj1 as Character;
            Faction faction = obj2 as Faction;
            PlayerManager.Instance.player.AdjustMana(-GetJoinManaCostForCharacter(character));
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, faction.characters[0], "join_faction_normal");
            PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.INTERFERE).OnExecuteSpellActionAffliction();
        }
        private void OnHoverJoinCharacter(Character character) {
            UIManager.Instance.ShowSmallInfo($"Mana cost make {character.name} join a faction: {GetJoinManaCostForCharacter(character)}");
        }
        private void OnHoverExitJoinCharacter(Character character) {
            UIManager.Instance.HideSmallInfo();
        }
        private int GetJoinManaCostForCharacter(Character character) {
            return 50;
        }
        #endregion

        #region Leave Faction
        private void OnClickLeaveFaction(bool isOn) {
            List<Faction> viableFactions = new List<Faction>();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.isMajorNonPlayer && faction.name != "Undead") {
                    viableFactions.Add(faction);
                }
            }
            viableFactions = viableFactions.OrderBy(x => x.name).ToList();
            UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(viableFactions, "Choose Faction", 
                CanChooseFactionToLeave, null, null, 
                OnPickFaction, ConfirmLeave, "Leave Faction");
        }
        private bool CanChooseFactionToLeave(Faction faction) {
            return true;
        }
        private void OnPickFaction(object obj) {
            Faction faction = obj as Faction;
            UIManager.Instance.dualObjectPicker.PopulateColumn(faction.characters, CanChooseCharacterToLeave, 
                OnHoverLeaveCharacter, OnHoverExitLeaveCharacter, UIManager.Instance.dualObjectPicker.column2ScrollView, 
                UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Character");
        }
        private bool CanChooseCharacterToLeave(Character character) {
            return character.traitContainer.GetNormalTrait<Trait>("Blessed") == null 
                   && PlayerManager.Instance.player.mana >= GetLeaveManaCostForCharacter(character);
        }
        private void ConfirmLeave(object obj1, object obj2) {
            Faction faction = obj1 as Faction;
            Character character = obj2 as Character;
            PlayerManager.Instance.player.AdjustMana(-GetLeaveManaCostForCharacter(character));
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
            PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.INTERFERE).OnExecuteSpellActionAffliction();
        }
        private int GetLeaveManaCostForCharacter(Character character) {
            if (character.isFactionLeader) {
                return 200;
            } else {
                return 50;
            }
        }
        private void OnHoverLeaveCharacter(Character character) {
            UIManager.Instance.ShowSmallInfo($"Mana cost to make {character.name} leave {character.faction.name}: {GetLeaveManaCostForCharacter(character)}");
        }
        private void OnHoverExitLeaveCharacter(Character character) {
            UIManager.Instance.HideSmallInfo();
        }
        #endregion
    }
}