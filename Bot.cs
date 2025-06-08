using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net.Models;
using FMWOTB;
using FMWOTB.Tools.Replays;
using JsonObjectConverter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FMWOTB.Account;
using FMWOTB.Vehicles;
using FMWOTB.Clans;
using FMWOTB.Tools;
using FMWOTB.Tournament;
using DiscordHelper;
using System.Timers;
using FMWOTB.Exceptions;
using Microsoft.VisualBasic;
using DSharpPlus.EventArgs;

namespace NLBE_Bot {
    //version 4.0.0-nightly-00760
    public class Bot {
        // public static string version = "THIMO LOCAL";
        public static string version = "2.12";
        // public const string Prefix = "test ";
        private const string Token = "";//nlbe bot
        public const string Prefix = "nlbe ";
        public const string logInputPath = "./loginputlines.txt";
        public const string WG_APPLICATION_ID = "";
        public const string ERROR_REACTION = ":x:";
        public const string IN_PROGRESS_REACTION = ":hourglass_flowing_sand:";
        public const string ACTION_COMPLETED_REACTION = ":white_check_mark:";
        public const string MAINTENANCE_REACTION = ":tool_logo:";
        public const int HOF_AMOUNT_PER_TANK = 3;
        public const ulong NLBE_SERVER_ID = 507575681593638913;
        public const ulong DA_BOIS_ID = 693519504235561080;
        public const ulong MOET_REGELS_NOG_LEZEN_ROLE = 793830434551103500;
        public const ulong NOOB_ROLE = 782272112505258054;
        public const ulong LEDEN_ROLE = 681965919614009371;
        public const ulong NLBE_ROLE = 668534098729631745;
        public const ulong NLBE2_ROLE = 781625012695728140;
        public const ulong TOERNOOI_DIRECTIE = 782751703559700530;
        public const ulong DISCORD_ADMIN_ROLE = 781634960930242614;
        public const ulong DEPUTY_ROLE = 557951586975088662;
        public const ulong DEPUTY_NLBE_ROLE = 805783688783724604;
        public const ulong DEPUTY_NLBE2_ROLE = 805783828312227840;
        public const ulong BEHEERDER_ROLE = 681865080803033109;
        public const ulong NLBE_BOT = 781618903314202644;
        public const ulong TESTBEASTV2_BOT = 794166024135639050;
        public const ulong THIBEASTMO_ID = 239109910321823744;//781618903314202644; //239109910321823744
        public const ulong THIBEASTMO_ALT_ID = 756193463913021512;
        public const ulong MASTERY_REPLAYS_ID = 734359875253174323;
        public const ulong BOTTEST_ID = 781617141069774898;
        public const ulong PRIVE_ID = 702607178892312587;
        public const ulong NLBE_TOERNOOI_AANMELDEN_KANAAL_ID = 714860361894854780;
        public const ulong DA_BOIS_TOERNOOI_AANMELDEN_KANAAL_ID = 808324144197271573;
        public const long NLBE_CLAN_ID = 865;
        public const long NLBE2_CLAN_ID = 48814;
        public const char LOG_SPLIT_CHAR = '|';
        public const char UNDERSCORE_REPLACEMENT_CHAR = 'ˍ';

        public const char REPLACEABLE_UNDERSCORE_CHAR = '＿';
        //https?:\/\/[a-zA-Z0-9]*.[a-z\.]*\/?[a-zA-Z0-9\/\.?=&\-#]* --> site regex
        //public const string LINK_REGEX = @"https?:\/\/[a-zA-Z0-9]*.[a-z\.]*\/?[a-zA-Z0-9\/\.?=&\-#]*";
        public static DiscordColor WEEKLY_EVENT_COLOR { get; } = DiscordColor.Gold;
        public static DiscordColor HOF_COLOR { get; } = DiscordColor.Blurple;
        public static DiscordColor BOT_COLOR { get; } = DiscordColor.Red;
        public static DiscordClient discordClient;
        CommandsNextExtension Commands;
        public static IReadOnlyDictionary<ulong, DiscordGuild> discGuildslist;
        public static bool ignoreCommands = false;
        public static bool ignoreEvents = false;
        public static Tuple<ulong, DateTime> weeklyEventWinner = new Tuple<ulong, DateTime>(0, DateTime.Now);
        public static int waitTime = 30;
        public static int hofWaitTime = 2;//in minutes
        public static int newPlayerWaitTime = 1;//In days
        private static DiscordMessage discordMessage;//temp message
        private DateTime lasTimeNamesWereUpdated;
        private short heartBeatCounter = 0;
        public async Task RunAsync()
        {
            //fmwotb = new Application(WG_APPLICATION_ID);
            Console.ForegroundColor = ConsoleColor.Gray;

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            discordClient.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(Bot.waitTime),
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = false,
                EnableDefaultHelp = false
            };

            Commands = discordClient.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<BotCommands>();
            Commands.CommandErrored += Commands_CommandErrored;
            Commands.CommandExecuted += Commands_CommandExecuted;

            var act = new DiscordActivity(Prefix, ActivityType.ListeningTo);
            await discordClient.ConnectAsync(act, UserStatus.Online);
            //Events
            discordClient.Heartbeated += DiscordClient_Heartbeated;
            discordClient.Ready += Discord_Ready;
            discordClient.GuildMemberAdded += Discord_GuildMemberAdded;
            discordClient.MessageReactionAdded += Discord_MessageReactionAdded;
            discordClient.GuildMemberRemoved += Discord_GuildMemberRemoved;
            discordClient.MessageReactionRemoved += Discord_MessageReactionRemoved;
            discordClient.MessageDeleted += Discord_MessageDeleted;
            discordClient.GuildMemberUpdated += Discord_GuildMemberUpdated;
            discordClient.MessageCreated += Discord_MessageCreated;
            //discordClient. += DiscordClient_DmChannelCreated;

            //WGSettings wgsettings = new WGSettings(WGLanguage.English);

            //wotb = new WGClient.WOTB.Application(WG_APPLICATION_ID, WGRegion.Europe, wgsettings);
            ////var iets = new WGClient.WOTB.Entities.Clans.ClanDetail();

            await Task.Delay(-1);
        }

        private async Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs args)
        {
            discordClient.Logger.Log(LogLevel.Information, "Command executed: " + args.Command.Name);
            await Task.CompletedTask;
        }

        public static async Task<DiscordMessage> SendMessage(DiscordChannel channel, DiscordMember member, string guildName, string Message)
        {
            bool Worked = false;
            try{
                return await channel.SendMessageAsync(Message);
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                await handleError("[" + guildName + "] (" + channel.Name + ") Could not send message: ", ex.Message, ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                Worked = false;
                if (ex.Message.ToLower().Contains("unauthorized")){
                    await Bot.SayBotNotAuthorized(channel);
                }
                else{
                    await Bot.SayTooManyCharacters(channel);
                }
            }
            if (!Worked && member != null){
                await Bot.SendPrivateMessage(member, guildName, Message);
            }
            return null;
        }
        public static async Task<bool> SendPrivateMessage(DiscordMember member, string guildName, string Message)
        {
            try{
                await member.SendMessageAsync(Message);
                return true;
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                await handleError("[" + guildName + "] Could not send private message: ", ex.Message, ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return false;
        }


        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, string.Empty, discEmbFieldList, emojiList, imageURL, embedAuthor, Bot.BOT_COLOR, false);
        }
        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, bool isForReplay)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, string.Empty, discEmbFieldList, emojiList, imageURL, embedAuthor, Bot.BOT_COLOR, isForReplay);
        }
        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, DiscordColor color)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, string.Empty, discEmbFieldList, emojiList, imageURL, embedAuthor, Bot.BOT_COLOR, false);
        }

        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, string footer, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, footer, discEmbFieldList, emojiList, imageURL, embedAuthor, Bot.BOT_COLOR, false);
        }
        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, string footer, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, bool isForReplay)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, footer, discEmbFieldList, emojiList, imageURL, embedAuthor, Bot.BOT_COLOR, isForReplay);
        }
        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, string footer, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, DiscordColor color)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, footer, discEmbFieldList, emojiList, imageURL, embedAuthor, Bot.BOT_COLOR, false);
        }

        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, string footer, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, DiscordColor color, bool isForReplay)
        {
            return await CreateEmbed(channel, thumbnail, content, title, description, footer, discEmbFieldList, emojiList, imageURL, embedAuthor, color, isForReplay, string.Empty);
        }
        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, string thumbnail, string content, string title, string description, string footer, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, DiscordColor color, bool isForReplay, string nextMessage)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = color;
            newDiscEmbedBuilder.Title = title;

            newDiscEmbedBuilder.Description = description;
            //newDiscEmbedBuilder.Description = description.adaptToDiscordChat().Replace(REPLACEABLE_UNDERSCORE_CHAR, '_');
            if (thumbnail != string.Empty){
                try{
                    if (imageURL != string.Empty){
                        newDiscEmbedBuilder.WithThumbnail(thumbnail);
                    }
                }
                catch (Exception ex){
                    await handleError("Could not set imageurl for embed: ", ex.Message, ex.StackTrace);
                }
            }
            if (embedAuthor != null){
                newDiscEmbedBuilder.Author = embedAuthor;
            }
            bool imageWorked = true;
            try{
                if (imageURL != string.Empty){
                    newDiscEmbedBuilder.ImageUrl = imageURL;
                }
            }
            catch{
                imageWorked = false;
            }
            if (!imageWorked){
                try{
                    if (imageURL != string.Empty){
                        newDiscEmbedBuilder.WithImageUrl(new Uri(imageURL.Replace("\\", string.Empty)));
                    }
                }
                catch (Exception ex){
                    imageWorked = false;
                    await handleError("Could not set imageurl for embed: ", ex.Message, ex.StackTrace);
                }
            }

            if (discEmbFieldList != null){
                if (discEmbFieldList.Count > 0){
                    foreach (var field in discEmbFieldList){
                        if (field.Value.Length > 0){
                            try{
                                newDiscEmbedBuilder.AddField(field.Name, field.Value, field.Inline);
                            }
                            catch (Exception ex){
                                Console.ForegroundColor = ConsoleColor.Red;
                                await handleError("Something went wrong while trying to add a field to an embedded message:", ex.Message, ex.StackTrace);
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                        }
                    }
                }
            }
            if (footer != null && footer.Length > 0){
                DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter();
                embedFooter.Text = footer;
                newDiscEmbedBuilder.Footer = embedFooter;
            }
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                DiscordMessage theMessage;
                if (isForReplay){
                    theMessage = Bot.discordMessage.RespondAsync(content, embed).Result;
                }
                else{
                    theMessage = discordClient.SendMessageAsync(channel, content, embed).Result;
                }
                try{
                    if (emojiList != null){
                        foreach (DiscordEmoji anEmoji in emojiList){
                            await theMessage.CreateReactionAsync(anEmoji);
                        }
                    }
                }
                catch (Exception ex){
                    await handleError("Error while adding emoji's:", ex.Message, ex.StackTrace);
                }
                if (nextMessage != null && nextMessage.Length > 0){
                    await channel.SendMessageAsync(nextMessage);
                }
                return theMessage;
            }
            catch (Exception ex){
                await handleError("Error in createEmbed:", ex.Message, ex.StackTrace);
            }
            return null;
        }
        public static async Task<DiscordMessage> CreateEmbed(DiscordMember member, string thumbnail, string content, string title, string description, List<DEF> discEmbFieldList, List<DiscordEmoji> emojiList, string imageURL, DiscordEmbedBuilder.EmbedAuthor embedAuthor, DiscordColor color)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = color;
            newDiscEmbedBuilder.Title = title;

            newDiscEmbedBuilder.Description = description.adaptToDiscordChat().Replace(REPLACEABLE_UNDERSCORE_CHAR, '_');
            if (thumbnail != string.Empty){
                newDiscEmbedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
                newDiscEmbedBuilder.Thumbnail.Url = thumbnail;
            }
            if (embedAuthor != null){
                newDiscEmbedBuilder.Author = embedAuthor;
            }

            bool imageWorked = true;
            try{
                if (imageURL != string.Empty){
                    newDiscEmbedBuilder.ImageUrl = imageURL;
                }
            }
            catch{
                imageWorked = false;
            }
            if (!imageWorked){
                try{
                    if (imageURL != string.Empty){
                        newDiscEmbedBuilder.WithImageUrl(imageURL);
                    }
                }
                catch (Exception ex){
                    imageWorked = false;
                    await handleError("Could not set imageurl for embed:", ex.Message, ex.StackTrace);
                }
            }


            if (discEmbFieldList != null){
                if (discEmbFieldList.Count > 0){
                    foreach (var field in discEmbFieldList){
                        try{
                            newDiscEmbedBuilder.AddField(field.Name, field.Value, field.Inline);
                        }
                        catch (Exception ex){
                            Console.ForegroundColor = ConsoleColor.Red;
                            await handleError("Something went wrong while trying to add a field to an embedded message:", ex.Message, ex.StackTrace);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                }
            }
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                DiscordMessage theMessage = member.SendMessageAsync(null, embed).Result;
                try{
                    if (emojiList != null){
                        foreach (DiscordEmoji anEmoji in emojiList){
                            await theMessage.CreateReactionAsync(anEmoji);
                        }
                    }
                }
                catch (Exception ex){
                    await handleError("Error while adding emoji's:", ex.Message, ex.StackTrace);
                }
                return theMessage;
            }
            catch (Exception ex){
                await handleError("Error in createEmbed:", ex.Message, ex.StackTrace);
            }
            return null;
        }

        #region Events

        private Task DiscordClient_Heartbeated(DiscordClient sender, DSharpPlus.EventArgs.HeartbeatEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    #region Voor een berichtje te sturen

                    //DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
                    //newDiscEmbedBuilder.Color = DiscordColor.Red;
                    //newDiscEmbedBuilder.Title = "Prettige feestdagen!";
                    //newDiscEmbedBuilder.ImageUrl = "https://i0.wp.com/hyperallergic-newspack.s3.amazonaws.com/uploads/2012/12/christmas-animated-gifs-06.gif?quality=100";
                    ////SAO
                    ////newDiscEmbedBuilder.ImageUrl = "https://www.google.com/url?sa=i&url=https%3A%2F%2Fgifer.com%2Fen%2FSFHs&psig=AOvVaw39TqYc__Z9zXxOvH9ID36P&ust=1640524344469000&source=images&cd=vfe&ved=0CAsQjRxqFwoTCMiry9uD__QCFQAAAAAdAAAAABAD";
                    //DiscordEmbed embed = newDiscEmbedBuilder.Build();

                    //DiscordChannel algemeen = Bot.GetAlgemeenChannel().Result;
                    //if (algemeen != null)
                    //{
                    //    try
                    //    {
                    //        algemeen.SendMessageAsync(string.Empty, false, embed);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        //algemeen.SendMessageAsync("**Er ging iets mis:\n" + e.Message + "**", false, null);
                    //    }
                    //}

                    #endregion


                    heartBeatCounter++;
                    const int hourToCheck = 14;
                    const DayOfWeek dayToCheck = DayOfWeek.Monday;
                    //discordClient.Logger.LogInformation("Heartbeat nr. " + heartBeatCounter);
                    if ((DateTime.Now.DayOfWeek != dayToCheck || DateTime.Now.Hour != hourToCheck) && heartBeatCounter > 2){
                        //update usernames
                        heartBeatCounter = 0;
                        bool update = false;
                        if (lasTimeNamesWereUpdated != null){
                            if (lasTimeNamesWereUpdated.DayOfYear != DateTime.Now.DayOfYear){
                                update = true;
                                lasTimeNamesWereUpdated = DateTime.Now;
                            }
                        }
                        else{
                            update = true;
                            lasTimeNamesWereUpdated = DateTime.Now;
                        }
                        if (update){
                            try{
                                await updateUsers();
                            }
                            catch (InternalServerErrorException ex){
                                var message = "\nERROR updating users:\nInternal server exception from api request\n" + ex.Message;
                                await Bot.sendThibeastmo(message, string.Empty, string.Empty);
                                DiscordChannel bottestChannel = await GetBottestChannel();
                                await bottestChannel.SendMessageAsync(message);
                            }
                            catch (Exception ex){
                                var message = "\nERROR updating users:\n" + ex.Message;
                                await Bot.sendThibeastmo(message, string.Empty, string.Empty);
                                DiscordChannel bottestChannel = await GetBottestChannel();
                                await bottestChannel.SendMessageAsync(message);
                            }
                        }
                    }
                    else if (DateTime.Now.DayOfWeek == dayToCheck && DateTime.Now.Hour == hourToCheck && heartBeatCounter == 2)//14u omdat wotb ook wekelijks op maandag 14u restart
                    {
                        //We have a weekly winner
                        string winnerMessage = "We hebben een wekelijkse winnaar.";
                        DiscordChannel bottestChannel = await GetBottestChannel();
                        try{
                            discordClient.Logger.LogInformation(winnerMessage);
                            WeeklyEventHandler weeklyEventHandler = new WeeklyEventHandler();
                            await weeklyEventHandler.ReadWeeklyEvent();
                            if (weeklyEventHandler.WeeklyEvent.StartDate.DayOfYear == DateTime.Now.DayOfYear - 7)//-7 omdat het dan zeker een nieuwe week is maar niet van twee weken gelden
                            {
                                winnerMessage += "\nNa 1 week...";
                                WeeklyEventItem weeklyEventItemMostDMG = weeklyEventHandler.WeeklyEvent.WeeklyEventItems.Find(weeklyEventItem => weeklyEventItem.WeeklyEventType == WeeklyEventType.Most_damage);
                                if (weeklyEventItemMostDMG.Player != null && weeklyEventItemMostDMG.Player.Length > 0){
                                    foreach (KeyValuePair<ulong, DiscordGuild> guild in discGuildslist){
                                        if (guild.Key == Bot.NLBE_SERVER_ID || guild.Key == Bot.DA_BOIS_ID){
                                            await WeHaveAWinner(guild.Value, weeklyEventItemMostDMG, weeklyEventHandler.WeeklyEvent.Tank);
                                            break;
                                        }
                                    }
                                }
                            }
                            await bottestChannel.SendMessageAsync(winnerMessage);
                            await Bot.sendThibeastmo(winnerMessage, string.Empty, string.Empty);
                        }
                        catch (Exception ex){
                            var message = winnerMessage + "\nERROR:\n" + ex.Message;
                            await bottestChannel.SendMessageAsync(message);
                            await Bot.sendThibeastmo(message, string.Empty, string.Empty);
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }
        public static async Task WeHaveAWinner(DiscordGuild guild, WeeklyEventItem weeklyEventItemMostDMG, string tank)
        {
            bool userNotFound = true;
            IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
            if (weeklyEventItemMostDMG.Player != null){
                var weeklyEventItemMostDMGPlayer = weeklyEventItemMostDMG.Player
                    .Replace("\\", string.Empty)
                    .Replace(Bot.UNDERSCORE_REPLACEMENT_CHAR, '_')
                    .ToLower();
                foreach (DiscordMember member in members){
                    if (!member.IsBot){
                        Tuple<string, string> gebruiker = Bot.getIGNFromMember(member.DisplayName);
                        var x = gebruiker.Item2
                            .Replace("\\", string.Empty)
                            .Replace(Bot.UNDERSCORE_REPLACEMENT_CHAR, '_')
                            .ToLower();
                        if (x == weeklyEventItemMostDMGPlayer
                            || (member.Id == Bot.THIBEASTMO_ID
                                && guild.Id == DA_BOIS_ID)){
                            userNotFound = false;
                            weeklyEventWinner = new Tuple<ulong, DateTime>(member.Id, DateTime.Now);
                            try{
                                await member.SendMessageAsync("Hallo " + member.Mention + ",\n\nProficiat! Je hebt het wekelijkse event gewonnen van de **" + tank + "** met **" + weeklyEventItemMostDMG.Value + "** damage.\n" +
                                                              "Dit wilt zeggen dat jij de tank voor het wekelijkse event mag kiezen.\n" +
                                                              "Je kan je keuze maken door enkel de naam van de tank naar mij te sturen. Indien ik de tank niet kan vinden dan zal ik je voorthelpen.\n" +
                                                              "De enige voorwaarde is wel dat je niet een recent gekozen tank opnieuw kiest."
                                                              + "\n\nSucces met je keuze!");
                            }
                            catch (Exception ex){
                                await Bot.handleError("Could not send private message towards winner of weekly event.", ex.Message, ex.StackTrace);
                            }
                            try{
                                DiscordChannel algemeenChannel = await Bot.GetAlgemeenChannel();
                                if (algemeenChannel != null){
                                    await algemeenChannel.SendMessageAsync("Feliciteer **" + weeklyEventItemMostDMG.Player.Replace(Bot.UNDERSCORE_REPLACEMENT_CHAR, '_').adaptToDiscordChat() + "** want hij heeft het wekelijkse event gewonnen! **Proficiat!**" +
                                                                           "\n" +
                                                                           "`" + tank + "` met `" + weeklyEventItemMostDMG.Value + "` damage" +
                                                                           "\n\n" +
                                                                           "We wachten nu af tot de winnaar een nieuwe tank kiest.");
                                }
                            }
                            catch (Exception ex){
                                await Bot.handleError("Could not send message in algemeen channel for weekly event winner announcement.", ex.Message, ex.StackTrace);
                            }
                            break;
                        }
                    }
                }
            }
            else{
                DiscordChannel algemeenChannel = await Bot.GetAlgemeenChannel();
                if (algemeenChannel != null){
                    await algemeenChannel.SendMessageAsync("Het wekelijkse event is gedaan, helaas heeft er __niemand__ deelgenomen en is er dus geen winnaar.");
                }
            }
            DiscordChannel bottestChannel = await GetBottestChannel();
            if (userNotFound){
                var message = "Weekly event winnaar was niet gevonden! Je zal het zelf moeten regelen met het `weekly` commando.";
                if (bottestChannel != null){
                    await bottestChannel.SendMessageAsync(message);
                }
                else{
                    await handleError(message, string.Empty, string.Empty);
                }
            }
            else{
                var message = "Weekly event winnaar gevonden!";
                if (bottestChannel != null){
                    await bottestChannel.SendMessageAsync(message);
                }
                else{
                    await handleError(message, string.Empty, string.Empty);
                }
            }
        }
        private Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Context.Guild.Id.Equals(NLBE_SERVER_ID) || e.Context.Guild.Id.Equals(DA_BOIS_ID)){
                if (e.Exception.Message.ToLower().Contains("unauthorized")){
                    e.Context.Channel.SendMessageAsync("**De bot heeft hier geen rechten voor!**");
                }
                else if (e.Command != null){
                    e.Context.Message.DeleteReactionsEmojiAsync(getDiscordEmoji(IN_PROGRESS_REACTION));
                    e.Context.Message.CreateReactionAsync(getDiscordEmoji(ERROR_REACTION));
                    handleError("Error with command (" + e.Command.Name + "):\n", e.Exception.Message.Replace("`", "'"), e.Exception.StackTrace).Wait();
                }
            }
            return Task.CompletedTask;
        }
        private Task Discord_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            discGuildslist = sender.Guilds;
            foreach (KeyValuePair<ulong, DiscordGuild> guild in discGuildslist){
                if (!guild.Key.Equals(NLBE_SERVER_ID) && !guild.Key.Equals(DA_BOIS_ID)){
                    guild.Value.LeaveAsync();
                }
            }
            discordClient.Logger.Log(LogLevel.Information, "Client( v" + Bot.version + " ) is ready to process events.");

            //SendMessageTomember
            //foreach(KeyValuePair<ulong, DiscordGuild> guild in discGuildslist)
            //{
            //    if (guild.Key.Equals(DA_BOIS_ID))
            //    {
            //        DiscordMember member = guild.Value.GetMemberAsync(296232182127525889).Result;
            //        CreateEmbed(member, string.Empty, string.Empty, "Don't", string.Empty, null, null, null, null, DiscordColor.Brown).Wait();
            //    }
            //}

            //DiscordChannel logChannel = GetLogChannel().Result;
            //if (logChannel != null)
            //{
            //    clearLog();
            //    if (File.Exists(logInputPath))
            //    {
            //        string[] lines = File.ReadAllLines(logInputPath);
            //        for (int i = 0; i < lines.Length; i++)
            //        {
            //            writeInLog(lines[i]);
            //            Thread.Sleep(875);
            //        }
            //    }
            //}


            return Task.CompletedTask;
        }

        private Task Discord_MessageReactionAdded(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    if (!e.User.IsBot){
                        if (e.Guild.Id == NLBE_SERVER_ID || e.Guild.Id == DA_BOIS_ID){
                            //await Task.Delay(875);
                            bool alreadyDone = false;
                            DiscordChannel toernooiAanmeldenChannel = await GetToernooiAanmeldenChannel(e.Guild.Id);
                            if (toernooiAanmeldenChannel != null){
                                if (e.Channel.Equals(toernooiAanmeldenChannel)){
                                    DiscordMessage message = await toernooiAanmeldenChannel.GetMessageAsync(e.Message.Id);
                                    alreadyDone = true;
                                    await generateLogMessage(message, toernooiAanmeldenChannel, e.User.Id, Bot.getDiscordEmoji(e.Emoji.Name));
                                }
                            }
                            if (!alreadyDone){
                                DiscordChannel regelsChannel = await GetRegelsChannel();
                                if (regelsChannel != null){
                                    if (e.Channel.Equals(regelsChannel)){
                                        alreadyDone = true;
                                        string rulesReadEmoji = ":ok:";
                                        if (e.Emoji.GetDiscordName().Equals(rulesReadEmoji)){
                                            var users = await e.Message.GetReactionsAsync(e.Emoji);
                                            List<DiscordUser> reactionUsers = new List<DiscordUser>();
                                            foreach (DiscordUser aUser in users){
                                                if (!aUser.IsBot){
                                                    await e.Message.DeleteReactionAsync(e.Emoji, aUser);
                                                    DiscordMember member = await e.Guild.GetMemberAsync(aUser.Id);
                                                    if (member != null){
                                                        bool hadRulesNotReadrole = false;
                                                        if (member.Roles != null){
                                                            foreach (var memberRole in member.Roles){
                                                                if (memberRole.Id.Equals(Bot.MOET_REGELS_NOG_LEZEN_ROLE)){
                                                                    hadRulesNotReadrole = true;
                                                                    await member.RevokeRoleAsync(memberRole);//een oorzaak
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        if (hadRulesNotReadrole || member.Roles == null || member.Roles.Count() == 0){
                                                            if (member.DisplayName.Split(']').Length > 1){
                                                                if (member.DisplayName.Split(']').Length > 2){
                                                                    string[] splitted = member.DisplayName.Split(']');
                                                                    string tempName = splitted[splitted.Length - 1].Trim(' ');
                                                                    await changeMemberNickname(member, "[] " + tempName);//een oorzaak

                                                                }
                                                                else if (member.DisplayName.Contains("[NLBE]")){
                                                                    await changeMemberNickname(member, "[] " + member.DisplayName.Replace("[NLBE]", string.Empty).Trim(' '));//een oorzaak
                                                                }
                                                                else if (member.DisplayName.Contains("[NLBE2]")){
                                                                    await changeMemberNickname(member, "[] " + member.DisplayName.Replace("[NLBE2]", string.Empty).Trim(' '));//een oorzaak
                                                                }
                                                            }
                                                            else{
                                                                await changeMemberNickname(member, "[] " + member.Username);//een oorzaak
                                                            }
                                                            DiscordRole ledenRole = e.Guild.GetRole(LEDEN_ROLE);
                                                            if (ledenRole != null){
                                                                await member.GrantRoleAsync(ledenRole);//een oorzaak
                                                            }
                                                            DiscordChannel algemeenChannel = await GetAlgemeenChannel();//een oorzaak
                                                            if (algemeenChannel != null){
                                                                await algemeenChannel.SendMessageAsync(e.User.Mention + " , welkom op de NLBE discord server. GLHF!");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else{
                                            var users = await e.Message.GetReactionsAsync(e.Emoji);
                                            foreach (var user in users){
                                                if (!user.IsBot){
                                                    await e.Message.DeleteReactionAsync(e.Emoji, user);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }
        private Task Discord_MessageReactionRemoved(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    if (e.Guild.Id == NLBE_SERVER_ID || e.Guild.Id == DA_BOIS_ID){
                        DiscordChannel toernooiAanmeldenChannel = await GetToernooiAanmeldenChannel(e.Guild.Id);
                        if (toernooiAanmeldenChannel != null){
                            if (e.Channel.Equals(toernooiAanmeldenChannel)){
                                bool removeInLog = true;
                                DiscordMessage message = await toernooiAanmeldenChannel.GetMessageAsync(e.Message.Id);
                                if (message.Author != null){
                                    if (!message.Author.Id.Equals(Bot.NLBE_BOT) && !message.Author.Id.Equals(Bot.TESTBEASTV2_BOT)){
                                        removeInLog = false;
                                    }
                                }
                                if (removeInLog){
                                    //check if reaction has to be added by bot (from 0 reactions to 1 reaction)
                                    var users = await e.Message.GetReactionsAsync(e.Emoji);
                                    if (users == null || users.Count == 0){
                                        await e.Message.CreateReactionAsync(e.Emoji);
                                    }

                                    //remove in log
                                    DiscordChannel logchannel = await GetLogChannel(e.Guild.Id);
                                    if (logchannel != null){
                                        Dictionary<DateTime, List<DiscordMessage>> sortedMessages = sortMessages(await logchannel.GetMessagesAsync(100));
                                        foreach (KeyValuePair<DateTime, List<DiscordMessage>> messageList in sortedMessages){
                                            try{
                                                if (compareDateTime(e.Message.CreationTimestamp.LocalDateTime, messageList.Key)){
                                                    foreach (DiscordMessage aMessage in messageList.Value){
                                                        DiscordMember member = await getDiscordMember(e.Guild, e.User.Id);
                                                        if (member != null){
                                                            string[] splitted = aMessage.Content.Split(Bot.LOG_SPLIT_CHAR);
                                                            string theEmoji = getEmojiAsString(e.Emoji.Name);
                                                            if (splitted[2].Replace("\\", string.Empty).ToLower().Equals(member.DisplayName.ToLower()) && getEmojiAsString(splitted[3]).Equals(theEmoji)){
                                                                await aMessage.DeleteAsync("Log updated: reaction was removed from message in Toernooi-aanmelden for this user.");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex){
                                                await handleError("Could not compare TimeStamps in MessageReactionRemoved:", ex.Message, ex.StackTrace);
                                            }
                                        }
                                    }
                                    else{
                                        await handleError("Could not find log channel at MessageReactionRemoved!", string.Empty, string.Empty);
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }
        private Task Discord_MessageDeleted(DiscordClient sender, DSharpPlus.EventArgs.MessageDeleteEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    DiscordChannel toernooiAanmeldenChannel = await GetToernooiAanmeldenChannel(e.Guild.Id);
                    if (e.Channel.Equals(toernooiAanmeldenChannel)){
                        DateTime timeStamp = e.Message.Timestamp.LocalDateTime;
                        DiscordChannel logChannel = await GetLogChannel(e.Guild.Id);
                        if (logChannel != null){
                            var messages = await logChannel.GetMessagesAsync(100);
                            foreach (DiscordMessage message in messages){
                                string[] splitted = message.Content.Split('|');
                                DateTime tempDateTime = new DateTime();
                                bool isDateTime = true;
                                try{
                                    tempDateTime = Convert.ToDateTime(splitted[0]);
                                }
                                catch{
                                    isDateTime = false;
                                }
                                if (isDateTime){
                                    if (compareDateTime(tempDateTime, timeStamp)){
                                        await message.DeleteAsync();
                                        await Task.Delay(875);
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task Discord_GuildMemberAdded(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    if (e.Guild.Id == NLBE_SERVER_ID){
                        DiscordRole noobRole = e.Guild.GetRole(NOOB_ROLE);
                        if (noobRole != null){
                            e.Member.GrantRoleAsync(noobRole).Wait();
                            DiscordChannel welkomChannel = GetWelkomChannel().Result;
                            if (welkomChannel != null){
                                DiscordChannel regelsChannel = GetRegelsChannel().Result;
                                welkomChannel.SendMessageAsync(e.Member.Mention + " welkom op de NLBE discord server. Beantwoord eerst de vraag en lees daarna de " + (regelsChannel != null ? regelsChannel.Mention : "#regels") + " aub.").Wait();
                                DiscordGuild guild = Bot.getGuild(e.Guild.Id).Result;
                                if (guild != null){
                                    DiscordUser user = Bot.discordClient.GetUserAsync(e.Member.Id).Result;
                                    if (user != null){
                                        //IWGResponse<List<WGClient.Entities.Account.AccountBase>> searchResults = null;
                                        //List<IWGResponse<WGClient.WOTB.Entities.Clans.ClanAccountInfo>> basicInfoList = new List<IWGResponse<WGClient.WOTB.Entities.Clans.ClanAccountInfo>>();
                                        IReadOnlyList<WGAccount> searchResults = new List<WGAccount>();
                                        bool resultFound = false;
                                        StringBuilder sbDescription = new StringBuilder();
                                        int counter = 0;
                                        bool firstTime = true;
                                        while (!resultFound){
                                            string question = user.Mention + " Wat is je gebruikersnaam van je wargaming account?";
                                            if (firstTime){
                                                firstTime = false;
                                            }
                                            else{
                                                question = "**We konden dit Wargamingaccount niet vinden, probeer opnieuw! (Hoofdlettergevoelig)**\n" + question;
                                            }
                                            string ign = Bot.askQuestion(await Bot.GetWelkomChannel(), user, guild, question).Result;
                                            searchResults = await WGAccount.searchByName(SearchAccuracy.EXACT, ign, WG_APPLICATION_ID, false, true, false);
                                            if (searchResults != null){
                                                if (searchResults != null){
                                                    if (searchResults.Count > 0){
                                                        resultFound = true;
                                                        foreach (WGAccount tempAccount in searchResults){
                                                            string tempClanName = string.Empty;
                                                            if (tempAccount.clan != null){
                                                                tempClanName = tempAccount.clan.tag;
                                                            }
                                                            try{
                                                                sbDescription.AppendLine(++counter + ". " + tempAccount.nickname + " " + (tempClanName.Length > 0 ? '`' + tempClanName + '`' : string.Empty));
                                                            }
                                                            catch (Exception e){
                                                                discordClient.Logger.LogWarning("Error while looking for basicInfo for " + ign + ":\n" + e.StackTrace);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        resultFound = false;

                                        int selectedAccount = 0;
                                        if (searchResults.Count > 1){
                                            selectedAccount = -1;
                                            while (selectedAccount == -1){ await waitForReply(welkomChannel, user, sbDescription.ToString(), counter); }
                                        }
                                        //WGClient.WOTB.Entities.Clans.ClanAccountInfo account = basicInfoList[selectedAccount].Data;
                                        WGAccount account = searchResults[selectedAccount];

                                        string clanName = string.Empty;
                                        if (account.clan != null){
                                            if (account.clan.tag != null){
                                                if (account.clan.clan_id.Equals(NLBE_CLAN_ID) || account.clan.clan_id.Equals(NLBE2_CLAN_ID)){
                                                    await e.Member.SendMessageAsync("Indien je echt van **" + account.clan.tag + "** bent dan moet je even vragen of iemand jouw de **" + account.clan.tag + "** rol wilt geven.");
                                                    ;
                                                }
                                                else{
                                                    clanName = account.clan.tag;
                                                }
                                            }
                                        }
                                        Bot.changeMemberNickname(e.Member, "[" + clanName + "] " + account.nickname).Wait();
                                        await e.Member.SendMessageAsync("We zijn er bijna. Als je nog even de regels wilt lezen in **#regels** dan zijn we klaar.");
                                        DiscordRole rulesNotReadRole = e.Guild.GetRole(MOET_REGELS_NOG_LEZEN_ROLE);
                                        if (rulesNotReadRole != null){
                                            e.Member.RevokeRoleAsync(noobRole).Wait();
                                            e.Member.GrantRoleAsync(rulesNotReadRole).Wait();
                                        }
                                        IReadOnlyCollection<DiscordMember> allMembers = await e.Guild.GetAllMembersAsync();
                                        bool atLeastOneOtherPlayerWithNoobRole = false;
                                        foreach (DiscordMember aMember in allMembers){
                                            if (aMember.Roles.Contains(noobRole)){
                                                atLeastOneOtherPlayerWithNoobRole = true;
                                                break;
                                            }
                                        }
                                        if (atLeastOneOtherPlayerWithNoobRole){
                                            await Bot.cleanWelkomChannel(e.Member.Id);
                                        }
                                        else{
                                            await Bot.cleanWelkomChannel();
                                        }
                                    }
                                }
                            }
                        }
                        else{
                            await handleError("Could not grant new member[" + e.Member.DisplayName + " (" + e.Member.Username + "#" + e.Member.Discriminator + ")] the Noob role.", string.Empty, string.Empty);
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }
        private Task Discord_GuildMemberRemoved(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberRemoveEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    if (!e.Member.Id.Equals(THIBEASTMO_ALT_ID)){
                        if (e.Guild.Id.Equals(NLBE_SERVER_ID)){
                            DiscordChannel oudLedenChannel = await GetOudLedenChannel();
                            if (oudLedenChannel != null){
                                IReadOnlyDictionary<ulong, DiscordRole> serverRoles = null;
                                foreach (var guild in discGuildslist){
                                    if (guild.Value.Id.Equals(NLBE_SERVER_ID)){
                                        serverRoles = guild.Value.Roles;
                                    }
                                }
                                if (serverRoles != null){
                                    if (serverRoles.Count > 0){
                                        var memberRoles = e.Member.Roles;
                                        StringBuilder sbRoles = new StringBuilder();
                                        bool firstRole = true;
                                        foreach (var role in memberRoles){
                                            if (serverRoles != null){
                                                foreach (var serverRole in serverRoles){
                                                    if (serverRole.Key.Equals(role.Id)){
                                                        if (role.Id.Equals(NOOB_ROLE)){
                                                            await cleanWelkomChannel();
                                                        }
                                                        if (firstRole){
                                                            firstRole = false;
                                                        }
                                                        else{
                                                            sbRoles.Append(", ");
                                                        }
                                                        sbRoles.Append(role.Name);
                                                    }
                                                }
                                            }
                                        }
                                        List<DEF> defList = new List<DEF>();
                                        DEF newDef1 = new DEF();
                                        newDef1.Inline = true;
                                        newDef1.Name = "Bijnaam:";
                                        newDef1.Value = e.Member.DisplayName;
                                        defList.Add(newDef1);
                                        DEF newDef2 = new DEF();
                                        newDef2.Inline = true;
                                        newDef2.Name = "Gebruiker:";
                                        newDef2.Value = e.Member.Username + "#" + e.Member.Discriminator;
                                        defList.Add(newDef2);
                                        DEF newDef3 = new DEF();
                                        newDef3.Inline = true;
                                        newDef3.Name = "GebruikersID:";
                                        newDef3.Value = e.Member.Id.ToString();
                                        defList.Add(newDef3);
                                        if (sbRoles.Length > 0){
                                            DEF newDef = new DEF();
                                            newDef.Inline = true;
                                            newDef.Name = "Rollen:";
                                            newDef.Value = sbRoles.ToString();
                                            defList.Add(newDef);
                                        }
                                        await CreateEmbed(oudLedenChannel, string.Empty, string.Empty, e.Member.Username + " heeft de server verlaten", string.Empty, defList, null, string.Empty, null);
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }
        private Task Discord_GuildMemberUpdated(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberUpdateEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    foreach (var guild in discGuildslist){
                        if (guild.Key.Equals(NLBE_SERVER_ID)){
                            DiscordMember member = await getDiscordMember(guild.Value, e.Member.Id);
                            if (member != null){
                                var userRoles = member.Roles;
                                bool isNoob = false;
                                bool hasRoles = false;
                                foreach (var role in userRoles){
                                    hasRoles = true;
                                    if (role.Id.Equals(NOOB_ROLE)){
                                        isNoob = true;
                                        break;
                                    }
                                }
                                if (!isNoob && hasRoles){
                                    if (e.RolesAfter != null){
                                        if (member != null){
                                            string editedName = updateName(member, member.DisplayName);
                                            if (!editedName.Equals(member.DisplayName) && !editedName.Equals(string.Empty)){
                                                await changeMemberNickname(member, editedName);
                                            }
                                        }
                                    }
                                    if (e.NicknameAfter != null){
                                        if (e.NicknameAfter != string.Empty){
                                            if (member != null){
                                                string editedName = updateName(member, member.DisplayName);
                                                if (!editedName.Equals(member.DisplayName) && !editedName.Equals(string.Empty)){
                                                    await changeMemberNickname(member, editedName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            _ = Task.Run(async () => {
                if (!ignoreEvents){
                    if (false && e.Message.Content.StartsWith(Prefix)) 
                    // VOOR IN HET GEVAL DAT OOIT JE COMMANDS MOET MAKEN OP BASIS VAN DE MESSAGE CREATED EVENT
                    {
                        // Remove the prefix and split the message into command and arguments
                        var content = e.Message.Content[Prefix.Length..];
                        var parts = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 0) return;

                        // Extract the command name and arguments
                        var commandName = parts[0];
                        var args = content.Substring(Prefix.Length, content.Length - Prefix.Length);



                        var commandsNext = sender.GetCommandsNext();
                        if (commandsNext == null)
                        {
                            discordClient.Logger.Log(LogLevel.Information, "CommandsNext is not enabled.");
                            return;
                        }

                        var command = commandsNext.FindCommand(commandName, out var rawArguments);
                        if (command == null)
                        {
                            discordClient.Logger.Log(LogLevel.Information, "Unknown command.");
                            return;
                        }

                        var ctx = commandsNext.CreateContext(e.Message, Prefix, command, args);

                        try
                        {
                            // Execute the command (this will internally create and manage a CommandContext)
                            await commandsNext.ExecuteCommandAsync(ctx);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle any errors that occur during command execution
                            Console.WriteLine();
                            discordClient.Logger.Log(LogLevel.Error, $"Error executing command: {ex.Message}");
                        }

                        await commandsNext.ExecuteCommandAsync(ctx);

                        // Handle the command
                        //await HandleCommand(e, commandName, args);
                        return;
                    }
                        if (!e.Author.IsBot && e.Channel.Guild != null){
                        bool validChannel = false;
                        DiscordChannel masteryChannel = await Bot.GetMasteryReplaysChannel(e.Guild.Id);
                        if (masteryChannel != null){
                            if (masteryChannel.Equals(e.Channel) || e.Channel.Id.Equals(BOTTEST_ID)){
                                validChannel = true;
                            }
                        }
                        if (!validChannel){
                            masteryChannel = await Bot.GetBottestChannel();
                            if (masteryChannel != null){
                                if (masteryChannel.Equals(e.Channel)){
                                    validChannel = true;
                                }
                            }
                            if (!validChannel){
                                masteryChannel = await Bot.GetReplayResultsChannel();
                                if (masteryChannel != null){
                                    if (masteryChannel.Equals(e.Channel)){
                                        validChannel = true;
                                    }
                                }
                                if (!validChannel){
                                    masteryChannel = await Bot.GetReplayResultsChannel();
                                    if (masteryChannel != null){
                                        if (masteryChannel.Equals(e.Channel)){
                                            validChannel = true;
                                        }
                                    }
                                    if (!validChannel){
                                        masteryChannel = await Bot.GetTestChannel();
                                        if (masteryChannel != null){
                                            if (masteryChannel.Equals(e.Channel)){
                                                validChannel = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (validChannel){
                            Bot.discordMessage = e.Message;
                            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
                            if (e.Channel.Id.Equals(PRIVE_ID) || member.Roles.Contains(e.Guild.GetRole(NLBE_ROLE)) && (e.Channel.Id.Equals(MASTERY_REPLAYS_ID) || e.Channel.Id.Equals(BOTTEST_ID)) || member.Roles.Contains(e.Guild.GetRole(NLBE2_ROLE)) && (e.Channel.Id.Equals(MASTERY_REPLAYS_ID) || e.Channel.Id.Equals(BOTTEST_ID))){
                                //MasteryChannel (komt wel in HOF)
                                if (e.Message.Attachments.Count > 0){
                                    foreach (DiscordAttachment attachment in e.Message.Attachments){
                                        if (attachment.FileName.EndsWith(".wotbreplay")){
                                            Tuple<string, DiscordMessage> returnedTuple = await Bot.handle(string.Empty, e.Channel, await e.Guild.GetMemberAsync(e.Author.Id), e.Guild.Name, e.Guild.Id, attachment);
                                            await Bot.hofAfterUpload(returnedTuple, e.Message);
                                            break;
                                        }
                                    }
                                }
                                else{
                                    if (e.Message != null){
                                        if (e.Message.Content.StartsWith("http") && e.Message.Content.Contains("wotinspector")){
                                            string[] splitted = e.Message.Content.Split(' ');
                                            string url = splitted[0];
                                            Tuple<string, DiscordMessage> returnedTuple = await Bot.handle(string.Empty, e.Channel, await e.Guild.GetMemberAsync(e.Author.Id), e.Guild.Name, e.Guild.Id, url);
                                            await Bot.hofAfterUpload(returnedTuple, e.Message);
                                        }
                                    }
                                }
                            }
                            else{
                                //ReplayResults die niet in HOF komen
                                WGBattle replayInfo = new WGBattle(string.Empty);
                                bool wasReplay = false;
                                if (e.Message.Attachments.Count > 0){
                                    foreach (DiscordAttachment attachment in e.Message.Attachments){
                                        if (attachment.FileName.EndsWith(".wotbreplay")){
                                            await Bot.confirmCommandExecuting(e.Message);
                                            wasReplay = true;
                                            replayInfo = await Bot.getReplayInfo(string.Empty, attachment, Bot.getIGNFromMember(member.DisplayName).Item2, null);
                                        }
                                    }
                                }
                                else{
                                    if (e.Message != null){
                                        if (e.Message.Content.StartsWith("http") && e.Message.Content.Contains("wotinspector")){
                                            await Bot.confirmCommandExecuting(e.Message);
                                            wasReplay = true;
                                            replayInfo = await Bot.getReplayInfo(string.Empty, null, Bot.getIGNFromMember(member.DisplayName).Item2, e.Message.Content);
                                        }
                                    }
                                }
                                if (wasReplay && replayInfo != null){
                                    string thumbnail = string.Empty;
                                    string eventDescription = string.Empty;
                                    try{
                                        WeeklyEventHandler weeklyEventHandler = new WeeklyEventHandler();
                                        eventDescription = await weeklyEventHandler.GetStringForWeeklyEvent(replayInfo);
                                    }
                                    catch (Exception ex){
                                        await handleError("Tijdens het nakijken van het wekelijkse event: ", ex.Message, ex.StackTrace);
                                    }
                                    List<Tuple<string, string>> images = await Bot.getAllMaps(e.Guild.Id);
                                    foreach (Tuple<string, string> map in images){
                                        if (replayInfo.map_name.ToLower().Contains(map.Item1.ToLower())){
                                            try{
                                                if (map.Item1 != string.Empty){
                                                    thumbnail = map.Item2;
                                                }
                                            }
                                            catch (Exception ex){
                                                await handleError("Could not set thumbnail for embed:", ex.Message, ex.StackTrace);
                                            }
                                            break;
                                        }
                                    }
                                    await Bot.CreateEmbed(e.Channel, thumbnail, string.Empty, "Resultaat", await Bot.getDescriptionForReplay(replayInfo, -1, eventDescription), null, null, string.Empty, null, true);
                                    await Bot.confirmCommandExecuted(e.Message);
                                }
                                else if (wasReplay){
                                    await e.Message.DeleteReactionsEmojiAsync(getDiscordEmoji(IN_PROGRESS_REACTION));
                                    await e.Message.CreateReactionAsync(getDiscordEmoji(ERROR_REACTION));
                                }
                            }
                        }
                        Bot.discordMessage = null;
                    }
                    else if (e.Channel.IsPrivate){
                        HandleWeeklyEventDM(e.Channel, e.Message);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private async Task HandleCommand(MessageCreateEventArgs e, string commandName, string[] args)
        {
            switch (commandName.ToLower())
            {
                case "ping":
                    await e.Message.RespondAsync("Pong!");
                    break;

                case "echo":
                    if (args.Length > 0)
                    {
                        var message = string.Join(" ", args);
                        await e.Message.RespondAsync(message);
                    }
                    else
                    {
                        await e.Message.RespondAsync("You need to provide a message to echo!");
                    }
                    break;

                default:
                    await e.Message.RespondAsync("Unknown command. Use `!help` for a list of commands.");
                    break;
            }
        }


        private void HandleWeeklyEventDM(DiscordChannel Channel, DiscordMessage lastMessage)
        {
            if (!ignoreEvents && Channel.IsPrivate && weeklyEventWinner != null && weeklyEventWinner.Item1 != 0){
                _ = Task.Run(async () => {
                    if (!lastMessage.Author.IsBot && Channel.Guild == null && lastMessage.CreationTimestamp > weeklyEventWinner.Item2){
                        string vehiclesInString = await WGVehicle.vehiclesToString(Bot.WG_APPLICATION_ID, new List<string>() { "name" });
                        Json json = new Json(vehiclesInString, string.Empty);
                        List<Json> jsons = json.subJsons[1].subJsons;
                        List<string> tanks = new List<string>();
                        foreach (var item in jsons){
                            tanks.Add(item.tupleList[0].Item2.Item1.Trim('"').Replace("\\", string.Empty));
                        }
                        json = null;
                        string chosenTank = tanks.Find(tank => tank == lastMessage.Content);
                        if (chosenTank == null || chosenTank.Length == 0){
                            //specifieker vragen
                            tanks.Sort();
                            IEnumerable<string> containsStringList = tanks.Where(tank => tank.ToLower().Contains(lastMessage.Content.ToLower()));
                            if (containsStringList.Count() > 20){
                                await Channel.SendMessageAsync("Wees iets specifieker want er werden te veel resultaten gevonden!");
                            }
                            else if (containsStringList.Count() == 0){
                                await Channel.SendMessageAsync("Die tank kon niet gevonden worden! Zoekterm: `" + lastMessage.Content + "`");
                            }
                            else{
                                StringBuilder sb = new StringBuilder("```");
                                sb.Append(Environment.NewLine);
                                foreach (string tank in containsStringList){
                                    sb.Append(tank + Environment.NewLine);
                                }
                                sb.AppendLine("```");
                                await Channel.SendMessageAsync("Deze tanks bevatten je zoekterm. **Kopieer** de naam van de tank en stuur hem naar mij door om zo de juiste te selecteren. (**Hoofdlettergevoelig**):");
                                await Channel.SendMessageAsync(sb.ToString());
                            }
                        }
                        else{
                            //tank was chosen
                            await Channel.SendMessageAsync("Je hebt de **" + chosenTank + "** geselecteerd. Goede keuze!\nIk zal hem onmiddelijk instellen als nieuwe tank voor het wekelijks event.");
                            WeeklyEventHandler weeklyEventHandler = new WeeklyEventHandler();
                            await weeklyEventHandler.CreateNewWeeklyEvent(chosenTank, await Bot.GetWeeklyEventChannel());
                            weeklyEventWinner = new Tuple<ulong, DateTime>(0, DateTime.Now);//dit vermijdt dat deze event telkens opnieuw zal opgeroepen worden + dat anderen het zomaar kunnen aanpassen
                        }
                    }
                });
            }
        }

        #endregion

        #region getChannel

        public static async Task<DiscordChannel> GetHallOfFameChannel(ulong GuildID)
        {
            ulong ChatID = 0;
            if (GuildID.Equals(NLBE_SERVER_ID)){
                ChatID = 793268894454251570;
            }
            else{
                ChatID = 793429499403304960;
            }
            return await getChannel(GuildID, ChatID);
        }
        public static async Task<DiscordChannel> GetMasteryReplaysChannel(ulong GuildID)
        {
            ulong ChatID = 0;
            if (GuildID.Equals(NLBE_SERVER_ID)){
                ChatID = MASTERY_REPLAYS_ID;
            }
            else{
                ChatID = PRIVE_ID;
            }
            return await getChannel(GuildID, ChatID);
        }
        public static async Task<DiscordChannel> GetReplayResultsChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 583958593129414677;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetWeeklyEventChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 897749692895596565;
            if (version.ToLower().Contains("local")){
                ServerID = Bot.DA_BOIS_ID;
                ChatID = 901480697011777538;
            }
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetAlgemeenChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 507575682046492692;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetOudLedenChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 744462244951228507;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetDeputiesChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 668211371522916389;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetWelkomChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 681960256296976405;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetRegelsChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 679531304882012165;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetLogChannel(ulong GuildID)
        {
            if (GuildID == Bot.NLBE_SERVER_ID){
                return await getChannel(GuildID, 782308602882031660);
            }
            else{
                return await getChannel(GuildID, 808319637447376899);
            }
        }
        public static async Task<DiscordChannel> GetToernooiAanmeldenChannel(ulong GuildID)
        {
            if (GuildID == Bot.NLBE_SERVER_ID){
                return await getChannel(GuildID, Bot.NLBE_TOERNOOI_AANMELDEN_KANAAL_ID);
            }
            else{
                return await getChannel(GuildID, Bot.DA_BOIS_TOERNOOI_AANMELDEN_KANAAL_ID);
            }
        }
        public static async Task<DiscordChannel> GetMappenChannel(ulong GuildID)
        {
            ulong ChatID = 0;
            if (GuildID.Equals(NLBE_SERVER_ID)){
                ChatID = 782240999190953984;
            }
            else{
                ChatID = 804856157918855209;
            }
            return await getChannel(GuildID, ChatID);
        }
        public static async Task<DiscordChannel> GetBottestChannel()
        {
            ulong ServerID = NLBE_SERVER_ID;
            ulong ChatID = 781617141069774898;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetTestChannel()
        {
            ulong ServerID = DA_BOIS_ID;
            ulong ChatID = 804477788676685874;
            return await getChannel(ServerID, ChatID);
        }
        public static async Task<DiscordChannel> GetPollsChannel(bool isDeputyPoll, ulong GuildID)
        {
            if (GuildID == NLBE_SERVER_ID){
                ulong ChatID = 0;
                if (isDeputyPoll){
                    ChatID = 805800443178909756;
                }
                else{
                    ChatID = 781522161159897119;
                }
                return await getChannel(GuildID, ChatID);
            }
            else{
                return await GetTestChannel();
            }
        }
        private static async Task<DiscordChannel> getChannel(ulong serverID, ulong chatID)
        {
            try{
                DiscordGuild guild = await Bot.discordClient.GetGuildAsync(serverID);
                if (guild != null){
                    return guild.GetChannel(chatID);
                }
            }
            catch{

            }
            return null;
        }
        private static async Task<DiscordGuild> getGuild(ulong serverID)
        {
            return await Bot.discordClient.GetGuildAsync(serverID);
        }
        public static async Task<DiscordChannel> getChannelBasedOnString(string guildNameOrTag, ulong guildID)
        {
            bool isId = false;
            if (guildNameOrTag.StartsWith('<')){
                isId = true;
                guildNameOrTag = guildNameOrTag.TrimStart('<');
                guildNameOrTag = guildNameOrTag.TrimStart('#');
                guildNameOrTag = guildNameOrTag.TrimEnd('>');
            }
            DiscordGuild guild = await getGuild(guildID);
            if (guild != null){
                foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Channels){
                    if (isId){
                        if (channel.Value.Id.ToString().Equals(guildNameOrTag.ToLower())){
                            return channel.Value;
                        }
                    }
                    else{
                        if (channel.Value.Name.ToLower().Equals(guildNameOrTag.ToLower())){
                            return channel.Value;
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        public static async Task updateUsers()
        {
            DiscordChannel bottestChannel = await Bot.GetBottestChannel();
            if (bottestChannel != null){
                bool sjtubbersUserNameIsOk = true;
                DiscordMember sjtubbersMember = await bottestChannel.Guild.GetMemberAsync(359817512109604874);
                if (sjtubbersMember.DisplayName != "[NLBE] sjtubbers") sjtubbersUserNameIsOk = false;
                if (!sjtubbersUserNameIsOk){
                    await Bot.SendMessage(bottestChannel, await bottestChannel.Guild.GetMemberAsync(THIBEASTMO_ID), bottestChannel.Guild.Name, "**De bijnaam van sjtubbers was al incorrect dus ben ik gestopt voor ik begon met nakijken van de rest van de bijnamen.**");
                    return;
                }
                const int maxMemberChangesAmount = 7;
                IReadOnlyCollection<DiscordMember> members = await bottestChannel.Guild.GetAllMembersAsync();
                var memberChanges = new Dictionary<DiscordMember, string>();
                var membersNotFound = new List<DiscordMember>();
                var correctNicknamesOfMembers = new List<DiscordMember>();
                //test 3x if names are correct
                for (int i = 0; i < 3; i++){
                    foreach (DiscordMember member in members){
                        if ((memberChanges.Count + membersNotFound.Count) >= maxMemberChangesAmount) break;
                        if (!member.IsBot){
                            if (member.Roles != null){
                                if (member.Roles.Contains(bottestChannel.Guild.GetRole(Bot.LEDEN_ROLE))){
                                    bool accountFound = false;
                                    bool goodClanTag = false;
                                    Tuple<string, string> gebruiker = Bot.getIGNFromMember(member.DisplayName);
                                    IReadOnlyList<WGAccount> wgAccounts = await WGAccount.searchByName(SearchAccuracy.EXACT, gebruiker.Item2, Bot.WG_APPLICATION_ID, false, true, false);
                                    if (wgAccounts != null && wgAccounts.Count > 0){
                                        //Account met exact deze gebruikersnaam gevonden
                                        accountFound = true;
                                        string clanTag = string.Empty;
                                        if (gebruiker.Item1.Length > 1){
                                            if (gebruiker.Item1.StartsWith('[') && gebruiker.Item1.EndsWith(']')){
                                                goodClanTag = true;
                                                string currentClanTag = string.Empty;
                                                if (wgAccounts[0].clan != null){
                                                    if (wgAccounts[0].clan.tag != null){
                                                        currentClanTag = wgAccounts[0].clan.tag;
                                                    }
                                                }
                                                string goodDisplayName = '[' + currentClanTag + "] " + wgAccounts[0].nickname;
                                                if (wgAccounts[0].nickname != null && !member.DisplayName.Equals(goodDisplayName)){
                                                    if (!memberChanges.Keys.Contains(member)){
                                                        memberChanges.Add(member, goodDisplayName);
                                                    }
                                                }
                                                else if (member.DisplayName.Equals(goodDisplayName)){
                                                    correctNicknamesOfMembers.Add(member);
                                                }
                                            }
                                        }
                                        if (!goodClanTag){
                                            if (wgAccounts[0].clan != null){
                                                if (wgAccounts[0].clan.tag != null){
                                                    clanTag = wgAccounts[0].clan.tag;
                                                }
                                            }
                                            string goodDisplayName = '[' + clanTag + "] " + wgAccounts[0].nickname;
                                            if (!memberChanges.Keys.Contains(member)){
                                                memberChanges.Add(member, goodDisplayName);
                                            }
                                        }
                                    }
                                    if (!accountFound){
                                        membersNotFound.Add(member);
                                    }
                                }
                            }
                        }
                    }
                }
                var correctNicknames = correctNicknamesOfMembers.Distinct();
                for (int i = 0; i < memberChanges.Count; i++){
                    var currentMember = memberChanges.Keys.ElementAt(i);
                    if (correctNicknames.Contains(currentMember)){
                        memberChanges.Remove(currentMember);
                        i--;
                    }
                }
                for (int i = 0; i < membersNotFound.Count; i++){
                    var currentMember = membersNotFound.ElementAt(i);
                    if (correctNicknames.Contains(currentMember)){
                        memberChanges.Remove(currentMember);
                        i--;
                    }
                }
                if (memberChanges.Count + membersNotFound.Count == 0){
                    string bericht = "Bijnamen van gebruikers nagekeken maar geen namen moesten aangepast worden.";
                    await bottestChannel.SendMessageAsync("**" + bericht + "**");
                    discordClient.Logger.LogInformation(bericht);
                }
                else if (memberChanges.Count + membersNotFound.Count < maxMemberChangesAmount){
                    foreach (var memberChange in memberChanges){
                        await Bot.SendMessage(bottestChannel, await bottestChannel.Guild.GetMemberAsync(THIBEASTMO_ID), bottestChannel.Guild.Name, "**Gaat bijnaam van **`" + memberChange.Key.DisplayName + "`** aanpassen naar **`" + memberChange.Value + "`");
                        await Bot.changeMemberNickname(memberChange.Key, memberChange.Value);
                    }
                    foreach (var memberNotFound in membersNotFound){
                        await Bot.SendMessage(bottestChannel, await bottestChannel.Guild.GetMemberAsync(THIBEASTMO_ID), bottestChannel.Guild.Name, "**Bijnaam van **`" + memberNotFound.DisplayName + "` (Discord ID: `" + memberNotFound.Id + "`)** komt niet overeen met WoTB account.**");
                        await Bot.SendPrivateMessage(memberNotFound, bottestChannel.Guild.Name, "Hallo,\n\nEr werd voor iedere gebruiker in de NLBE discord server gecontroleerd of je bijnaam overeenkomt met je wargaming account.\nHelaas is dit voor jou niet het geval.\nZou je dit zelf even willen aanpassen aub?\nPas je bijnaam aan naargelang de vereisten het #regels kanaal.\n\nAlvast bedankt!\n- [NLBE] sjtubbers#4241");
                    }
                }
                else{
                    StringBuilder sb = new StringBuilder("Deze spelers hadden een verkeerde bijnaam:\n```");
                    if (memberChanges.Count > 0){
                        foreach (var memberChange in memberChanges){
                            sb.AppendLine(memberChange.Key.DisplayName + " -> " + memberChange.Value);
                        }
                        sb.Append("```");
                    }
                    if (membersNotFound.Count > 0){
                        if (sb.Length > 3){
                            sb.Append("Deze spelers konden niet gevonden worden:\n```");
                        }
                        foreach (var memberNotFound in membersNotFound){
                            sb.AppendLine(memberNotFound.DisplayName);
                        }
                        sb.Append("```");
                    }
                    await Bot.SendMessage(bottestChannel, await bottestChannel.Guild.GetMemberAsync(THIBEASTMO_ID), bottestChannel.Guild.Name, "**De bijnamen van 7 of meer spelers waren incorrect of niet gevonden dus ben ik gestopt voor ik begon met nakijken van de rest van de bijnamen.\nHier is een lijstje van aanpassingen die zouden gemaakt zijn:**\n" + sb);
                }
            }
        }

        public static async Task generateLogMessage(DiscordMessage message, DiscordChannel toernooiAanmeldenChannel, ulong userID, string emojiAsEmoji)
        {
            bool addInLog = true;
            if (message.Author != null){
                if (!message.Author.Id.Equals(Bot.NLBE_BOT) && !message.Author.Id.Equals(Bot.TESTBEASTV2_BOT)){
                    addInLog = false;
                }
            }
            if (addInLog){
                if (Emoj.getIndex(Bot.getEmojiAsString(emojiAsEmoji)) > 0){
                    try{
                        bool botReactedWithThisEmoji = false;
                        IReadOnlyList<DiscordUser> userListOfThisEmoji = await message.GetReactionsAsync(Bot.getDiscordEmoji(emojiAsEmoji));
                        foreach (DiscordUser user in userListOfThisEmoji){
                            if (user.Id.Equals(Bot.NLBE_BOT) || user.Id.Equals(Bot.TESTBEASTV2_BOT)){
                                botReactedWithThisEmoji = true;
                            }
                        }
                        if (botReactedWithThisEmoji){
                            DiscordMember member = await toernooiAanmeldenChannel.Guild.GetMemberAsync(userID);
                            if (member != null){
                                string organisator = await getOrganisator(await toernooiAanmeldenChannel.GetMessageAsync(message.Id));
                                string logMessage = "Teams|" + member.DisplayName.adaptToDiscordChat() + "|" + emojiAsEmoji + "|" + organisator + "|" + userID;
                                await writeInLog(toernooiAanmeldenChannel.Guild.Id, convertToDate(message.Timestamp.LocalDateTime), logMessage);
                            }
                        }
                        else{
                            await message.DeleteReactionsEmojiAsync(Bot.getDiscordEmoji(emojiAsEmoji));
                        }
                    }
                    catch (Exception ex){
                        await handleError("While adding to log: ", ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        public static async Task<string> askQuestion(DiscordChannel channel, DiscordUser user, DiscordGuild guild, string question)
        {
            if (channel != null){
                try{
                    await channel.SendMessageAsync(question);
                    InteractivityResult<DiscordMessage> message = await channel.GetNextMessageAsync(user, TimeSpan.FromDays(newPlayerWaitTime));
                    //var interactivity = Bot.discordClient.GetInteractivity();
                    //InteractivityResult<DiscordMessage> message = await interactivity.WaitForMessageAsync(x => x.Channel == channel && x.Author == user);
                    if (!message.TimedOut){
                        return message.Result.Content;
                    }
                    else{
                        await Bot.SayNoResponse(channel);
                        DiscordMember member = await guild.GetMemberAsync(user.Id);
                        if (member != null){
                            try{
                                await member.RemoveAsync("[New member] No answer");
                            }
                            catch{
                                bool isBanned = false;
                                try{
                                    await guild.BanMemberAsync(member);
                                    isBanned = true;
                                }
                                catch{
                                    Bot.discordClient.Logger.LogWarning(member.DisplayName + "(" + member.Username + "#" + member.Discriminator + ") could not be kicked from the server!");
                                }
                                if (isBanned){
                                    try{
                                        await guild.UnbanMemberAsync(user);
                                    }
                                    catch{
                                        try{
                                            await user.UnbanAsync(guild);
                                        }
                                        catch{
                                            Bot.discordClient.Logger.LogWarning(member.DisplayName + "(" + member.Username + "#" + member.Discriminator + ") could not be unbanned from the server!");
                                            DiscordMember thibeastmo = await guild.GetMemberAsync(THIBEASTMO_ID);
                                            if (thibeastmo != null){
                                                await thibeastmo.SendMessageAsync("**Gebruiker [" + member.DisplayName + "(" + member.Username + "#" + member.Discriminator + ")] kon niet geünbanned worden!**");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        await cleanChannel(guild.Id, channel.Id);
                    }
                }
                catch (Exception e){
                    Console.WriteLine("goOverAllQuestions:\n" + e.StackTrace);
                }
                return null;
            }
            else{
                discordClient.Logger.LogWarning("Channel for new members couldn't be found! Giving the noob role to user: " + user.Username + "#" + user.Discriminator);
                DiscordRole noobRole = guild.GetRole(NOOB_ROLE);
                bool roleWasGiven = false;
                if (noobRole != null){
                    DiscordMember member = guild.GetMemberAsync(user.Id).Result;
                    if (member != null){
                        await member.GrantRoleAsync(noobRole);
                        roleWasGiven = true;
                    }
                }
                if (!roleWasGiven){
                    discordClient.Logger.LogWarning("The noob role could not be given to user: " + user.Username + "#" + user.Discriminator);
                }
            }
            return null;
        }

        public static async Task confirmCommandExecuting(DiscordMessage message)
        {
            await Task.Delay(875);
            await message.CreateReactionAsync(getDiscordEmoji(IN_PROGRESS_REACTION));
        }
        public static async Task confirmCommandExecuted(DiscordMessage message)
        {
            await Task.Delay(875);
            await message.DeleteReactionsEmojiAsync(getDiscordEmoji(IN_PROGRESS_REACTION));
            await Task.Delay(875);
            await message.CreateReactionAsync(getDiscordEmoji(ACTION_COMPLETED_REACTION));
        }

        public static DiscordEmoji getDiscordEmoji(string name)
        {
            try{
                return DiscordEmoji.FromName(Bot.discordClient, name);
            }
            catch{
            }
            DiscordEmoji theEmoji = DiscordEmoji.FromUnicode(name);
            if (theEmoji != null){
                return theEmoji;
            }
            else{
                try{
                    theEmoji = DiscordEmoji.FromName(discordClient, name);
                }
                catch (Exception ex){
                    handleError("Could not load emoji:", ex.Message, ex.StackTrace).Wait();
                }
                return theEmoji;
            }
        }

        public static string getEmojiAsString(string emoji)
        {
            DiscordEmoji theEmoji = getDiscordEmoji(emoji);
            if (!theEmoji.GetDiscordName().Equals(emoji)){
                return theEmoji.GetDiscordName();
            }

            try{
                return DiscordEmoji.FromUnicode(Bot.discordClient, emoji).Name;
            }
            catch{
                return emoji;
            }
        }

        public static Dictionary<DiscordEmoji, List<DiscordUser>> sortReactions(DiscordMessage message)
        {
            Dictionary<DiscordEmoji, List<DiscordUser>> sortedReactions = new Dictionary<DiscordEmoji, List<DiscordUser>>();
            foreach (DiscordReaction reaction in message.Reactions){
                DiscordEmoji emoji = reaction.Emoji;
                IReadOnlyList<DiscordUser> users = message.GetReactionsAsync(reaction.Emoji).Result;
                List<DiscordUser> userList = new List<DiscordUser>();
                foreach (var user in users){
                    userList.Add(user);
                }
                sortedReactions.Add(emoji, userList);
            }
            return sortedReactions;
        }

        public static Dictionary<DateTime, List<DiscordMessage>> sortMessages(IReadOnlyList<DiscordMessage> messages)
        {
            Dictionary<DateTime, List<DiscordMessage>> sortedMessages = new Dictionary<DateTime, List<DiscordMessage>>();
            foreach (DiscordMessage message in messages){
                string[] splitted = message.Content.Split(LOG_SPLIT_CHAR);
                //DateTime date = Convert.ToDateTime(splitted[0]);
                string[] dateTimeSplitted = splitted[0].Split(' ');
                string[] dateSplitted = dateTimeSplitted[0].Split('-');
                string[] timeSplitted = dateTimeSplitted[1].Split(':');
                DateTime date = new DateTime(Convert.ToInt32(dateSplitted[2]), Convert.ToInt32(dateSplitted[1]), Convert.ToInt32(dateSplitted[0]), Convert.ToInt32(timeSplitted[0]), Convert.ToInt32(timeSplitted[1]), Convert.ToInt32(timeSplitted[2]));

                bool containsItem = false;
                foreach (KeyValuePair<DateTime, List<DiscordMessage>> item in sortedMessages){
                    string xdate = Bot.convertToDate(item.Key);
                    string ydate = Bot.convertToDate(date);
                    if (xdate.Equals(ydate)){
                        containsItem = true;
                        item.Value.Add(message);
                    }
                }
                if (!containsItem){
                    List<DiscordMessage> tempMessageList = new List<DiscordMessage>();
                    tempMessageList.Add(message);
                    sortedMessages.Add(date, tempMessageList);
                }
            }
            return sortedMessages;
        }

        public static string getProperFileName(string file)
        {
            string[] splitted = file.Split('\\');
            string name = splitted[splitted.Length - 1];
            return Path.GetFileNameWithoutExtension(name).Replace('_', ' ');
        }

        public static async Task<DiscordMember> getDiscordMember(DiscordGuild guild, ulong userID)
        {
            return await guild.GetMemberAsync(userID);
        }

        public static async Task<string> getOrganisator(DiscordMessage message)
        {
            var embeds = message.Embeds;
            if (embeds.Count > 0){
                foreach (DiscordEmbed anEmbed in embeds){
                    foreach (var field in anEmbed.Fields){
                        if (field.Name.ToLower().Equals("organisator")){
                            return field.Value;
                        }
                    }
                }
            }
            else{
                if (message.Author != null){
                    DiscordMember member = await getDiscordMember(message.Channel.Guild, message.Author.Id);
                    if (member != null){
                        return member.DisplayName;
                    }
                }
            }
            return string.Empty;
        }

        public static async Task<List<Tier>> readTeams(DiscordChannel channel, DiscordMember member, string guildName, string[] parameters_as_in_hoeveelste_team)
        {
            if (parameters_as_in_hoeveelste_team.Length <= 1){
                int hoeveelste = 1;
                bool isInt = true;
                if (parameters_as_in_hoeveelste_team.Length > 0){
                    try{
                        hoeveelste = Convert.ToInt32(parameters_as_in_hoeveelste_team[0]);
                    }
                    catch{
                        isInt = false;
                    }
                }
                if (isInt){
                    bool goodNumber = true;
                    if (hoeveelste >= 1){
                        if (hoeveelste > 100){
                            await Bot.SendMessage(channel, member, guildName, "**Het getal mag maximum 100 zijn!**");
                            goodNumber = false;
                        }
                        else{
                            hoeveelste--;
                        }
                    }
                    else if (hoeveelste < 1){
                        await Bot.SendMessage(channel, member, guildName, "**Het getal moet groter zijn dan 0!**");
                        goodNumber = false;
                    }

                    if (goodNumber){
                        DiscordChannel toernooiAanmeldenChannel = await Bot.GetToernooiAanmeldenChannel(channel.Guild.Id);
                        if (toernooiAanmeldenChannel != null){
                            List<DiscordMessage> messages = new List<DiscordMessage>();
                            try{
                                var xMessages = toernooiAanmeldenChannel.GetMessagesAsync((hoeveelste + 1)).Result;
                                foreach (var message in xMessages){
                                    messages.Add(message);
                                }
                            }
                            catch (Exception ex){
                                await handleError("Could not load messages from " + toernooiAanmeldenChannel.Name + ":", ex.Message, ex.StackTrace);
                            }
                            if (messages.Count == (hoeveelste + 1)){
                                DiscordMessage theMessage = messages[hoeveelste];
                                if (theMessage != null){
                                    if (theMessage.Author.Id.Equals(Bot.NLBE_BOT) || theMessage.Author.Id.Equals(Bot.TESTBEASTV2_BOT)){
                                        DiscordChannel logChannel = await Bot.GetLogChannel(channel.Guild.Id);
                                        if (logChannel != null){
                                            var logMessages = await logChannel.GetMessagesAsync(100);
                                            Dictionary<DateTime, List<DiscordMessage>> sortedMessages = Bot.sortMessages(logMessages);
                                            List<Tier> tiers = new List<Tier>();

                                            foreach (KeyValuePair<DateTime, List<DiscordMessage>> sMessage in sortedMessages){
                                                string xdate = Bot.convertToDate(theMessage.Timestamp);
                                                string ydate = Bot.convertToDate(sMessage.Key);
                                                //Bot.discordClient.Logger.LogInformation("[Teams commando] TimeStamp bericht in Toernooi-aanmelden = " + xdate + "\t\t | TimeStamp geschreven in logbericht: " + ydate);
                                                if (xdate.Equals(ydate)){
                                                    sMessage.Value.Sort((x, y) => x.Timestamp.CompareTo(y.Timestamp));
                                                    foreach (DiscordMessage discMessage in sMessage.Value){
                                                        string[] splitted = discMessage.Content.Split(Bot.LOG_SPLIT_CHAR);
                                                        if (splitted[1].ToLower().Equals("teams")){
                                                            Tier newTeam = new Tier();
                                                            bool found = false;
                                                            foreach (Tier aTeam in tiers){
                                                                if (aTeam.TierNummer.Equals(Bot.getEmojiAsString(splitted[3]))){
                                                                    found = true;
                                                                    newTeam = aTeam;
                                                                    break;
                                                                }
                                                            }
                                                            ulong id = 0;
                                                            if (splitted.Length > 4){
                                                                try{
                                                                    id = Convert.ToUInt64(splitted[4]);
                                                                }
                                                                catch{

                                                                }
                                                            }
                                                            newTeam.addDeelnemer(splitted[2], id);
                                                            if (!found){
                                                                if (newTeam.TierNummer.Equals(string.Empty)){
                                                                    newTeam.TierNummer = Bot.getEmojiAsString(splitted[3]);
                                                                    string emojiAsString = Bot.getEmojiAsString(splitted[3]);
                                                                    int index = Emoj.getIndex(emojiAsString);
                                                                    newTeam.tier = index;
                                                                }
                                                                if (newTeam.Organisator.Equals(string.Empty)){
                                                                    newTeam.Organisator = splitted[4].Replace("\\", string.Empty);
                                                                }
                                                                tiers.Add(newTeam);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }

                                            tiers = Bot.editWhenRedundance(tiers);
                                            tiers.Sort((x, y) => x.tier.CompareTo(y.tier));

                                            return tiers;

                                        }
                                        else{
                                            await handleError("Could not find log channel!", string.Empty, string.Empty);
                                        }
                                    }
                                    else{
                                        Dictionary<DiscordEmoji, List<DiscordUser>> reactions = Bot.sortReactions(theMessage);

                                        List<Tier> teams = new List<Tier>();
                                        foreach (KeyValuePair<DiscordEmoji, List<DiscordUser>> reaction in reactions){
                                            Tier aTeam = new Tier();
                                            int counter = 1;
                                            foreach (DiscordUser user in reaction.Value){
                                                string displayName = user.Username;
                                                DiscordMember memberx = toernooiAanmeldenChannel.Guild.GetMemberAsync(user.Id).Result;
                                                if (memberx != null){
                                                    displayName = memberx.DisplayName;
                                                }
                                                aTeam.addDeelnemer(displayName, user.Id);
                                                counter++;
                                            }
                                            if (aTeam.Organisator.Equals(string.Empty)){
                                                foreach (var aGuild in Bot.discGuildslist){
                                                    if (aGuild.Key.Equals(Bot.NLBE_SERVER_ID)){
                                                        DiscordMember theMemberAuthor = await Bot.getDiscordMember(aGuild.Value, theMessage.Author.Id);
                                                        if (theMemberAuthor != null){
                                                            aTeam.Organisator = theMemberAuthor.DisplayName;
                                                        }
                                                    }
                                                }
                                                if (aTeam.Organisator.Equals(string.Empty)){
                                                    aTeam.Organisator = "Niet gevonden";
                                                }
                                            }
                                            if (aTeam.TierNummer.Equals(string.Empty)){
                                                aTeam.TierNummer = reaction.Key;
                                                string emojiAsString = Bot.getEmojiAsString(reaction.Key);
                                                int index = Emoj.getIndex(emojiAsString);
                                                if (index != 0){
                                                    aTeam.tier = index;
                                                    teams.Add(aTeam);
                                                }
                                            }
                                        }
                                        teams = Bot.editWhenRedundance(teams);
                                        teams.Sort((x, y) => x.tier.CompareTo(y.tier));
                                        List<DEF> deflist = new List<DEF>();
                                        List<string> userList = new List<string>();
                                        foreach (Tier aTeam in teams){
                                            DEF def = new DEF();
                                            def.Inline = true;
                                            def.Name = "Tier " + aTeam.TierNummer;
                                            int counter = 1;
                                            StringBuilder sb = new StringBuilder();
                                            foreach (Tuple<ulong, string> user in aTeam.Deelnemers){
                                                string tempName = string.Empty;
                                                DiscordMember tempUser = await channel.Guild.GetMemberAsync(user.Item1);
                                                if (tempUser != null){
                                                    tempName = tempUser.DisplayName;
                                                }
                                                else{
                                                    tempName = user.Item2;
                                                }
                                                sb.AppendLine(counter + ". " + tempName);
                                                counter++;
                                            }
                                            def.Value = sb.ToString();
                                            deflist.Add(def);
                                        }

                                        await Bot.CreateEmbed(channel, string.Empty, string.Empty, "Teams", (teams.Count > 0 ? string.Empty : "Geen teams"), deflist, null, string.Empty, null);
                                        return new List<Tier>();
                                    }
                                }
                                else{
                                    await Bot.SendMessage(channel, member, guildName, "**Het bericht kon niet gevonden worden!**");
                                }
                            }
                            else{
                                await Bot.SendMessage(channel, member, guildName, "**Dit bericht kon niet gevonden worden!**");
                            }
                        }
                        else{
                            await Bot.SendMessage(channel, member, guildName, "**Het kanaal #Toernooi-aanmelden kon niet gevonden worden!**");
                        }
                    }
                }
                else{
                    await Bot.SendMessage(channel, member, guildName, "**Je moet cijfer meegeven!**");
                }
            }
            else{
                await Bot.SendMessage(channel, member, guildName, "**Je mag maar één cijfer meegeven!**");
            }
            return null;
        }

        public static List<Tier> editWhenRedundance(List<Tier> teams)
        {
            if (teams.Count > 1){
                List<Tier> newTeams = new List<Tier>();
                int aCounter = 0;
                foreach (Tier aTeam in teams){
                    Tier newTeam = new Tier();
                    foreach (Tuple<ulong, string> aDeelnemer in aTeam.Deelnemers){
                        bool neverFound = true;
                        int bCounter = 0;
                        int amountFound = 0;
                        foreach (Tier bTeam in teams){
                            if (aCounter != bCounter){
                                foreach (Tuple<ulong, string> bDeelnemer in bTeam.Deelnemers){
                                    if (aDeelnemer.Equals(bDeelnemer)){
                                        neverFound = false;
                                        amountFound++;
                                    }
                                }
                            }
                            bCounter++;
                        }
                        if (neverFound){
                            newTeam.addDeelnemer("**" + aDeelnemer.Item2 + "**", aDeelnemer.Item1);
                            newTeam.uniekelingen.Add(aDeelnemer.Item2);
                        }
                        else if (amountFound == 1){
                            newTeam.addDeelnemer("`" + aDeelnemer.Item2.Replace("\\", string.Empty) + "`", aDeelnemer.Item1);
                        }
                        else{
                            newTeam.addDeelnemer(aDeelnemer.Item2, aDeelnemer.Item1);
                        }
                    }
                    newTeam.Datum = aTeam.Datum;
                    newTeam.Organisator = aTeam.Organisator;
                    newTeam.TierNummer = aTeam.TierNummer;
                    newTeam.tier = aTeam.tier;
                    newTeams.Add(newTeam);
                    aCounter++;
                }
                return newTeams;
            }
            else{
                return teams;
            }
        }
        public static async Task<List<Tuple<ulong, string>>> getIndividualParticipants(List<Tier> teams, DiscordGuild guild)
        {
            List<Tuple<ulong, string>> participants = new List<Tuple<ulong, string>>();
            if (teams != null){
                if (teams.Count > 0){
                    participants.Add(new Tuple<ulong, string>(0, teams[0].Organisator));
                    foreach (Tier team in teams){
                        foreach (Tuple<ulong, string> participant in team.Deelnemers){
                            string temp = string.Empty;
                            try{
                                DiscordMember tempMember = await guild.GetMemberAsync(participant.Item1);
                                if (tempMember != null){
                                    if (tempMember.DisplayName != null){
                                        if (tempMember.DisplayName.Length > 0){
                                            temp = tempMember.DisplayName;
                                        }
                                    }
                                }
                            }
                            catch{

                            }
                            if (temp.Equals(string.Empty)){
                                temp = participant.Item2;
                            }
                            if (!temp.Equals(string.Empty)){
                                temp = Bot.removeSyntaxe(participant.Item2);
                            }
                            bool alreadyInList = false;
                            foreach (Tuple<ulong, string> participantX in participants){
                                if (participantX.Item1.Equals(participant.Item1) && participant.Item1 > 0 || participantX.Item2.Equals(removeSyntaxe(participant.Item2))){
                                    alreadyInList = true;
                                    break;
                                }
                            }
                            if (!alreadyInList){
                                participants.Add(new Tuple<ulong, string>(participant.Item1, temp));
                            }
                        }
                    }
                    //return participants.AsEnumerable<string>().OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase).ToList();
                    return participants.AsEnumerable<Tuple<ulong, string>>().OrderBy(x => x.Item2).ToList();
                }
                else return participants;
            }
            else return participants;
        }
        private static string removeSyntaxe(string stringItem)
        {
            stringItem = stringItem.Replace("\\", string.Empty);
            if (stringItem.StartsWith("**") && stringItem.EndsWith("**")){
                return stringItem.Trim('*');
            }
            else if (stringItem.StartsWith('`') && stringItem.EndsWith('`')){
                return stringItem.Trim('`');
            }
            else{
                return stringItem;
            }
        }
        public static List<string> removeSyntaxes(List<string> stringList)
        {
            List<string> tempList = new List<string>();
            foreach (string item in stringList){
                tempList.Add(removeSyntaxe(item));
            }
            return tempList;
        }
        public static List<Tuple<ulong, string>> removeSyntaxes(List<Tuple<ulong, string>> stringList)
        {
            List<Tuple<ulong, string>> tempList = new List<Tuple<ulong, string>>();
            foreach (Tuple<ulong, string> item in stringList){
                tempList.Add(new Tuple<ulong, string>(item.Item1, removeSyntaxe(item.Item2)));
            }
            return tempList;
        }
        public static async Task<List<string>> getMentions(List<Tuple<ulong, string>> memberList, ulong guildID)
        {
            DiscordGuild guild = await getGuild(guildID);
            if (guild != null){
                List<string> mentionList = new List<string>();
                foreach (Tuple<ulong, string> member in memberList){
                    bool addByString = true;
                    if (member.Item1 > 1){
                        DiscordMember tempMember = await guild.GetMemberAsync(member.Item1);
                        if (tempMember != null){
                            if (tempMember.Mention != null){
                                if (tempMember.Mention.Length > 0){
                                    addByString = false;
                                    mentionList.Add(tempMember.Mention);
                                }
                            }
                        }
                    }
                    if (addByString){
                        bool added = false;
                        IReadOnlyCollection<DiscordMember> usersList = await guild.GetAllMembersAsync();
                        if (usersList != null){
                            foreach (DiscordMember memberItem in usersList){
                                if (memberItem.DisplayName != null){
                                    if (memberItem.DisplayName.ToLower().Equals(member.Item2.ToLower())){
                                        if (memberItem.Mention != null){
                                            if (memberItem.Mention.Length > 0){
                                                mentionList.Add(memberItem.Mention);
                                                added = true;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        if (!added){
                            mentionList.Add("@" + member.Item2);
                        }
                    }
                }
                return mentionList;
            }
            return null;
        }

        public static async Task writeInLog(ulong guildID, string date, string message)
        {
            DiscordChannel logChannel = await GetLogChannel(guildID);
            if (logChannel != null){
                await logChannel.SendMessageAsync(date + "|" + message);
            }
            else{
                await handleError("Could not find log channel, message: " + date + "|" + message, string.Empty, string.Empty);
            }
        }
        public static async Task writeInLog(ulong guildID, string message)
        {
            DiscordChannel logChannel = await GetLogChannel(guildID);
            if (logChannel != null){
                await logChannel.SendMessageAsync(message);
            }
            else{
                await handleError("Could not find log channel, message: " + message, string.Empty, string.Empty);
            }
        }
        public static async Task clearLog(ulong guildID, int amount)
        {
            DiscordChannel logChannel = await GetLogChannel(guildID);
            if (logChannel != null){
                var messages = await logChannel.GetMessagesAsync(amount);
                foreach (DiscordMessage message in messages){
                    try{
                        await message.DeleteAsync();
                        await Task.Delay(875);
                    }
                    catch (Exception ex){
                        await handleError("Could not delete message:", ex.Message, ex.StackTrace);
                    }
                }
            }
            else{
                await handleError("Could not find log channel!", string.Empty, string.Empty);
            }
        }
        public static async Task clearLog(ulong guildID)
        {
            await clearLog(500);
        }

        public static async Task changeMemberNickname(DiscordMember member, string nickname)
        {
            try{
                Action<MemberEditModel> mem = item => {
                    item.Nickname = nickname;
                    item.AuditLogReason = "Changed by NLBE-Bot.";
                };
                await member.ModifyAsync(mem);
            }
            catch (Exception ex){
                await handleError("Could not edit displayname for " + member.Username + ":", ex.Message, ex.StackTrace);
            }
        }
        public static string updateName(DiscordMember member, string oldName)
        {
            string returnString = oldName;
            var memberRoles = member.Roles;
            if (oldName.Contains('[') && oldName.Contains(']')){
                string[] splitted = oldName.Split('[');
                StringBuilder sb = new StringBuilder();
                if (oldName.StartsWith('[')){
                    for (int i = 1; i < splitted.Length; i++){
                        sb.Append(splitted[i]);
                    }
                    splitted = sb.ToString().Split(']');

                    sb = new StringBuilder();

                    foreach (var role in memberRoles){
                        if (role.Id.Equals(NLBE_ROLE) || role.Id.Equals(NLBE2_ROLE)){
                            if (!oldName.StartsWith("[" + role.Name + "]")){
                                returnString = oldName.Replace("[" + splitted[0] + "]", "[" + role.Name + "]");
                            }
                            if (!returnString.StartsWith("[" + role.Name + "] ")){
                                returnString = returnString.Replace("[" + role.Name + "]", "[" + role.Name + "] ");
                            }
                            break;
                        }
                    }
                }
                else{
                    foreach (var role in memberRoles){
                        if (role.Id.Equals(NLBE_ROLE) || role.Id.Equals(NLBE2_ROLE)){
                            if (oldName.StartsWith(role.Name + "] ")){
                                returnString = "[" + oldName;
                            }
                            else{
                                for (int i = 1; i < splitted.Length; i++){
                                    if (i > 1){
                                        sb.Append('[');
                                    }
                                    sb.Append(splitted[i]);
                                }
                                splitted = sb.ToString().Split(']');
                                sb = new StringBuilder();
                                for (int i = 1; i < splitted.Length; i++){
                                    if (i > 1){
                                        sb.Append(']');
                                    }
                                    sb.Append(splitted[i]);
                                }
                                returnString = "[" + role.Name + "] " + sb.ToString();
                            }
                        }
                    }
                }

            }
            else if (oldName.Contains('[')){
                foreach (var role in memberRoles){
                    if (role.Id.Equals(NLBE_ROLE) || role.Id.Equals(NLBE2_ROLE)){
                        if (!oldName.StartsWith("[" + role.Name)){
                            string[] splitted = oldName.Split(' ');
                            if (splitted.Length > 1){
                                StringBuilder sb = new StringBuilder();
                                for (int i = 1; i < splitted.Length; i++){
                                    if (i > 1){
                                        sb.Append(' ');
                                    }
                                    sb.Append(splitted[i]);
                                }
                                returnString = "[" + role.Name + "] " + sb.ToString();
                            }
                            else{
                                returnString = "[" + role.Name + "] " + oldName.Replace("[", string.Empty);
                            }
                        }
                        else{
                            string[] splitted = oldName.Split(' ');
                            if (splitted.Length > 1){
                                StringBuilder sb = new StringBuilder();
                                for (int i = 1; i < splitted.Length; i++){
                                    if (i > 1){
                                        sb.Append(' ');
                                    }
                                    sb.Append(splitted[i]);
                                }
                                returnString = "[" + role.Name + "] " + sb.ToString();
                            }
                            else{
                                returnString = oldName.Replace("[" + role.Name, "[" + role.Name + "] ") + oldName.Replace("[" + role.Name, string.Empty);
                            }
                        }
                    }
                }
            }
            else if (oldName.Contains(']')){
                foreach (var role in memberRoles){
                    if (role.Id.Equals(NLBE_ROLE) || role.Id.Equals(NLBE2_ROLE)){
                        if (!oldName.StartsWith(role.Name + "]")){
                            string[] splitted = oldName.Split(' ');
                            if (splitted.Length > 1){
                                StringBuilder sb = new StringBuilder();
                                for (int i = 1; i < splitted.Length; i++){
                                    if (i > 1){
                                        sb.Append(' ');
                                    }
                                    sb.Append(splitted[i]);
                                }
                                returnString = "[" + role.Name + "] " + sb.ToString().Replace("]", string.Empty);
                            }
                            else{
                                returnString = "[" + role.Name + "] " + oldName.Replace("]", string.Empty);
                            }
                        }
                        else{
                            string[] splitted = oldName.Split(' ');
                            if (splitted.Length > 1){
                                StringBuilder sb = new StringBuilder();
                                for (int i = 1; i < splitted.Length; i++){
                                    if (i > 1){
                                        sb.Append(' ');
                                    }
                                    sb.Append(splitted[i]);
                                }
                                returnString = "[" + role.Name + "] " + sb.ToString();
                            }
                            else{
                                returnString = oldName.Replace(role.Name + "]", "[" + role.Name + "] ") + oldName.Replace("]" + role.Name, string.Empty);
                            }
                        }
                    }
                }
            }
            else{
                foreach (var role in memberRoles){
                    if (role.Id.Equals(NLBE_ROLE) || role.Id.Equals(NLBE2_ROLE)){
                        string[] splitted = oldName.Split(' ');
                        if (splitted.Length > 1){
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < splitted.Length; i++){
                                if (i > 0){
                                    sb.Append(' ');
                                }
                                if (!splitted[i].Equals(string.Empty) && !splitted[i].Equals(" ")){
                                    sb.Append(splitted[i]);
                                }
                            }
                            returnString = "[" + role.Name + "] " + sb.ToString();
                        }
                        else{
                            returnString = "[" + role.Name + "] " + oldName;
                        }
                    }
                }
            }

            bool isFromNLBE = false;
            foreach (var role in member.Roles){
                if (role.Id.Equals(NLBE_ROLE) || role.Id.Equals(NLBE2_ROLE)){
                    isFromNLBE = true;
                }
            }
            if (!isFromNLBE){
                if (returnString.StartsWith("[NLBE]") || returnString.StartsWith("[NLBE2]")){
                    returnString = returnString.Replace("[NLBE2]", "[]").Replace("[NLBE]", "[]");
                }
            }

            while (returnString.EndsWith(' ')){
                returnString = returnString.Remove(returnString.Length - 1);
            }

            return returnString;
        }

        public static bool checkIfAllWithinRange(string[] tiers, int min, int max)
        {
            bool allWithinRange = true;
            for (int i = 0; i < tiers.Length; i++){
                int temp = Convert.ToInt32(tiers[i]);
                if (temp < min || temp > max){
                    allWithinRange = false;
                    break;
                }
            }
            return allWithinRange;
        }

        public static async Task cleanChannel(ulong serverID, ulong channelID)
        {
            DiscordChannel channel = await getChannel(serverID, channelID);
            var messages = channel.GetMessagesAsync(100).Result;
            foreach (DiscordMessage message in messages){
                if (!message.Pinned){
                    await channel.DeleteMessageAsync(message);
                    await Task.Delay(875);
                }
            }
        }
        public static async Task cleanWelkomChannel()
        {
            DiscordChannel welkomChannel = await GetWelkomChannel();
            var messages = welkomChannel.GetMessagesAsync(100).Result;
            foreach (DiscordMessage message in messages){
                if (!message.Pinned){
                    await welkomChannel.DeleteMessageAsync(message);
                    await Task.Delay(875);
                }
            }
        }
        public static async Task cleanWelkomChannel(ulong userID)
        {
            DiscordChannel welkomChannel = await GetWelkomChannel();
            var messages = welkomChannel.GetMessagesAsync(100).Result;
            foreach (DiscordMessage message in messages){
                bool deleteMessage = false;
                if (!message.Pinned){
                    if (message.Author.Id.Equals(NLBE_BOT)){
                        if (message.Content.Contains("<@" + userID + ">")){
                            deleteMessage = true;
                        }
                    }
                    else if (message.Author.Id.Equals(userID)){
                        deleteMessage = true;
                    }
                }
                if (deleteMessage){
                    await welkomChannel.DeleteMessageAsync(message);
                    await Task.Delay(875);
                }
            }
        }

        public static DiscordRole GetDiscordRole(ulong serverID, ulong id)
        {
            foreach (var guild in discGuildslist){
                if (guild.Key.Equals(serverID)){
                    foreach (var role in guild.Value.Roles){
                        if (role.Key.Equals(id)){
                            return role.Value;
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        public static bool hasRight(DiscordMember member, Command command)
        {
            if (member.Guild.Id.Equals(DA_BOIS_ID) || member.Guild.Id.Equals(NLBE_SERVER_ID)){
                bool hasRights = false;
                if (member.Guild.Id.Equals(DA_BOIS_ID)){
                    return true;
                }
                switch (command.Name.ToLower()){
                    case "help":
                        hasRights = true;
                        break;
                    case "map":
                        hasRights = true;
                        break;
                    case "gebruiker":
                        hasRights = true;
                        break;
                    case "gebruikerslijst":
                        hasRights = true;
                        break;
                    case "clan":
                        hasRights = true;
                        break;
                    case "clanmembers":
                        hasRights = true;
                        break;
                    case "spelerinfo":
                        hasRights = true;
                        break;
                    case "toernooi":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.TOERNOOI_DIRECTIE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "toernooien":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.TOERNOOI_DIRECTIE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "teams":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.NLBE_ROLE) || role.Id.Equals(Bot.NLBE2_ROLE) || role.Id.Equals(Bot.DISCORD_ADMIN_ROLE) || role.Id.Equals(Bot.DEPUTY_ROLE) || role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.TOERNOOI_DIRECTIE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "tagteams":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.DISCORD_ADMIN_ROLE) || role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.TOERNOOI_DIRECTIE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "hof":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.NLBE_ROLE) || role.Id.Equals(Bot.NLBE2_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "hofplayer":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.NLBE_ROLE) || role.Id.Equals(Bot.NLBE2_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "resethof":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.DISCORD_ADMIN_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "weekly":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.DISCORD_ADMIN_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "removeplayerhof":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.DISCORD_ADMIN_ROLE) || role.Id.Equals(Bot.DEPUTY_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "renameplayerhof":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.DISCORD_ADMIN_ROLE) || role.Id.Equals(Bot.DEPUTY_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "poll":
                        if (member.Id.Equals(414421187888676875)){
                            hasRights = true;
                        }
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.DISCORD_ADMIN_ROLE) || role.Id.Equals(Bot.DEPUTY_ROLE) || role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.TOERNOOI_DIRECTIE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "updategebruikers":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.DISCORD_ADMIN_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    case "deputypoll":
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.DEPUTY_ROLE) || role.Id.Equals(Bot.DEPUTY_NLBE_ROLE) || role.Id.Equals(Bot.DEPUTY_NLBE2_ROLE) || role.Id.Equals(Bot.DISCORD_ADMIN_ROLE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                    default:
                        foreach (var role in member.Roles){
                            if (role.Id.Equals(Bot.DISCORD_ADMIN_ROLE) || role.Id.Equals(Bot.DEPUTY_ROLE) || role.Id.Equals(Bot.BEHEERDER_ROLE) || role.Id.Equals(Bot.TOERNOOI_DIRECTIE)){
                                hasRights = true;
                                break;
                            }
                        }
                        break;
                }
                return hasRights;
            }
            else{
                return false;
            }
        }

        public static async Task showMemberInfo(DiscordChannel channel, object gebruiker)
        {
            if (gebruiker is DiscordMember){
                DiscordMember member = (DiscordMember)gebruiker;
                DiscordEmbedBuilder.EmbedAuthor newAuthor = new DiscordEmbedBuilder.EmbedAuthor();
                newAuthor.Name = member.Username.Replace('_', '▁');
                newAuthor.IconUrl = member.AvatarUrl;
                //newAuthor.IconUrl = Bot.discordClient.CurrentApplication.Icon;
                List<DEF> deflist = new List<DEF>();
                DEF newDef1 = new DEF();
                newDef1.Name = "Gebruiker";
                newDef1.Value = (member.Username + "#" + member.Discriminator).adaptToDiscordChat();
                newDef1.Inline = true;
                deflist.Add(newDef1);
                DEF newDef2 = new DEF();
                newDef2.Name = "Bijnaam";
                newDef2.Value = member.DisplayName.adaptToDiscordChat();
                newDef2.Inline = true;
                deflist.Add(newDef2);
                DEF newDef3 = new DEF();
                newDef3.Name = "GebruikersID";
                newDef3.Value = member.Id.ToString();
                newDef3.Inline = true;
                deflist.Add(newDef3);
                DEF newDef4 = new DEF();
                newDef4.Name = "Rol" + (member.Roles.Count() > 1 ? "len" : string.Empty);
                StringBuilder sbRoles = new StringBuilder();
                bool firstTime = true;
                foreach (var role in member.Roles){
                    if (firstTime){
                        firstTime = false;
                    }
                    else{
                        sbRoles.Append(", ");
                    }
                    sbRoles.Append(role.Name.Replace('_', '▁'));
                }
                if (sbRoles.Length == 0){
                    sbRoles.Append("`Had geen rol`");
                }
                newDef4.Value = sbRoles.ToString().adaptToDiscordChat();
                newDef4.Inline = true;
                deflist.Add(newDef4);
                DEF newDef5 = new DEF();
                newDef5.Name = "Gejoined op";
                string[] splitted = convertToDate(member.JoinedAt).Split(' ');
                newDef5.Value = splitted[0] + " " + splitted[1];
                newDef5.Inline = true;
                deflist.Add(newDef5);
                if (member.Presence != null){
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(member.Presence.Status.ToString());
                    if (member.Presence.Activity != null){
                        if (member.Presence.Activity.CustomStatus != null){
                            if (member.Presence.Activity.CustomStatus.Name != null){
                                sb.AppendLine(member.Presence.Activity.CustomStatus.Name);
                            }
                        }
                    }
                    DEF newDef6 = new DEF();
                    newDef6.Name = "Status";
                    newDef6.Value = sb.ToString();
                    newDef6.Inline = true;
                    deflist.Add(newDef6);
                }
                if (member.Verified.HasValue){
                    if (!member.Verified.Value){
                        DEF newDef6 = new DEF();
                        newDef6.Name = "Niet bevestigd!";
                        newDef6.Value = "Dit account is niet bevestigd!";
                        newDef6.Inline = true;
                        deflist.Add(newDef6);
                    }
                }

                await Bot.CreateEmbed(channel, string.Empty, string.Empty, "Info over " + member.DisplayName.adaptToDiscordChat() + (member.IsBot ? " [BOT]" : ""), string.Empty, deflist, null, string.Empty, newAuthor, member.Color);
            }
            else if (gebruiker is WGAccount){
                List<DEF> deflist = new List<DEF>();
                WGAccount member = (WGAccount)gebruiker;
                try{
                    DEF newDef1 = new DEF();
                    newDef1.Name = "Gebruikersnaam";
                    newDef1.Value = member.nickname.adaptToDiscordChat();
                    newDef1.Inline = true;
                    deflist.Add(newDef1);
                    if (member.clan != null){
                        if (member.clan.tag != null){
                            DEF newDef2 = new DEF();
                            newDef2.Name = "Clan";
                            newDef2.Value = member.clan.tag.adaptToDiscordChat();
                            newDef2.Inline = true;
                            deflist.Add(newDef2);
                            DEF newDef4 = new DEF();
                            newDef4.Name = "Rol";
                            newDef4.Value = member.clan.role.ToString().adaptToDiscordChat();
                            newDef4.Inline = true;
                            deflist.Add(newDef4);
                            DEF newDef5 = new DEF();
                            newDef5.Name = "Clan gejoined op";
                            string[] splitted = convertToDate(member.clan.joined_at.Value).Split(' ');
                            newDef5.Value = splitted[0] + " " + splitted[1];
                            newDef5.Inline = true;
                            deflist.Add(newDef5);
                        }
                    }
                }
                catch{

                }
                finally{
                    DEF newDef3 = new DEF();
                    newDef3.Name = "SpelerID";
                    newDef3.Value = member.account_id.ToString();
                    newDef3.Inline = true;
                    deflist.Add(newDef3);
                    if (member.created_at.HasValue){
                        DEF newDef6 = new DEF();
                        newDef6.Name = "Gestart op";
                        newDef6.Value = convertToDate(member.created_at.Value);
                        newDef6.Inline = true;
                        deflist.Add(newDef6);
                    }
                    if (member.last_battle_time.HasValue){
                        DEF newDef6 = new DEF();
                        newDef6.Name = "Laatst actief";
                        newDef6.Value = convertToDate(member.last_battle_time.Value);
                        newDef6.Inline = true;
                        deflist.Add(newDef6);
                    }
                    if (member.statistics != null){
                        if (member.statistics.rating != null){
                            DEF newDef4 = new DEF();
                            newDef4.Name = "Rating (WR)";
                            newDef4.Value = (member.statistics.rating.battles > 0 ? String.Format("{0:.##}", (100 * ((double)member.statistics.rating.wins / (double)member.statistics.rating.battles))) : "Nog geen rating gespeeld");
                            newDef4.Inline = true;
                            deflist.Add(newDef4);
                        }
                        if (member.statistics.all != null){
                            DEF newDef5 = new DEF();
                            newDef5.Name = "Winrate";
                            newDef5.Value = String.Format("{0:.##}", (100 * ((double)member.statistics.all.wins / (double)member.statistics.all.battles)));
                            newDef5.Inline = true;
                            deflist.Add(newDef5);
                            DEF newDef6 = new DEF();
                            newDef6.Name = "Gem. damage";
                            newDef6.Value = (member.statistics.all.damage_dealt / member.statistics.all.battles).ToString();
                            newDef6.Inline = true;
                            deflist.Add(newDef6);
                            DEF newDef7 = new DEF();
                            newDef7.Name = "Battles";
                            newDef7.Value = member.statistics.all.battles.ToString();
                            newDef7.Inline = true;
                            deflist.Add(newDef7);
                        }
                    }
                    //await Bot.CreateEmbed(channel, string.Empty, member.blitzstars + Environment.NewLine, "Info over " + member.nickname.adaptToDiscordChat(), string.Empty, string.Empty, deflist, null, string.Empty, null);
                    await Bot.CreateEmbed(channel, string.Empty, string.Empty, "Info over " + member.nickname.adaptToDiscordChat(), string.Empty, string.Empty, deflist, null, string.Empty, null, Bot.BOT_COLOR, false, member.blitzstars);
                }
            }
            else if (gebruiker is DiscordUser){
                DiscordUser member = (DiscordUser)gebruiker;
                DiscordEmbedBuilder.EmbedAuthor newAuthor = new DiscordEmbedBuilder.EmbedAuthor();
                newAuthor.Name = member.Username.adaptToDiscordChat();
                newAuthor.IconUrl = member.AvatarUrl;
                //newAuthor.IconUrl = Bot.discordClient.CurrentApplication.Icon;
                List<DEF> deflist = new List<DEF>();
                DEF newDef1 = new DEF();
                newDef1.Name = "Gebruikersnaam";
                newDef1.Value = member.Username.adaptToDiscordChat();
                newDef1.Inline = true;
                deflist.Add(newDef1);
                DEF newDef3 = new DEF();
                newDef3.Name = "GebruikersID";
                newDef3.Value = member.Id.ToString();
                newDef3.Inline = true;
                deflist.Add(newDef3);
                DEF newDef4 = new DEF();
                newDef4.Name = "Gecreëerd op";
                string[] splitted = convertToDate(member.CreationTimestamp).Split(' ');
                newDef4.Value = splitted[0] + " " + splitted[1];
                newDef4.Inline = true;
                deflist.Add(newDef4);
                if (member.Flags.HasValue){
                    if (!member.Flags.Value.ToString().Equals("None")){
                        DEF newDef2 = new DEF();
                        newDef2.Name = "Discord Medailles";
                        newDef2.Value = member.Flags.Value.ToString();
                        newDef2.Inline = true;
                        deflist.Add(newDef2);
                    }
                }
                if (member.Email != null){
                    if (member.Email.Length > 0){
                        DEF newDef5 = new DEF();
                        newDef5.Name = "E-mail";
                        newDef5.Value = member.Email;
                        newDef5.Inline = true;
                        deflist.Add(newDef5);
                    }
                }
                if (member.Locale != null){
                    if (member.Locale.Length > 0){
                        DEF newDef5 = new DEF();
                        newDef5.Name = "Taal";
                        newDef5.Value = member.Locale;
                        newDef5.Inline = true;
                        deflist.Add(newDef5);
                    }
                }
                if (member.Presence != null){
                    if (member.Presence.Activity != null){
                        if (member.Presence.Activity.CustomStatus != null){
                            if (member.Presence.Activity.CustomStatus.Name != null){
                                DEF newDef7 = new DEF();
                                newDef7.Name = "Custom status";
                                newDef7.Value = (member.Presence.Activity.CustomStatus.Emoji != null ? member.Presence.Activity.CustomStatus.Emoji.Name : string.Empty) + member.Presence.Activity.CustomStatus.Name.adaptToDiscordChat();
                                newDef7.Inline = true;
                                deflist.Add(newDef7);
                            }
                        }
                    }
                    if (member.Presence.Activities != null){
                        if (member.Presence.Activities.Count > 0){
                            StringBuilder sb = new StringBuilder();
                            foreach (var item in member.Presence.Activities){
                                string temp = string.Empty;
                                bool customStatus = false;
                                if (item.CustomStatus != null){
                                    if (item.CustomStatus.Name.Length > 0){
                                        customStatus = true;
                                        temp = (item.CustomStatus.Emoji != null ? item.CustomStatus.Emoji.Name : string.Empty) + item.CustomStatus.Name;
                                    }
                                }
                                if (!customStatus){
                                    temp = item.Name;
                                }
                                bool streaming = false;
                                if (item.StreamUrl != null){
                                    if (item.StreamUrl.Length > 0){
                                        streaming = true;
                                        sb.AppendLine("[" + temp + "](" + item.StreamUrl + ")");
                                    }
                                }
                                if (!streaming){
                                    sb.AppendLine(temp);
                                }
                            }
                            DEF newDef7 = new DEF();
                            newDef7.Name = "Recente activiteiten";
                            newDef7.Value = sb.ToString().adaptToDiscordChat();
                            newDef7.Inline = true;
                            deflist.Add(newDef7);
                        }
                    }
                    if (member.Presence.Activity != null){
                        if (member.Presence.Activity.Name != null){
                            string temp = string.Empty;
                            bool customStatus = false;
                            if (member.Presence.Activity.CustomStatus != null){
                                if (member.Presence.Activity.CustomStatus.Name.Length > 0){
                                    customStatus = true;
                                    temp = (member.Presence.Activity.CustomStatus.Emoji != null ? member.Presence.Activity.CustomStatus.Emoji.Name : string.Empty) + member.Presence.Activity.CustomStatus.Name;
                                }
                            }
                            if (!customStatus){
                                bool streaming = false;
                                if (member.Presence.Activity.StreamUrl != null){
                                    if (member.Presence.Activity.StreamUrl.Length > 0){
                                        streaming = true;
                                        temp = "[" + member.Presence.Activity.Name + "](" + member.Presence.Activity.StreamUrl + ")";
                                    }
                                }
                                if (!streaming){
                                    temp = member.Presence.Activity.Name;
                                }
                            }
                            DEF newDefx = new DEF();
                            newDefx.Name = "Activiteit";
                            newDefx.Value = temp;
                            newDefx.Inline = true;
                            deflist.Add(newDefx);
                        }
                    }
                    if (member.Presence.ClientStatus != null){
                        StringBuilder sb = new StringBuilder();
                        if (member.Presence.ClientStatus.Desktop != null){
                            if (member.Presence.ClientStatus.Desktop.HasValue){
                                sb.AppendLine("Desktop");
                            }
                        }
                        if (member.Presence.ClientStatus.Mobile != null){
                            if (member.Presence.ClientStatus.Mobile.HasValue){
                                sb.AppendLine("Mobiel");
                            }
                        }
                        if (member.Presence.ClientStatus.Web != null){
                            if (member.Presence.ClientStatus.Web.HasValue){
                                sb.AppendLine("Web");
                            }
                        }
                        if (sb.Length > 0){
                            DEF newDef7 = new DEF();
                            newDef7.Name = "Op discord via";
                            newDef7.Value = sb.ToString();
                            newDef7.Inline = true;
                            deflist.Add(newDef7);
                        }
                    }
                }
                if (member.PremiumType.HasValue){
                    DEF newDef7 = new DEF();
                    newDef7.Name = "Premiumtype";
                    newDef7.Value = member.PremiumType.Value.ToString();
                    newDef7.Inline = true;
                    deflist.Add(newDef7);
                }
                if (member.Verified.HasValue){
                    if (!member.Verified.Value){
                        DEF newDef6 = new DEF();
                        newDef6.Name = "Niet bevestigd!";
                        newDef6.Value = "Dit account is niet bevestigd!";
                        newDef6.Inline = true;
                        deflist.Add(newDef6);
                    }
                }

                await Bot.CreateEmbed(channel, string.Empty, string.Empty, "Info over " + member.Username.adaptToDiscordChat() + "#" + member.Discriminator + (member.IsBot ? " [BOT]" : ""), string.Empty, deflist, null, string.Empty, newAuthor);
            }
        }
        public static async Task showClanInfo(DiscordChannel channel, WGClan clan)
        {
            //DiscordEmbedBuilder.EmbedAuthor newAuthor = new DiscordEmbedBuilder.EmbedAuthor();
            //newAuthor.Name = clan.Data.Name.Replace('_', '▁');
            //newAuthor.IconUrl = clan.Data.GetEmblemUrl(WGRegion.Europe, WGClient.Entities.Clans.EmblemSize.x64);
            List<DEF> deflist = new List<DEF>();
            DEF newDef1 = new DEF();
            newDef1.Name = "Clannaam";
            newDef1.Value = clan.name.adaptToDiscordChat();
            newDef1.Inline = true;
            deflist.Add(newDef1);
            DEF newDef2 = new DEF();
            newDef2.Name = "Aantal leden";
            newDef2.Value = clan.members_count.ToString();
            newDef2.Inline = true;
            deflist.Add(newDef2);
            DEF newDef3 = new DEF();
            newDef3.Name = "ClanID";
            newDef3.Value = clan.clan_id.ToString();
            newDef3.Inline = true;
            deflist.Add(newDef3);
            DEF newDef4 = new DEF();
            newDef4.Name = "ClanTag";
            newDef4.Value = clan.tag.adaptToDiscordChat();
            newDef4.Inline = true;
            deflist.Add(newDef4);
            if (clan.created_at.HasValue){
                DEF newDef5 = new DEF();
                newDef5.Name = "Gemaakt op";
                string[] splitted = convertToDate(clan.created_at.Value).Split(' ');
                newDef5.Value = splitted[0] + " " + splitted[1];
                newDef5.Inline = true;
                deflist.Add(newDef5);
            }
            DEF newDef6 = new DEF();
            newDef6.Name = "Clan motto";
            newDef6.Value = clan.motto.adaptDiscordLink().adaptToDiscordChat();
            newDef6.Inline = false;
            deflist.Add(newDef6);
            DEF newDef7 = new DEF();
            newDef7.Name = "Clan beschrijving";
            newDef7.Value = clan.description.adaptDiscordLink().adaptToDiscordChat();
            newDef7.Inline = false;
            deflist.Add(newDef7);

            await Bot.CreateEmbed(channel, string.Empty, string.Empty, "Info over " + clan.name.adaptToDiscordChat(), string.Empty, deflist, null, string.Empty, null);
        }
        public static async Task showTournamentInfo(DiscordChannel channel, WGTournament tournament, string titel)
        {
            List<DEF> deflist = new List<DEF>();
            DEF newDef1 = new DEF();
            newDef1.Name = "Titel";
            newDef1.Value = tournament.title;
            newDef1.Inline = true;
            deflist.Add(newDef1);
            DEF newDef2 = new DEF();
            newDef2.Name = "Status";
            newDef2.Value = tournament.status.Replace('_', ' ');
            newDef2.Inline = true;
            deflist.Add(newDef2);
            if (tournament.other_rules != null){
                if (tournament.other_rules.Length > 0){
                    DEF newDef7 = new DEF();
                    newDef7.Name = "Prijsbeschrijving";
                    newDef7.Value = tournament.other_rules;
                    newDef7.Inline = true;
                    deflist.Add(newDef7);
                }
            }
            if (tournament.start_at.HasValue){
                DEF newDef5 = new DEF();
                newDef5.Name = "Start op";
                string[] splittedx = convertToDate(tournament.start_at.Value).Split(' ');
                newDef5.Value = splittedx[0] + " " + splittedx[1];
                newDef5.Inline = true;
                deflist.Add(newDef5);
            }
            if (tournament.registration_start_at.HasValue){
                DEF newDef5 = new DEF();
                newDef5.Name = "Registreren";
                string[] splittedx = convertToDate(tournament.registration_start_at.Value).Split(' ');
                StringBuilder sb = new StringBuilder("Vanaf\n" + splittedx[0] + " " + splittedx[1]);
                if (tournament.registration_end_at.HasValue){
                    string[] splittedb = convertToDate(tournament.registration_end_at.Value).Split(' ');
                    sb.Append("\ntot\n" + splittedb[0] + " " + splittedb[1]);
                }
                newDef5.Value = sb.ToString();
                newDef5.Inline = true;
                deflist.Add(newDef5);
            }
            if (tournament.matches_start_at.HasValue){
                DEF newDef7 = new DEF();
                newDef7.Name = "Matchen beginnen op";
                string[] splittedb = convertToDate(tournament.matches_start_at.Value).Split(' ');
                newDef7.Value = splittedb[0] + " " + splittedb[1];
                newDef7.Inline = true;
                deflist.Add(newDef7);
            }
            if (tournament.end_at.HasValue){
                DEF newDef7 = new DEF();
                newDef7.Name = "Matchen eindigen op";
                string[] splittedb = convertToDate(tournament.end_at.Value).Split(' ');
                newDef7.Value = splittedb[0] + " " + splittedb[1];
                newDef7.Inline = true;
                deflist.Add(newDef7);
            }
            if (tournament.min_players_count > 0){
                DEF newDef7 = new DEF();
                newDef7.Name = "Minimum spelers vereist";
                newDef7.Value = tournament.min_players_count.ToString();
                newDef7.Inline = true;
                deflist.Add(newDef7);
            }
            if (tournament.prize_description != null){
                if (tournament.prize_description.Length > 0){
                    DEF newDef7 = new DEF();
                    newDef7.Name = "Prijsbeschrijving";
                    newDef7.Value = tournament.prize_description;
                    newDef7.Inline = true;
                    deflist.Add(newDef7);
                }
            }
            if (tournament.fee != null){
                if (tournament.fee.amount > 0){
                    DEF newDef7 = new DEF();
                    newDef7.Name = "Inschrijvingsgeld";
                    newDef7.Value = tournament.fee.amount.ToString() + (tournament.fee.currency != null ? (tournament.fee.currency.Length > 0 ? " (" + tournament.fee.currency + ")" : string.Empty) : string.Empty);
                    newDef7.Inline = true;
                    deflist.Add(newDef7);
                }
            }
            if (tournament.winner_award != null){
                if (tournament.winner_award.amount > 0){
                    DEF newDef7 = new DEF();
                    newDef7.Name = "Winnaarsgeld";
                    newDef7.Value = tournament.winner_award.amount.ToString() + (tournament.winner_award.currency != null ? (tournament.winner_award.currency.Length > 0 ? " (" + tournament.winner_award.currency + ")" : string.Empty) : string.Empty);
                    newDef7.Inline = true;
                    deflist.Add(newDef7);
                }
            }
            if (tournament.media_Links != null){
                if (tournament.media_Links.url.Length > 0){
                    DEF newDef7 = new DEF();
                    newDef7.Name = "Extra media link";
                    newDef7.Value = "[" + tournament.media_Links.url.Replace('_', Bot.UNDERSCORE_REPLACEMENT_CHAR) + "](" + tournament.media_Links.url + ")";
                    newDef7.Inline = true;
                    deflist.Add(newDef7);
                }
            }

            if (tournament.stages != null){
                int hoogsteTier = 0;
                int laagsteTier = 0;
                int bestOff = 0;
                string type = string.Empty;
                string state = string.Empty;
                foreach (Stage stage in tournament.stages){
                    if (hoogsteTier < stage.max_tier){
                        hoogsteTier = stage.max_tier;
                        if (laagsteTier == 0){
                            laagsteTier = hoogsteTier;
                        }
                    }
                    if (laagsteTier < stage.min_tier){
                        laagsteTier = stage.min_tier;
                    }
                    if (bestOff == 0){
                        bestOff = stage.battle_limit;
                    }
                    if (stage.type != null){
                        if (stage.type.Length > 0){
                            if (type.Length > 0){
                                switch (stage.type.ToLower()){
                                    case "rr":
                                        type = "Round robin";
                                        break;
                                    case "se":
                                        type = "Single elimination";
                                        break;
                                    case "de":
                                        type = "Double elimination";
                                        break;
                                }
                            }
                        }
                    }
                    if (stage.state != null){
                        if (stage.state.Length > 0){
                            if (state.Length > 0){
                                state = stage.state.Replace("_", string.Empty);
                            }
                        }
                    }
                }
                if (hoogsteTier > 0){
                    string tiers = Emoj.getName(laagsteTier) + (laagsteTier != hoogsteTier ? " tot " + Emoj.getName(hoogsteTier) : string.Empty);
                    DEF newDef3 = new DEF();
                    newDef3.Name = "Tiers";
                    newDef3.Value = tiers;
                    newDef3.Inline = true;
                    deflist.Add(newDef3);
                }
                if (type.Length > 0){
                    DEF newDef3 = new DEF();
                    newDef3.Name = "Type";
                    newDef3.Value = type;
                    newDef3.Inline = true;
                    deflist.Add(newDef3);
                }
                if (state.Length > 0){
                    DEF newDef3 = new DEF();
                    newDef3.Name = "Staat";
                    newDef3.Value = state;
                    newDef3.Inline = true;
                    deflist.Add(newDef3);
                }
                if (bestOff > 0){
                    DEF newDef3 = new DEF();
                    newDef3.Name = "Best of";
                    newDef3.Value = bestOff.ToString();
                    newDef3.Inline = true;
                    deflist.Add(newDef3);
                }
            }

            if (tournament.rules != null){
                if (tournament.rules.Length > 0){
                    bool voegToe = true;
                    if (tournament.description != null){
                        if (tournament.rules.Equals(tournament.description)){
                            voegToe = false;
                        }
                    }
                    if (voegToe){
                        string tempRules = tournament.rules.adaptDiscordLink().adaptToDiscordChat().adaptMutlipleLines();
                        if (tempRules.Length > 1024){
                            StringBuilder sbRules = new StringBuilder();
                            string[] splitted = tournament.description.Split('\n');
                            for (int i = 0; i < splitted.Length; i++){
                                if (splitted[i].EndsWith(':')){
                                    bool firstLine = true;
                                    StringBuilder sbTemp = new StringBuilder();
                                    for (int j = i + 1; j < splitted.Length; j++){
                                        if (splitted[j].Length == 0 && !firstLine){
                                            DEF newDefx = new DEF();
                                            newDefx.Name = splitted[i].adaptToDiscordChat();
                                            newDefx.Value = sbTemp.ToString().adaptDiscordLink().adaptToDiscordChat();
                                            newDefx.Inline = true;
                                            deflist.Add(newDefx);
                                            i = j - 1;
                                            break;
                                        }
                                        else{
                                            sbTemp.AppendLine(splitted[j]);
                                        }
                                        if (firstLine){
                                            firstLine = false;
                                        }
                                        if (j + 1 == splitted.Length){
                                            DEF newDefx = new DEF();
                                            newDefx.Name = splitted[i].adaptToDiscordChat();
                                            newDefx.Value = sbTemp.ToString().adaptDiscordLink().adaptToDiscordChat();
                                            newDefx.Inline = true;
                                            deflist.Add(newDefx);
                                            i = j;
                                        }
                                    }
                                }
                                else{
                                    sbRules.AppendLine(splitted[i]);
                                }
                            }
                            tempRules = sbRules.ToString().adaptDiscordLink().adaptToDiscordChat().adaptMutlipleLines();

                        }
                        DEF newDef4 = new DEF();
                        newDef4.Name = "Regels";
                        newDef4.Value = tempRules;
                        newDef4.Inline = true;
                        deflist.Add(newDef4);
                    }
                }
            }
            string tempDescription = tournament.description.adaptDiscordLink().adaptToDiscordChat().adaptMutlipleLines();
            if (tempDescription.Length > 1024){
                StringBuilder sbDescription = new StringBuilder();
                string[] splitted = tournament.description.Split('\n');
                for (int i = 0; i < splitted.Length; i++){
                    if (splitted[i].EndsWith(':')){
                        bool firstLine = true;
                        StringBuilder sbTemp = new StringBuilder();
                        for (int j = i + 1; j < splitted.Length; j++){
                            if (splitted[j].Length == 0 && !firstLine){
                                DEF newDefx = new DEF();
                                newDefx.Name = splitted[i].adaptToDiscordChat();
                                newDefx.Value = sbTemp.ToString().adaptDiscordLink().adaptToDiscordChat();
                                newDefx.Inline = true;
                                deflist.Add(newDefx);
                                i = j - 1;
                                break;
                            }
                            else{
                                sbTemp.AppendLine(splitted[j]);
                            }
                            if (firstLine){
                                firstLine = false;
                            }
                            if (j + 1 == splitted.Length){
                                DEF newDefx = new DEF();
                                newDefx.Name = splitted[i].adaptToDiscordChat();
                                newDefx.Value = sbTemp.ToString().adaptDiscordLink().adaptToDiscordChat();
                                newDefx.Inline = true;
                                deflist.Add(newDefx);
                                i = j;
                            }
                        }
                    }
                    else{
                        sbDescription.AppendLine(splitted[i]);
                    }
                }
                tempDescription = sbDescription.ToString().adaptDiscordLink().adaptToDiscordChat().adaptMutlipleLines();

            }
            if (tempDescription.Length <= 1024){
                DEF newDef3 = new DEF();
                newDef3.Name = "Toernooi beschrijving";
                newDef3.Value = tempDescription;
                newDef3.Inline = false;
                deflist.Add(newDef3);
            }

            await Bot.CreateEmbed(channel, string.Empty, string.Empty, titel, string.Empty, deflist, null, (tournament.logo != null ? (tournament.logo.original != null ? tournament.logo.original : string.Empty) : string.Empty), null);
        }

        public static string convertToDate(DateTimeOffset date)
        {
            string theDate = date.Day.ToString() + "-" + (date.Month < 10 ? "0" : "") + date.Month.ToString() + "-" + date.Year.ToString() + " " + date.Hour.ToString() + ":" + (date.Minute < 10 ? "0" : "") + date.Minute.ToString() + ":" + (date.Second < 10 ? "0" : "") + date.Second.ToString();
            return convertToDate(theDate);
        }
        public static string convertToDate(DateTime dateTime)
        {
            string theDate = dateTime.Day.ToString() + "-" + (dateTime.Month < 10 ? "0" : "") + dateTime.Month.ToString() + "-" + dateTime.Year.ToString() + " " + dateTime.Hour.ToString() + ":" + (dateTime.Minute < 10 ? "0" : "") + dateTime.Minute.ToString() + ":" + (dateTime.Second < 10 ? "0" : "") + dateTime.Second.ToString();
            return convertToDate(theDate);
        }
        public static DateTime convertToDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
        private static string convertToDate(string date)
        {
            string[] splitted = date.Replace('/', '-').Split(' ');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < splitted.Length; i++){
                if (i < 2){
                    if (i > 0){
                        sb.Append(' ');
                    }
                    sb.Append(splitted[i]);
                }
            }
            return sb.ToString();
        }
        public static DateTime convertStringToDateTime(string date)
        {
            date = convertToDate(date);
            string[] splitted = date.Split(' ');
            string[] firstPart = splitted[0].Split('-');
            string[] secondPart = splitted[1].Split(':');
            return new DateTime(Convert.ToInt32(firstPart[0]), Convert.ToInt32(firstPart[1]), Convert.ToInt32(firstPart[2]), Convert.ToInt32(secondPart[0]), Convert.ToInt32(secondPart[1]), Convert.ToInt32(secondPart[2]));
        }
        public static bool compareDateTime(DateTime x, DateTime y)
        {
            TimeSpan tempTimeSpan = x.Subtract(y);
            if (tempTimeSpan.Hours.Equals(0) && tempTimeSpan.Minutes.Equals(0) && tempTimeSpan.Seconds.Equals(0) && x.Year.Equals(y.Year) && x.Month.Equals(y.Month) && x.Day.Equals(y.Day)){
                return true;
            }
            else{
                return false;
            }
        }

        public static async Task<DiscordMessage> SayCannotBePlayedAt(DiscordChannel channel, DiscordMember member, string guildName)
        {
            return await Bot.SendMessage(channel, member, guildName, "**De battle moet meetellen voor je statistieken!**");
        }
        public static async Task<DiscordMessage> SayCannotBePlayedAt(DiscordChannel channel, DiscordMember member, string guildName, string roomType)
        {
            if (roomType.Length == 0){
                roomType = "deze";
                return await member.SendMessageAsync("Geef aub even door welk type room dit is want het werd niet herkent door de bot. Tag gebruiker thibeastmo#9998");
            }
            return await Bot.SendMessage(channel, member, guildName, "**De battle mag niet in een " + roomType + " room gespeeld zijn!**");
        }
        public static async Task SaySomethingWentWrong(DiscordChannel channel, DiscordMember member, string guildName)
        {
            await SaySomethingWentWrong(channel, member, guildName, "**Er ging iets mis, probeer het opnieuw!**");
        }
        public static async Task<DiscordMessage> SaySomethingWentWrong(DiscordChannel channel, DiscordMember member, string guildName, string text)
        {
            return await Bot.SendMessage(channel, member, guildName, text);
        }
        public static async Task SayWrongAttachments(DiscordChannel channel, DiscordMember member, string guildName)
        {
            await Bot.SendMessage(channel, member, guildName, "**Geen bruikbare documenten in de bijlage gevonden!**");
        }
        public static async Task SayNoAttachments(DiscordChannel channel, DiscordMember member, string guildName)
        {
            await Bot.SendMessage(channel, member, guildName, "**Geen documenten in de bijlage gevonden!**");
        }
        public static async Task SayNoResponse(DiscordChannel channel)
        {
            await channel.SendMessageAsync("`Time-out: Geen antwoord.`");
        }
        public static async Task SayNoResponse(DiscordChannel channel, DiscordMember member, string guildName)
        {
            await Bot.SendMessage(channel, member, guildName, "`Time-out: Geen antwoord.`");
        }
        public static async Task SayMustBeNumber(DiscordChannel channel)
        {
            await channel.SendMessageAsync("**Je moest een cijfer geven!**");
        }
        public static async Task SayNumberTooSmall(DiscordChannel channel)
        {
            await channel.SendMessageAsync("**Dat cijfer was te klein!**");
        }
        public static async Task SayNumberTooBig(DiscordChannel channel)
        {
            await channel.SendMessageAsync("**Dat cijfer was te groot!**");
        }
        public static async Task SayBeMoreSpecific(DiscordChannel channel, DiscordMember member, string guildName)
        {
            await Bot.CreateEmbed(channel, string.Empty, string.Empty, "Wees specifieker", "Er waren te veel resultaten, probeer iets specifieker te zijn!", null, null, string.Empty, null);
        }
        public static DiscordMessage SayMultipleResults(DiscordChannel channel, string description)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Meerdere resultaten gevonden";
            newDiscEmbedBuilder.Description = description.adaptToDiscordChat();
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                return channel.SendMessageAsync(null, embed).Result;
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace).Wait();
                Console.ForegroundColor = ConsoleColor.Gray;
                return null;
            }
        }
        public static async Task SayNoResults(DiscordChannel channel, string description)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Geen resultaten gevonden";
            newDiscEmbedBuilder.Description = description.Replace('_', '▁');
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                await channel.SendMessageAsync(null, embed);
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                await handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public static async Task SayTheUserIsNotAllowed(DiscordChannel channel)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Geen toegang";
            newDiscEmbedBuilder.Description = ":raised_back_of_hand: Je hebt niet voldoende rechten om deze commando uit te voeren!";
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                await channel.SendMessageAsync(null, embed);
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                await handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public static async Task SayBotNotAuthorized(DiscordChannel channel)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Onvoldoende rechten";
            newDiscEmbedBuilder.Description = ":raised_back_of_hand: De bot heeft voldoende rechten om dit uit te voeren!";
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                await channel.SendMessageAsync(null, embed);
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                await handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public static async Task SayTooManyCharacters(DiscordChannel channel)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Onvoldoende rechten";
            newDiscEmbedBuilder.Description = ":raised_back_of_hand: Er zaten te veel characters in het bericht dat de bot wilde verzenden!";
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            try{
                await channel.SendMessageAsync(null, embed);
            }
            catch (Exception ex){
                Console.ForegroundColor = ConsoleColor.Red;
                await handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public static async Task<DiscordMessage> SayReplayNotWorthy(DiscordChannel channel, WGBattle battle)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Helaas...";
            string extraDescription = await getDescriptionForReplay(battle, 0);
            newDiscEmbedBuilder.Description = "De statistieken van deze replay waren onvoldoende om in de Hall Of Fame te komen te staan!\n\n" + extraDescription;
            List<Tuple<string, string>> images = await Bot.getAllMaps(channel.Guild.Id);
            foreach (Tuple<string, string> map in images){
                if (map.Item1.ToLower() == battle.map_name.ToLower()){
                    try{
                        if (map.Item1 != string.Empty){
                            newDiscEmbedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
                            newDiscEmbedBuilder.Thumbnail.Url = map.Item2;
                        }
                    }
                    catch (Exception ex){
                        await handleError("Could not set thumbnail for embed:", ex.Message, ex.StackTrace);
                    }
                    break;
                }
            }
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            if (Bot.discordMessage != null){
                try{
                    return await Bot.discordMessage.RespondAsync(null, embed);
                    //return await channel.SendMessageAsync(string.Empty, false, embed);
                }
                catch (Exception ex){
                    Console.ForegroundColor = ConsoleColor.Red;
                    await handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else{
                return await channel.SendMessageAsync(embed: embed);
            }
            return null;
        }
        public static async Task<DiscordMessage> SayReplayIsWorthy(DiscordChannel channel, WGBattle battle, int position)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = DiscordColor.Red;
            newDiscEmbedBuilder.Title = "Hoera! :trophy:";
            string extraDescription = await getDescriptionForReplay(battle, position);
            newDiscEmbedBuilder.Description = "Je replay heeft een plaatsje gekregen in onze Hall Of Fame!\n\n" + extraDescription;
            List<Tuple<string, string>> images = await Bot.getAllMaps(channel.Guild.Id);
            foreach (Tuple<string, string> map in images){
                if (map.Item1.ToLower() == battle.map_name.ToLower()){
                    try{
                        if (map.Item1 != string.Empty){
                            newDiscEmbedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
                            newDiscEmbedBuilder.Thumbnail.Url = map.Item2;
                        }
                    }
                    catch (Exception ex){
                        await handleError("Could not set thumbnail for embed:", ex.Message, ex.StackTrace);
                    }
                    break;
                }
            }
            DiscordEmbed embed = newDiscEmbedBuilder.Build();
            if (Bot.discordMessage != null){
                try{
                    return await Bot.discordMessage.RespondAsync(null, embed);
                    //return await channel.SendMessageAsync(string.Empty, false, embed);
                }
                catch (Exception ex){
                    Console.ForegroundColor = ConsoleColor.Red;
                    await handleError("Something went wrong while trying to send an embedded message:", ex.Message, ex.StackTrace);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            {
                return await channel.SendMessageAsync(embed: embed);
            }
        }
        private static async Task<string> getDescriptionForReplay(WGBattle battle, int position, string preDescription = "")
        {
            StringBuilder sb = new StringBuilder(preDescription);
            try{
                WeeklyEventHandler weeklyEventHandler = new WeeklyEventHandler();
                string weeklyEventDescription = await weeklyEventHandler.GetStringForWeeklyEvent(battle);
                if (weeklyEventDescription.Length > 0){
                    sb.Append(Environment.NewLine + weeklyEventDescription);
                }
            }
            catch (Exception ex){
                await handleError("Tijdens het nakijken van het wekelijkse event: ", ex.Message, ex.StackTrace);
            }
            sb.Append(getSomeReplayInfoAsText(battle, position).Replace(REPLACEABLE_UNDERSCORE_CHAR, '_'));
            return sb.ToString();
        }
        private static string getSomeReplayInfoAsText(WGBattle battle, int position)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(getInfoInFormat("Link", "[" + battle.title.adaptToDiscordChat().Replace('_', UNDERSCORE_REPLACEMENT_CHAR) + "](" + battle.view_url.adaptToDiscordChat() + ")", false));
            sb.AppendLine(getInfoInFormat("Speler", battle.player_name.adaptToDiscordChat()));
            sb.AppendLine(getInfoInFormat("Clan", battle.details.clan_tag));
            sb.AppendLine(getInfoInFormat("Tank", battle.vehicle));
            sb.AppendLine(getInfoInFormat("Tier", Emoj.getName(battle.vehicle_tier), false));
            sb.AppendLine(getInfoInFormat("Damage", battle.details.damage_made.ToString()));
            sb.AppendLine(getInfoInFormat("Damage bounced", battle.details.damage_blocked.ToString()));
            sb.AppendLine(getInfoInFormat("Assist damage", (battle.details.damage_assisted + battle.details.damage_assisted_track).ToString()));
            sb.AppendLine(getInfoInFormat("exp", battle.details.exp.ToString()));
            sb.AppendLine(getInfoInFormat("Hits", battle.details.shots_pen.ToString()));
            sb.AppendLine(getInfoInFormat("Tanks vernietigd", battle.details.enemies_destroyed.ToString()));
            sb.AppendLine(getInfoInFormat("Map", battle.map_name));
            string resultaat = "Gewonnen";
            if (battle.protagonist_team != battle.winner_team){
                if (battle.winner_team != 2 && battle.winner_team != 1){
                    resultaat = "Gelijk gespeeld";
                }
                else{
                    resultaat = "Verloren";
                }
            }
            sb.AppendLine(getInfoInFormat("Resultaat", resultaat));
            if (battle.battle_start_time.HasValue){
                sb.AppendLine(getInfoInFormat("Datum", (battle.battle_start_time.Value.Day < 10 ? "0" : string.Empty) + battle.battle_start_time.Value.Day + "-" + battle.battle_start_time.Value.Month + "-" + battle.battle_start_time.Value.Year + " " + battle.battle_start_time.Value.Hour + ":" + (battle.battle_start_time.Value.Minute < 10 ? "0" : string.Empty) + battle.battle_start_time.Value.Minute + ":" + (battle.battle_start_time.Value.Second < 10 ? "0" : string.Empty) + battle.battle_start_time.Value.Second));
            }
            sb.AppendLine(getInfoInFormat("Type", WGBattle.getBattleType(battle.battle_type)));
            sb.AppendLine(getInfoInFormat("Mode", WGBattle.getBattleRoom(battle.room_type)));
            if (position > 0){
                sb.AppendLine(getInfoInFormat("Positie in HOF", position.ToString()));
            }
            if (battle.details.achievements != null && battle.details.achievements.Count > 0){
                List<FMWOTB.Achievement> achievementList = new List<FMWOTB.Achievement>();
                for (int i = 0; i < battle.details.achievements.Count; i++){
                    FMWOTB.Achievement tempAchievement = FMWOTB.Achievement.getAchievement(Bot.WG_APPLICATION_ID, battle.details.achievements.ElementAt(i).t).Result;
                    if (tempAchievement != null){
                        achievementList.Add(tempAchievement);
                    }
                }
                if (achievementList.Count > 0){
                    achievementList = achievementList.OrderBy(x => x.order).ToList();
                    sb.AppendLine("Achievements:");
                    sb.Append("```");
                    foreach (FMWOTB.Achievement tempAchievement in achievementList){
                        sb.AppendLine(tempAchievement.name.Replace("\n", string.Empty).Replace("(" + tempAchievement.achievement_id + ")", string.Empty));
                    }
                    sb.Append("```");
                }
            }
            return sb.ToString();
        }
        private static string getInfoInFormat(string key, string value, bool bold = true)
        {
            if (value != null){
                if (bold){
                    if (value != string.Empty){
                        value = "**" + value + "**";
                    }
                }
            }
            return key + ": " + (value != null ? value : string.Empty);
        }


        public static async Task<WGClan> searchForClan(DiscordChannel channel, DiscordMember member, string guildName, string clan_naam, bool loadMembers, DiscordUser user, Command command)
        {
            try{
                var clans = await WGClan.searchByName(SearchAccuracy.STARTS_WITH_CASE_INSENSITIVE, clan_naam, Bot.WG_APPLICATION_ID, loadMembers);
                int aantalClans = clans.Count;
                List<WGClan> clanList = new List<WGClan>();
                foreach (var clan in clans){
                    if (clan_naam.ToLower().Equals(clan.tag.ToLower())){
                        clanList.Add(clan);
                    }
                }

                if (clanList.Count > 1){
                    StringBuilder sbFound = new StringBuilder();
                    for (int i = 0; i < clanList.Count; i++){
                        sbFound.AppendLine((i + 1) + ". `" + clanList[i].tag + "`");
                    }
                    if (sbFound.Length < 1024){
                        int index = await waitForReply(channel, user, clan_naam, clanList.Count);
                        if (index >= 0){
                            return clanList[index];
                        }
                    }
                    else{
                        await Bot.SayBeMoreSpecific(channel, member, guildName);
                    }
                }
                else if (clanList.Count == 1){
                    return clanList[0];
                }
                else if (clanList.Count == 0){
                    await Bot.SendMessage(channel, member, guildName, "**Clan(" + clan_naam + ") is niet gevonden! (In een lijst van " + aantalClans + " clans)**");
                }
            }
            catch (TooManyResultsException e){
                Bot.discordClient.Logger.LogWarning("(" + command.Name + ") " + e.Message);
                await Bot.SendMessage(channel, member, guildName, "**Te veel resultaten waren gevonden, wees specifieker!**");
            }
            return null;
        }

        public static async Task<int> waitForReply(DiscordChannel channel, DiscordUser user, string description, int count)
        {
            DiscordMessage discMessage = Bot.SayMultipleResults(channel, description);
            var interactivity = discordClient.GetInteractivity();
            InteractivityResult<DiscordMessage> message = await interactivity.WaitForMessageAsync(x => x.Channel == channel && x.Author == user);
            if (!message.TimedOut){
                bool isInt = false;
                int number = -1;
                try{
                    number = Convert.ToInt32(message.Result.Content);
                    isInt = true;
                }
                catch{
                    isInt = false;
                }
                if (isInt){
                    if (number > 0 && number <= count){
                        return (number - 1);
                        //return itemList[(number - 1)];
                    }
                    else if (number > count){
                        await Bot.SayNumberTooBig(channel);
                    }
                    else if (1 > number){
                        await Bot.SayNumberTooSmall(channel);
                    }
                }
                else{
                    await Bot.SayMustBeNumber(channel);
                }
            }
            else if (discMessage != null){
                List<DiscordEmoji> reacted = new List<DiscordEmoji>();
                for (int i = 1; i <= 10; i++){
                    DiscordEmoji emoji = Bot.getDiscordEmoji(Emoj.getName(i));
                    if (emoji != null){
                        var users = discMessage.GetReactionsAsync(emoji).Result;
                        foreach (var tempUser in users){
                            if (tempUser.Id.Equals(user.Id)){
                                reacted.Add(emoji);
                            }
                        }
                    }
                }

                if (reacted.Count == 1){
                    int index = Emoj.getIndex(Bot.getEmojiAsString(reacted[0].Name));
                    if (index > 0 && index <= count){
                        return (index - 1);
                        //return itemList[(index - 1)];
                    }
                    else{
                        await channel.SendMessageAsync("**Dat was geen van de optionele emoji's!**");
                    }
                }
                else if (reacted.Count > 1){
                    await channel.SendMessageAsync("**Je mocht maar 1 reactie geven!**");
                }
                else{
                    await Bot.SayNoResponse(channel);
                }
            }
            else{
                await Bot.SayNoResponse(channel);
            }
            return -1;
        }

        public static List<DEF> listInMemberEmbed(int columns, List<DiscordMember> memberList)
        {
            return listInMemberEmbed(columns, memberList, string.Empty);
        }
        public static List<DEF> listInMemberEmbed(int columns, List<DiscordMember> memberList, string searchTerm)
        {
            var backupMemberList = new List<DiscordMember>();
            backupMemberList.AddRange(memberList);
            var sbs = new List<StringBuilder>();
            for (int i = 0; i < columns; i++){
                sbs.Add(new StringBuilder());
            }
            int counter = 0;
            int columnCounter = 0;
            int rest = memberList.Count % columns;
            int membersPerColumn = (memberList.Count - rest) / columns;
            int amountOfMembers = memberList.Count;
            if (amountOfMembers > 0){
                while (memberList.Count > 0){
                    try{
                        if (searchTerm.ToLower().Contains('b')){
                            sbs[columnCounter].AppendLine(memberList[0].DisplayName.adaptToDiscordChat());
                        }
                        else{
                            sbs[columnCounter].AppendLine(memberList[0].Username + "#" + memberList[0].Discriminator);
                        }

                        //hier

                        memberList.RemoveAt(0);

                        if (counter == (membersPerColumn + (columnCounter == columns - 1 ? rest : 0)) || memberList.Count == 1){
                            if (columnCounter < (columns - 1)){
                                columnCounter++;
                            }
                            counter = 0;
                        }
                        else{
                            counter++;
                        }
                    }
                    catch (Exception ex){
                        handleError("Error in gebruikerslijst:", ex.Message, ex.StackTrace).Wait();
                    }
                }
                List<DEF> deflist = new List<DEF>();
                bool firstTime = true;
                foreach (StringBuilder item in sbs){
                    if (item.Length > 0 && item.Length > 0){
                        var defValue = item.ToString().adaptToDiscordChat();
                        if (defValue.Length > 1024){
                            return listInMemberEmbed(columns + 1, backupMemberList, searchTerm);
                        }
                        string[] splitted = item.ToString().Split(Environment.NewLine);
                        string firstChar = Bot.removeSyntaxe(splitted[0]).Substring(0, 1);
                        string lastChar = string.Empty;
                        string defName = string.Empty;
                        if (searchTerm.Contains('o') || searchTerm.Contains('c')){
                            if (firstTime){
                                firstTime = false;
                                defName = "Recentst";
                            }
                            else if (item.Equals(sbs[sbs.Count - 1])){
                                defName = "minst recent";
                            }
                            else{
                                defName = "minder recent";
                            }
                        }
                        else{
                            defName = firstChar.ToUpper() + (splitted.Length > 2 ? " - " + lastChar.ToUpper() : "");
                        }
                        if (splitted.Length > 2){
                            lastChar = Bot.removeSyntaxe(splitted[splitted.Length - 2]).Substring(0, 1);
                        }
                        DEF newDef = new DEF();
                        newDef.Inline = true;
                        newDef.Name = defName.adaptToDiscordChat();
                        newDef.Value = defValue;
                        deflist.Add(newDef);
                    }
                }
                return deflist;
            }
            return new List<DEF>();
        }
        public static List<DEF> listInPlayerEmbed(int columns, object memberList)
        {
            return listInPlayerEmbed(columns, memberList, string.Empty, null);
        }
        public static List<DEF> listInPlayerEmbed(int columns, object memberList, string searchTerm, DiscordGuild guild)
        {
            List<string> nameList = new List<string>();
            if (memberList is List<Members>){
                if (searchTerm.Contains('d')){
                    List<Members> memberListx = (List<Members>)memberList;
                    List<WGAccount> wgAccountList = new List<WGAccount>();
                    foreach (Members memberx in memberListx){
                        wgAccountList.Add(new WGAccount(Bot.WG_APPLICATION_ID, memberx.account_id, false, false, false));
                    }
                    wgAccountList = wgAccountList.OrderBy(p => p.last_battle_time).ToList();
                    wgAccountList.Reverse();
                    foreach (WGAccount memberx in wgAccountList){
                        nameList.Add(memberx.nickname);
                    }
                }
                else{
                    List<Members> memberListx = (List<Members>)memberList;
                    foreach (Members memberx in memberListx){
                        nameList.Add(memberx.account_name);
                    }
                }
            }
            else if (memberList is List<WGAccount>){
                List<WGAccount> memberListx = (List<WGAccount>)memberList;
                foreach (WGAccount memberx in memberListx){
                    nameList.Add(memberx.nickname);
                }
            }

            var sbs = new List<StringBuilder>();
            for (int i = 0; i < columns; i++){
                sbs.Add(new StringBuilder());
            }
            int counter = 0;
            int columnCounter = 0;
            int rest = nameList.Count % columns;
            int membersPerColumn = (nameList.Count - rest) / columns;
            int amountOfMembers = nameList.Count;
            if (amountOfMembers > 0){
                IReadOnlyCollection<DiscordMember> members = new List<DiscordMember>();
                if (searchTerm.Contains('s')){
                    members = guild.GetAllMembersAsync().Result;
                }
                while (nameList.Count > 0){
                    try{
                        if (searchTerm.Contains('s')){
                            bool found = false;
                            foreach (var memberx in members){
                                string[] splittedName = memberx.DisplayName.Split(']');
                                if (splittedName.Length > 1){
                                    string tempName = splittedName[1].Trim(' ');
                                    if (tempName.ToLower().Equals(nameList[0].ToLower())){
                                        sbs[columnCounter].AppendLine("`" + nameList[0] + "`");
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found){
                                sbs[columnCounter].AppendLine("**" + nameList[0].adaptToDiscordChat() + "**");
                            }
                        }
                        else{
                            sbs[columnCounter].AppendLine(nameList[0].adaptToDiscordChat());
                        }

                        nameList.RemoveAt(0);

                        if (counter == (membersPerColumn + (columnCounter == columns - 1 ? rest : 0)) || nameList.Count == 1){
                            if (columnCounter < (columns - 1)){
                                columnCounter++;
                            }
                            counter = 0;
                        }
                        else{
                            counter++;
                        }
                    }
                    catch (Exception ex){
                        handleError("Error in listInPlayerEmbed:", ex.Message, ex.StackTrace).Wait();
                    }
                }

                List<DEF> deflist = new List<DEF>();
                bool firstTime = true;
                foreach (var item in sbs){
                    if (item.Length > 0 && item.Length > 0){
                        string[] splitted = item.ToString().Split(Environment.NewLine);
                        string firstChar = Bot.removeSyntaxe(splitted[0]).Substring(0, 1);
                        string lastChar = string.Empty;
                        for (int i = splitted.Length - 1; i > 0; i--){
                            if (splitted[i] != string.Empty){
                                lastChar = Bot.removeSyntaxe(splitted[i]).ToUpper().First().ToString();
                                break;
                            }
                        }
                        string defName = string.Empty;
                        if (searchTerm.Contains('d')){
                            if (firstTime){
                                firstTime = false;
                                defName = "Recentst";
                            }
                            else if (item.Equals(sbs[sbs.Count - 1])){
                                defName = "minst recent";
                            }
                            else{
                                defName = "minder recent";
                            }
                        }
                        else{
                            defName = (firstChar.ToUpper() + (splitted.Length > 2 ? " - " + lastChar.ToUpper() : ""));
                        }
                        DEF newDef = new DEF();
                        newDef.Inline = true;
                        newDef.Name = defName.adaptToDiscordChat();
                        newDef.Value = item.ToString();
                        deflist.Add(newDef);
                    }
                }
                return deflist;
            }
            return new List<DEF>();
        }

        public static List<string> getSearchTermAndCondition(params string[] parameter)
        {
            string searchTerm = string.Empty;
            string conditie = string.Empty;
            if (parameter.Length > 1){
                // -s --> duid discordmembers aan met ``
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < parameter.Length; i++){
                    if (i == 0){
                        if (parameter[0].StartsWith('-')){
                            searchTerm = parameter[0];
                        }
                        else{
                            sb.Append(parameter[0]);
                        }
                    }
                    else{
                        if (sb.Length > 0){
                            sb.Append(' ');
                        }
                        sb.Append(parameter[i]);
                    }
                }
                conditie = sb.ToString();
            }
            else if (parameter.Length == 1){
                conditie = parameter[0];
            }
            List<string> temp = new List<string>();
            temp.Add(searchTerm);
            temp.Add(conditie);
            return temp;
        }

        public static async Task<List<Tuple<string, string>>> getAllMaps(ulong GuildID)
        {
            DiscordChannel mapChannel = await Bot.GetMappenChannel(GuildID);
            if (mapChannel != null){
                List<Tuple<string, string>> images = new List<Tuple<string, string>>();
                try{
                    var xMessages = mapChannel.GetMessagesAsync(100).Result;
                    foreach (var message in xMessages){
                        var attachments = message.Attachments;
                        foreach (var item in attachments){
                            images.Add(new Tuple<string, string>(Bot.getProperFileName(item.Url), item.Url));
                        }
                    }
                }
                catch (Exception ex){
                    await handleError("Could not load messages from " + mapChannel.Name + ":", ex.Message, ex.StackTrace);
                }
                images.Sort((x, y) => y.Item1.CompareTo(x.Item1));
                images.Reverse();
                return images;
            }
            return null;
        }

        public static Tuple<string, string> getIGNFromMember(string displayName)
        {
            string[] splitted = displayName.Split(']');
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < splitted.Length; i++){
                if (i > 1){
                    sb.Append(' ');
                }
                sb.Append(splitted[i]);
            }
            string clan = string.Empty;
            if (splitted.Length > 1){
                clan = splitted[0] + ']';
            }
            return new Tuple<string, string>(clan, sb.ToString().Trim(' '));
        }

        public static async Task<WGVehicle> getSpecificTank(long tank_id)
        {
            string jsonString = await WGVehicle.vehiclesToString(Bot.WG_APPLICATION_ID, tank_id);
            Json json = new Json(jsonString, "WGVehicle");
            foreach (Json subJson in json.subJsons){
                if (subJson.head.ToLower().Equals("data")){
                    foreach (Json subSubJson in subJson.subJsons){
                        return new WGVehicle(subSubJson);
                    }
                }
            }
            return null;
        }
        public static async Task<Dictionary<long, WGVehicle>> getAllTanks(DiscordChannel channel)
        {
            Dictionary<long, WGVehicle> vehicleList = new Dictionary<long, WGVehicle>();
            await channel.SendMessageAsync("**Gaat alle tanks verzamelen.**");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            string jsonString = await WGVehicle.vehiclesToString(Bot.WG_APPLICATION_ID);
            stopWatch.Stop();
            await channel.SendMessageAsync("**Response van Wargaming duurde: ** `" + (int)stopWatch.Elapsed.TotalSeconds + " s`\n**Gaat het antwoord omzetten naar een object.** (Kan even duren, circa 80s)");
            stopWatch.Restart();
            stopWatch.Start();
            Json json = new Json(jsonString, "WGVehicles");
            stopWatch.Stop();
            await channel.SendMessageAsync("**Antwoord is omgezet naar een object. Duurde: ** `" + (int)stopWatch.Elapsed.TotalSeconds + " s`\nNog even geduld.");
            foreach (Json subJson in json.subJsons){
                if (subJson.head.ToLower().Equals("data")){
                    foreach (Json subSubJson in subJson.subJsons){
                        WGVehicle vehicle = new WGVehicle(subSubJson);
                        vehicleList.Add(vehicle.tank_id, vehicle);
                    }
                }
            }
            return vehicleList;
        }

        public static async Task<WGAccount> searchPlayer(DiscordChannel channel, DiscordMember member, DiscordUser user, string guildName, string naam)
        {
            try{
                IReadOnlyList<WGAccount> searchResults = await WGAccount.searchByName(SearchAccuracy.STARTS_WITH_CASE_INSENSITIVE, naam, Bot.WG_APPLICATION_ID, false, false, true);
                StringBuilder sb = new StringBuilder();
                int index = 0;
                if (searchResults != null){
                    if (searchResults.Count > 1){
                        int counter = 0;
                        foreach (var account in searchResults){
                            counter++;
                            sb.AppendLine(counter + ". " + account.nickname.adaptToDiscordChat());
                        }
                        index = await Bot.waitForReply(channel, user, sb.ToString(), searchResults.Count);
                    }
                    if (index >= 0 && searchResults.Count >= 1){
                        WGAccount account = new WGAccount(Bot.WG_APPLICATION_ID, searchResults[index].account_id, false, true, true);
                        await Bot.showMemberInfo(channel, account);
                        return account;
                    }
                    else{
                        await Bot.SendMessage(channel, member, guildName, "**Gebruiker (**`" + naam + "`**) kon niet gevonden worden!**");
                    }
                }
                else{
                    await Bot.SendMessage(channel, member, guildName, "**Gebruiker (**`" + naam.adaptToDiscordChat() + "`**) kon niet gevonden worden!**");
                }
            }
            catch (TooManyResultsException e){
                Bot.discordClient.Logger.LogWarning("While searching for player by name: " + e.Message);
                await Bot.SendMessage(channel, member, guildName, "**Te veel resultaten waren gevonden, wees specifieker!**");
            }
            return null;
        }

        public static async Task<List<WGTournament>> initialiseTournaments(bool all)
        {
            string tournamentJson = await Tournaments.tournamentsToString(Bot.WG_APPLICATION_ID);
            Json json = new Json(tournamentJson, "Tournaments");
            List<WGTournament> tournamentsList = new List<WGTournament>();
            if (json != null){
                if (json.subJsons != null){
                    foreach (Json subjson in json.subJsons){
                        if (subjson.head.ToLower().Equals("data")){
                            foreach (Json subsubjson in subjson.subJsons){
                                Tournaments tournaments = new Tournaments(subsubjson);
                                if (tournaments.start_at.HasValue){
                                    if (tournaments.start_at.Value > DateTime.Now || all){
                                        string wgTournamentJsonString = await WGTournament.tournamentsToString(Bot.WG_APPLICATION_ID, tournaments.tournament_id);
                                        Json wgTournamentJson = new Json(wgTournamentJsonString, "WGTournament");
                                        WGTournament eenToernooi = new WGTournament(wgTournamentJson, Bot.WG_APPLICATION_ID);
                                        tournamentsList.Add(eenToernooi);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            tournamentsList.Reverse();
            return tournamentsList;
        }

        public static async Task<List<FMWOTB.Achievement>> getAllAchievements()
        {
            return await FMWOTB.Achievement.getAchievements(Bot.WG_APPLICATION_ID);
        }

        #region Hall Of Fame

        //Methods are set chronological
        public static async Task<Tuple<string, DiscordMessage>> handle(string titel, DiscordChannel channel, DiscordMember member, string guildName, ulong guildID, string url)
        {
            return await handle(titel, null, channel, guildName, guildID, url, member);
        }
        public static async Task<Tuple<string, DiscordMessage>> handle(string titel, DiscordChannel channel, DiscordMember member, string guildName, ulong guildID, DiscordAttachment attachment)
        {
            return await handle(titel, attachment, channel, guildName, guildID, null, member);
        }
        private static async Task<Tuple<string, DiscordMessage>> handle(string titel, object discAttach, DiscordChannel channel, string guildName, ulong guildID, string? iets, DiscordMember member)
        {
            if (discAttach is DiscordAttachment){
                discAttach = (DiscordAttachment)discAttach;
            }
            WGBattle replayInfo = await Bot.getReplayInfo(titel, discAttach, Bot.getIGNFromMember(member.DisplayName).Item2, iets);
            try{
                if (replayInfo != null){
                    bool validChannel = false;
                    if (guildID.Equals(DA_BOIS_ID)){
                        validChannel = true;
                    }
                    else{
                        DiscordChannel goodChannel = await Bot.GetMasteryReplaysChannel(guildID);
                        if (goodChannel != null){
                            if (goodChannel.Id.Equals(channel.Id)){
                                validChannel = true;
                            }
                        }
                        if (!validChannel){
                            goodChannel = await Bot.GetBottestChannel();
                            if (goodChannel.Id.Equals(channel.Id)){
                                validChannel = true;
                            }
                        }
                    }
                    if (validChannel){
                        return await goHOFDetails(replayInfo, channel, member, guildName, guildID);
                    }
                    else return new Tuple<string, DiscordMessage>("Kanaal is niet geschikt voor HOF.", null);
                }
                else return new Tuple<string, DiscordMessage>("Replayobject was null.", null);
            }
            catch{
                return new Tuple<string, DiscordMessage>("Er ging iets mis.", null);
            }
        }
        private static async Task<Tuple<string, DiscordMessage>> goHOFDetails(WGBattle replayInfo, DiscordChannel channel, DiscordMember member, string guildName, ulong guildID)
        {
            //battle_type 0 = encounter
            //battle_type 1 = supremacy
            //room_type 26 = Burning
            //room_type 25 = Skirmish
            //room_type 24 = Gravity Force
            //room_type 22 = realistic
            //room_type 23 = uprising
            //room_type 8 = mad games
            //room_type 7 = rating
            //room_type 5 = tournament
            //room_type 4 = tournament
            //room_type 1 = normal
            DiscordMessage tempMessage = (await channel.GetMessagesAsync(1))[0];
            if (replayInfo.battle_type == 0 || replayInfo.battle_type == 1){
                if (replayInfo.room_type == 1 || replayInfo.room_type == 5 || replayInfo.room_type == 7 || replayInfo.room_type == 4){
                    if (replayInfo != null){
                        try{
                            if (replayInfo.details != null){
                                return await Bot.replayHOF(replayInfo, guildID, channel, member, guildName);
                            }
                            else{
                                return new Tuple<string, DiscordMessage>("Replay bevatte geen details.", null);
                            }
                        }
                        catch (JsonNotFoundException e){
                            tempMessage = await Bot.SaySomethingWentWrong(channel, member, guildName, "**Er ging iets mis tijdens het inlezen van de gegevens!**");
                            await handleError("While reading json from a replay:\n", e.Message, e.StackTrace);
                        }
                        catch (Exception e){
                            tempMessage = await Bot.SaySomethingWentWrong(channel, member, guildName, "**Er ging iets mis bij het controleren van de HOF!**");
                            await handleError("While checking HOF with a replay:\n", e.Message, e.StackTrace);
                        }
                        tempMessage = await Bot.SendMessage(channel, member, guildName, "**Dit is een speciale replay waardoor de gegevens niet fatsoenlijk konden inlezen worden!**");
                    }
                    else{
                        tempMessage = await Bot.SaySomethingWentWrong(channel, member, guildName, "**Er ging iets mis tijdens het inlezen van de gegevens!**");
                    }
                    return new Tuple<string, DiscordMessage>(tempMessage.Content, tempMessage);
                }
                else if (replayInfo.room_type == 2){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "training");
                }
                else if (replayInfo.room_type == 26){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "burning");
                }
                else if (replayInfo.room_type == 25){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "skirmish");
                }
                else if (replayInfo.room_type == 24){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "gravity force");
                }
                else if (replayInfo.room_type == 23){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "uprising");
                }
                else if (replayInfo.room_type == 22){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "realistic");
                }
                else if (replayInfo.room_type == 8){
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, "mad games");
                }
                else{
                    tempMessage = await Bot.SayCannotBePlayedAt(channel, member, guildName, string.Empty);
                }
            }
            else{
                tempMessage = await Bot.SaySomethingWentWrong(channel, member, guildName, "**Je mag enkel de standaardbattles gebruiken! (Geen speciale gamemodes)**");
            }
            string thumbnail = string.Empty;
            List<Tuple<string, string>> images = await Bot.getAllMaps(channel.Guild.Id);
            foreach (Tuple<string, string> map in images){
                if (map.Item1.ToLower() == replayInfo.map_name.ToLower()){
                    try{
                        if (map.Item1 != string.Empty){
                            thumbnail = map.Item2;
                        }
                    }
                    catch (Exception ex){
                        await handleError("Could not set thumbnail for embed:", ex.Message, ex.StackTrace);
                    }
                    break;
                }
            }
            await Bot.CreateEmbed(channel, thumbnail, string.Empty, "Resultaat", await Bot.getDescriptionForReplay(replayInfo, -1), null, null, string.Empty, null);
            return new Tuple<string, DiscordMessage>(tempMessage.Content, tempMessage);
            //return new Tuple<string, DiscordMessage>(string.Empty, null);
        }
        public static async Task<WGBattle> getReplayInfo(string titel, object attachment, string ign, string? url)
        {
            string json = string.Empty;
            bool playerIDFound = false;
            IReadOnlyList<WGAccount> accountInfo = await WGAccount.searchByName(SearchAccuracy.EXACT, ign, Bot.WG_APPLICATION_ID, false, true, false);
            if (accountInfo != null){
                if (accountInfo.Count > 0){
                    playerIDFound = true;
                    if (attachment != null){
                        DiscordAttachment attach = (DiscordAttachment)attachment;
                        url = attach.Url;
                    }
                    json = await replayToString(url, titel, accountInfo[0].account_id);
                }
            }
            if (!playerIDFound){
                if (attachment != null){
                    DiscordAttachment attach = (DiscordAttachment)attachment;
                    url = attach.Url;
                }
                json = await replayToString(url, titel, null);
            }
            try{
                Json tempJson = new Json(json, "WGBattle");
                WGBattle battle = new WGBattle(json);
                return battle;
            }
            catch (Exception ex){
                string attachUrl = string.Empty;
                if (attachment != null){
                    DiscordAttachment attach = (DiscordAttachment)attachment;
                    attachUrl = attach.Url;
                }
                await handleError("Initializing WGBattle object from (" + (url != null && url.Length > 0 ? url : attachment != null ? attachUrl : "Nothing") + "):\n", ex.Message, ex.StackTrace);
            }
            return null;
        }
        public static async Task<string> replayToString(string pathOrKey, string title, long? wg_id)
        {
            string url = @"https://wotinspector.com/api/replay/upload?url=";
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form1 = new MultipartFormDataContent();
            String AsBase64String = null;
            if (pathOrKey.StartsWith("http")){
                if (pathOrKey.Contains("wotinspector")){
                    //return een json in deze else
                    HttpResponseMessage iets = await httpClient.GetAsync("https://api.wotinspector.com/replay/upload?details=full&key=" + Path.GetFileName(pathOrKey));
                    if (iets != null){
                        if (iets.Content != null){
                            return await iets.Content.ReadAsStringAsync();
                        }
                    }
                    return null;
                }
                else{
                    AsBase64String = Convert.ToBase64String(await httpClient.GetByteArrayAsync(pathOrKey));
                }
            }
            else if (pathOrKey.Contains('\\') || pathOrKey.Contains('/')){
                AsBase64String = Convert.ToBase64String(File.ReadAllBytes(pathOrKey));
            }

            form1.Add(new StringContent(Path.GetFileName(pathOrKey)), "filename");//filename
            form1.Add(new StringContent(AsBase64String), "file");
            if (title != null){
                if (!title.Equals(string.Empty)){
                    form1.Add(new StringContent(title), "title");//title
                }
            }
            if (wg_id != null){
                form1.Add(new StringContent(wg_id.ToString()), "loaded_by");
            }

            HttpResponseMessage response = await httpClient.PostAsync(url, form1);
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<Tuple<string, DiscordMessage>> replayHOF(WGBattle battle, ulong guildID, DiscordChannel channel, DiscordMember member, string guildName)
        {
            if (battle.details.clanid.Equals(NLBE_CLAN_ID) || battle.details.clanid.Equals(NLBE2_CLAN_ID)){
                DiscordMessage message = await getHOFMessage(guildID, battle.vehicle_tier, battle.vehicle);
                if (message != null){
                    List<Tuple<string, List<TankHof>>> tierHOF = convertHOFMessageToTupleListAsync(message, battle.vehicle_tier);
                    bool alreadyAdded = false;
                    if (tierHOF != null){
                        alreadyAdded = false;
                        foreach (Tuple<string, List<TankHof>> tank in tierHOF){
                            foreach (TankHof hof in tank.Item2){
                                if (Path.GetFileName(hof.link).Equals(battle.hexKey)){
                                    alreadyAdded = true;
                                    break;
                                }
                            }
                        }
                        if (!alreadyAdded){
                            foreach (Tuple<string, List<TankHof>> tank in tierHOF){
                                if (tank.Item1.ToLower().Equals(battle.vehicle.ToLower())){
                                    tank.Item2.OrderBy(x => x.damage);
                                    if (tank.Item2.Count == HOF_AMOUNT_PER_TANK){
                                        if (tank.Item2[HOF_AMOUNT_PER_TANK - 1].damage < battle.details.damage_made){
                                            tank.Item2.Add(initializeTankHof(battle));
                                            List<TankHof> sortedTankHofList = tank.Item2.OrderBy(x => x.damage).Reverse().ToList();
                                            sortedTankHofList.RemoveAt(sortedTankHofList.Count - 1);
                                            tank.Item2.Clear();
                                            int counter = 1;
                                            int position = 0;
                                            foreach (TankHof item in sortedTankHofList){
                                                tank.Item2.Add(item);
                                                if (item.link.Equals(battle.view_url)){
                                                    position = counter;
                                                }
                                                else{
                                                    counter++;
                                                }
                                                item.place = (short)position;
                                            }
                                            await editHOFMessage(message, tierHOF);
                                            DiscordMessage tempMessage = await Bot.SayReplayIsWorthy(channel, battle, position);
                                            return new Tuple<string, DiscordMessage>(tempMessage.Content, tempMessage);
                                        }
                                        else{
                                            DiscordMessage tempMessage = await Bot.SayReplayNotWorthy(channel, battle);
                                            return new Tuple<string, DiscordMessage>(tempMessage.Content, tempMessage);
                                        }
                                    }
                                    else{
                                        DiscordMessage tempMessage = await addReplayToMessage(battle, message, channel, tierHOF);
                                        return new Tuple<string, DiscordMessage>((tempMessage != null ? tempMessage.Content : string.Empty), tempMessage);
                                    }
                                }
                            }
                        }
                        else{
                            string thumbnail = string.Empty;
                            List<Tuple<string, string>> images = await Bot.getAllMaps(channel.Guild.Id);
                            foreach (Tuple<string, string> map in images){
                                if (map.Item1.ToLower() == battle.map_name.ToLower()){
                                    try{
                                        if (map.Item1 != string.Empty){
                                            thumbnail = map.Item2;
                                        }
                                    }
                                    catch (Exception ex){
                                        await handleError("Could not set thumbnail for embed:", ex.Message, ex.StackTrace);
                                    }
                                    break;
                                }
                            }
                            DiscordMessage tempMessage = await Bot.CreateEmbed(channel, thumbnail, string.Empty, "Helaas... Deze replay staat er al in.", await getDescriptionForReplay(battle, 0), null, null, string.Empty, null);
                            return new Tuple<string, DiscordMessage>(string.Empty, tempMessage);//string empty omdat dan hofafterupload het niet verkeerd opvat
                        }
                    }
                    if (tierHOF == null){
                        tierHOF = new List<Tuple<string, List<TankHof>>>();
                    }
                    if (!alreadyAdded){
                        DiscordMessage tempMessage = await addReplayToMessage(battle, message, channel, tierHOF);
                        return new Tuple<string, DiscordMessage>((tempMessage != null ? tempMessage.Content : string.Empty), tempMessage);
                    }
                }
                else{
                    DiscordMessage tempMessage = await Bot.SaySomethingWentWrong(channel, member, guildName, "**Het bericht van de tier van de replay kon niet gevonden worden!**");
                    return new Tuple<string, DiscordMessage>(tempMessage.Content, tempMessage);
                }
            }
            else{
                DiscordMessage tempMessage = await Bot.SaySomethingWentWrong(channel, member, guildName, "**Enkel replays van NLBE-clanleden mogen gebruikt worden!**");
                return new Tuple<string, DiscordMessage>(tempMessage.Content, tempMessage);
            }
            return null;
        }
        public static async Task<DiscordMessage> getHOFMessage(ulong GuildID, int tier, string vehicle)
        {
            DiscordChannel channel = await GetHallOfFameChannel(GuildID);
            if (channel != null){
                IReadOnlyList<DiscordMessage> messages = await channel.GetMessagesAsync(100);
                if (messages != null){
                    List<DiscordMessage> tierMessages = getTierMessages(tier, messages);
                    foreach (DiscordMessage tierMessage in tierMessages){
                        if (tierMessage.Embeds[0].Fields != null){
                            if (tierMessage.Embeds[0].Fields.Count > 0){
                                foreach (DiscordEmbedField field in tierMessage.Embeds[0].Fields){
                                    if (field.Name.Equals(vehicle)){
                                        return tierMessage;
                                    }
                                }
                            }
                        }
                    }
                    foreach (DiscordMessage tierMessage in tierMessages){
                        if (tierMessage.Embeds[0].Fields != null){
                            if (tierMessage.Embeds[0].Fields.Count > 0){
                                if (tierMessage.Embeds[0].Fields.Count < 15)//15 fields in embed
                                {
                                    return tierMessage;
                                }
                            }
                            else{
                                return tierMessage;
                            }
                        }
                        else{
                            return tierMessage;
                        }
                    }
                    //Tier exists but message is must be created (move all the lower tiers to the front)
                    //Get messages that should be moved
                    List<DiscordMessage> LTmessages = new List<DiscordMessage>();
                    foreach (DiscordMessage tierMessage in messages){
                        if (tierMessage.Embeds != null){
                            if (tierMessage.Embeds.Count > 0){
                                string emojiAsString = tierMessage.Embeds[0].Title.Replace("Tier ", string.Empty);
                                int index = Emoj.getIndex(Bot.getEmojiAsString(emojiAsString));
                                if (index < tier){
                                    LTmessages.Add(tierMessage);
                                }
                                else{
                                    break;
                                }
                            }
                        }
                    }
                    LTmessages.Reverse();
                    ulong messageToReturnID = 0;
                    //Move them
                    for (int i = 0; i <= LTmessages.Count; i++){
                        if (i == 0){
                            //set new message for the tier
                            await LTmessages[i].ModifyAsync(createHOFResetEmbed(tier));
                            messageToReturnID = LTmessages[i].Id;
                        }
                        else if (i == LTmessages.Count){
                            //Create new message for tier 1
                            await channel.SendMessageAsync(null, LTmessages[i - 1].Embeds[0]);
                        }
                        else{
                            //modify
                            await LTmessages[i].ModifyAsync(null, LTmessages[i - 1].Embeds[0]);
                        }
                    }
                    return await channel.GetMessageAsync(messageToReturnID);
                }
                return null;
            }
            else{
                return null;
            }
        }
        public static List<DiscordMessage> getTierMessages(int tier, IReadOnlyList<DiscordMessage> messages)
        {
            messages = messages.Reverse().ToList();
            List<DiscordMessage> tierMessages = new List<DiscordMessage>();
            foreach (DiscordMessage message in messages){
                if (message.Embeds != null){
                    if (message.Embeds.Count > 0){
                        if (message.Embeds[0].Title.Contains(getDiscordEmoji(Emoj.getName(tier)))){
                            tierMessages.Add(message);
                        }
                    }
                }
            }
            return tierMessages;
        }
        public static List<Tuple<string, List<TankHof>>> convertHOFMessageToTupleListAsync(DiscordMessage message, int TIER)
        {
            if (message.Embeds != null){
                if (message.Embeds.Count > 0){
                    foreach (DiscordEmbed embed in message.Embeds){
                        if (embed.Fields != null){
                            if (embed.Fields.Count > 0){
                                List<Tuple<string, List<TankHof>>> generatedTupleListFromMessage = new List<Tuple<string, List<TankHof>>>();
                                foreach (DiscordEmbedField field in embed.Fields){
                                    List<TankHof> hofList = new List<TankHof>();
                                    string[] lines = field.Value.Split('\n');
                                    short counter = -1;
                                    foreach (string line in lines){
                                        counter++;
                                        string speler = string.Empty;
                                        string link = string.Empty;
                                        string damage = string.Empty;
                                        bool firstTime = true;
                                        string[] splitted = line.Split(" `");
                                        splitted[1].Insert(0, "`");
                                        foreach (string item in splitted){
                                            if (firstTime){
                                                firstTime = false;
                                                string[] split = item.Split(']');
                                                StringBuilder sb = new StringBuilder();
                                                string[] firstPartSplitted = split[0].Split(' ');
                                                for (int i = 1; i < firstPartSplitted.Length; i++){
                                                    if (i > 1){
                                                        sb.Append(' ');
                                                    }
                                                    sb.Append(firstPartSplitted[i]);
                                                }
                                                speler = sb.ToString().Trim('[').Trim(']');
                                                link = split[1].Trim('(').Trim(')');
                                            }
                                            else{
                                                damage = item.Replace(" dmg`", string.Empty).Trim('`');
                                            }
                                        }
                                        string fieldName = field.Name.Replace("\\_", "_");
                                        hofList.Add(new TankHof(link, speler.Replace("\\", string.Empty), fieldName, Convert.ToInt32(damage), TIER));
                                        hofList[counter].place = (short)(counter + 1);
                                    }
                                    generatedTupleListFromMessage.Add(new Tuple<string, List<TankHof>>(field.Name, hofList));
                                }
                                return generatedTupleListFromMessage;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public static async Task editHOFMessage(DiscordMessage message, List<Tuple<string, List<TankHof>>> tierHOF)
        {
            try{
                DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
                newDiscEmbedBuilder.Color = Bot.HOF_COLOR;
                newDiscEmbedBuilder.Description = string.Empty;

                int tier = 0;
                foreach (Tuple<string, List<TankHof>> item in tierHOF){
                    if (item.Item2.Count > 0){
                        List<TankHof> sortedTankHofList = item.Item2.OrderBy(x => x.damage).Reverse().ToList();
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < sortedTankHofList.Count; i++){
                            if (tier == 0){
                                tier = sortedTankHofList[i].tier;
                            }
                            // ˍ
                            // ＿
                            // ̲
                            // _ --> underscore
                            // ▁
                            sb.AppendLine((i + 1) + ". [" + sortedTankHofList[i].speler.Replace("\\", string.Empty).Replace('_', Bot.UNDERSCORE_REPLACEMENT_CHAR) + "](" + sortedTankHofList[i].link + ") `" + sortedTankHofList[i].damage + " dmg`");
                        }
                        newDiscEmbedBuilder.AddField(item.Item1, sb.ToString().adaptToDiscordChat());
                    }
                }

                newDiscEmbedBuilder.Title = "Tier " + getDiscordEmoji(Emoj.getName(tier));

                DiscordEmbed embed = newDiscEmbedBuilder.Build();
                await message.ModifyAsync(embed);
            }
            catch (Exception e){
                await handleError("While editing HOF message: ", e.Message, e.StackTrace);
                await discordMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.discordClient, MAINTENANCE_REACTION));
            }
        }
        public static async Task<DiscordMessage> addReplayToMessage(WGBattle battle, DiscordMessage message, DiscordChannel channel, List<Tuple<string, List<TankHof>>> tierHOF)
        {
            bool foundItem = false;
            int position = 1;
            foreach (Tuple<string, List<TankHof>> item in tierHOF){
                if (item.Item1.Equals(battle.vehicle)){
                    item.Item2.Add(initializeTankHof(battle));
                    foundItem = true;
                    break;
                }
            }
            if (!foundItem){
                List<TankHof> list = new List<TankHof> { initializeTankHof(battle) };
                tierHOF.Add(new Tuple<string, List<TankHof>>(battle.vehicle, list));
            }
            else{
                foreach (Tuple<string, List<TankHof>> item in tierHOF){
                    if (item.Item1.Equals(battle.vehicle)){
                        List<TankHof> sortedTankHofList = item.Item2.OrderBy(x => x.damage).Reverse().ToList();
                        for (int i = 0; i < sortedTankHofList.Count; i++){
                            sortedTankHofList[i].place = (short)(i + 1);
                            if (sortedTankHofList[i].link.Equals(battle.view_url)){
                                position = i + 1;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            await editHOFMessage(message, tierHOF);
            return await Bot.SayReplayIsWorthy(channel, battle, position);
        }
        public static async Task<List<Tuple<string, List<TankHof>>>> getTankHofsPerPlayer(ulong guildID)
        {
            List<Tuple<string, List<TankHof>>> players = new List<Tuple<string, List<TankHof>>>();
            DiscordChannel channel = await Bot.GetHallOfFameChannel(guildID);
            if (channel != null){
                IReadOnlyList<DiscordMessage> messages = await channel.GetMessagesAsync(100);
                if (messages != null && messages.Count > 0){
                    List<Tuple<DiscordMessage, int>> allTierMessages = new List<Tuple<DiscordMessage, int>>();
                    for (int i = 1; i <= 10; i++){
                        List<DiscordMessage> tierMessages = Bot.getTierMessages(i, messages);
                        foreach (DiscordMessage tempMessage in tierMessages){
                            allTierMessages.Add(new Tuple<DiscordMessage, int>(tempMessage, i));
                        }
                    }

                    //Has all HOF messages
                    foreach (Tuple<DiscordMessage, int> message in allTierMessages){
                        List<Tuple<string, List<TankHof>>> tempTanks = Bot.convertHOFMessageToTupleListAsync(message.Item1, message.Item2);
                        if (tempTanks != null){
                            foreach (Tuple<string, List<TankHof>> tank in tempTanks){
                                foreach (TankHof th in tank.Item2){
                                    bool found = false;
                                    for (int i = 0; i < players.Count; i++){
                                        if (players[i].Item1.Equals(th.speler)){
                                            found = true;
                                            players[i].Item2.Add(th);
                                        }
                                    }
                                    if (!found){
                                        players.Add(new Tuple<string, List<TankHof>>(th.speler, new List<TankHof>() { th }));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return players;
        }


        public static async Task<bool> createOrCleanHOFMessages(DiscordChannel HOFchannel, List<Tuple<int, DiscordMessage>> tiersFound)
        {
            tiersFound.Reverse();
            for (int i = 10; i >= 1; i--){
                bool made = false;
                for (int j = 0; j < tiersFound.Count; j++){
                    if (tiersFound[j].Item1.Equals(i)){
                        if (!made){
                            await tiersFound[j].Item2.ModifyAsync(createHOFResetEmbed(i));
                            tiersFound[j] = new Tuple<int, DiscordMessage>(i, tiersFound[j].Item2);
                            made = true;
                        }
                        else{
                            await tiersFound[j].Item2.DeleteAsync();
                            tiersFound[j] = new Tuple<int, DiscordMessage>(i, null);
                        }
                    }
                    else if (!made && tiersFound[j].Item1 < i){
                        await tiersFound[j].Item2.ModifyAsync(createHOFResetEmbed(i));
                        tiersFound[j] = new Tuple<int, DiscordMessage>(i, tiersFound[j].Item2);
                        made = true;
                        break;
                    }
                }
                if (!made){
                    await HOFchannel.SendMessageAsync(null, createHOFResetEmbed(i));
                }
            }
            return true;
        }
        private static DiscordEmbed createHOFResetEmbed(int tier)
        {
            DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
            newDiscEmbedBuilder.Color = Bot.HOF_COLOR;
            newDiscEmbedBuilder.Description = "Nog geen replays aan deze tier toegevoegd.";

            newDiscEmbedBuilder.Title = "Tier " + getDiscordEmoji(Emoj.getName(tier));

            return newDiscEmbedBuilder.Build();
        }
        public static TankHof initializeTankHof(WGBattle battle)
        {
            return new TankHof(battle.view_url, battle.player_name, battle.vehicle, battle.details.damage_made, battle.vehicle_tier);
        }
        public static async Task hofAfterUpload(Tuple<string, DiscordMessage> returnedTuple, DiscordMessage uploadMessage)
        {
            bool good = false;
            if (returnedTuple.Item1.Equals(string.Empty)){
                await Task.Delay(Bot.hofWaitTime * 1000 * 60);//wacht 2 minuten
                string description = string.Empty;
                string thumbnail = string.Empty;
                if (returnedTuple.Item2 != null){
                    if (returnedTuple.Item2.Embeds != null){
                        if (returnedTuple.Item2.Embeds.Count > 0){
                            foreach (DiscordEmbed embed in returnedTuple.Item2.Embeds){
                                if (embed.Description != null){
                                    description = embed.Description;
                                    if (embed.Thumbnail != null){
                                        if (embed.Thumbnail.Url.ToString().Length > 0){
                                            thumbnail = embed.Thumbnail.Url.ToString();
                                        }
                                    }
                                }
                                if (embed.Title.ToLower().Contains("hoera")){
                                    good = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (good){
                    await uploadMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.discordClient, ":thumbsup:"));
                }
                else{
                    await uploadMessage.CreateReactionAsync(DiscordEmoji.FromName(Bot.discordClient, ":thumbsdown:"));
                }
                //Pas bericht aan
                string[] splitted = description.Split('\n');
                StringBuilder sb = new StringBuilder();
                bool emptyLineFound = false;
                if (!splitted.Contains(string.Empty)){
                    emptyLineFound = true;
                }
                foreach (string line in splitted){
                    if (emptyLineFound){
                        sb.AppendLine(line.Replace("\n", string.Empty).Replace("\r", string.Empty));
                    }
                    else if (line.Length == 0){
                        emptyLineFound = true;
                    }
                }
                try{
                    DiscordEmbedBuilder newDiscEmbedBuilder = new DiscordEmbedBuilder();
                    newDiscEmbedBuilder.Color = Bot.BOT_COLOR;
                    WeeklyEventHandler weeklyEventHandler = new WeeklyEventHandler();
                    newDiscEmbedBuilder.Description = sb.ToString();
                    newDiscEmbedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
                    newDiscEmbedBuilder.Thumbnail.Url = thumbnail;

                    newDiscEmbedBuilder.Title = "Resultaat";

                    DiscordEmbed embed = newDiscEmbedBuilder.Build();
                    await returnedTuple.Item2.ModifyAsync(embed);
                }
                catch (Exception e){
                    await handleError("While editing Resultaat message: ", e.Message, e.StackTrace);
                }
                //await returnedTuple.Item2.DeleteAsync();
            }
            //else
            //{
            //    await Bot.SendMessage(uploadMessage.Channel, await uploadMessage.Channel.Guild.GetMemberAsync(uploadMessage.Author.Id), uploadMessage.Channel.Guild.Name, "**" + returnedTuple.Item1 + "**");
            //}
        }

        #endregion

        public static async Task handleError(string message, string exceptionMessage, string stackTrace)
        {
            discordClient.Logger.LogError(message + exceptionMessage + Environment.NewLine + stackTrace);
            await sendThibeastmo(message, exceptionMessage, stackTrace);
        }

        //no longer sends to thibeastmo. This used to be a method to send towards thibeastmo but since thibeastmo is no longer hosting this it's moved towards the test channel
        public static async Task sendThibeastmo(string message, string exceptionMessage = "", string stackTrace = "")
        {
            var bottestChannel = await GetBottestChannel();
            if (bottestChannel != null)
            {
                StringBuilder sb = new StringBuilder();
                if (exceptionMessage != null && stackTrace != null && exceptionMessage.Length > 0 && stackTrace.Length > 0)
                {
                    for (int i = 0; i < message.Length / 2; i++)
                    {
                        sb.Append('━');
                    }
                }
                StringBuilder firstMessage = new StringBuilder((sb.Length > 0 ? "**" + sb.ToString() + "**\n" : string.Empty) + message);
                if (exceptionMessage.Length > 0)
                {
                    firstMessage.Append("\n" + "`" + exceptionMessage + "`");
                }
                await bottestChannel.SendMessageAsync(firstMessage.ToString());
                if (stackTrace.Length > 0)
                {
                    //await thibeastmo.SendMessageAsync("```" + stackTrace + "```");
                }
            }
        }
    }
}
