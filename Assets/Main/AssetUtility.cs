﻿using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace Main
{
    public class AssetUtility
    {
        public const string AssetsFolderName = "Assets";
        public const string AssetBundlesFolderName = "AssetBundles";

        public static string FormatToUnityPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string FormatToSysFilePath(string path)
        {
            return path.Replace("/", "\\");
        }

        public static string FullPathToAssetPath(string full_path)
        {
            full_path = FormatToUnityPath(full_path);
            if (!full_path.StartsWith(Application.dataPath))
            {
                return null;
            }
            string ret_path = full_path.Replace(Application.dataPath, "");
            return AssetsFolderName + ret_path;
        }
        
        public static string GetStreamingAssetsFilePath(string assetPath = null)
        {
            string outputPath = Path.Combine("file://" + Application.streamingAssetsPath, AssetBundlesFolderName);
#if UNITY_IPHONE || UNITY_IOS
            string outputPath = Path.Combine("file://" + Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
#elif UNITY_ANDROID
            string outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
#elif !UNITY_EDITOR
            Debug.LogError("Unsupported platform!!!");
#endif
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }

            // outputPath = FormatToUnityPath(outputPath);
            return outputPath;
        }

        public static string GetStreamingAssetsDataPath(string assetPath = null)
        {
            string outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundlesFolderName);
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }

            return outputPath;
        }

        public static string GetPersistentFilePath(string assetPath = null)
        {
            return "file://" + GetPersistentDataPath(assetPath);
        }

        public static string GetPersistentDataPath(string assetPath = null)
        {
            string outputPath = Path.Combine(Application.persistentDataPath, AssetBundlesFolderName);
            if (!string.IsNullOrEmpty(assetPath))
            {
                outputPath = Path.Combine(outputPath, assetPath);
            }
#if UNITY_STANDALONE_WIN
            return FormatToSysFilePath(outputPath);
#else
            return outputPath;
#endif
        }


        public static bool CheckPersistentFileExsits(string filePath)
        {
            var path = GetPersistentDataPath(filePath);
            return File.Exists(path);
        }

        // 注意：这个路径是给WWW读文件使用的url，如果要直接磁盘写persistentDataPath，使用GetPlatformPersistentDataPath
        public static string GetAssetBundleFileUrl(string filePath)
        {
            if (CheckPersistentFileExsits(filePath))
            {
                return GetPersistentFilePath(filePath);
            }
            else
            {
                return GetStreamingAssetsFilePath(filePath);
            }
        }

        public static string GetAssetBundleDataPath(string filePath)
        {
            if (CheckPersistentFileExsits(filePath))
            {
                return GetPersistentDataPath(filePath);
            }
            else
            {
                return GetStreamingAssetsDataPath(filePath);
            }
        }
        
        public static string GetFileExtension(string path)
        {
            return Path.GetExtension(path).ToLower();
        }
        public static string[] GetSpecifyFilesInFolder(string path, string[] extensions = null, bool exclude = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (extensions == null)
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            }
            else if (exclude)
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => !extensions.Contains(GetFileExtension(f))).ToArray();
            }
            else
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Contains(GetFileExtension(f))).ToArray();
            }
        }
        
        public static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }

                if (!File.Exists(filePath))
                {
                    return true;
                }
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
                return false;
            }
        }
        
        public static void ExplorerFolder(string folder)
        {
            folder = string.Format("\"{0}\"", folder);
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    System.Diagnostics.Process.Start("Explorer.exe", folder.Replace('/', '\\'));
                    break;
                case RuntimePlatform.OSXEditor:
                    System.Diagnostics.Process.Start("open", folder);
                    break;
                default:
                    Debug.LogError(string.Format("Not support open folder on '{0}' platform.", Application.platform.ToString()));
                    break;
            }
        }
        
        public static string GetMD5(string filepath)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5ch;
            using (FileStream fs = File.OpenRead(filepath))
            {
                md5ch = md5.ComputeHash(fs);
            }

            md5.Clear();
            string strMd5 = "";
            for (int i = 0; i < md5ch.Length - 1; i++)
            {
                strMd5 += md5ch[i].ToString("x").PadLeft(2, '0');
            }

            return strMd5;
        }
    }
}