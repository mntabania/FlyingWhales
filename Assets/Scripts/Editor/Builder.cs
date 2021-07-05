using System.IO;
using UnityEditor;
using UnityEngine;
public class Builder {
    [MenuItem("Build/Build Release Windows 32-Bit")]
    public static void BuildWindows32Bit() {
        //get the path
        string path = $"{Application.dataPath}/../bin/Ruinarch v{Application.version}/32-Bit"; 
        // Build player
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/Ruinarch.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        // Do other things with the build folder
        PlaceRedistributables(path);
        RelocateDLLs(path, "x86", false);
    }
    
    [MenuItem("Build/Build Release Windows 64-Bit")]
    public static void BuildWindows64Bit() {
        //get the path
        string path = $"{Application.dataPath}/../bin/Ruinarch v{Application.version}/64-Bit"; 
        // Build player
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/Ruinarch.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        // Do other things with the build folder
        PlaceRedistributables(path);
        RelocateDLLs(path, "x86_64", true);
    }

    [MenuItem("Build/Build Release Windows 64 and 32 Bit")]
    public static void BuildWindows64And32Bit() {
        BuildWindows32Bit();
        BuildWindows64Bit();
    }
    
    [MenuItem("Build/Build Development Build Windows 64-Bit")]
    public static void BuildDevelopmentBuildWindows64Bit() {
        BuildOptions buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
        //get the path
        string path = $"{Application.dataPath}/../bin/Ruinarch v{Application.version}/Ruinarch v{Application.version} Development Build/Ruinarch"; 
        // Build player
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path + "/Ruinarch.exe", BuildTarget.StandaloneWindows64, buildOptions);
        // Do other things with the build folder
        PlaceRedistributables(path);
        RelocateDLLs(path, "x86_64", true);
    }

    #region Utilities
    private static void RelocateDLLs(string buildPath, string pluginsFolder, bool is64bit) {
        //SQLite.Interop.dll
        string sqliteDLL = $"{buildPath}/Ruinarch_Data/Plugins/{pluginsFolder}/SQLite.Interop.dll";
        FileUtil.CopyFileOrDirectory(sqliteDLL, $"{buildPath}/SQLite.Interop.dll");
        Directory.CreateDirectory($"{buildPath}/Ruinarch_Data/Mono/");
        FileUtil.CopyFileOrDirectory(sqliteDLL, $"{buildPath}/Ruinarch_Data/Mono/SQLite.Interop.dll");

        string sqliteDirectory = is64bit ? $"{buildPath}/redist/sqlite 64bit/" : $"{buildPath}/redist/sqlite 32bit/";
        string[] dllsToCopy = new[] {"System.Data.SQLite.dll", "System.Data.SQLite.EF6.dll", "System.Data.SQLite.Linq.dll"};
        for (int i = 0; i < dllsToCopy.Length; i++) {
            string dllToCopy = dllsToCopy[i];
            sqliteDLL = $"{sqliteDirectory}/{dllToCopy}";
            FileUtil.CopyFileOrDirectory(sqliteDLL, $"{buildPath}/{dllToCopy}");    
        }
    }
    private static void PlaceRedistributables(string buildPath) {
        string redistributablesLocation = $"{Application.dataPath}/../bin/redist/";
        FileUtil.CopyFileOrDirectory(redistributablesLocation, $"{buildPath}/redist/");
    }
    #endregion
}
