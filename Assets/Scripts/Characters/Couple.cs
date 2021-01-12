using System;

public class Couple : IEquatable<Couple> {
    public PreCharacterData character1 { get; }
    public PreCharacterData character2 { get; }

    public Couple(PreCharacterData _character1, PreCharacterData _character2) {
        character1 = _character1;
        character2 = _character2;
    }
    public bool Equals(Couple other) {
        if (other == null) {
            return false;
        }
        return (character1.id == other.character1.id && character2.id == other.character2.id) ||
               (character1.id == other.character2.id && character2.id == other.character1.id);
    }
    public override bool Equals(object obj) {
        return Equals(obj as  Couple);
    }
    public override int GetHashCode() {
        return character1.id + character2.id;
    }
}