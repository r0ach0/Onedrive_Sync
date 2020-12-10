using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Onedrive_Sync.JSon;
using File = System.IO.File;

namespace Onedrive_Sync {
	class Program {
		static void Main(string[] args) {
			var task = Do();
			task.Wait();

		}

		static async Task Do() {
			Console.WriteLine("1. I need url for code");
			Console.WriteLine("2. I have code");
			Console.Write("What: ");

			switch (Console.ReadLine()) {
				case "1":
					Console.WriteLine(MsGraphAPI.GetLoginUri());
					break;
				case "2":
					Console.Write("Token: ");
					var code = Console.ReadLine();
					var api = new MsGraphAPI("t6.VS_8k96r3ts59Y8.Mpz4PiE-81iTWio", code);
					await api.SetAccessToken();

					if (SyncSettingHelper.Setting == null) {
						await SyncSettingHelper.InitializeSetting(api);
					}

					Console.WriteLine("1. Upload all");
					Console.WriteLine("2. Download all");
					Console.Write("choice: ");

					switch (Console.ReadLine()) {
						case "1":
							Console.WriteLine("Wait! Uploading...");

							var files = Directory.GetFiles(SyncSettingHelper.Setting.local_dir);
							foreach (var f in files)
								await api.UploadAsync(SyncSettingHelper.Setting.remote_dir, f);

							break;
						case "2":
							Console.WriteLine("Wait! Downloading..");
							var items = await api.Request<Listitems>($"https://graph.microsoft.com/v1.0/me/drive/root:{SyncSettingHelper.Setting.remote_dir}:/children");

							foreach (var item in items.value) {
								if (item.file != null) {
									await api.DownloadAsync($"https://graph.microsoft.com/v1.0/me/drive/root:{SyncSettingHelper.Setting.remote_dir + "/" + item.name}:/content", SyncSettingHelper.Setting.local_dir + "/" + item.name);
								}
							}

							break;
					}

					break;
			}
		}
	}
}
