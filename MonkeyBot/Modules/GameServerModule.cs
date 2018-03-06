﻿using Discord.Commands;
using MonkeyBot.Common;
using MonkeyBot.Preconditions;
using MonkeyBot.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    /// <summary>Module that provides support for game servers that implement the steam server query api</summary>
    [Group("GameServer")]
    [Name("GameServer")]
    [MinPermissions(AccessLevel.ServerAdmin)]
    [RequireContext(ContextType.Guild)]
    public class GameServerModule : ModuleBase
    {
        private IGameServerService gameServerService;

        public GameServerModule(IGameServerService gameServerService)
        {
            this.gameServerService = gameServerService;
        }

        [Command("Add")]
        [Remarks("Adds the specified game server and posts it's info info in the current channel")]
        public async Task AddGameServerAsync([Summary("The ip adress and query port of the server e.g. 127.0.0.1:1234")] string ip)
        {
            await AddGameServerInternalAsync(ip, Context.Channel.Id);
        }

        [Command("Add")]
        [Remarks("Adds the specified game server and sets the channel of the current guild where the info will be posted.")]
        public async Task AddGameServerAsync([Summary("The ip adress and query port of the server e.g. 127.0.0.1:1234")] string ip, [Summary("The name of the channel where the server info should be posted")] string channelName)
        {
            var allChannels = await Context.Guild.GetTextChannelsAsync();
            var channel = allChannels.Where(x => x.Name.ToLower() == channelName.ToLower()).FirstOrDefault();
            if (channel == null)
                await ReplyAsync("The specified channel does not exist");
            else
                await AddGameServerInternalAsync(ip, channel.Id);
        }

        private async Task AddGameServerInternalAsync(string ip, ulong channelID)
        {
            //Do parameter checks
            var endPoint = await ParseIPAsync(ip);
            if (endPoint == null)
                return;

            try
            {
                // Add the Server to the Service to activate it
                await gameServerService.AddServerAsync(endPoint, Context.Guild.Id, channelID);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"There was an error while adding the game server:{Environment.NewLine}{ex.Message}");
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        [Command("Remove")]
        [Remarks("Removes the specified game server")]
        public async Task RemoveGameServerAsync([Summary("The ip adress and query port of the server e.g. 127.0.0.1:1234")] string ip)
        {
            await RemoveGameServerInternalAsync(ip);
        }

        private async Task RemoveGameServerInternalAsync(string ip)
        {
            //Do parameter checks
            var endPoint = await ParseIPAsync(ip);
            if (endPoint == null)
                return;

            try
            {
                // Remove the server from the Service
                await gameServerService.RemoveServerAsync(endPoint, Context.Guild.Id);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"There was an error while trying to remove the game server:{Environment.NewLine}{ex.Message}");
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        private async Task<IPEndPoint> ParseIPAsync(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                await ReplyAsync("You need to specify an IP-Adress + Port for the server! For example 127.0.0.1:1234");
                return null;
            }
            var splitIP = ip.Split(':');
            if (splitIP == null || splitIP.Length != 2)
            {
                await ReplyAsync("You need to specify a valid IP-Adress + Port for the server! For example 127.0.0.1:1234");
                return null;
            }
            IPAddress parsedIP = null;
            if (!IPAddress.TryParse(splitIP[0], out parsedIP))
            {
                await ReplyAsync("You need to specify a valid IP-Adress + Port for the server! For example 127.0.0.1:1234");
                return null;
            }
            int port = 0;
            if (!int.TryParse(splitIP[1], out port))
            {
                await ReplyAsync("You need to specify a valid IP-Adress + Port for the server! For example 127.0.0.1:1234");
                return null;
            }
            IPEndPoint endPoint = new IPEndPoint(parsedIP, port);
            return endPoint;
        }
    }
}