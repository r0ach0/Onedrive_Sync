using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Onedrive_Sync.JSon;
using File = System.IO.File;

namespace Onedrive_Sync {
	class SyncSettingHelper {
		private static SyncSetting setting = null;
		private const string SETTING_FILE = "setting.json";

		public static SyncSetting Setting {
			get {
				if (setting == null) {
					if (File.Exists(SETTING_FILE)) {
						var file = JsonSerializer.Deserialize<SyncSetting>(File.ReadAllText(SETTING_FILE));
						if(!string.IsNullOrEmpty(file.local_dir) && !string.IsNullOrEmpty(file.remote_dir)) {
							setting = file;
						}
					}
				}

				return setting;
			}
			set {
				setting = value;
			}
		}

		public static async Task InitializeSetting(MsGraphAPI auth) {
			setting = new SyncSetting();

			var items = await PrintSettingOption(auth, "https://graph.microsoft.com/v1.0/me/drive/root/children");

			string idx;
			var remote_dir = string.Empty;
			while ((idx = Console.ReadLine()) != "done") {
				remote_dir += "/" + items.value[Int32.Parse(idx)].name;
				items = await PrintSettingOption(auth, $"https://graph.microsoft.com/v1.0/me/drive/root:{remote_dir}:/children");
			}

			setting.remote_dir = remote_dir;


			var local_dir = string.Empty;
			do {
				Console.Clear();
				Console.WriteLine("Input local directory for sync.");
				Console.Write("Directory: ");
				local_dir = Console.ReadLine();
			} while (string.IsNullOrEmpty(local_dir) || !Directory.Exists(local_dir));

			setting.local_dir = local_dir;
			File.WriteAllText("setting.json", JsonSerializer.Serialize<SyncSetting>(setting));
		}

		private static async Task<Listitems> PrintSettingOption(MsGraphAPI auth, string uri) {
			Console.Clear();
			Console.WriteLine("Select remote directory for sync.");
			Console.WriteLine("Input 'done' for selection.\n");
			var items = await auth.Request<Listitems>(uri);

			for (int i = 0; i < items.value.Length; i++) {
				if (items.value[i].folder != null)
					Console.WriteLine($"{i}: {items.value[i].name}");
			}

			Console.Write("index: ");
			return items;
		}
	}
}
