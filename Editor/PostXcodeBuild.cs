using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class PostXcodeBuild
{
#if UNITY_IOS
    [PostProcessBuild]
    public static void SetXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS) return;

        var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // set info.plist
        var array = plist.root.CreateArray("SKAdNetworkItems");
        PlistElementDict dict = array.AddDict();
        dict.SetString("SKAdNetworkIdentifier", "cstr6suwn9.skadnetwork");
        File.WriteAllText(plistPath, plist.WriteToString());
    }
#endif
}