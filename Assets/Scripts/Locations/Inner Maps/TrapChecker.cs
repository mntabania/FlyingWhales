using System;
using JetBrains.Annotations;

public class TrapChecker {
    private System.Func<Character, bool> _trapChecker;

    public TrapChecker([NotNull]Func<Character, bool> trapChecker) {
        _trapChecker = trapChecker;
    }

    public bool CanTrapAffectCharacter(Character character) {
        return _trapChecker.Invoke(character);
    }
}