namespace Factions.Faction_Types {
    public class HumanEmpire : FactionType {
        public HumanEmpire() : base(FACTION_TYPE.Human_Empire) { }
        
        public override void SetAsDefault() {
            Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
            AddIdeology(peaceful);
            
            Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            exclusive.SetRequirement(RACE.HUMANS);
            AddIdeology(exclusive);

            DivineWorship divineWorship =
                FactionManager.Instance.CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
            AddIdeology(divineWorship);
        }
    }
}