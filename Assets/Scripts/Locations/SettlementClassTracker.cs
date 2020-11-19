using System.Collections.Generic;
using JetBrains.Annotations;
namespace Locations {
    public class SettlementClassTracker {

        private List<string> _neededClasses;
        private List<string> _currentResidentClasses;
        private string[] _characterClassOrder;
        private int _currentClassOrderIndex;
        
        public SettlementClassTracker() {
            _neededClasses = new List<string>();
            _currentResidentClasses = new List<string>();
            _currentClassOrderIndex = 0;
            _characterClassOrder = new string[] {
                "Craftsman",
                "Peasant",
                "Combatant",
                "Civilian",
                "Combatant",
                "Combatant",
                "Noble",
                "Combatant",
            };
        }

        public void AddNeededClass(string p_className) {
            _neededClasses.Add(p_className);
        }
        public void RemoveNeededClass(string p_className) {
            _neededClasses.Remove(p_className);
        }
        public void OnResidentAdded(Character p_newResident) {
            _currentResidentClasses.Add(p_newResident.characterClass.className);
        }
        public void OnResidentRemoved(Character p_newResident) {
            _currentResidentClasses.Remove(p_newResident.characterClass.className);
        }
        private void OnResidentChangedClass(string p_previousClass, Character p_character) {
            
        }
        public string GetNextClassToCreateAndIncrementOrder([NotNull]Faction p_faction) {
            string classToCreate = GetCurrentClassInClassOrder(p_faction);
            _currentClassOrderIndex++;
            if (_currentClassOrderIndex == _characterClassOrder.Length) {
                _currentClassOrderIndex = 0;
            }
            return classToCreate;
        }
        private string GetCurrentClassInClassOrder([NotNull]Faction p_faction) {
            string currentClass = _characterClassOrder[_currentClassOrderIndex];
            if (currentClass == "Combatant") {
                currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(p_faction.factionType.combatantClasses);
            } else if (currentClass == "Civilian") {
                currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(p_faction.factionType.civilianClasses);
            }
            return currentClass;
        }

    }
}