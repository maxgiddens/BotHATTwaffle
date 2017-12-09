﻿using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using System;

namespace BotHATTwaffle
{
    class Eavesdropping
    {
        string[] pakRatEavesDrop;
        string[] howToPackEavesDrop;
        string[] carveEavesDrop;
        string[] propperEavesDrop;
        string[] vbEavesDrop;
        string[] yorkEavesDrop;
        string[] agreeEavesDrop;
        string[] agreeStrings;
        Random _random;

        public Eavesdropping(Dictionary<string, string> config)
        {
            if (config.ContainsKey("pakRatEavesDropCSV"))
                pakRatEavesDrop = (config["pakRatEavesDropCSV"]).Split(',') ;
            if (config.ContainsKey("howToPackEavesDropCSV"))
                howToPackEavesDrop = (config["howToPackEavesDropCSV"]).Split(',');
            if (config.ContainsKey("carveEavesDropCSV"))
                carveEavesDrop = (config["carveEavesDropCSV"]).Split(',');
            if (config.ContainsKey("propperEavesDropCSV"))
                propperEavesDrop = (config["propperEavesDropCSV"]).Split(',');
            if (config.ContainsKey("vbEavesDropCSV"))
                vbEavesDrop = (config["vbEavesDropCSV"]).Split(',');
            if (config.ContainsKey("yorkCSV"))
                yorkEavesDrop = (config["yorkCSV"]).Split(',');
            if (config.ContainsKey("agreeUserCSV"))
                agreeEavesDrop = (config["agreeUserCSV"]).Split(',');

            _random = new Random();
            agreeStrings = new string[]{
                "^",
                "^^^",
                "^^^ I agree with ^^^",
            };
        }

        async internal Task UserJoin(SocketUser user)
        {
            await Program.ChannelLog($"userjoin", $"{user.Username} has joined the server. Apply roles and sending them a message.");

            var builder = new EmbedBuilder();
            var authBuilder = new EmbedAuthorBuilder();
            var footBuilder = new EmbedFooterBuilder();
            authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {user.Username}! Welcome to the r/sourceengine discord server!",
                IconUrl = "https://cdn.discordapp.com/icons/111951182947258368/0e82dec99052c22abfbe989ece074cf5.png"
            };

            footBuilder = new EmbedFooterBuilder()
            {
                Text = "Once again thanks for joining, hope you enjoy your stay!",
                IconUrl = Program._client.CurrentUser.GetAvatarUrl()
            };

            builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"Thanks for joining!",
                Url = "https://www.tophattwaffle.com",
                ImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/12/Discord-LogoWordmark-Color.png",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = $"Hi there! Thanks for joining the SourceEngine Discord server!\n" +
                $"There are a few things to know before you are able to talk in the server. Feel free to ask a question in " +
                $"any of the relevant channels you see. Just try to keep things on topic. \n\nAdditionally, you've been given a role called" +
                $" `Playtester`. This role is used to notify you when we have a playtest starting. You can remove yourself from the " +
                $"notifications by typing: `>playtester`.\n\nIf you want to see any of my commands, type: `>help`. Thanks for reading," +
                $" and we hope you enjoy your stay here!\n\nGLHF"
            };
            await (user as IGuildUser).AddRoleAsync(Program.playTesterRole);
            await user.SendMessageAsync("",false,builder);
        }

        async internal Task Listen(SocketMessage message)
        {
            bool proceed = true;
            //If the message is from a bot, just break.
            if (message.Author.IsBot)
                return;

            //Let's check against the values defined in the settings file.

            if (proceed)
                if (message.Content.Contains(":KMS:") || message.Content.Contains(":ShootMyself:"))
                {
                    var builder = new EmbedBuilder()
                    {
                        ImageUrl = "https://content.tophattwaffle.com/BotHATTwaffle/doit.jpg",
                    };
                    await message.Channel.SendMessageAsync("",false, builder);
                 }
            if (proceed)
                if (message.Content.ToLower().Equals("who is daddy") || message.Content.ToLower().Equals("who is tophattwaffle"))
                {
                    await message.Channel.SendMessageAsync("TopHATTwaffle my daddy.");
                }
            if (proceed)
                foreach (string s in agreeEavesDrop)
                {
                    if (message.Content.Equals("^") && message.Author.Username.Equals(s))
                    {
                            await message.Channel.SendMessageAsync(agreeStrings[_random.Next(0, agreeStrings.Length)]);
                            break;
                    }
                }
            if (proceed)
                foreach (string s in pakRatEavesDrop)
                {
                    if(message.Content.ToLower().Contains(s))
                    {
                        await PakRat(message);
                            proceed = false;
                        break;
                    }
                }
            if(proceed)
                foreach (string s in howToPackEavesDrop)
                {
                    if (message.Content.ToLower().Contains(s))
                    {
                        await HowToPack(message);
                        proceed = false;
                        break;
                    }
                }
            if (proceed)
                foreach (string s in carveEavesDrop)
                {
                    if (message.Content.ToLower().Contains(s))
                    {
                        await Carve(message);
                        proceed = false;
                        break;
                    }
                }
            if (proceed)
                foreach (string s in propperEavesDrop)
                {
                    if (message.Content.ToLower().Contains(s))
                    {
                        await Propper(message);
                        proceed = false;
                        break;
                    }
                }
            if (proceed)
                foreach (string s in vbEavesDrop)
                {
                    if (message.Content.ToLower().Contains(s))
                    {
                        await VB(message);
                        proceed = false;
                        break;
                    }
                }
            if (proceed)
                foreach (string s in yorkEavesDrop)
                {
                    if (message.Content.ToLower().Contains(s))
                    {
                        await DeYork(message);
                        proceed = false;
                        break;
                    }
                }
        }

        private static Task KYS(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} was asking about PakRat in #{message.Channel}");

            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder()
            {
                Text = "This was an automated message about - PakRat"

            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"Click here to learn how to use VIDE!",
                Url = "https://www.tophattwaffle.com/packing-custom-content-using-vide-in-steampipe/",
                ImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2013/11/vide.png",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = "I was minding my own business when I heard you mention something about PakRat. " +
                "Don't know if you know this, but PakRat is super old and has been know to cause issues in newer games. " +
                "There is a newer program that handles packing better called VIDE. You should check that out instead."
            };

            message.Channel.SendMessageAsync(":kms:");

            return Task.CompletedTask;
        }

        private static Task PakRat(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} was asking about PakRat in #{message.Channel}");

            var authBuilder = new EmbedAuthorBuilder() {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder() {
                Text = "This was an automated message about - PakRat"

            };

            var builder = new EmbedBuilder() {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"Click here to learn how to use VIDE!",
                Url = "https://www.tophattwaffle.com/packing-custom-content-using-vide-in-steampipe/",
                ImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2013/11/vide.png",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = "I was minding my own business when I heard you mention something about PakRat. " +
                "Don't know if you know this, but PakRat is super old and has been know to cause issues in newer games. " +
                "There is a newer program that handles packing better called VIDE. You should check that out instead."
            };

            message.Channel.SendMessageAsync("",false,builder);

            return Task.CompletedTask;
        }

        private static Task HowToPack(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} was asking how to pack a level in #{message.Channel}");

            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder()
            {
                Text = "This was an automated message about - HowToPack"

            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"Click here to learn how to use VIDE!",
                Url = "https://www.tophattwaffle.com/packing-custom-content-using-vide-in-steampipe/",
                ImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2013/11/vide.png",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = $"I noticed you may be looking for information on how to pack custom content into your level. " +
                $"This is easily done using VIDE. Click the like above to download VIDE and learn how to use it."
            };

            message.Channel.SendMessageAsync("", false, builder);

            return Task.CompletedTask;
        }

        private static Task Carve(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} was asking how to carve in #{message.Channel}. You should probably kill them.");

            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder()
            {
                Text = "This was an automated message about - Carve"

            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"DO NOT USE CARVE",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = $"I was minding my own damn business when you come around asking how to carve." +
                $"\n\n\n**__DON'T__**"
            };

            message.Channel.SendMessageAsync("", false, builder);

            return Task.CompletedTask;
        }

        private static Task Propper(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} was asking about Propper in #{message.Channel}. You should go WWMT fanboy.");

            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder()
            {
                Text = "This was an automated message about - Propper"

            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"Click here to go to the WallWorm site!",
                Url = "https://dev.wallworm.com/",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                ImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/12/worm_logo.png",
                Color = new Color(243, 128, 72),

                Description = $"I saw you were asking about propper. While Propper still works, it's advised to learn" +
                $"a better modeling solution. The prefered method for Source Engine is using 3dsmax with WallWorm Model Tools" +
                $" If you don't want to learn 3dsmax and WWMT, you can learn to configure propper at the link below.: " + 
                $"\n\nhttps://www.tophattwaffle.com/configuring-propper-for-steampipe/"
            };

            message.Channel.SendMessageAsync("", false, builder);

            return Task.CompletedTask;
        }

        private static Task VB(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} posted about Velocity Brawl #{message.Channel}. You should go kill them.");
            message.DeleteAsync(); //Delete their message about shit game
            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder()
            {
                Text = "This was an automated message about - Velocity Brawl"

            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"Please no...",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = $"I saw you posted about Velocity Brawl. How about we do not do that."
            };
            
            message.Channel.SendMessageAsync("", false, builder);

            return Task.CompletedTask;
        }

        private static Task DeYork(SocketMessage message)
        {
            Program.ChannelLog($"{message.Author} posted about de_york #{message.Channel}. You should go meme them.");
            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var footBuilder = new EmbedFooterBuilder()
            {
                Text = "This was an automated message about - de_york"

            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,
                Footer = footBuilder,

                Title = $"You talking about the best level ever?",

                ImageUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/690235828_preview_20161014231026_1-1.jpg",
                ThumbnailUrl = "https://www.tophattwaffle.com/wp-content/uploads/2017/11/1024_png-300x300.png",
                Color = new Color(243, 128, 72),

                Description = $"I see that we both share the same love for amazing levels."
            };

            message.Channel.SendMessageAsync("", false, builder);

            return Task.CompletedTask;
        }
    }
}
