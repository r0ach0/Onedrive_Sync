using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Onedrive_Sync.JSon;
using File = System.IO.File;

namespace Onedrive_Sync {
	class MsGraphAPI {
		static private string client_id = "01df8190-8a05-4709-920c-70c7d06f7e17";
		static private string redirect_uri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
		static private string scope = "files.readwrite.all+offline_access";
		static private string type = "code";

		private HttpClient httpClient;
		private string code;
		private string client_secret;
		private Response_OK token;

		public MsGraphAPI(string client_secret, string code) {
			this.client_secret = client_secret;
			this.code = code;

			httpClient = new HttpClient();
		}

		public MsGraphAPI(string token) {
			this.token = new Response_OK() {
				access_token = token
			};

			httpClient = new HttpClient();
		}

		public static string GetLoginUri() {
			return $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={client_id}&scope={scope}&response_type={type}&redirect_uri={redirect_uri}";
		}

		public async Task<Response_OK> SetAccessToken() {
			var headers = new Dictionary<string, string>() {
				{ "client_id", client_id },
				{ "redirect_uri", redirect_uri },
				{ "client-secret", client_secret },
				{ "code", code },
				{ "grant_type", "authorization_code" }
			};

			var content = new FormUrlEncodedContent(headers);

			var result = await httpClient.PostAsync("https://login.live.com/oauth20_token.srf", content);
			var str_result = await result.Content.ReadAsStringAsync();
			var response = JsonSerializer.Deserialize<Response_OK>(str_result);

			if (response.access_token == null) {
				var error = JsonSerializer.Deserialize<Response_Error>(str_result);
				Console.WriteLine(error.error_description);
				return null;
			} else {
				token = response;
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
				return response;
			}
		}

		public async Task<T> Request<T>(string url) {
			return JsonSerializer.Deserialize<T>(await RequestString(url));
		}

		public async Task<String> RequestString(string url) {
			using (var req = new HttpRequestMessage(HttpMethod.Get, url)) {
				var result = await httpClient.SendAsync(req);
				var str_result = await result.Content.ReadAsStringAsync();
				return str_result;
			}
		}

		public async Task UploadAsync(string remotePath, string filePath) {
			filePath = filePath.Replace("\\", "/");
			using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
			var response = await httpClient.PutAsync($"https://graph.microsoft.com/v1.0/me/drive/root:{remotePath}/{filePath}:/content", fileContent);
			var responseContent = await response.Content.ReadAsStringAsync();
		}

		public async Task DownloadAsync(string requestUri, string filename) {
			using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri)) {
				var result = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
				using (Stream contentStream = result, stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 1024, true)) {
					await contentStream.CopyToAsync(stream);
				}
			}
		}

		public void SignOut() {
			httpClient.GetAsync($"https://login.live.com/oauth20_logout.srf?client_id={client_id}&redirect_uri={redirect_uri}");
		}
	}
}
