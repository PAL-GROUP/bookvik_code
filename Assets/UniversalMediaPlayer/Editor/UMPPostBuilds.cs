using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UMP.Helpers;
using Ionic.Zip;
#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.Collections.Generic;
using System.IO;

public class UMPPostBuilds : MonoBehaviour
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows:
                BuildWindowsPlayer32(path);
                break;

            case BuildTarget.StandaloneWindows64:
                BuildWindowsPlayer64(path);
                break;

            case BuildTarget.StandaloneOSXIntel64:
                BuildMacPlayer64(path);
                break;

            case BuildTarget.StandaloneLinux:
                BuildLinuxPlayer32(path);
                break;

            case BuildTarget.StandaloneLinux64:
                BuildLinuxPlayer64(path);
                break;

            case BuildTarget.StandaloneLinuxUniversal:
                BuildLinuxPlayerUniversal(path);
                break;

            case BuildTarget.Android:
                BuildForAndroid(path);
                break;

            case BuildTarget.iOS:
                BuildForiOS(path);
                break;
        }
    }

    private static void BuildForiOS(string path)
    {
#if UNITY_IPHONE
        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        Debug.Log("Build iOS. path: " + projPath);

        PBXProject proj = new PBXProject();
        var file = File.ReadAllText(projPath);
        proj.ReadFromString(file);

        string target = proj.TargetGuidByName("Unity-iPhone");

        proj.AddFrameworkToProject(target, "AddressBook.framework", false);
        proj.AddFrameworkToProject(target, "AssetsLibrary.framework", false);
        proj.AddFrameworkToProject(target, "CoreData.framework", false);
        proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
        proj.AddFrameworkToProject(target, "CoreText.framework", false);
        proj.AddFrameworkToProject(target, "Security.framework", false);
        proj.AddFrameworkToProject(target, "WebKit.framework", false);
        proj.AddFrameworkToProject(target, "ImageIO.framework", false);
        proj.AddFrameworkToProject(target, "EventKit.framework", false);
        proj.AddFrameworkToProject(target, "EventKitUI.framework", false);
        proj.AddFrameworkToProject (target, "AdSupport.framework", false);
        proj.AddFrameworkToProject (target, "AudioToolbox.framework", false);
        proj.AddFrameworkToProject (target, "AVFoundation.framework", false);
        proj.AddFrameworkToProject (target, "CoreGraphics.framework", false);
        proj.AddFrameworkToProject (target, "EventKit.framework", false);
        proj.AddFrameworkToProject (target, "EventKitUI.framework", false);
        proj.AddFrameworkToProject (target, "MessageUI.framework", false);
        proj.AddFrameworkToProject (target, "StoreKit.framework", false);
        proj.AddFrameworkToProject (target, "SystemConfiguration.framework", false);

        string fileGuid = proj.AddFile("usr/lib/" + "libc++.dylib", "Frameworks/" + "libc++.dylib", PBXSourceTree.Sdk);
        proj.AddFileToBuild(target, fileGuid);
        fileGuid = proj.AddFile("usr/lib/" + "libz.dylib", "Frameworks/" + "libz.dylib", PBXSourceTree.Sdk);
        proj.AddFileToBuild(target, fileGuid);
        fileGuid = proj.AddFile("usr/lib/" + "libz.tbd", "Frameworks/" + "libz.tbd", PBXSourceTree.Sdk);
        proj.AddFileToBuild(target, fileGuid);

        fileGuid = proj.AddFile("usr/lib/" + "libbz2.dylib", "Frameworks/" + "libbz2.dylib", PBXSourceTree.Sdk);
        proj.AddFileToBuild(target, fileGuid);
        fileGuid = proj.AddFile("usr/lib/" + "libbz2.tbd", "Frameworks/" + "libbz2.tbd", PBXSourceTree.Sdk);
        proj.AddFileToBuild(target, fileGuid);

        File.WriteAllText(projPath, proj.WriteToString());
#endif
    }

    private static void BuildForAndroid(string path)
    {
        if (UMPPreference.Instance.UseAndroidNative)
        {
            ZipFile apkFile = ZipFile.Read(path);
            string[] libs = Directory.GetFiles(Application.dataPath + "/UniversalMediaPlayer/Plugins/Android/libs/armeabi-v7a");
            List<ZipEntry> rlibs = new List<ZipEntry>();

            foreach (var entry in apkFile.Entries)
            {
                foreach (var lib in libs)
                {
                    if (entry.FileName.Contains(Path.GetFileName(lib)) && !lib.Contains("media_player_helper"))
                        rlibs.Add(entry);
                }
            }
            apkFile.RemoveEntries(rlibs);
            apkFile.Save();
        }
    }

    public static void BuildWindowsPlayer32(string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!UMPPreference.Instance.UseExternalLibs)
            {
                CopyPlugins(Application.dataPath + "/UniversalMediaPlayer/Plugins/Win/x86/plugins/", dataPath + "/Plugins/plugins/");
            }
            else
            {
                File.Delete(dataPath + "/Plugins/libvlc.dll");
                File.Delete(dataPath + "/Plugins/libvlccore.dll");
            }
        }
        Debug.Log("Standalone Windows (x86) build is completed: " + path);
    }

    public static void BuildWindowsPlayer64(string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!UMPPreference.Instance.UseExternalLibs)
            {
                CopyPlugins(Application.dataPath + "/UniversalMediaPlayer/Plugins/Win/x86_64/plugins/", dataPath + "/Plugins/plugins/");
            }
            else
            {
                File.Delete(dataPath + "/Plugins/libvlc.dll");
                File.Delete(dataPath + "/Plugins/libvlccore.dll");
            }
        }
        Debug.Log("Standalone Windows (x86_x64) build is completed: " + path);
    }

    public static void BuildMacPlayer64(string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileName(path) + "/Contents";

        if (!string.IsNullOrEmpty(buildPath) && UMPPreference.Instance.UseExternalLibs)
        {
            Directory.Delete(dataPath + "/Plugins/libvlc.bundle", true);
        }

        Debug.Log("Standalone Mac (x86_x64) build is completed: " + dataPath);
    }

    public static void BuildLinuxPlayer32(string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";
        string umpLauncherPath = Application.dataPath + "/UniversalMediaPlayer/Plugins/Linux/UMPLauncher.sh";
        string umpRemoverPath = Application.dataPath + "/UniversalMediaPlayer/Plugins/Linux/UMPRemover.sh";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!UMPPreference.Instance.UseExternalLibs)
            {
                string vlcFolderPath32 = dataPath + "/Plugins/x86/vlc";
                if (!Directory.Exists(vlcFolderPath32))
                    Directory.CreateDirectory(vlcFolderPath32);

                CopyPlugins(Application.dataPath + "/UniversalMediaPlayer/Plugins/Linux/x86/plugins/", vlcFolderPath32 + "/plugins/");
            }
            else
            {
                File.Delete(dataPath + "/Plugins/x86/libvlc.so");
                File.Delete(dataPath + "/Plugins/x86/libvlccore.so");
            }

            CopyShellScript("x86", umpLauncherPath, buildPath, Path.GetFileNameWithoutExtension(path));
            CopyShellScript("x86", umpRemoverPath, buildPath, Path.GetFileNameWithoutExtension(path));
        }
        Debug.Log("Standalone Linux (x86) build is completed: " + path);
    }

    public static void BuildLinuxPlayer64(string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";
        string umpLauncherPath = Application.dataPath + "/UniversalMediaPlayer/Plugins/Linux/UMPLauncher.sh";
        string umpRemoverPath = Application.dataPath + "/UniversalMediaPlayer/Plugins/Linux/UMPRemover.sh";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!UMPPreference.Instance.UseExternalLibs)
            {
                string vlcFolderPath64 = dataPath + "/Plugins/x86_64/vlc";
                if (!Directory.Exists(vlcFolderPath64))
                    Directory.CreateDirectory(vlcFolderPath64);

                CopyPlugins(Application.dataPath + "/UniversalMediaPlayer/Plugins/Linux/x86_64/plugins/", vlcFolderPath64 + "/plugins/");
            }
            else
            {
                File.Delete(dataPath + "/Plugins/x86_64/libvlc.so");
                File.Delete(dataPath + "/Plugins/x86_64/libvlccore.so");
            }

            CopyShellScript("x86_64", umpLauncherPath, buildPath, Path.GetFileNameWithoutExtension(path));
            CopyShellScript("x86_64", umpRemoverPath, buildPath, Path.GetFileNameWithoutExtension(path));
        }
        Debug.Log("Standalone Linux (x86_x64) build is completed: " + path);
    }

    public static void BuildLinuxPlayerUniversal(string path)
    {
        string buildPath = Path.GetDirectoryName(path);
        string dataPath = buildPath + "/" + Path.GetFileNameWithoutExtension(path) + "_Data";
        string umpLauncherPath = Application.dataPath + "/Plugins/Linux/UMPLauncher.sh";
        string umpRemoverPath = Application.dataPath + "/Plugins/Linux/UMPRemover.sh";

        if (!string.IsNullOrEmpty(buildPath))
        {
            if (!UMPPreference.Instance.UseExternalLibs)
            {
                string vlcFolderPath32 = dataPath + "/Plugins/x86/vlc";
                if (!Directory.Exists(vlcFolderPath32))
                    Directory.CreateDirectory(vlcFolderPath32);

                string vlcFolderPath64 = dataPath + "/Plugins/x86_64/vlc";
                if (!Directory.Exists(vlcFolderPath64))
                    Directory.CreateDirectory(vlcFolderPath64);

                CopyPlugins(Application.dataPath + "/Plugins/Linux/x86/plugins/", vlcFolderPath32 + "/plugins/");
                CopyPlugins(Application.dataPath + "/Plugins/Linux/x86_64/plugins/", vlcFolderPath64 + "/plugins/");
            }
            {
                File.Delete(dataPath + "/Plugins/x86/libvlc.so");
                File.Delete(dataPath + "/Plugins/x86/libvlccore.so");
                File.Delete(dataPath + "/Plugins/x86_64/libvlc.so");
                File.Delete(dataPath + "/Plugins/x86_64/libvlccore.so");
            }

            CopyShellScript(string.Empty, umpLauncherPath, buildPath, Path.GetFileNameWithoutExtension(path));
            CopyShellScript(string.Empty, umpRemoverPath, buildPath, Path.GetFileNameWithoutExtension(path));
        }
        Debug.Log("Standalone Linux (Universal) build is completed: " + path);
    }

    private static void CopyPlugins(string sourcePath, string targetPath)
    {
        string fileName = string.Empty;
        string destFile = targetPath;

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        string[] directories = Directory.GetDirectories(sourcePath);

        foreach (var d in directories)
        {
            string[] files = Directory.GetFiles(d);

            if (files.Length > 0)
            {
                destFile = Path.Combine(targetPath, Path.GetFileName(d));
                Directory.CreateDirectory(destFile);
            }

            foreach (var s in files)
            {
                if (Path.GetExtension(s).Equals(".meta"))
                    continue;

                fileName = Path.GetFileName(s);
                File.Copy(s, Path.Combine(destFile, fileName), false);
            }
        }
    }

    private static void CopyShellScript(string bitCapacity, string scriptPath, string targetPath, string fileName)
    {
        string scriptName = Path.GetFileName(scriptPath);
        File.Copy(scriptPath, Path.Combine(targetPath, scriptName), false);

        using (FileStream fs = new FileStream(Path.Combine(targetPath, scriptName), FileMode.Append, FileAccess.Write))
        using (StreamWriter sw = new StreamWriter(fs))
        {
            if (scriptName.Contains("Launcher"))
            {
                sw.WriteLine("sudo sh -c \"echo '$(dirname \"$0\")/" + fileName + "_Data/Plugins/x86/' >> /etc/ld.so.conf.d/ump.conf\"");
                sw.WriteLine("sudo ldconfig");

                sw.WriteLine("sudo sh -c \"echo '$(dirname \"$0\")/" + fileName + "_Data/Plugins/x86_64/' >> /etc/ld.so.conf.d/ump.conf\"");
                sw.WriteLine("sudo ldconfig");

                if (bitCapacity.Equals("x86"))
                {
                    sw.WriteLine("chmod +x " + fileName + ".x86");
                    sw.WriteLine("exec ./" + fileName + ".x86 \"$@\"");
                }
                else if (bitCapacity.Equals("x86_64"))
                {
                    sw.WriteLine("chmod +x " + fileName + ".x86_64");
                    sw.WriteLine("exec ./" + fileName + ".x86_64 \"$@\"");
                }
                else
                {
                    sw.WriteLine("chmod +x " + fileName + ".x86");
                    sw.WriteLine("exec ./" + fileName + ".x86 \"$@\"");
                    sw.WriteLine("chmod +x " + fileName + ".x86_64");
                    sw.WriteLine("exec ./" + fileName + ".x86_64 \"$@\"");
                }
            }

            sw.Close();
            fs.Close();
        }
    }
}
