﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MonkeyBot.Common;
using MonkeyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    [MinPermissions(AccessLevel.User)]
    [RequireGuild]
    [Description("Dog pictures")]
    public class DogModule : BaseCommandModule
    {
        private static readonly DiscordColor dogColor = DiscordColor.Brown;

        private readonly IDogService dogService;

        public DogModule(IDogService dogService)
        {
            this.dogService = dogService;
        }

        [Command("Dog")]
        [Aliases("Dogger", "Doggo")]
        [Description("Gets a random Dog picture. Optionally a breed can be provided.")]
        public async Task GetDogPicAsync(CommandContext ctx, [RemainingText][Description("Optional: The breed of the dogger")] string breed = "")
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            Uri pictureURL = breed.IsEmpty() ?
                await (dogService?.GetRandomPictureUrlAsync()).ConfigureAwait(false) :
                await (dogService?.GetRandomPictureUrlAsync(breed.ToLowerInvariant())).ConfigureAwait(false);
            if (pictureURL == null)
            {
                _ = await ctx.ErrorAsync($"Could not get a dog pic :(. Try using {ctx.Prefix}dogbreeds to get a list of dog breeds I can show you").ConfigureAwait(false);
                return;
            }
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithColor(dogColor)
                .WithImageUrl(pictureURL);
            _ = await ctx.RespondDeletableAsync(embed: builder.Build()).ConfigureAwait(false);
        }

        [Command("Dogbreeds")]
        [Aliases("Doggerbreeds", "Doggobreeds")]
        [Description("Gets a list of available dog breeds.")]
        public async Task GetDogBreedsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            List<string> breeds = await (dogService?.GetBreedsAsync()).ConfigureAwait(false);
            if (breeds == null || !breeds.Any())
            {
                _ = await ctx.ErrorAsync("Could not get the dog breeds :(").ConfigureAwait(false);
                return;
            }
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithColor(dogColor)
                .WithTitle("Here's a list of available dog breeds:")
                .WithDescription(string.Join(", ", breeds));
            _ = await ctx.RespondDeletableAsync(embed: builder.Build()).ConfigureAwait(false);
        }
    }
}