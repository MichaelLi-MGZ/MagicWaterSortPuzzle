using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;

namespace MyGamez.Demo
{
    /// <summary>
    /// Android helper for loading user status and applying it to PlayerPrefs.
    /// Mirrors IOSLoginController user-status handling.
    /// </summary>
    public class AndroidLoginController
    {
        private readonly MonoBehaviour host;
        private readonly string serverBaseUrl;

        /// <summary>
        /// Configure which user_status keys to read and optional mapping to PlayerPrefs keys.
        /// Delegates to UserStatusSync.ConfigureKeys.
        /// </summary>
        public static void ConfigureUserStatusKeys(IEnumerable<string> intKeys, IDictionary<string, string> keyRenameMap = null)
        {
            UserStatusSync.ConfigureKeys(intKeys, keyRenameMap);
        }

        public AndroidLoginController(MonoBehaviour host, string serverBaseUrl)
        {
            this.host = host;
            this.serverBaseUrl = string.IsNullOrEmpty(serverBaseUrl) ? ServerConfig.BaseUrl : serverBaseUrl.TrimEnd('/');
            ServerConfig.SetBaseUrl(this.serverBaseUrl);
            Debug.Log("[AndroidLoginController] Ctor: host=" + (host != null) + ", baseUrl=" + ServerConfig.BaseUrl);
        }

        /// <summary>
        /// Load user status for playerId, apply to PlayerPrefs, then invoke onDone (always).
        /// </summary>
        public void LoadUserStatus(string playerId, Action onDone)
        {
            if (host == null)
            {
                Debug.LogError("[AndroidLoginController] LoadUserStatus requires a host MonoBehaviour");
                onDone?.Invoke();
                return;
            }
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogWarning("[AndroidLoginController] LoadUserStatus skipped: empty playerId");
                onDone?.Invoke();
                return;
            }
            host.StartCoroutine(LoadUserStatusCoroutine(playerId, onDone));
        }

        private IEnumerator LoadUserStatusCoroutine(string playerId, Action onDone)
        {
            string url = serverBaseUrl.TrimEnd('/') + "/api/user_status/get";
            var payload = new Dictionary<string, object>
            {
                { "player_id", playerId },
                { "vendor", "android" }
            };
            string json = MiniJSON.Json.Serialize(payload);

            Debug.Log("[AndroidLoginController] Loading user status: url=" + url + ", playerId=" + playerId);
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool ok = request.result == UnityWebRequest.Result.Success;
            string resp = request.downloadHandler != null ? request.downloadHandler.text : "";
            Debug.Log("[AndroidLoginController] User status load response: ok=" + ok + ", respLen=" + (resp == null ? 0 : resp.Length) + ", responseCode=" + request.responseCode);

            if (ok)
            {
                try
                {
                    var respObj = MiniJSON.Json.Deserialize(resp) as System.Collections.IDictionary;
                    if (respObj != null && respObj.Contains("code") && Convert.ToInt32(respObj["code"]) == 0)
                    {
                        var data = respObj["data"] as System.Collections.IDictionary;
                        var sessionToken = data != null ? data["session_token"] as string : null;
                        var userStatus = data != null ? data["user_status"] as System.Collections.IDictionary : null;

                        if (!string.IsNullOrEmpty(sessionToken))
                        {
                            Debug.Log("[AndroidLoginController] Received session_token, storing to PlayerPrefs (len=" + sessionToken.Length + ")");
                            PlayerPrefs.SetString("session_token", sessionToken);
                            PlayerPrefs.Save();
                        }
                        else
                        {
                            Debug.LogWarning("[AndroidLoginController] session_token missing in response data");
                        }

                        if (userStatus != null)
                        {
                            Debug.Log("[AndroidLoginController] Applying user status from server");
                            UserStatusSync.ApplyUserStatusToPlayerPrefs(userStatus);
                        }
                        else
                        {
                            Debug.LogWarning("[AndroidLoginController] user_status missing in response data");
                        }
                    }
                    else
                    {
                        var codeVal = respObj != null && respObj.Contains("code") ? respObj["code"].ToString() : "<null>";
                        Debug.LogWarning("[AndroidLoginController] Failed to load user status, code=" + codeVal);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("[AndroidLoginController] Parse user status response failed: " + e);
                }
            }
            else
            {
                Debug.LogWarning("[AndroidLoginController] User status request failed: error=\"" + request.error + "\"");
            }

            onDone?.Invoke();
        }
    }
}

