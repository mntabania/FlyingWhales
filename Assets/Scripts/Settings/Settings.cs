namespace Settings {
    [System.Serializable]
    public struct Settings {

        //Controls
        public bool useEdgePanning;
        public bool skipTutorials;
        // public bool skipAdvancedTutorials;
        public bool confineCursor;
        
        //Graphics
        public string resolution;
        public int graphicsQuality;
        public bool fullscreen;
        public bool isVsyncOn;
        public bool doNotShowVideos;

        //Audio
        public float musicVolume;
        public float masterVolume;

        //Misc
        public bool skipEarlyAccessAnnouncement;
        public bool disableCameraShake;
        public bool randomizeMonsterNames;
    }
}