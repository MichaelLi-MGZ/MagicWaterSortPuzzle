using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using MiniJSON;

namespace MyGamez.Demo
{
	public class IOSLoginController
	{
		private readonly MonoBehaviour host;
		private readonly Action onSessionReady; // callback to init SDK once session is valid

		private IAppleAuthManager appleAuthManager;
		private string appleUserId = "";
		private string playerId = "";
		private string appleIdToken = "";

		// User status configuration now handled by UserStatusSync

		/// <summary>
		/// Configure which user_status keys to read and optional mapping to PlayerPrefs keys.
		/// Delegates to UserStatusSync.ConfigureKeys.
		/// </summary>
		public static void ConfigureUserStatusKeys(IEnumerable<string> intKeys, IDictionary<string, string> keyRenameMap = null)
		{
			UserStatusSync.ConfigureKeys(intKeys, keyRenameMap);
		}

		public IOSLoginController(MonoBehaviour host, string authServerBaseUrl, Action onSessionReady)
		{
			this.host = host;
			this.onSessionReady = onSessionReady;
			ServerConfig.SetBaseUrl(authServerBaseUrl);

			Debug.Log("[IOSLoginController] Ctor: host=" + (host != null) + ", authServerBaseUrl=" + ServerConfig.BaseUrl);

			InitializeAppleSignIn();
		}

		public void Update()
		{
			appleAuthManager?.Update();
		}

		public void BeginLoginFlow()
		{
			Debug.Log("[IOSLoginController] BeginLoginFlow: starting session check or Apple Sign-In");
			host.StartCoroutine(CheckSessionAndProceed());
		}

		/// <summary>
		/// Request a JWT from the backend using current player id. Calls onDone with the token or null on failure.
		/// </summary>
		public void RequestJwtToken(string env, string appName, Action<string> onDone)
		{
			if (string.IsNullOrEmpty(playerId))
			{
				Debug.LogWarning("[IOSLoginController] RequestJwtToken called but playerId is empty");
			}
			host.StartCoroutine(RequestJwtCoroutine(env, appName, onDone));
		}

		private IEnumerator RequestJwtCoroutine(string env, string appName, Action<string> onDone)
		{

			
			long iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var payload = new Dictionary<string, object>
			{
				{"player_id", playerId},
				{"iat", iat},
				{"app", appName}
			};
			var json = MiniJSON.Json.Serialize(payload);
			var url = ServerConfig.BaseUrl.TrimEnd('/') + "/api/apple/jwt/sign?env=" + (string.IsNullOrEmpty(env) ? "dev" : env);
			Debug.Log("[IOSLoginController] Requesting JWT: url=" + url + ", payloadLen=" + (json == null ? 0 : json.Length));
			Debug.Log("[IOSLoginController] Requesting JWT: url=" + url + ", payload=" + (json == null ? 0 : json));
			yield return PostJson(url, json, (ok, respJson) =>
			{
				if (!ok || string.IsNullOrEmpty(respJson))
				{
					Debug.LogError("[IOSLoginController] JWT sign request failed");
					onDone?.Invoke(null);
					return;
				}
				try
				{
					var root = MiniJSON.Json.Deserialize(respJson) as System.Collections.IDictionary;
					if (root == null || !root.Contains("code") || Convert.ToInt32(root["code"]) != 0)
					{
						Debug.LogError("[IOSLoginController] JWT sign returned error: " + respJson);
						onDone?.Invoke(null);
						return;
					}
					var data = root["data"] as System.Collections.IDictionary;
					string token = data != null && data.Contains("token") ? (string)data["token"] : null;
					if (string.IsNullOrEmpty(token))
					{
						Debug.LogError("[IOSLoginController] JWT token missing in response");
						onDone?.Invoke(null);
						return;
					}
					Debug.Log("[IOSLoginController] JWT received, len=" + token.Length);
					onDone?.Invoke(token);
				}
				catch (Exception e)
				{
					Debug.LogError("[IOSLoginController] Parse JWT sign response failed: " + e);
					onDone?.Invoke(null);
				}
			});
		}

		private void InitializeAppleSignIn()
		{
			if (AppleAuthManager.IsCurrentPlatformSupported)
			{
				var deserializer = new PayloadDeserializer();
				appleAuthManager = new AppleAuthManager(deserializer);
				Debug.Log("[IOSLoginController] Apple Sign-In initialized");
			}
			else
			{
				Debug.LogError("[IOSLoginController] Apple Sign-In not supported on this platform");
			}
		}

		private IEnumerator CheckSessionAndProceed()
		{
			string sessionToken = PlayerPrefs.GetString("session_token", string.Empty);
			Debug.Log("[IOSLoginController] CheckSessionAndProceed: hasToken=" + (!string.IsNullOrEmpty(sessionToken)));
			if (string.IsNullOrEmpty(sessionToken))
			{
				Debug.Log("[IOSLoginController] No session token found, showing Apple Sign-In dialog");
				ShowAppleSignInDialog();
				yield break;
			}

			var url = ServerConfig.BaseUrl + "/api/session/check";
			var payload = "{\"session_token\":\"" + sessionToken + "\"}";
			Debug.Log("[IOSLoginController] Session check POST: url=" + url + ", payloadLen=" + payload.Length);
			yield return PostJson(url, payload, (ok, respJson) =>
			{
				Debug.Log("[IOSLoginController] Session check response: ok=" + ok + ", respLen=" + (respJson == null ? 0 : respJson.Length));
				if (!ok)
				{
					Debug.LogWarning("[IOSLoginController] Session check failed at transport level, fallback to Apple Sign-In");
					ShowAppleSignInDialog();
					return;
				}
				try
				{
					var resp = MiniJSON.Json.Deserialize(respJson) as System.Collections.IDictionary;
					if (resp != null && resp.Contains("code") && Convert.ToInt32(resp["code"]) == 0)
					{
						var data = resp["data"] as System.Collections.IDictionary;
						var newToken = data["session_token"] as string;
                        playerId = data["player_id"] as string ?? "";
                        Debug.Log("[IOSLoginController] Session check ok, player_id=" + playerId);
						if (!string.IsNullOrEmpty(newToken))
						{
							Debug.Log("[IOSLoginController] Session check ok, refreshing session_token (len=" + newToken.Length + ")");
							PlayerPrefs.SetString("session_token", newToken);
							PlayerPrefs.Save();
						}
						Debug.Log("[IOSLoginController] Session valid, loading user status and invoking onSessionReady");
						loadUserStatusFromResponse(data);
						onSessionReady?.Invoke();
					}
					else
					{
						var codeVal = (resp != null && resp.Contains("code")) ? resp["code"].ToString() : "<null>";
						Debug.LogWarning("[IOSLoginController] Session check returned error code=" + codeVal + ", showing Apple Sign-In");
						ShowAppleSignInDialog();
					}
				}
				catch (Exception e)
				{
					Debug.LogError("[IOSLoginController] Parse session check failed: " + e);
					ShowAppleSignInDialog();
				}
			});
		}

		private void ShowAppleSignInDialog()
		{
			if (appleAuthManager == null)
			{
				Debug.LogError("[IOSLoginController] AppleAuthManager not initialized");
				return;
			}

			Debug.Log("[IOSLoginController] Showing Apple Sign-In dialog");
			var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
			Debug.Log("[IOSLoginController] AppleAuth login args prepared: IncludeEmail|IncludeFullName");
			appleAuthManager.LoginWithAppleId(
				loginArgs,
				credential =>
				{
					if (credential is IAppleIDCredential appleIdCredential)
					{
						appleUserId = appleIdCredential.User;
						appleIdToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
						Debug.Log("[IOSLoginController] Apple Sign-In success: appleUserIdLen=" + (appleUserId == null ? 0 : appleUserId.Length) + ", tokenLen=" + (appleIdToken == null ? 0 : appleIdToken.Length));
						host.StartCoroutine(ExchangeAppleForSessionAndProceed(appleUserId, appleIdToken));
					}
				},
				error =>
				{
					Debug.LogError("[IOSLoginController] Apple Sign-In failed: " + error);
					// retry by showing the dialog again to keep UX consistent
					ShowAppleSignInDialog();
				});
		}

		private IEnumerator ExchangeAppleForSessionAndProceed(string appleUserId, string idToken)
		{
			Debug.Log("[IOSLoginController] ExchangeAppleForSessionAndProceed: appleUserIdLen=" + (string.IsNullOrEmpty(appleUserId) ? 0 : appleUserId.Length) + ", tokenLen=" + (string.IsNullOrEmpty(idToken) ? 0 : idToken.Length));
			var url = ServerConfig.BaseUrl + "/api/apple/signin";
			var body = new Dictionary<string, object>
			{
				{"appleUserId", appleUserId},
				{"identityToken", idToken},
				{"device", "ios"}
			};
			var json = MiniJSON.Json.Serialize(body);
			Debug.Log("[IOSLoginController] Apple signin exchange POST: url=" + url + ", jsonLen=" + (json == null ? 0 : json.Length));
			yield return PostJson(url, json, (ok, respJson) =>
			{
				Debug.Log("[IOSLoginController] Apple signin exchange response: ok=" + ok + ", respLen=" + (respJson == null ? 0 : respJson.Length));
				if (!ok)
				{
					Debug.LogError("[IOSLoginController] Apple signin exchange failed");
					ShowAppleSignInDialog();
					return;
				}
				try
				{
					var resp = MiniJSON.Json.Deserialize(respJson) as System.Collections.IDictionary;
					if (resp != null && resp.Contains("code") && Convert.ToInt32(resp["code"]) == 0)
					{
						var data = resp["data"] as System.Collections.IDictionary;
						var token = data["session_token"] as string;
						playerId = data["player_id"] as string ?? "";
						Debug.Log("[IOSLoginController] Apple signin exchange ok, player_id=" + playerId);
						if (!string.IsNullOrEmpty(token))
						{
							Debug.Log("[IOSLoginController] Apple signin exchange ok, storing session_token (len=" + token.Length + ")");
							PlayerPrefs.SetString("session_token", token);
							PlayerPrefs.Save();
						}
						Debug.Log("[IOSLoginController] Apple signin success, loading user status and invoking onSessionReady");
						loadUserStatusFromResponse(data);
						onSessionReady?.Invoke();
					}
					else
					{
						Debug.LogError("[IOSLoginController] Apple signin exchange returned error");
						ShowAppleSignInDialog();
					}
				}
				catch (Exception e)
				{
					Debug.LogError("[IOSLoginController] Parse apple signin response failed: " + e);
					ShowAppleSignInDialog();
				}
			});
		}

		private void loadUserStatusFromResponse(System.Collections.IDictionary data)
		{
			var userStatus = data["user_status"] as System.Collections.IDictionary;
			if (userStatus != null)
			{
				Debug.Log("[IOSLoginController] loadUserStatusFromResponse: keys=" + userStatus.Count);
				UserStatusSync.ApplyUserStatusToPlayerPrefs(userStatus);
			}
		}

		/// <summary>
		/// Public API wrapper to shared sync logic.
		/// </summary>
		public void SaveUserStatus()
		{
			UserStatusSync.SaveUserStatus(host);
		}

		private IEnumerator PostJson(string url, string json, Action<bool, string> onDone)
		{
			Debug.Log("[IOSLoginController] PostJson: url=" + url + ", bodyLen=" + (json == null ? 0 : json.Length));
			var request = new UnityWebRequest(url, "POST");
			byte[] bodyRaw = Encoding.UTF8.GetBytes(json ?? "{}");
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			yield return request.SendWebRequest();
			bool ok = request.result == UnityWebRequest.Result.Success;
			string resp = request.downloadHandler != null ? request.downloadHandler.text : "";
			Debug.Log("[IOSLoginController] PostJson done: ok=" + ok + ", responseCode=" + request.responseCode + ", error=\"" + request.error + "\"");
			onDone?.Invoke(ok, resp);
		}
	}
}


