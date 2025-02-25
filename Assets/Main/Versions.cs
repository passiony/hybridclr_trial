﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class Versions
    {
        public const string HotUrl = "http://192.168.0.95/StandaloneWindows64/";
        public const string VersionName = "Versions.txt";

        public Dictionary<string, string> localVersion = new Dictionary<string, string>();
        public Dictionary<string, string> removeVersion = new Dictionary<string, string>();

        public List<string> UpdateList = new List<string>();

        public IEnumerator InitVersion()
        {
            string persisPath = AssetUtility.GetPersistentDataPath(VersionName);
            if (!File.Exists(persisPath))
            {
                string streamingPath = AssetUtility.GetStreamingAssetsFilePath(VersionName);
                UnityWebRequest request = UnityWebRequest.Get(streamingPath);
                request.timeout = 5;
                yield return request.SendWebRequest();

                if (request.isDone)
                {
                    var dir = AssetUtility.GetPersistentDataPath();
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(persisPath, request.downloadHandler.text.TrimEnd());
                }
            }
        }

        IEnumerator LoadLocalVersion()
        {
            string persisPath = AssetUtility.GetPersistentDataPath(VersionName);
            string[] content = File.ReadAllLines(persisPath);
            foreach (var line in content)
            {
                string[] arr = line.Split(',');
                localVersion.Add(arr[0], arr[1]);
            }

            yield return null;
        }

        IEnumerator DownloadVersion()
        {
            string fromPath = HotUrl + VersionName;
            UnityWebRequest request = UnityWebRequest.Get(fromPath);
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.isDone)
            {
                string[] content = request.downloadHandler.text.TrimEnd().Split('\n');
                foreach (var line in content)
                {
                    string[] arr = line.TrimEnd('\r').Split(',');
                    removeVersion.Add(arr[0], arr[1]);
                }
            }
        }

        IEnumerator LoadVersion()
        {
            yield return InitVersion();
            yield return LoadLocalVersion();
            yield return DownloadVersion();
        }

        void CompareVersion()
        {
            UpdateList.Clear();
            foreach (var remote in removeVersion)
            {
                if (!localVersion.ContainsKey(remote.Key) || localVersion[remote.Key] != remote.Value)
                {
                    UpdateList.Add(remote.Key);
                }
            }
        }

        public IEnumerator Update()
        {
            yield return LoadVersion();
            CompareVersion();
            foreach (var file in UpdateList)
            {
                string fromPath = HotUrl + "/" + file;
                UnityWebRequest request = UnityWebRequest.Get(fromPath);
                request.timeout = 5;
                yield return request.SendWebRequest();

                if (request.isDone)
                {
                    Debug.Log("download file :" + file);
                    byte[] assetData = request.downloadHandler.data;
                    File.WriteAllBytes(AssetUtility.GetPersistentDataPath(file), assetData);
                }
            }
        }
    }
}