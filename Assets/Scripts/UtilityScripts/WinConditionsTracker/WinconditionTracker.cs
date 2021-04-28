using System.Collections.Generic;

public abstract class WinConditionTracker {

    private IBookmarkable[] _winConditionSteps;
    
    public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }
    public abstract System.Type serializedData { get; }

    #region getters
    public IBookmarkable[] winConditionSteps => _winConditionSteps;
    #endregion
    
    protected WinConditionTracker() {
        bookmarkEventDispatcher = new BookmarkableEventDispatcher();
    }

    #region Loading
    public virtual void LoadReferences(SaveDataWinConditionTracker data) { } 
    #endregion

    public virtual void Initialize(List<Character> p_allCharacters) {
        _winConditionSteps = CreateWinConditionSteps();
    }
    protected abstract IBookmarkable[] CreateWinConditionSteps();
    protected bool ShouldConsiderCharacterAsEliminated(Character character) {
        if (character.isDead) {
            return true;
        }
        if (character.traitContainer.HasTrait("Cultist")) {
            return true;
        }
        if (character.faction != null) {
            if (!character.faction.isMajorNonPlayerOrVagrant && character.faction.factionType.type != FACTION_TYPE.Ratmen) {
                return true;
            }
        }
        return false;
    }
    protected List<Character> GetAllCharactersToBeEliminated(List<Character> p_allCharacters) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < p_allCharacters.Count; i++) {
            Character character = p_allCharacters[i];
            if (!character.isDead && character.isNormalCharacter && character.race.IsSapient()) {
                characters.Add(character);
            }
        }
        return characters;
    }

    #region IBookmarkable Implementation
    protected void UpdateStepsChangedNameEvent() {
        if (winConditionSteps != null) {
            for (int i = 0; i < winConditionSteps.Length; i++) {
                IBookmarkable bookmarkable = winConditionSteps[i];
                bookmarkable.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(bookmarkable);
            }    
        }
    }
    public void AddStepsToBookmark() {
        for (int i = 0; i < winConditionSteps.Length; i++) {
            IBookmarkable winConditionStep = winConditionSteps[i];
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(winConditionStep, BOOKMARK_CATEGORY.Win_Condition);    
        }
    }
    public void RemoveStepsFromBookmark() {
        for (int i = 0; i < winConditionSteps.Length; i++) {
            IBookmarkable winConditionStep = winConditionSteps[i];
            PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(winConditionStep, BOOKMARK_CATEGORY.Win_Condition);    
        }
    }
    #endregion
}

public class SaveDataWinConditionTracker : SaveData<WinConditionTracker> {
    public override void Save(WinConditionTracker data) {
        base.Save(data);
    }
}
