namespace Settings {
    [System.Serializable]
    public struct Settings {

        //Controls
        public bool useEdgePanning;
        
        //Misc
        public bool skipTutorials;
        
        //Graphics
        public string resolution;
        public int graphicsQuality;
        public bool fullscreen;

        //Audio
        public float musicVolume;
        public float masterVolume;
    }
}