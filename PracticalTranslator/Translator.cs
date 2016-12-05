
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using System.Collections.Generic;


namespace PracticalTranslator
{
	class Translator
	{
		private const string ClientId = "TranslationTwitterSample";
		private const string ClientSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
		private string _token;
		String fm;
		String to;

		public Translator()
		{
			SetLocation(Location.Japanese);
		}

		public void SetLocation(Location location)
		{
			to = (location == Location.English) ? "en" : "ja";
			fm = (location == Location.English) ? "ja" : "en";
		}

		async Task<bool> InitializeToken()
		{
			if (_token == null)
			{
				var client = new HttpClient();
				const string url = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
				var content = new FormUrlEncodedContent(new Dictionary<string, string> {
				{"client_id", ClientId},
				{"client_secret", ClientSecret},
				{"scope", "http://api.microsofttranslator.com"},
				{"grant_type", "client_credentials"},
			});
				var response = await client.PostAsync(url, content);
				var adm = JsonConvert.DeserializeObject<AdmAccessToken>(await response.Content.ReadAsStringAsync());
				_token = adm.access_token;
				return true;
			}
			return false;
		}

		public class AdmAccessToken
		{
			public string access_token { get; set; }
		}

		public async Task<string> Conversion(string str)
		{
			if (_token == null)
			{
				await InitializeToken();
			}

			if (str == "")
			{
				await Task.Delay(1000);
				return "";
			}

			var client = new HttpClient();
			var url = string.Format("https://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&from={1}&to={2}", WebUtility.UrlEncode(str),fm,to);
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
			var response = await client.GetStringAsync(url);
			var doc = XDocument.Parse(response);
			return doc.Root.FirstNode.ToString();
		}
	}
}