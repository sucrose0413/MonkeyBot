﻿using Discord;
using Discord.WebSocket;
using MonkeyBot;
using MonkeyBot.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


//https://github.com/Aux/Dogey

public class Program
{
    private Initializer initializer = new Initializer();

    private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

    public async Task StartAsync()
    {
        await Configuration.EnsureExistsAsync(); // Ensure the configuration file has been created.

        var services = await initializer.ConfigureServices();        

        var manager = services.GetService<CommandManager>();
        await manager.StartAsync();

        await manager.BuildDocumentation(); // Write the documentation

        await Task.Delay(-1); // Prevent the console window from closing.
    }        
}