using System;
using System.Threading.Tasks;

namespace Onedrive_Sync {
	class Program {
		static void Main(string[] args) {
			var task = Do();
			task.Wait();
		}

		static async Task Do() {
			var auth = new Authentication("", "");

			Authentication.GetLoginUri();
			await auth.GetAccessToken();
		}
	}
}
