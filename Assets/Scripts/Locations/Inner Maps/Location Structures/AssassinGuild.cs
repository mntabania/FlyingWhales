namespace Inner_Maps.Location_Structures {
    public class AssassinGuild : ManMadeStructure {
        public AssassinGuild(Region location) : base(STRUCTURE_TYPE.ASSASSIN_GUILD, location) { }
        public AssassinGuild(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}