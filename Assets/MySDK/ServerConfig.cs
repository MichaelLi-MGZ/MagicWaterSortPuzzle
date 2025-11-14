using UnityEngine;

namespace MyGamez.Demo
{
	public static class ServerConfig
	{
		private static string iOS_env = "sandbox";
		private static string mygamez_env = "dev";
		private static string baseUrl = "https://weixin.mygamez.cn";

		public static void SetiOSEnv(string iOS_env)
		{
			ServerConfig.iOS_env = iOS_env;
		}

		public static string iOSEnv
		{
			get { return iOS_env; }
		}

		public static void SetMygamezEnv(string mygamez_env)
		{
			ServerConfig.mygamez_env = mygamez_env;
		}

		public static string MygamezEnv
		{
			get { return mygamez_env; }
		}
		
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


