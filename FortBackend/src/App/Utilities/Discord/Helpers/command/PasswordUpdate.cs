﻿using Discord;
using Discord.WebSocket;
using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortLibrary;
using FortLibrary.Encoders;
using FortLibrary.MongoDB.Module;
using Newtonsoft.Json;

namespace FortBackend.src.App.Utilities.Discord.Helpers.command
{
    public class PasswordUpdate
    {
        public static async Task Respond(SocketSlashCommand command)
        {
            try
            { 
                var PasswordOption = command.Data.Options.FirstOrDefault(o => o.Name == "password");

                if (PasswordOption?.Value is string password)
                {
                    var FindDiscordID = await GrabData.ProfileDiscord(command.User.Id.ToString());
                    if (FindDiscordID == null || string.IsNullOrEmpty(FindDiscordID.AccountId))
                    {
                        var embed = new EmbedBuilder()
                        .WithTitle("Failed To Change Password")
                        .WithDescription("You don't have a FortBackend Account.")
                        .WithColor(Color.Blue)
                        .WithCurrentTimestamp();

                        await command.RespondAsync(embed: embed.Build(), ephemeral: true);
                    }
                    else
                    {
                        if (password.Length >= 7)
                        {
                            try
                            {
                                FindDiscordID.UserData.Password = CryptoGen.HashPassword(password);
                                await MongoSaveData.SaveToDB(FindDiscordID.AccountId);

                                var embed = new EmbedBuilder()
                                 .WithTitle("Password Changed")
                                 .WithDescription($"You have changed the password on your account!")
                                 .WithColor(Color.Blue)
                                 .WithCurrentTimestamp();

                                await command.RespondAsync(embed: embed.Build(), ephemeral: true);

                                CacheMiddleware.GlobalCacheProfiles.Remove(FindDiscordID.AccountId);
                            }
                            catch (Exception ex) { }

                         
                            return;
                            //CryptoGen.HashPassword(CreateAccArg.Password)
                        }
                        else
                        {
                            var embed = new EmbedBuilder()
                            .WithTitle("Failed To Change Password")
                            .WithDescription("Min Password Length Is 7")
                            .WithColor(Color.Blue)
                            .WithCurrentTimestamp();

                            await command.RespondAsync(embed: embed.Build(), ephemeral: true);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                var embed = new EmbedBuilder()
                .WithTitle("Server Error")
                .WithDescription("?")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp();

                await command.RespondAsync(embed: embed.Build(), ephemeral: true);
            }
        }
    }
}
