using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;

namespace MyGamez.Demo
{
	public static class UserStatusSync
	{
		private static readonly string[] DefaultUserStatusIntKeys = new[]
		{
			"UseBottles", "AdsEnabled", "ShowAds", "removeAds",
			"CurrentLevel", "Coin", "Start", "Undo", "RestartNumber",
			"Bottle0", "Bottle1", "Bottle2", "Bottle3", "Bottle4", "Bottle5",
			"Wall0", "Wall1", "Wall2", "Wall3", "Wall4", "Wall5",
			"Palette0", "Palette1", "Palette2", "Palette3", "Palette4", "Palette5",
			"CurrentBottle", "CurrentWall", "CurrentPalette",
			"Music", "Haptic"
		};

		private static string[] configuredUserStatusIntKeys;
		private static Dictionary<string, string> userStatusToPlayerPrefKeyMap;

		public static void ConfigureKeys(IEnumerable<string> intKeys, IDictionary<string, string> keyRenameMap = null)
		{
			configuredUserStatusIntKeys = intKeys != null ? new List<string>(intKeys).ToArray() : null;
			userStatusToPlayerPrefKeyMap = keyRenameMap != null ? new Dictionary<string, string>(keyRenameMap) : null;
		}

		public static void ApplyUserStatusToPlayerPrefs(System.Collections.IDictionary userStatus)
		{
			var keys = configuredUserStatusIntKeys ?? DefaultUserStatusIntKeys;
			for (int i = 0; i < keys.Length; i++)
			{
				string statusKey = keys[i];
				if (userStatus.Contains(statusKey))
				{
					int value = System.Convert.ToInt32(userStatus[statusKey]);
					string prefKey = (userStatusToPlayerPrefKeyMap != null && userStatusToPlayerPrefKeyMap.ContainsKey(statusKey))
						? userStatusToPlayerPrefKeyMap[statusKey]
						: statusKey;
					PlayerPrefs.SetInt(prefKey, value);
				}
			}
			PlayerPrefs.Save();
		}

		public static Dictionary<string, int> BuildUserStatusFromPlayerPrefs()
		{
			var result = new Dictionary<string, int>();
			var keys = configuredUserStatusIntKeys ?? DefaultUserStatusIntKeys;
			for (int i = 0; i < keys.Length; i++)
			{
				string statusKey = keys[i];
				string prefKey = (userStatusToPlayerPrefKeyMap != null && userStatusToPlayerPrefKeyMap.ContainsKey(statusKey))
					? userStatusToPlayerPrefKeyMap[statusKey]
					: statusKey;
				int value = PlayerPrefs.GetInt(prefKey, 0);
				result[statusKey] = value;
			}
			return result;
		}

		/// <summary>
		/// Save user status to server using configured ServerConfig.BaseUrl.
		/// </summary>
		public static void SaveUserStatus(MonoBehaviour host)
		{
			SaveUserStatus(host, ServerConfig.BaseUrl);
		}

		/// <summary>
		/// Save user status to server using specified server URL.
		/// </summary>
		public static void SaveUserStatus(MonoBehaviour host, string serverBaseUrl)
		{
			if (host == null)
			{
				Debug.LogError("[UserStatusSync] SaveUserStatus requires a host MonoBehaviour to run coroutine");
				return;
			}
			host.StartCoroutine(SaveUserStatusCoroutine(serverBaseUrl));
		}

		private static IEnumerator SaveUserStatusCoroutine(string serverBaseUrl)
		{
			string sessionToken = PlayerPrefs.GetString("session_token", string.Empty);
			if (string.IsNullOrEmpty(sessionToken))
			{
				Debug.LogWarning("[UserStatusSync] SaveUserStatus skipped: no session_token");
				yield break;
			}

			var userStatus = BuildUserStatusFromPlayerPrefs();
			var body = new Dictionary<string, object>
			{
				{"session_token", sessionToken},
				{"user_status", userStatus}
			};
			string json = MiniJSON.Json.Serialize(body);
			var url = serverBaseUrl.TrimEnd('/') + "/api/user_status/save";
			Debug.Log("[UserStatusSync] SaveUserStatus POST: url=" + url + ", jsonLen=" + (json == null ? 0 : json.Length));
			yield return PostJson(url, json, (ok, resp) =>
			{
				Debug.Log("[UserStatusSync] SaveUserStatus response: ok=" + ok + ", respLen=" + (resp == null ? 0 : resp.Length));
			});
		}

		private static IEnumerator PostJson(string url, string json, Action<bool, string> onDone)
		{
			var request = new UnityWebRequest(url, "POST");
			byte[] bodyRaw = Encoding.UTF8.GetBytes(json ?? "{}");
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			yield return request.SendWebRequest();
			bool ok = request.result == UnityWebRequest.Result.Success;
			string resp = request.downloadHandler != null ? request.downloadHandler.text : "";
			onDone?.Invoke(ok, resp);
		}
	}
}


