using UnityEditor;
using UnityEngine;
public class Builder {
    [MenuItem("Build/Build Release Windows 32-Bit")]
    public static void BuildWindows32Bit() {
        //get the path
        string path = $"{Application.dataPath}/../bin/Ruinarch v{Application.version}/Ruinarch 32-Bit"; 
        // Build player
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/Ruinarch.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        // Do other things with the build folder
    }
    
    [MenuItem("Build/Build Release Windows 64-Bit")]
    public static void BuildWindows64Bit() {
        //get the path
        string path = $"{Application.dataPath}/../bin/Ruinarch v{Application.version}/Ruinarch 64-Bit"; 
        // Build player
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/Ruinarch.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        // Do other things with the build folder
    }
    
    [MenuItem("Build/Build Release Windows 64 & 32 Bit")]
    public static void BuildWindows64And32Bit() {
        BuildWindows32Bit();
        BuildWindows64Bit();
    }
    
    [MenuItem("Build/Build Development Build Windows 64-Bit")]
    public static void BuildDevelopmentBuildWindows64Bit() {
        BuildOptions buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
        //get the path
        string path = $"{Application.dataPath}/../bin/Ruinarch v{Application.version} Development Build/Ruinarch"; 
        // Build player
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/Ruinarch.exe", BuildTarget.StandaloneWindows64, buildOptions);
        // Do other things with the build folder
    }
}
