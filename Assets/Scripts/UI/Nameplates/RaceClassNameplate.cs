
public class RaceClassNameplate : NameplateItem<RaceClass> {
    
    public override void SetObject(RaceClass o) {
        base.SetObject(o);
        mainLbl.text = o.className;
        subLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(o.race.ToString());
    }
}
