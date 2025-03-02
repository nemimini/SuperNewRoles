﻿using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    [HarmonyPatch]
    public class CustomVisors
    {

        public class CustomVisor
        {
            public string author { get; set; }
            public string name { get; set; }
            public string resource { get; set; }
            public string reshasha { get; set; }
        }
    }
    public static class DownLoadClassVisor
    {
        public static bool IsEndDownload = false;
        public static List<string> fetchs = new List<string>();
        public static List<CustomVisors.CustomVisor> Visordetails = new List<CustomVisors.CustomVisor>();
        public static void Load()
        {
            IsEndDownload = false;
            Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\");
            Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\");
            SuperNewRolesPlugin.Logger.LogInfo("バイザーダウンロード開始");
            FetchHats("https://raw.githubusercontent.com/ykundesu/SuperNewNamePlates/main");
        }
        private static string sanitizeResourcePath(string res)
        {
            if (res == null || !res.EndsWith(".png"))
                return null;

            res = res.Replace("\\", "")
                     .Replace("/", "")
                     .Replace("*", "")
                     .Replace("..", "");
            return res;
        }
        private static bool doesResourceRequireDownload(string respath, string reshash, MD5 md5)
        {
            if (reshash == null || !File.Exists(respath))
                return true;

            using (var stream = File.OpenRead(respath))
            {
                var hash = System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                return !reshash.Equals(hash);
            }
        }
        public static async Task<HttpStatusCode> FetchHats(string repo)
        {
            fetchs.Add(repo);
            SuperNewRolesPlugin.Logger.LogInfo("バイザーダウンロード開始:"+repo);
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var response = await http.GetAsync(new System.Uri($"{repo}/CustomVisors.json"), HttpCompletionOption.ResponseContentRead);
            try
            {
                SuperNewRolesPlugin.Logger.LogInfo("StatusCode:"+ response.StatusCode);
                if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
                if (response.Content == null)
                {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                    return HttpStatusCode.ExpectationFailed;
                }
                string json = await response.Content.ReadAsStringAsync();
                JToken jobj = JObject.Parse(json)["Visors"];
                if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

                List<CustomVisors.CustomVisor> Visordatas = new List<CustomVisors.CustomVisor>();

                for (JToken current = jobj.First; current != null; current = current.Next)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("1");
                    if (current.HasValues)
                    {
                        CustomVisors.CustomVisor info = new CustomVisors.CustomVisor();

                        info.name = current["name"]?.ToString();
                        info.resource = sanitizeResourcePath(current["resource"]?.ToString());
                        if (info.resource == null || info.name == null) // required
                            continue;
                        info.author = current["author"]?.ToString();
                        info.reshasha = current["name"]?.ToString();
                        Visordatas.Add(info);
                    }
                }

                List<string> markedfordownload = new List<string>();

                string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\";
                MD5 md5 = MD5.Create();
                foreach (CustomVisors.CustomVisor data in Visordatas)
                {
                    if (doesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                        markedfordownload.Add(data.resource);
                }

                foreach (var file in markedfordownload)
                {

                    var hatFileResponse = await http.GetAsync($"{repo}/Visors/{file}", HttpCompletionOption.ResponseContentRead);
                    if (hatFileResponse.StatusCode != HttpStatusCode.OK) continue;
                    using (var responseStream = await hatFileResponse.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = File.Create($"{filePath}\\{file}"))
                        {
                            responseStream.CopyTo(fileStream);
                        }
                    }
                }

                Visordetails.AddRange(Visordatas);
            }
            catch (System.Exception ex)
            {
                SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }
            SuperNewRolesPlugin.Logger.LogInfo("バイザーダウンロード終了:"+repo);
            fetchs.Remove(repo);
            if (fetchs.Count <= 0)
            {
                IsEndDownload = true;
            }
            return HttpStatusCode.OK;
        }
    }
}
