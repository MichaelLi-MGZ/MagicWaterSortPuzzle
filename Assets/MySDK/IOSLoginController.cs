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
		private readonly string authServerBaseUrl;
		private readonly Action onSessionReady; // callback to init SDK once session is valid

		private IAppleAuthManager appleAuthManager;
		private string appleUserId = "";
		private string appleIdToken = "";

		public string AppleUserId { get { return appleUserId; } }
		public string AppleIdToken { get { return appleIdToken; } }

		public IOSLoginController(MonoBehaviour host, string authServerBaseUrl, Action onSessionReady)
		{
			this.host = host;
			this.authServerBaseUrl = authServerBaseUrl.TrimEnd('/');
			this.onSessionReady = onSessionReady;

			Debug.Log("[IOSLoginController] Ctor: host=" + (host != null) + ", authServerBaseUrl=" + this.authServerBaseUrl);

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

			var url = authServerBaseUrl + "/api/session/check";
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
                        appleUserId = data["player_id"] as string ?? "";
                        Debug.Log("[IOSLoginController] Session check ok, player_id=" + appleUserId);
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
						Debug.Log("[IOSLoginController] Apple Sign-In success: userIdLen=" + (appleUserId == null ? 0 : appleUserId.Length) + ", tokenLen=" + (appleIdToken == null ? 0 : appleIdToken.Length));
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

		private IEnumerator ExchangeAppleForSessionAndProceed(string userId, string idToken)
		{
			Debug.Log("[IOSLoginController] ExchangeAppleForSessionAndProceed: userIdLen=" + (string.IsNullOrEmpty(userId) ? 0 : userId.Length) + ", tokenLen=" + (string.IsNullOrEmpty(idToken) ? 0 : idToken.Length));
			var url = authServerBaseUrl + "/api/apple/signin";
			var body = new Dictionary<string, object>
			{
				{"appleUserId", userId},
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
                if (userStatus.Contains("UseBottles"))
                {
                    int useBottles = System.Convert.ToInt32(userStatus["UseBottles"]);
                    PlayerPrefs.SetInt("UseBottles", useBottles);
                }

                if (userStatus.Contains("AdsEnabled"))
                {
                    int adsEnabled = System.Convert.ToInt32(userStatus["AdsEnabled"]);
                    PlayerPrefs.SetInt("AdsEnabled", adsEnabled);
                }

                if (userStatus.Contains("ShowAds"))
                {
                    int showAds = System.Convert.ToInt32(userStatus["ShowAds"]);
                    PlayerPrefs.SetInt("ShowAds", showAds);
                }

                if (userStatus.Contains("removeAds"))
                {
                    int removeAds = System.Convert.ToInt32(userStatus["removeAds"]);
                    PlayerPrefs.SetInt("removeAds", removeAds);
                }

                if (userStatus.Contains("CurrentLevel"))
                {
                    int currentLevel = System.Convert.ToInt32(userStatus["CurrentLevel"]);
                    PlayerPrefs.SetInt("CurrentLevel", currentLevel);
                }

                if (userStatus.Contains("Coin"))
                {
                    int coin = System.Convert.ToInt32(userStatus["Coin"]);
                    PlayerPrefs.SetInt("Coin", coin);
                }

                if (userStatus.Contains("Start"))
                {
                    int start = System.Convert.ToInt32(userStatus["Start"]);
                    PlayerPrefs.SetInt("Start", start);
                }

                if (userStatus.Contains("Undo"))
                {
                    int undo = System.Convert.ToInt32(userStatus["Undo"]);
                    PlayerPrefs.SetInt("Undo", undo);
                }
                
                if (userStatus.Contains("RestartNumber"))
                {
                    int restartNumber = System.Convert.ToInt32(userStatus["RestartNumber"]);
                    PlayerPrefs.SetInt("RestartNumber", restartNumber);
                }

                if (userStatus.Contains("Bottle0"))
                {
                    int bottle0 = System.Convert.ToInt32(userStatus["Bottle0"]);
                    PlayerPrefs.SetInt("Bottle0", bottle0);
                }
                
                if (userStatus.Contains("Bottle1"))
                {
                    int bottle1 = System.Convert.ToInt32(userStatus["Bottle1"]);
                    PlayerPrefs.SetInt("Bottle1", bottle1);
                }
                

                if (userStatus.Contains("Bottle2"))
                {
                    int bottle2 = System.Convert.ToInt32(userStatus["Bottle2"]);
                    PlayerPrefs.SetInt("Bottle2", bottle2);
                }
                

                if (userStatus.Contains("Bottle3"))
                {
                    int bottle3 = System.Convert.ToInt32(userStatus["Bottle3"]);
                    PlayerPrefs.SetInt("Bottle3", bottle3);
                }
                

                if (userStatus.Contains("Bottle4"))
                {
                    int bottle4 = System.Convert.ToInt32(userStatus["Bottle4"]);
                    PlayerPrefs.SetInt("Bottle4", bottle4);
                }

                if (userStatus.Contains("Bottle5"))
                {
                    int bottle5 = System.Convert.ToInt32(userStatus["Bottle5"]);
                    PlayerPrefs.SetInt("Bottle5", bottle5);
                }
                
                if (userStatus.Contains("Wall0"))
                {
                    int wall0 = System.Convert.ToInt32(userStatus["Wall0"]);
                    PlayerPrefs.SetInt("Wall0", wall0);
                }
                
                if (userStatus.Contains("Wall1"))
                {
                    int wall1 = System.Convert.ToInt32(userStatus["Wall1"]);
                    PlayerPrefs.SetInt("Wall1", wall1);
                }
                
                if (userStatus.Contains("Wall2"))
                {
                    int wall2 = System.Convert.ToInt32(userStatus["Wall2"]);
                    PlayerPrefs.SetInt("Wall2", wall2);
                }
                
                if (userStatus.Contains("Wall3"))
                {
                    int wall3 = System.Convert.ToInt32(userStatus["Wall3"]);
                    PlayerPrefs.SetInt("Wall3", wall3);
                }
                
                if (userStatus.Contains("Wall4"))
                {
                    int wall4 = System.Convert.ToInt32(userStatus["Wall4"]);
                    PlayerPrefs.SetInt("Wall4", wall4);
                }
                
                if (userStatus.Contains("Wall5"))
                {
                    int wall5 = System.Convert.ToInt32(userStatus["Wall5"]);
                    PlayerPrefs.SetInt("Wall5", wall5);
                }
                
                if (userStatus.Contains("Palette0"))
                {
                    int palette0 = System.Convert.ToInt32(userStatus["Palette0"]);
                    PlayerPrefs.SetInt("Palette0", palette0);
                }
                
                if (userStatus.Contains("Palette1"))
                {
                    int palette1 = System.Convert.ToInt32(userStatus["Palette1"]);
                    PlayerPrefs.SetInt("Palette1", palette1);
                }
                
                if (userStatus.Contains("Palette2"))
                {
                    int palette2 = System.Convert.ToInt32(userStatus["Palette2"]);
                    PlayerPrefs.SetInt("Palette2", palette2);
                }

                if (userStatus.Contains("Palette3"))
                {
                    int palette3 = System.Convert.ToInt32(userStatus["Palette3"]);
                    PlayerPrefs.SetInt("Palette3", palette3);
                }
                
                if (userStatus.Contains("Palette4"))
                {
                    int palette4 = System.Convert.ToInt32(userStatus["Palette4"]);
                    PlayerPrefs.SetInt("Palette4", palette4);
                }
                
                if (userStatus.Contains("Palette5"))
                {
                    int palette5 = System.Convert.ToInt32(userStatus["Palette5"]);
                    PlayerPrefs.SetInt("Palette5", palette5);
                }
                
                if (userStatus.Contains("CurrentBottle"))
                {
                    int currentBottle = System.Convert.ToInt32(userStatus["CurrentBottle"]);
                    PlayerPrefs.SetInt("CurrentBottle", currentBottle);
                }
                
                if (userStatus.Contains("CurrentWall"))
                {
                    int currentWall = System.Convert.ToInt32(userStatus["CurrentWall"]);
                    PlayerPrefs.SetInt("CurrentWall", currentWall);
                }
                
                if (userStatus.Contains("CurrentPalette"))
                {
                    int currentPalette = System.Convert.ToInt32(userStatus["CurrentPalette"]);
                    PlayerPrefs.SetInt("CurrentPalette", currentPalette);
                }
                
                if (userStatus.Contains("Music"))
                {
                    int music = System.Convert.ToInt32(userStatus["Music"]);
                    PlayerPrefs.SetInt("Music", music);
                }
                
                if (userStatus.Contains("Haptic"))
                {
                    int haptic = System.Convert.ToInt32(userStatus["Haptic"]);
                    PlayerPrefs.SetInt("Haptic", haptic);
                }

                PlayerPrefs.Save();
            }
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


