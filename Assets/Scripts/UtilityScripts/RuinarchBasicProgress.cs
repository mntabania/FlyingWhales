namespace UtilityScripts {
    public class RuinarchBasicProgress : RuinarchProgressable {
        public override string name { get; }
        public override BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Progress_Bar;

        public RuinarchBasicProgress(string p_name) {
            name = p_name;
        }
    }
}