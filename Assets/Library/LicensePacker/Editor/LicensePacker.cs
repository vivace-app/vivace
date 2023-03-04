using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class LicensePacker : IPostprocessBuildWithReport
{
    #region Field

    public static string   LicenseFileName = "LICENSE";
    public static string   Split = "------------------------------------------------";
    public static string[] IgnoreDirectoriesStartsWith = new string[] { "com.unity" };

    private static readonly string[] ReturnCodes = new string[] { "\n", "\r", "\rn" };

    #endregion Field

    #region Property

    public int callbackOrder { get { return 0; } }

    #endregion Property

    #region Method

    public void OnPreprocessBuild(BuildReport buildReport)
    {
        // NOTE:
        // Nothing to do.
        // But need to implemnt IPostprocessBuildWithReport.
    }
 
    public void OnPostprocessBuild(BuildReport buildReport)
    {
        // NOTE:
        // BuildReport sometime gets Unknown even if Succeeded.
        // in Unity ver.2018/2019.

        if (buildReport.summary.result == (BuildResult.Cancelled | BuildResult.Failed))
        {
            return;
        }

        string outputDirectoryPath = Path.GetDirectoryName(buildReport.summary.outputPath);

        string packedLicenseText = PackLicenses(GetPackedLicenseFiles(buildReport));

        TextFileReadWriter.Write(outputDirectoryPath + "/" + LicenseFileName, packedLicenseText);
    }

    public static string PackLicenses(List<string> licenseFiles)
    {
        return PackLicenses(licenseFiles, Split, IgnoreDirectoriesStartsWith);
    }

    public static string PackLicenses(List<string> licenseFiles, string split,
                                      params string[] ignoreDirectoriesStartsWith)
    {
        string packedLicense = "";

        foreach (var licenseFile in licenseFiles)
        {
            var readResult = TextFileReadWriter.Read(licenseFile);

            if (!readResult.success)
            {
                continue;
            }

            string directoryName = Path.GetFileName(Path.GetDirectoryName(licenseFile));

            if (ignoreDirectoriesStartsWith.Any
               (ignore => directoryName.StartsWith(ignore, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            packedLicense += directoryName + "\n";
            packedLicense += split         + "\n";
            packedLicense += readResult.text
                          + (ReturnCodes.Any(c => readResult.text.EndsWith(c)) ? "\n" : "\n\n");
        }

        return packedLicense.TrimEnd();
    }

    private static List<string> GetPackedLicenseFiles(BuildReport buildReport)
    {
        // NOTE:
        // packedAssetdsDirectories may not contains License files in there because of PackageManager system.
        // Assets in PackageManager are usually put into the directory which not include the license.

        List<string> packedAssetsDirectories = GetPackedAssetsDirectories(buildReport.packedAssets);

        // NOTE:
        // licenseFiles includes all of the path of License files even if its included in PackageManager.

        List<string> licenseFiles = GetLicenseFiles();

        List<string> packedLicenseFiles = new List<string>();

        foreach (string licensefile in licenseFiles)
        {
            string directory = Path.GetDirectoryName(licensefile);

            if (packedAssetsDirectories.Any
               (packedAssetDirectory => packedAssetDirectory.StartsWith(directory)))
            {
                packedLicenseFiles.Add(licensefile);
                continue;
            }
        }

        return packedLicenseFiles;
    }

    private static List<string> GetPackedAssetsDirectories(PackedAssets[] packedAssets)
    {
        List<string> directories = new List<string>();

        foreach (var packedAsset in packedAssets)
        {
            foreach (var content in packedAsset.contents)
            {
                if (string.IsNullOrWhiteSpace(content.sourceAssetPath))
                {
                    continue;
                }

                string directory = Path.GetDirectoryName(content.sourceAssetPath);

                if (directories.Contains(directory))
                {
                    continue;
                }

                directories.Add(directory);
            }
        }

        return directories;
    }

    public static List<string> GetLicenseFiles()
    {
        string[] licenseGUIDs = AssetDatabase.FindAssets(LicenseFileName);

        List<string> filePaths = new List<string>();

        foreach (string guid in licenseGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (File.GetAttributes(assetPath).HasFlag(FileAttributes.Directory))
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (!string.Equals(LicenseFileName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            filePaths.Add(assetPath);
        }

        return filePaths;
    }

    #endregion Method
}