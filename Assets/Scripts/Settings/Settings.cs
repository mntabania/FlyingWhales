namespace Settings {
    [System.Serializable]
    public struct Settings {

        //Controls
        public bool useEdgePanning;
        public bool skipTutorials;
        
        //Graphics
        public string resolution;
        public int graphicsQuality;
        public bool fullscreen;
        public bool isVsyncOn;

        //Audio
        public float musicVolume;
        public float masterVolume;
    }
}