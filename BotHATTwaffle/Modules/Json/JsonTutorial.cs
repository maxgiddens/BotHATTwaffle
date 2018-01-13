﻿using Newtonsoft.Json;

namespace BotHATTwaffle.Objects.Json
{
	public class Tutorial
	{
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("tags")]
		public string[] Tags { get; set; } // List<string> tags { get; set; }
	}
}
