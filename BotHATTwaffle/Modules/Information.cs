﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord.WebSocket;

namespace BotHATTwaffle.Modules
{
	public class InformationService
	{
		public InformationService()
		{
			//Nothing happens here, yet.
		}
	}

	public class InformationModule : ModuleBase<SocketCommandContext>
	{
		private readonly DiscordSocketClient _client;
		private readonly DataServices _dataServices;
		private readonly Random _random;

		public InformationModule(DiscordSocketClient client, DataServices data, Random random)
		{
			_client = client;
			_dataServices = data;
			_random = random;
		}

		/// <summary>
		/// Searches the VDC for the provided term.
		/// Then replies with what it found.
		/// </summary>
		/// <param name="searchTerm">Search Term</param>
		/// <returns></returns>
		[Command("vdc")]
		[Summary("`>vdc [Search]` Quick link back to a VDC search")]
		[Remarks("Does a search on the VDC and gives you the link back. Try to use the proper full term, for instance: " +
			"`func_detail` will give you better results than `detail`")]
		[Alias("v")]
		public async Task SearchAsync([Remainder] string searchTerm)
		{
			await Context.Channel.TriggerTypingAsync();

			searchTerm = searchTerm.Replace(' ','+');
			string builtUrl = $"https://developer.valvesoftware.com/w/index.php?search={searchTerm}&title=Special%3ASearch&go=Go";

			//Download web page title and store to string
			WebClient x = new WebClient();
			string siteTitle = x.DownloadString(builtUrl);
			string regex = @"(?<=<title.*>)([\s\S]*)(?=</title>)";
			Regex ex = new Regex(regex, RegexOptions.IgnoreCase);
			siteTitle = ex.Match(siteTitle).Value.Trim();

			//If the URL isn't properly formatted, default it
			if (!Uri.IsWellFormedUriString(builtUrl, UriKind.Absolute))
			{
				builtUrl = "https://developer.valvesoftware.com/wiki/Main_Page";
				searchTerm = "Valve Developer Community";
			}

			var authBuilder = new EmbedAuthorBuilder()
			{
				Name = $"This is what I was able to find for {searchTerm}",
				IconUrl = "https://cdn.discordapp.com/icons/111951182947258368/0e82dec99052c22abfbe989ece074cf5.png"
			};

			var footBuilder = new EmbedFooterBuilder()
			{
				Text = "Thanks for using the VDC search!",
				IconUrl = _client.CurrentUser.GetAvatarUrl()
			};

			var builder = new EmbedBuilder()
			{
				Author = authBuilder,
				Footer = footBuilder,

				Title = $"**Search Results**",
				Url = builtUrl,
				ImageUrl = "https://developer.valvesoftware.com/w/skins/valve/images-valve/logo.png",
				Color = new Color(71, 126, 159),

				Description = siteTitle
			};

			await ReplyAsync("",false,builder);
		}

		/// <summary>
		/// Searches the SearchData JSON for a term in a series and replies with what was found.
		/// Responses are limited in DMs
		/// RunMode is Async to prevent blocking the gateway on larger searches.
		/// </summary>
		/// <param name="series">Series to search</param>
		/// <param name="search">Search terms</param>
		/// <returns></returns>
		[Command("search", RunMode = RunMode.Async)]
		[Summary("`>search [series] [SearchTerm]` searches a tutorial series.")]
		[Remarks("`>search [series] [SearchTerm]` searches our tutorial database for a result." +
			"\nThere are a few series you can search from. You can use `>tutorial all [SearchTerm]` to search them all. `All` does not search the FAQ" +
			"\nExample:" +
			"\n `>search v2 displacements` or `>search f leak`" +
			"\n `1` `V2Series` `v2`" +
			"\n `2` `CSGOBootcamp` `bc` `csgobootcamp`" +
			"\n `3` `3dsmax` `3ds`" +
			"\n `4` `WrittenTutorials` `written`" +
			"\n `5` `LegacySeries` `v1` `lg`" +
			"\n `6` `HammerTroubleshooting` `ht` `misc`" +
			"\n `7` `FAQ` `f`" +
			"\n\n `>s [series] [dump/all]` Example: `>s v2 all` Will display ALL tutorials in that series. It can be slow to respond, so please wait!" +
			"\nReally big thanks to Mark for helping make the JSON searching work!")]
		[Alias("s")]
		public async Task SearchAsync(string series, [Remainder]string search)
		{
			var wait = await ReplyAsync($":eyes: Searching for **{search}** in **{series}**. This may take a moment! :eyes:");

			await _dataServices.ChannelLog($"{Context.User} ran a search",$"Series: {series}\nSearch Term: {search}");

			//Is the message a DM?
			bool isPrivate = Context.IsPrivate;

			//Preform the search
			List<List<string>> results = _dataServices.Search(series, search, isPrivate);

			//A dump was requested
			if (search.ToLower() == "dump" || search.ToLower() == "all")
			{
				//[0]title
				//[1]url
				//[2]tags
				List<string> reply = new List<string>();
				string text = null;
				foreach (var r in results)
				{
					text += $"[{r[0]}]({r[1]})\nTags: {r[2]}\n\n";

					if(text.Length > 1800)
					{
						reply.Add(text);
						text = null;
					}
				}

				if (!string.IsNullOrEmpty(text))
					reply.Add(text);

				try //If we cannot send a DM to the user, let them know.
				{
					foreach (var s in reply)
					{
						var builder = new EmbedBuilder()
						{
							Color = new Color(243, 128, 72),

							Description = s
						};
						await Context.User.SendMessageAsync("",false,builder);
					}
				}
				catch
				{
					await ReplyAsync("```\"dump\" and \"all\" search terms can ONLY send replies in a DM. This is to prevent flooding chat." +
						" You got this message because you do not accept DMs from non-friends.```");
				}

				if (!isPrivate)
				{
					await wait.DeleteAsync();

					//Delete the invoking message
					await Context.Message.DeleteAsync();
				}

				return;
			}

			//There are no search results. Build a reply letting them know.
			if (results.Count == 0)
			{
				List<string> singleResult = new List<string>
				{
					"Try a different search term",
					"http://tophattwaffle.com/faq",
					"I could not locate anything for the search term you provided. Please try a different search term.",
					null
				};
				results.Add(singleResult);
			}

			foreach (var r in results)
			{
				var authBuilder = new EmbedAuthorBuilder()
				{
					Name = r[0],
					IconUrl = "https://cdn.discordapp.com/icons/111951182947258368/0e82dec99052c22abfbe989ece074cf5.png"
				};

				var builder = new EmbedBuilder()
				{
					Author = authBuilder,

					Title = $"Click Here",
					Url = r[1],
					ThumbnailUrl = r[3],
					Color = new Color(243,128,72),

					Description = r[2]
				};
				await ReplyAsync("",false,builder);
			}
			if (!isPrivate)
				await wait.DeleteAsync();
		}

		/// <summary>
		/// Displays tutorial series
		/// </summary>
		/// <param name="searchSeries">Specific series to get info for</param>
		/// <returns></returns>
		[Command("tutorials")]
		[Summary("`>tutorials [Optional series]` Displays links to tutorial series")]
		[Remarks("`>tutorials [Optional series]` Example: `>tutorials` `>tutorials v2`" +
			"\nDisplays information about all tutorial series, or the specific one you're looking for" +
			"\n\n`1` `V2Series` `v2`" +
			"\n`2` `CSGOBootcamp` `bc` `csgobootcamp`" +
			"\n`3` `3dsmax` `3ds`" +
			"\n`4` `WrittenTutorials` `written`" +
			"\n`5` `LegacySeries` `v1` `lg`" +
			"\n`6` `HammerTroubleshooting` `ht`")]
		[Alias("t")]
		public async Task TutorialsAsync(string searchSeries = "all")
		{
			string authTitle = null;
			string authImgUrl = "https://cdn.discordapp.com/icons/111951182947258368/0e82dec99052c22abfbe989ece074cf5.png";
			string footText = null;
			string footImgUrl = _client.CurrentUser.GetAvatarUrl();
			string bodyTitle = "Click Here!";
			string bodyUrl = null;
			string bodyThumbUrl = null;  //"https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png"
			string bodyImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/header.png";
			string bodyDescription = null;

			//What series?
			switch (searchSeries.ToLower()) {
				case "v2series":
				case "v2":
				case "1":
					authTitle = "Version 2 Tutorial Series";

					bodyUrl = "https://goo.gl/XoVXzd";

					bodyDescription = "The Version 2 Tutorial series was created with the knowledge that I gained from creating the " +
									  "Version 1(Now Legacy) series of tutorials.The goal is to help someone who hasn’t ever touched the tools " +
									  "get up and running in Source Engine level design. You can watch them in any order, " +
									  "but they have been designed to build upon each other.";

					break;
				case "csgobootcamp":
				case "bc":
				case "2":
					authTitle = "CSGO Level Design Bootcamp";

					bodyUrl = "https://goo.gl/srFBxe";

					bodyDescription = "The CSGO Boot Camp series was created for ECS to air during their Twitch streams between matches." +
									  " It is created to help someone with no experience with the level design tools learn everything they need to" +
									  " create a competitive CSGO level. Most these tutorials apply to every Source Engine game," +
									  " but a handful are specific to CSGO.";

					break;
				case "3dsmax":
				case "3ds":
				case "3":
					authTitle = "3dsmax Tutorials";

					bodyUrl = "https://goo.gl/JGg48X";

					bodyDescription = "There are a few sub series in the 3dsmax section. If you’re looking to create and export your very first Source Engine prop, check out the **My First Prop** series." +
									  "\nIf you’re getting start with 3dsmax look at the **Beginners Guide** series, which is like the Version 2 Tutorial series but for 3dsmax." +
									  "\nThere are a few one - off tutorials listed on the page as well covering WallWorm functions";

					break;
				case "writtentutorials":
				case "written":
				case "4":
					authTitle = "Written Tutorials";

					bodyUrl = "https://goo.gl/i4aAqh";

					bodyDescription = "My library of written tutorials is typically about 1 off things that I want to cover. They are usually independent of any specific game.";

					break;
				case "legacyseries":
				case "v1":
				case "lg":
				case "5":
					authTitle = "Legacy Series";

					bodyUrl = "https://goo.gl/aHFcvX";

					bodyDescription = "Hammer Troubleshooting is a smaller series that is created off user questions that I see come up quite often.y are usually independent of any specific game.";

					break;
				case "hammertroubleshooting":
				case "ht":
				case "6":
					authTitle = "Hammer Troubleshooting";

					bodyUrl = "https://goo.gl/tBh7jT";

					bodyDescription = "The First tutorial series was my launching point for getting better at mapping. Not only did I learn a lot from making it, but I like to " +
									  "think that many others learned something from the series as well. The series was flawed in that it was not structured, and lacked quality control. But" +
									  " you may notice that the further along in the series you are, the better quality they get. Example is the 100th tutorial, it heavily reflects how the " +
									  "V2 series was created. You can view the entire series below. Just be warned that some of the information in these videos may not be correct, or even " +
									  "work any longer. Please watch at your own risk. I attempt to support these tutorials, but cannot due to time. Please watch the V2 series";

					break;
				case "all":
					authTitle = "All Tutorial Series Information";

					bodyUrl = "https://www.tophattwaffle.com/tutorials/";

					bodyDescription = $"Over the years I've built up quite the collection of tutorial series! " +
									  $"\n__Here they all are__" +
									  $"\n[Version 2 Series](https://goo.gl/XoVXzd)" +
									  $"\n[CSGO Bootcamp](https://goo.gl/srFBxe)" +
									  $"\n[3dsmax](https://goo.gl/JGg48X)" +
									  $"\n[Written Tutorials](https://goo.gl/i4aAqh)" +
									  $"\n[Hammer Troubleshooting](https://goo.gl/tBh7jT)" +
									  $"\n[Legacy Series V1](https://goo.gl/aHFcvX)";

					break;
				default:
					await ReplyAsync("Unknown series. Please try `>help tutorials` to see all the options.");
					return;
			}

			//Build and send message
			var authBuilder = new EmbedAuthorBuilder()
			{
				Name = authTitle,
				IconUrl = authImgUrl
			};

			var footBuilder = new EmbedFooterBuilder()
			{
				Text = footText,
				IconUrl = footImgUrl
			};

			var builder = new EmbedBuilder()
			{
				Author = authBuilder,
				Footer = footBuilder,

				Title = bodyTitle,
				Url = bodyUrl,
				ImageUrl = bodyImageUrl,
				ThumbnailUrl = bodyThumbUrl,
				Color = new Color(243,128,72),

				Description = bodyDescription
			};
			await ReplyAsync("",false,builder);

		}

		/// <summary>
		/// CAT FACTS THAT YOU CANNOT UNSUBSCRIBE FROM
		/// </summary>
		/// <returns></returns>
		[Command("catFact", RunMode = RunMode.Async)]
		[Summary("`>catFact` Gives you a cat fact!")]
		[Remarks("Ever want to know more about cats? Now you can.")]
		[Alias("gimme a cat fact", "hit me with a cat fact", "hit a nigga with a cat fact", "cat fact", "catfacts", "cat facts")]
		public async Task CatFactAsync()
		{
			var catFact = "Did you know cats have big bushy tails?";
			var name = "Cat Fact 0";

			//Get a fact from the file
			if (File.Exists(_dataServices.CatFactPath))
			{
				string[] allLines = File.ReadAllLines(_dataServices.CatFactPath);
				int lineNumber = _random.Next(0, allLines.Length);
				catFact = allLines[lineNumber];

				//Match on title for a fancy title
				Match match = Regex.Match(catFact, @"^\w+ Fact \d*", RegexOptions.IgnoreCase);
				name = match.Value;
				catFact = catFact.Substring(match.Length).Trim();
			}

			//Build and send
			var builder = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder {
					Name = "CAT FACTS!",
					IconUrl = Context.Message.Author.GetAvatarUrl(),
				},
				Footer = new EmbedFooterBuilder {
					Text = "This was cat facts, you cannot unsubscribe."
				},

				ThumbnailUrl = this._dataServices.GetRandomImgFromUrl("https://content.tophattwaffle.com/BotHATTwaffle/catfacts/"),
				Color = new Color(230, 235, 240),

				Fields =
				{
					new EmbedFieldBuilder
					{
						Name = name,
						Value = catFact
					}
				}
			};

			await _dataServices.ChannelLog($"{Context.Message.Author.Username.ToUpper()} JUST GOT HIT WITH A CAT FACT");
			await ReplyAsync("", false, builder.Build());
		}

		[Command("unsubscribe")]
		[Summary("`>unsubscribe` Unsubscribes you from cat facts")]
		[Remarks("Takes you off the cat fact list.")]
		public async Task CatFactUnsubAsync()
		{
			await ReplyAsync("You cannot unsubscribe from cat facts...");
		}

		/// <summary>
		/// Penguin Facts!
		/// </summary>
		/// <returns></returns>
		[Command("PenguinFact", RunMode = RunMode.Async)]
		[Summary("`>PenguinFact` Gives you a Penguin fact!")]
		[Remarks("Ever want to know more about Penguin? Now you can.")]
		[Alias("gimme a penguin fact", "hit me with a penguin fact", "hit a nigga with a penguin fact", "penguin fact", "penguinfacts", "penguin facts")]
		public async Task PenguinFactAsync()
		{
			//Get a fact from the file
			string penguinFact = "Did you know penguins have big bushy tails?";
			if (File.Exists(_dataServices.PenguinFactPath))
			{
				var allLines = File.ReadAllLines(_dataServices.PenguinFactPath);
				var lineNumber = _random.Next(0, allLines.Length);
				penguinFact = allLines[lineNumber];
			}

			//Build and send
			var authBuilder = new EmbedAuthorBuilder()
			{
				Name = $"PENGUIN FACTS!",
				IconUrl = Context.Message.Author.GetAvatarUrl(),
			};

			var footBuilder = new EmbedFooterBuilder()
			{
				Text = "This was penguin facts, you cannot unsubscribe."
			};
			var builder = new EmbedBuilder()
			{
				Author = authBuilder,
				Footer = footBuilder,

				ThumbnailUrl = this._dataServices.GetRandomImgFromUrl("https://content.tophattwaffle.com/BotHATTwaffle/penguinfacts/"),
				Color = new Color(230, 235, 240),

				Description = penguinFact
			};
			await _dataServices.ChannelLog($"{Context.Message.Author.Username.ToUpper()} JUST GOT HIT WITH A PENGUIN FACT");
			await ReplyAsync("", false, builder.Build());
		}

		/// <summary>
		/// Tanooki Facts!
		/// </summary>
		/// <returns></returns>
		[Command("TanookiFact", RunMode = RunMode.Async)]
		[Summary("`>tanookiFact` Gives you a Tanooki fact!")]
		[Remarks("Ever want to know more about Tanooki? Now you can.")]
		[Alias("gimme a tanooki fact", "hit me with a tanooki fact", "hit a nigga with a tanooki fact", "tanooki fact", "tanookifacts", "tanooki facts", "@TanookiSuit3")]
		public async Task TanookiFactAsync()
		{
			//Get a fact from the file
			string tanookiFact = "Did you know Tanooki has a big bushy tail?";
			if (File.Exists(_dataServices.TanookiFactPath))
			{
				var allLines = File.ReadAllLines(_dataServices.TanookiFactPath);
				var lineNumber = _random.Next(0, allLines.Length);
				tanookiFact = allLines[lineNumber];
			}

			//Build and send
			var authBuilder = new EmbedAuthorBuilder()
			{
				Name = $"TANOOKI FACTS!",
				IconUrl = Context.Message.Author.GetAvatarUrl(),
			};

			var footBuilder = new EmbedFooterBuilder()
			{
				Text = "This was Tanooki facts, you cannot unsubscribe."
			};
			var builder = new EmbedBuilder()
			{
				Author = authBuilder,
				Footer = footBuilder,

				ThumbnailUrl = this._dataServices.GetRandomImgFromUrl("https://content.tophattwaffle.com/BotHATTwaffle/tanookifacts/"),
				Color = new Color(230, 235, 240),

				Description = tanookiFact
			};
			await _dataServices.ChannelLog($"{Context.Message.Author.Username.ToUpper()} JUST GOT HIT WITH A TANOOKI FACT");
			await ReplyAsync("", false, builder.Build());
		}

		/// <summary>
		/// Shows a picture of glorious leader!
		/// </summary>
		/// <returns></returns>
		[Command("tanookiirl", RunMode = RunMode.Async)]
		[Summary("`>tanookiirl` Shows you Tanooki looking at stuff!")]
		[Alias("tanookilooksatthings")]
		public async Task TanookiLookAsync()
		{
			//Build and send
			var builder = new EmbedBuilder()
			{
				ImageUrl = this._dataServices.GetRandomImgFromUrl("https://content.tophattwaffle.com/BotHATTwaffle/kimjongillookingatthings/"),
				Color = new Color(138, 43, 226),
			};
			await ReplyAsync("", false, builder.Build());
		}
	}
}
