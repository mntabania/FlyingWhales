using System.Collections.Generic;

//public class CharacterLevelComparer : IComparer<Character> {
//    public int Compare(Character x, Character y) {
//        return x.level.CompareTo(y.level);
//    }
//}

public class CharacterFactionComparer : IComparer<Character> {
    public int Compare(Character x, Character y) {
        return x.faction.id.CompareTo(y.faction.id);
    }
}