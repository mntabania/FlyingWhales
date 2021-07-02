using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;
namespace Locations {
    public class SettlementClassTracker {

        #region IListener
        public interface ISettlementTrackerListener {
            void OnNeededClassRemoved(string p_className);
        }
        #endregion
        
        private static readonly string[] _characterClassOrder = new[] { "Civilian", "Combatant", "Civilian", "Combatant", "Combatant", "Noble", "Combatant" };
        private readonly List<string> _neededClasses;
        private readonly List<string> _currentResidentClasses;
        private int _currentClassOrderIndex;

        private System.Action<string> onNeededClassRemoved;
        
        #region getters
        public int currentClassOrderIndex => _currentClassOrderIndex;
        public List<string> neededClasses => _neededClasses;
        public List<string> currentResidentClasses => _currentResidentClasses;
        #endregion
        
        public SettlementClassTracker() {
            _neededClasses = new List<string>();
            _currentResidentClasses = new List<string>();
            _currentClassOrderIndex = 0;
        }
        public SettlementClassTracker(SaveDataSettlementClassTracker p_data) {
            _neededClasses = p_data.neededClasses;
            _currentResidentClasses = p_data.currentResidentClasses;
            _currentClassOrderIndex = p_data.currentClassOrderIndex;
        }
        
        public void AddNeededClass(string p_className) {
            neededClasses.Add(p_className);
        }
        public void RemoveNeededClass(string p_className) {
            neededClasses.Remove(p_className);
            if (!neededClasses.Contains(p_className)) {
                //only fire event if there is no more of the given class inside the list.
                onNeededClassRemoved?.Invoke(p_className);    
            }
        }
        public void OnResidentAdded(Character p_newResident) {
            currentResidentClasses.Add(p_newResident.characterClass.className);
        }
        public void OnResidentRemoved(Character p_newResident) {
            currentResidentClasses.Remove(p_newResident.characterClass.className);
        }
        public void OnResidentChangedClass(string p_previousClass, Character p_character) {
            currentResidentClasses.Remove(p_previousClass);
            currentResidentClasses.Add(p_character.characterClass.className);
        }
        public string GetNextClassToCreateAndIncrementOrder([NotNull]Faction p_faction) {
            string classToCreate = GetCurrentClassInClassOrder(p_faction);
            _currentClassOrderIndex = currentClassOrderIndex + 1;
            if (currentClassOrderIndex == _characterClassOrder.Length) {
                _currentClassOrderIndex = 0;
            }
            return classToCreate;
        }
        private string GetCurrentClassInClassOrder([NotNull]Faction p_faction) {
            string currentClass = _characterClassOrder[currentClassOrderIndex];
            if (currentClass == "Combatant") {
                currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(p_faction.factionType.combatantClasses);
            } else if (currentClass == "Civilian") {
                currentClass = UtilityScripts.CollectionUtilities.GetRandomElement(p_faction.factionType.civilianClasses);
            }
            return currentClass;
        }
        public int GetCurrentResidentClassAmount(string p_className) {
            int classCount = 0;
            for (int i = 0; i < _currentResidentClasses.Count; i++) {
                if (currentResidentClasses[i] == p_className) {
                    classCount++;
                }
            }
            return classCount;
        }
        public bool HasExcessOfClass(string p_className) {
            if (neededClasses.Contains(p_className)) {
                int neededAmount = neededClasses.Count(c => c == p_className);
                int currentAmount = _currentResidentClasses.Count(c => c == p_className);
                return currentAmount > neededAmount;
            }
            //if class isn't needed, then consider it as excess 
            return false;
        }

        #region Listeners
        public void SubscribeToNeededClassRemoved(ISettlementTrackerListener p_listener) {
            onNeededClassRemoved += p_listener.OnNeededClassRemoved;
        }
        public void UnsubscribeToNeededClassRemoved(ISettlementTrackerListener p_listener) {
            onNeededClassRemoved -= p_listener.OnNeededClassRemoved;
        }
        #endregion
    }

    public class SaveDataSettlementClassTracker : SaveData<SettlementClassTracker> {

        public int currentClassOrderIndex;
        public List<string> neededClasses;
        public List<string> currentResidentClasses;
        
        public override void Save(SettlementClassTracker data) {
            base.Save(data);
            currentClassOrderIndex = data.currentClassOrderIndex;
            neededClasses = new List<string>(data.neededClasses);
            currentResidentClasses = new List<string>(data.currentResidentClasses);
        }
    }
}