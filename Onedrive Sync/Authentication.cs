using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Onedrive_Sync.JSon;

namespace Onedrive_Sync {
	class Authentication {
		static private string client_id = "01df8190-8a05-4709-920c-70c7d06f7e17";
		static private string redirect_uri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
		static private string scope = "files.readwrite+offline_access";
		static private string type = "code";

		private HttpClient httpClient;
		private string code;
		private string client_secret;

		public Authentication(string client_secret, string code) {
			var handler = new HttpClientHandler() {
				AllowAutoRedirect = true
			};

			this.client_secret = client_secret;
			this.code = code;
			httpClient = new HttpClient(handler);
		}

		public static string GetLoginUri() {
			return $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={client_id}&scope={scope}&response_type={type}&redirect_uri={redirect_uri}";
		}

		public async Task GetAccessToken() {
			var headers = new Dictionary<string, string>() {
				{ "client_id", client_id },
				{ "redirect_uri", redirect_uri },
				{ "client-secret", client_secret },
				{ "code", code },
				{ "grant_type", "authorization_code" }
			};

			var content = new FormUrlEncodedContent(headers);

			var result = await httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);
			var str_result = await result.Content.ReadAsStringAsync();
			var response = JsonSerializer.Deserialize<Response_OK>(str_result);
			
			if(response.access_token == null) {
				var error = JsonSerializer.Deserialize<Response_Error>(str_result);

				Console.WriteLine(error.error_description);
			}
		}
	}
}
