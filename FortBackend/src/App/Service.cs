﻿using System.Net;
using FortBackend.src.App.Utilities.Saved;
using FortBackend.src.App.Utilities;
using System.Runtime.InteropServices;
using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortBackend.src.App.Utilities.Shop;
using FortBackend.src.App.XMPP_V2;
using Newtonsoft.Json;
using FortBackend.src.App.Utilities.Helpers;
using FortLibrary.EpicResponses.Profile.Query.Items;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Security.Cryptography.X509Certificates;
using FortBackend.src.App.Utilities.Discord;
using FortBackend.src.App.Utilities.Quests;
using FortBackend.src.App.Utilities.Helpers.BattlepassManagement;
using FortLibrary.ConfigHelpers;
using FortBackend.src.App.Utilities.ADMIN;
using FortBackend.src.App.Utilities.Helpers.Cached;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortLibrary;
using FortBackend.src.App.Utilities.Constants;
using FortLibrary.Shop;
namespace FortBackend.src.App
{
    public class Service
    {
        public static async Task Intiliazation(string[] args)
        {
            Logger.PlainLog(@"  ______         _   ____             _                  _ 
 |  ____|       | | |  _ \           | |                | |
 | |__ ___  _ __| |_| |_) | __ _  ___| | _____ _ __   __| |
 |  __/ _ \| '__| __|  _ < / _` |/ __| |/ / _ \ '_ \ / _` |
 | | | (_) | |  | |_| |_) | (_| | (__|   <  __/ | | | (_| |
 |_|  \___/|_|   \__|____/ \__,_|\___|_|\_\___|_| |_|\__,_|
                                                           
                                                           ");

            Logger.Log("MARVELCO IS LOADING (marcellowmellow)");
            Logger.Log($"Built on {RuntimeInformation.OSArchitecture}-bit");

            await CachedData.Init();

            var builder = WebApplication.CreateBuilder(args);
            var startup = new Startup(builder.Configuration);

            startup.ConfigureServices(builder.Services);

            if (Saved.DeserializeConfig.HTTPS)
            {
                Saved.BackendCachedData.DefaultProtocol = "https://";
                builder.WebHost.UseUrls($"https://0.0.0.0:{Saved.DeserializeConfig.BackendPort}");
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.Listen(IPAddress.Any, Saved.DeserializeConfig.BackendPort, listenOptions =>
                    {   
                        var certPath = Path.Combine(PathConstants.BaseDir, "Certificates", "FortBackend.pfx");
                        if (!File.Exists(certPath))
                        {
                            Logger.Error("Couldn't find FortBackend.pfx -> make sure you removed .temp from FortBackend.pfx.temp", "CERTIFICATES");
                            throw new Exception("Couldn't find FortBackend.pfx -> make sure you removed .temp from FortBackend.pfx.temp");
                        }
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                        var certificate = new X509Certificate2(certPath, Saved.DeserializeConfig.CertKey);
                        listenOptions.UseHttps(certificate);
                    });

                });
            }
            else
            {
                Saved.BackendCachedData.DefaultProtocol = "http://";
                builder.WebHost.UseUrls($"http://0.0.0.0:{Saved.DeserializeConfig.BackendPort}");
            }


            var app = builder.Build();



            // Fix ips not showing
            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
            //    ForwardedHeaders.XForwardedProto
            //});

            if (Saved.DeserializeConfig.HTTPS)
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            startup.Configure(app, app.Environment);

          


            //Setup.Initialize(app);
#if !DEVELOPMENT
            var DiscordBotServer = new Thread(async () =>
            {
                await DiscordBot.Start();
            });
            DiscordBotServer.Start();
#endif


            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var XmppServer = new Thread(() =>
            {
                Xmpp_Server.Intiliazation(args, cancellationTokenSource.Token);

               // FortBackend.src.App.XMPP_Server.XMPP.XmppServer.STOP();
            });
            XmppServer.Start();

            var LeadBoardLoop = new Thread(async () =>
            {
                await UpdateLeaderBoard.LeaderboardLoop();
            });
            LeadBoardLoop.Start();

            if(Saved.DeserializeGameConfig.ShopRotation)
            {
                string json = System.IO.File.ReadAllText(PathConstants.ShopJson.Shop);
                if (!string.IsNullOrEmpty(json))
                {
                    ShopJson shopData = JsonConvert.DeserializeObject<ShopJson>(json)!;
                    if(shopData.ShopItems.Daily.Count == 0 &&
                        shopData.ShopItems.Weekly.Count == 0)
                    {
                        // Shop is empty... how about we generate a new shop
                        var GeneraterShocked = new Thread(async () => {
                            await GenerateShop.Init();
                        });

                        GeneraterShocked.Start();
                    }
                }
                else
                {
                    // need to clean this up!
                    var GeneraterShocked = new Thread(async () => {
                        await GenerateShop.Init();
                    });

                    GeneraterShocked.Start();
                }

                var ItemShopGenThread = new Thread(async () =>
                {
                   // Logger.Log("Generating Shop at")
                    await GenerateItemShop(0);
                });
                ItemShopGenThread.Start();

                // ENABLE TO ONLY RUN ON STARTUP
                //GenerateShop.Init();
            }


            try
            {
                await app.StartAsync();
                // FortBackend uses a cache like system, instead of updating data it's updated on mongodb every 15 mins (equiping and many other functions will be faster)
                Logger.Warn("Make sure to click ctrl + c to not loose data before closing"); 
            }
            catch (IOException ex) when (ex.Message.Contains("address already in use")){ 
                Logger.Error($"The Port {Saved.DeserializeConfig.BackendPort} is already in use", "SERVER");
                await app.StopAsync();
            }
            catch (Exception ex) { Logger.Error(ex.Message); }


            var shutdownTask = app.WaitForShutdownAsync();
            //app.Run();
            await shutdownTask;

            Console.WriteLine("SHUTDOINW");

            cancellationTokenSource.Cancel();

            Environment.Exit(0);
        }

        async static Task GenerateItemShop(int i)
        {
            await Task.Delay(1000);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
            if (dateTime.Hour == 17 && dateTime.Minute == 59)
            {
                if (dateTime.Second >= 59)
                {
                    var GeneraterShocked = new Thread(async () => {
                        await GenerateShop.Init();
                    });

                    GeneraterShocked.Start();
                }
            }
            await GenerateItemShop(++i);
        }
    }
}
