﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MonkeyBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    /// <summary>Module that handles role assignments</summary>    
    [Description("Self role management")]
    [MinPermissions(AccessLevel.User)]
    [RequireGuild]
    public class SelfAssignRolesModule : BaseCommandModule
    {
        [Command("AddRole")]
        [Aliases(new[] { "GrantRole" })]
        [Description("Adds the specified role to your own roles.")]
        [Example("!addrole bf")]
        public async Task AddRoleAsync(CommandContext ctx, [Description("The role to add")] DiscordRole role)
        {
            if (role == null)
            {
                _ = await ctx.ErrorAsync("Invalid role").ConfigureAwait(false);
                return;
            }
            DiscordRole botRole = await GetBotRoleAsync(ctx).ConfigureAwait(false);

            // The bot's role must be higher than the role to be able to assign it
            if (botRole == null || botRole?.Position <= role.Position)
            {
                _ = await ctx.ErrorAsync("Sorry, I don't have sufficient permissions to give you this role!").ConfigureAwait(false);
                return;
            }

            if (ctx.Member.Roles.Contains(role))
            {
                _ = await ctx.ErrorAsync("You already have that role").ConfigureAwait(false);
                return;
            }
            await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
            _ = await ctx.OkAsync($"Role {role.Name} has been added").ConfigureAwait(false);
        }

        [Command("RemoveRole")]
        [Aliases(new[] { "RevokeRole" })]
        [Description("Removes the specified role from your roles.")]
        [Example("!RemoveRole bf")]
        public async Task RemoveRoleAsync(CommandContext ctx, [Description("The role to remove.")] DiscordRole role)
        {
            if (!ctx.Member.Roles.Contains(role))
            {
                _ = await ctx.ErrorAsync("You don't have that role").ConfigureAwait(false);
            }
            DiscordRole botRole = await GetBotRoleAsync(ctx).ConfigureAwait(false);
            // The bot's role must be higher than the role to be able to remove it
            if (botRole == null || botRole?.Position <= role.Position)
            {
                _ = await ctx.ErrorAsync("Sorry, I don't have sufficient permissions to take this role from you!").ConfigureAwait(false);
            }
            await ctx.Member.RevokeRoleAsync(role).ConfigureAwait(false);
            _ = await ctx.OkAsync($"Role {role.Name} has been revoked").ConfigureAwait(false);
        }

        [Command("ListRoles")]
        [Description("Lists all roles that can be mentioned and assigned.")]
        public async Task ListRolesAsync(CommandContext ctx)
        {
            IEnumerable<DiscordRole> assignableRoles = GetAssignableRoles(ctx);
            _ = assignableRoles.Any()
                ? await ctx.OkAsync(string.Join(", ", assignableRoles.Select(role => role.Name)), "The following assignable roles exist").ConfigureAwait(false)
                : await ctx.ErrorAsync("No assignable roles exist!").ConfigureAwait(false);
        }

        [Command("ListRolesWithMembers")]
        [Description("Lists all roles and the users who have these roles")]
        public async Task ListMembersAsync(CommandContext ctx)
        {
            var builder = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(114, 137, 218))
                .WithDescription("These are the are all the assignable roles and the users assigned to them:");

            IEnumerable<DiscordRole> assignableRoles = GetAssignableRoles(ctx);
            IReadOnlyCollection<DiscordMember> guildMembers = await ctx.Guild.GetAllMembersAsync().ConfigureAwait(false);
            foreach (DiscordRole role in assignableRoles)
            {
                IOrderedEnumerable<string> roleUsers = guildMembers?.Where(m => m.Roles.Contains(role))
                                                                    .Select(x => x.Username)
                                                                    .OrderBy(x => x);
                _ = roleUsers != null && roleUsers.Any()
                    ? builder.AddField(role.Name, string.Join(", ", roleUsers), false)
                    : builder.AddField(role.Name, "-", false);
            }
            _ = await ctx.RespondDeletableAsync(embed: builder.Build()).ConfigureAwait(false);
        }

        [Command("ListRoleMembers")]
        [Description("Lists all the members of the specified role")]
        [Example("!listrolemembers bf")]
        public async Task ListMembersAsync(CommandContext ctx, [Description("The role to display members for")] DiscordRole role)
        {
            IReadOnlyCollection<DiscordMember> guildMembers = await ctx.Guild.GetAllMembersAsync().ConfigureAwait(false);
            IOrderedEnumerable<string> roleUsers = guildMembers?.Where(x => x.Roles.Contains(role))
                                                              .Select(x => x.Username)
                                                              .OrderBy(x => x);
            if (roleUsers == null || !roleUsers.Any())
            {
                _ = await ctx.ErrorAsync("This role does not have any members!").ConfigureAwait(false);
                return;
            }
            var builder = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(114, 137, 218))
                .WithTitle($"These are the users assigned to the {role.Name} role:")
                .WithDescription(string.Join(", ", roleUsers));

            _ = await ctx.RespondDeletableAsync(embed: builder.Build()).ConfigureAwait(false);
        }

        private static async Task<DiscordRole> GetBotRoleAsync(CommandContext ctx)
        {
            // Get the role of the bot with permission manage roles            
            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
            return bot.Roles.FirstOrDefault(x => x.Permissions.HasPermission(Permissions.ManageRoles));
        }

        private static IEnumerable<DiscordRole> GetAssignableRoles(CommandContext ctx)
        {
            DiscordRole highestRole = ctx.Guild.Roles.Values.FirstOrDefault(x => x.Permissions.HasPermission(Permissions.ManageRoles));
            // Get all roles that are lower than the bot's role (roles the bot can assign)
            return ctx.Guild.Roles.Values
                .Where(role => role.IsMentionable && role != ctx.Guild.EveryoneRole && highestRole.Position > role.Position);

        }
    }
}