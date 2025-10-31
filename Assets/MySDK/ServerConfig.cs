using UnityEngine;

namespace MyGamez.Demo
{
	public static class ServerConfig
	{
		private static string baseUrl = "https://weixin.mygamez.cn";

		public static void SetBaseUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.LogWarning("[ServerConfig] Empty base URL provided; keeping existing value");
				return;
			}
			baseUrl = url.TrimEnd('/');
		}

		public static string BaseUrl
		{
			get { return baseUrl; }
		}
	}
}


