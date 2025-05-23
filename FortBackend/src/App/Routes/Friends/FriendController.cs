using FortBackend.src.App.Utilities;
using FortBackend.src.App.Utilities.Helpers.Middleware;
using FortBackend.src.App.Utilities.MongoDB.Helpers;
using FortLibrary.MongoDB.Module;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Xml.Linq;
using FortLibrary;
using FortLibrary.XMPP;
using FortBackend.src.App.SERVER.Send;
using FortBackend.src.XMPP.Data;
using FortBackend.src.App.SERVER.Root;
using FortLibrary.Encoders.JWTCLASS;

namespace FortBackend.src.App.Routes.Friends
{
    [ApiController]
    [Route("friends/api")]
    public class FriendController : ControllerBase
    {
        [HttpGet("v1/{accountId}/blocklist")]
        public async Task<ActionResult> GrabBlockList(string accountId)
        {
            Response.ContentType = "application/json";
            var FriendList = new List<dynamic>();
            try
            {
                ProfileCacheEntry profileCacheEntry = await GrabData.Profile(accountId);
                if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                {
                    foreach (FriendsObject BlockedUser in profileCacheEntry.UserFriends.Blocked)
                    {
                        FriendList.Add(new
                        {
                            accountId = BlockedUser.accountId,
                            created = BlockedUser.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("FriendController: " + ex.Message);
            }

            return Ok(FriendList);
        }

        [HttpGet("public/list/fortnite/{accountId}/recentPlayers")]
        public ActionResult GrabChapter1RecentPlayers(string accountId)
        {
            return Ok(new List<object>());
        }

        [HttpGet("v1/{accountId}/settings")]
        public ActionResult GrabChapter1Settings(string accountId)
        {
            return Ok(new
            {
                acceptInvites = "public"
            });
        }

        [HttpGet("public/blocklist/{accountId}")]
        public async Task<ActionResult> GrabChapter1BlockList(string accountId)
        {
            Response.ContentType = "application/json";
            var FriendList = new List<dynamic>();
            try
            {
                ProfileCacheEntry profileCacheEntry = await GrabData.Profile(accountId);
                if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                {
                    foreach (FriendsObject BlockedUser in profileCacheEntry.UserFriends.Blocked)
                    {
                        FriendList.Add(new
                        {
                            accountId = BlockedUser.accountId,
                            created = BlockedUser.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("FriendController: " + ex.Message);
            }

            return Ok(FriendList);
        }

        [HttpGet("public/friends/{accountId}")]
        public async Task<ActionResult> Chapter1FriendList(string accountId)
        {
            Response.ContentType = "application/json";
            // List that only changes when needed to shouldnt have errors
            var response = new List<object>(); ;
            try
            {
                ProfileCacheEntry profileCacheEntry = await GrabData.Profile(accountId);
                if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                {
                    foreach (FriendsObject AcceptedList in profileCacheEntry.UserFriends.Accepted)
                    {
                        response.Add(new
                        {
                            AcceptedList.accountId,
                            status = "ACCEPTED",
                            direction = "INBOUND",
                            created = AcceptedList.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }

                    foreach (FriendsObject IncomingList in profileCacheEntry.UserFriends.Incoming)
                    {
                        response.Add(new
                        {
                            accountId = IncomingList.accountId,
                            status = "PENDING",
                            direction = "INBOUND",
                            groups = Array.Empty<string>(),
                            created = IncomingList.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }

                    foreach (FriendsObject OutgoingList in profileCacheEntry.UserFriends.Outgoing)
                    {
                        response.Add(new
                        {
                            OutgoingList.accountId,
                            status = "PENDING",
                            direction = "OUTBOUND",
                            created = OutgoingList.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }

                    foreach (FriendsObject BlockedUser in profileCacheEntry.UserFriends.Blocked)
                    {
                        response.Add(new
                        {
                            BlockedUser.accountId,
                            status = "BLOCKED",
                            direction = "INBOUND",
                            created = BlockedUser.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[Friends:Chapter1Friendlist] ->" + ex.Message);
            }
            return Ok(response);
        }

        //http://127.0.0.1:1111/fortnite/api/v1/372da84236e342c297ca36599deb669d/summary
        [HttpGet("v1/{accountId}/summary")]
        public async Task<ActionResult> SummaryList(string accountId)
        {
            Response.ContentType = "application/json";
            // List that only changes when needed to shouldnt have errors
            var response = new
            {
                friends = new List<object>(),
                incoming = new List<object>(),
                outgoing = new List<object>(),
                suggested = new List<object>(),
                blocklist = new List<object>(),
                settings = new
                {
                    acceptInvites = "public"
                }
            };
            try
            {
                ProfileCacheEntry profileCacheEntry = await GrabData.Profile(accountId);
                if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                {
                
                    foreach (FriendsObject AcceptedList in profileCacheEntry.UserFriends.Accepted)
                    {
                        response.friends.Add(new
                        {
                            AcceptedList.accountId,
                            groups = Array.Empty<string>(),
                            mutual = 0,
                            alias = AcceptedList.alias != null ? AcceptedList.alias : "",
                            note = "",
                            created = AcceptedList.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }

                    foreach (FriendsObject IncomingList in profileCacheEntry.UserFriends.Incoming)
                    {
                        response.incoming.Add(new
                        {
                            IncomingList.accountId,
                            groups = Array.Empty<string>(),
                            mutual = 0,
                            alias = IncomingList.alias != null ? IncomingList.alias : "",
                            note = "",
                            created = IncomingList.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }

                    foreach (FriendsObject OutgoingList in profileCacheEntry.UserFriends.Outgoing)
                    {
                        response.outgoing.Add(new
                        {
                            OutgoingList.accountId,
                            groups = Array.Empty<string>(),
                            mutual = 0,
                            alias = OutgoingList.alias != null ? OutgoingList.alias : "",
                            note = "",
                            created = OutgoingList.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }

                    foreach (FriendsObject BlockedUser in profileCacheEntry.UserFriends.Blocked)
                    {
                        response.blocklist.Add(new
                        {
                            BlockedUser.accountId,
                            groups = Array.Empty<string>(),
                            mutual = 0,
                            alias = BlockedUser.alias != null ? BlockedUser.alias : "",
                            note = "",
                            created = BlockedUser.created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            favorite = false
                        });
                    }  
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[Friends:SummaryList] ->" + ex.Message);
            }
            return Ok(response);
        }

        [HttpGet("v1/{accountId}/recent/fortnite")]
        public ActionResult RecentFriends(string accountId)
        {
            Response.ContentType = "application/json";
            return Ok(Array.Empty<string>());
        }

        [HttpPost("public/friends/{accountId}/{friendId}")]
        [AuthorizeToken]
        public async Task<ActionResult> FriendsChapter1List(string accountId, string friendID)
        {
            Response.ContentType = "application/json";
            try
            {


                if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(friendID))
                {
                    var tokenPayload = HttpContext.Items["Payload"] as TokenPayload;

                    var displayName = tokenPayload?.Dn;
                    var accountId1 = tokenPayload?.Sub;
                    var clientId = tokenPayload?.Clid;

                    if (!string.IsNullOrEmpty(accountId1))
                    {
                        var profileCacheEntry = HttpContext.Items["ProfileData"] as ProfileCacheEntry;
                        if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                        {

                            if (profileCacheEntry.AccountData != null && profileCacheEntry.UserData != null)
                            {
                                if (profileCacheEntry.UserData.banned)
                                {
                                    return StatusCode(403);
                                }
                            }

                            ProfileCacheEntry friendsprofileCacheEntry = await GrabData.Profile(friendID); // friends
                            if (friendsprofileCacheEntry != null && !string.IsNullOrEmpty(friendsprofileCacheEntry.AccountId))
                            {
                                if (profileCacheEntry.UserFriends.Incoming != null && friendsprofileCacheEntry.UserFriends.AccountId != null)
                                {
                                    bool? FoundFriend = profileCacheEntry.UserFriends.Incoming.Any(account => account?.accountId == friendsprofileCacheEntry.UserFriends.AccountId?.ToString());

                                    if (FoundFriend.HasValue && FoundFriend.Value)
                                    {
                                        List<FriendsObject> incomingFriendsArray = profileCacheEntry.UserFriends.Incoming;
                                        List<FriendsObject> outgoingFriendsArray = friendsprofileCacheEntry.UserFriends.Outgoing;

                                        if (!incomingFriendsArray.Any(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId == friendID;
                                        })) return StatusCode(403);
                              

                                        if (!outgoingFriendsArray.Any(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId == accountId;
                                        }))   
                                            return StatusCode(403);

                                        var itemsToRemove = incomingFriendsArray.Where(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId == friendID;
                                        }).ToList();

                                        foreach (FriendsObject FriendsToRemove in itemsToRemove)
                                            incomingFriendsArray.Remove(FriendsToRemove);

                                        profileCacheEntry.UserFriends.Incoming.AddRange(incomingFriendsArray);

                                        profileCacheEntry.UserFriends.Accepted.Add(new FriendsObject
                                        {
                                            accountId = friendID,
                                            created = DateTime.UtcNow
                                        });

                                        var itemsToRemove2 = outgoingFriendsArray.Where(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId?.ToString() == accountId;
                                        }).ToList();

                                        foreach (FriendsObject FriendsToRemove in itemsToRemove2)
                                            outgoingFriendsArray.Remove(FriendsToRemove);

                                        friendsprofileCacheEntry.UserFriends.Outgoing.AddRange(outgoingFriendsArray);

                                        friendsprofileCacheEntry.UserFriends.Accepted.Add(new FriendsObject
                                        {
                                            accountId = accountId,
                                            created = DateTime.UtcNow
                                        });

                                        Clients targetClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == accountId)!;

                                        XNamespace clientNs = "jabber:client";
                                        if (targetClient != null)
                                        {
                                            await Client.SendClientMessage(targetClient,
                                              new XElement(clientNs + "message",
                                                    new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                    new XAttribute("to", accountId),
                                                    new XElement("body",
                                                        JsonConvert.SerializeObject(new
                                                        {
                                                            payload = new
                                                            {
                                                                accountId = friendsprofileCacheEntry.AccountId,
                                                                status = "ACCEPTED",
                                                                direction = "OUTBOUND",
                                                                created = DateTime.UtcNow.ToString("o"),
                                                                favorite = false
                                                            },
                                                            type = "com.epicgames.friends.core.apiobjects.Friend",
                                                            timestamp = DateTime.UtcNow.ToString("o")
                                                        })
                                                    )
                                                )
                                           );

                                            await XmppFriend.GrabSomeonesPresence(friendsprofileCacheEntry.AccountId, accountId1, false);
                                        }
                                        Clients targetClient2 = GlobalData.Clients.FirstOrDefault(client => client.accountId == friendID)!;
                                      
                                        if(targetClient2 != null)
                                        {
                                            await Client.SendClientMessage(targetClient2,
                                               new XElement(clientNs + "message",
                                                     new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                     new XAttribute("to", friendsprofileCacheEntry.AccountId),
                                                     new XElement("body",
                                                         JsonConvert.SerializeObject(new
                                                         {
                                                             payload = new
                                                             {
                                                                 accountId = accountId1
    ,
                                                                 status = "ACCEPTED",
                                                                 direction = "INBOUND",
                                                                 created = DateTime.UtcNow.ToString("o"),
                                                                 favorite = false
                                                             },
                                                             type = "com.epicgames.friends.core.apiobjects.Friend",
                                                             timestamp = DateTime.UtcNow.ToString("o")
                                                         })
                                                     )
                                                 )
                                            );

                                            await XmppFriend.GrabSomeonesPresence(accountId1, friendsprofileCacheEntry.AccountId, false);
                                        }


                                        return StatusCode(204);
                                    }
                                    else
                                    {
                                        //Jarray2 == FriendsAccountDataParsed
                                        List<FriendsObject> incomingToken = profileCacheEntry.UserFriends.Outgoing;
                                        List<FriendsObject> incomingToken2 = friendsprofileCacheEntry.UserFriends.Incoming;

                                        if (incomingToken != null && incomingToken2 != null)
                                        {
                                            profileCacheEntry.UserFriends.Outgoing.Add(new FriendsObject
                                            {
                                                accountId = friendsprofileCacheEntry.AccountId,
                                                alias = "", // idk
                                                created = DateTime.UtcNow
                                            });

                                            friendsprofileCacheEntry.UserFriends.Incoming.Add(new FriendsObject
                                            {
                                                accountId = accountId1,
                                                alias = "",
                                                created = DateTime.UtcNow
                                            });

                                            Clients targetClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == accountId)!;
                                            Clients friendClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == friendID)!;

                                            XNamespace clientNs = "jabber:client";
                                            if (targetClient != null)
                                            {
                                                await Client.SendClientMessage(targetClient,
                                                  new XElement(clientNs + "message",
                                                        new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                        new XAttribute("to", accountId),
                                                        new XElement("body",
                                                            JsonConvert.SerializeObject(new
                                                            {
                                                                payload = new
                                                                {
                                                                    accountId = friendsprofileCacheEntry.AccountId,
                                                                    status = "PENDING",
                                                                    direction = "OUTBOUND",
                                                                    created = DateTime.UtcNow.ToString("o"),
                                                                    favorite = false
                                                                },
                                                                type = "com.epicgames.friends.core.apiobjects.Friend",
                                                                timestamp = DateTime.UtcNow.ToString("o")
                                                            })
                                                        )
                                                    )
                                               );

                                                await XmppFriend.GrabSomeonesPresence(accountId1, friendsprofileCacheEntry.AccountId, false);
                                            }

                                            if (friendClient != null)
                                            {
                                                new XElement(clientNs + "message",
                                                     new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                     new XAttribute("to", friendsprofileCacheEntry.AccountId),
                                                     new XElement("body",
                                                         JsonConvert.SerializeObject(new
                                                         {
                                                             payload = new
                                                             {
                                                                 accountId = accountId1,
                                                                 status = "PENDING",
                                                                 direction = "INBOUND",
                                                                 created = DateTime.UtcNow.ToString("o"),
                                                                 favorite = false
                                                             },
                                                             type = "com.epicgames.friends.core.apiobjects.Friend",
                                                             timestamp = DateTime.UtcNow.ToString("o")
                                                         })
                                                     )
                                                );
                                            }

                                            return StatusCode(204);
                                        }
                                        else
                                        {
                                            return StatusCode(403);
                                        }
                                    }
                                }

                            }
                        }


                    }else
                    {
                        Logger.Error("WETFFF");
                    }
                }else
                {
                    Logger.Error("OK WHAT");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "FriendsChapter1List");
            }
            return StatusCode(403); // not proper response i'll recode this in the future
        }


        [HttpPost("v1/{accountId}/friends/{friendId}")]
        [AuthorizeToken]
        public async Task<ActionResult> FriendsAccountId(string accountId, string friendID)
        {
            Response.ContentType = "application/json";
            try
            {
                if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(friendID))
                {
                    var tokenPayload = HttpContext.Items["Payload"] as TokenPayload;

                    var displayName = tokenPayload?.Dn;
                    var accountId1 = tokenPayload?.Sub;
                    var clientId = tokenPayload?.Clid;

                    if (!string.IsNullOrEmpty(accountId1))
                    {
                        var profileCacheEntry = HttpContext.Items["ProfileData"] as ProfileCacheEntry; // use the auth account not from url
                        if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                        {
                            if (profileCacheEntry.AccountData != null && profileCacheEntry.UserData != null)
                            {
                                if (profileCacheEntry.UserData.banned)
                                {
                                    return StatusCode(403);
                                }

                                // BLOCK FRIENDS ARE 1000 ~ need to add on the other api + on the friends asa well!
                                if(profileCacheEntry.UserFriends.Outgoing.Count == 1000 /*|| profileCacheEntry.UserFriends.Incoming.Count == 1000*/ || profileCacheEntry.UserFriends.Incoming.Count == 1000)
                                {
                                    return StatusCode(403);
                                }
                            }

                            ProfileCacheEntry friendsprofileCacheEntry = await GrabData.Profile(friendID); // friends
                            if (friendsprofileCacheEntry != null && !string.IsNullOrEmpty(friendsprofileCacheEntry.AccountId))
                            {
                                if (profileCacheEntry.UserFriends.Incoming != null && friendsprofileCacheEntry.UserFriends.AccountId != null)
                                {
                                    bool? FoundFriend = profileCacheEntry.UserFriends.Incoming.Any(account => account?.accountId == friendsprofileCacheEntry.UserFriends.AccountId?.ToString());

                                    if (FoundFriend.HasValue && FoundFriend.Value)
                                    {
                                        //Jarray2 == FriendsAccountDataParsed
                                        List<FriendsObject> incomingFriendsArray = profileCacheEntry.UserFriends.Incoming;
                                        List<FriendsObject> outgoingFriendsArray = friendsprofileCacheEntry.UserFriends.Outgoing;

                                        if (!incomingFriendsArray.Any(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId == friendID;
                                        })) return StatusCode(403);
                                

                                        if (!outgoingFriendsArray.Any(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId?.ToString() == accountId;
                                        })) return StatusCode(403);
                
                                        var itemsToRemove = incomingFriendsArray.Where(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId?.ToString() == friendID;
                                        }).ToList();

                                        foreach (FriendsObject FriendsToRemove in itemsToRemove)
                                            incomingFriendsArray.Remove(FriendsToRemove);

                                        profileCacheEntry.UserFriends.Incoming.AddRange(incomingFriendsArray);


                                        profileCacheEntry.UserFriends.Accepted.Add(new FriendsObject
                                        {
                                            accountId = friendID,
                                            created = DateTime.UtcNow
                                        });

                                        var itemsToRemove2 = outgoingFriendsArray.Where(friend =>
                                        {
                                            var friendObject = friend;
                                            return friendObject != null && friendObject.accountId?.ToString() == accountId;
                                        }).ToList();

                                        foreach (FriendsObject FriendsToRemove in itemsToRemove2)
                                            outgoingFriendsArray.Remove(FriendsToRemove);

                                        friendsprofileCacheEntry.UserFriends.Outgoing.AddRange(outgoingFriendsArray);

                                        friendsprofileCacheEntry.UserFriends.Accepted.Add(new FriendsObject
                                        {
                                            accountId = accountId.ToString(),
                                            created = DateTime.UtcNow
                                        });
                                        XNamespace clientNs = "jabber:client";

                                        Clients targetClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == accountId)!;
                                        if(targetClient != null)
                                        {
                                            await Client.SendClientMessage(targetClient,
                                                 new XElement(clientNs + "message",
                                                       new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                       new XAttribute("to", accountId),
                                                       new XElement("body",
                                                           JsonConvert.SerializeObject(new
                                                           {
                                                               payload = new
                                                               {
                                                                   accountId = friendsprofileCacheEntry.AccountId,
                                                                   status = "ACCEPTED",
                                                                   direction = "OUTBOUND",
                                                                   created = DateTime.UtcNow.ToString("o"),
                                                                   favorite = false
                                                               },
                                                               type = "com.epicgames.friends.core.apiobjects.Friend",
                                                               timestamp = DateTime.UtcNow.ToString("o")
                                                           })
                                                       )
                                                   )
                                            );

                                            await Client.SendClientMessage(targetClient, new XElement(clientNs + "message",
                                                 new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                 new XAttribute("to", accountId),
                                                 new XElement("type", "available")
                                             ));
                                        }
                                        else
                                        {
                                            Logger.Log("COULDNT FIND ACC ON XMPP");
                                        }

                                        Clients friendsClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == friendID)!;
                                        if(friendsClient != null)
                                        {
                                            await Client.SendClientMessage(friendsClient,
                                                new XElement(clientNs + "message",
                                                   new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                   new XAttribute("to", friendsprofileCacheEntry.AccountId),
                                                   new XElement("body",
                                                       JsonConvert.SerializeObject(new
                                                       {
                                                           payload = new
                                                           {
                                                               accountId = accountId1,
                                                               status = "ACCEPTED",
                                                               direction = "INBOUND",
                                                               created = DateTime.UtcNow.ToString("o"),
                                                               favorite = false
                                                           },
                                                           type = "com.epicgames.friends.core.apiobjects.Friend",
                                                           timestamp = DateTime.UtcNow.ToString("o")
                                                       })
                                                   )
                                                )
                                            );

      
                                            await Client.SendClientMessage(friendsClient, new XElement(clientNs + "message",
                                                 new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                 new XAttribute("to", accountId),
                                                 new XElement("type", "available")
                                             ));
                                        }else
                                        {
                                            Logger.Log($"COULDNT FIND ACC ON XMPP + {friendID}");
                                        }

                                        return StatusCode(204);
                                    }
                                    else
                                    {
                                        //Jarray2 == FriendsAccountDataParsed
                                        List<FriendsObject> outgoingToken = profileCacheEntry.UserFriends.Outgoing;
                                        List<FriendsObject> incomingToken = friendsprofileCacheEntry.UserFriends.Incoming;

                                        if (outgoingToken != null && incomingToken != null)
                                        {
                                            profileCacheEntry.UserFriends.Outgoing.Add(new FriendsObject
                                            {
                                                accountId = friendsprofileCacheEntry.AccountId,
                                                alias = "", // idk
                                                created = DateTime.UtcNow
                                            });

                                            friendsprofileCacheEntry.UserFriends.Incoming.Add(new FriendsObject
                                            {
                                                accountId = accountId1,
                                                alias = "",
                                                created = DateTime.UtcNow
                                            });

                                            XNamespace clientNs = "jabber:client";

                                            Clients targetClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == accountId)!;
                                            if(targetClient != null)
                                            {
                                                await Client.SendClientMessage(targetClient,
                                                   new XElement(clientNs + "message",
                                                        new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                        new XAttribute("to", accountId),
                                                        new XElement("body",
                                                            JsonConvert.SerializeObject(new
                                                            {
                                                                payload = new
                                                                {
                                                                    accountId = friendsprofileCacheEntry.AccountId,
                                                                    status = "PENDING",
                                                                    direction = "OUTBOUND",
                                                                    created = DateTime.UtcNow.ToString("o"),
                                                                    favorite = false
                                                                },
                                                                type = "com.epicgames.friends.core.apiobjects.Friend",
                                                                timestamp = DateTime.UtcNow.ToString("o")
                                                            })
                                                        )
                                                    )
                                                );
                                            }

                                            Clients friendClient = GlobalData.Clients.FirstOrDefault(client => client.accountId == friendID)!;
                                           
                                            if(friendClient != null)
                                            {
                                                await Client.SendClientMessage(friendClient,
                                                   new XElement(clientNs + "message",
                                                        new XAttribute("from", $"xmpp-admin@prod.ol.epicgames.com"),
                                                        new XAttribute("to", friendsprofileCacheEntry.AccountId),
                                                        new XElement("body",
                                                            JsonConvert.SerializeObject(new
                                                            {
                                                                payload = new
                                                                {
                                                                    accountId = accountId1,
                                                                    status = "PENDING",
                                                                    direction = "INBOUND",
                                                                    created = DateTime.UtcNow.ToString("o"),
                                                                    favorite = false
                                                                },
                                                                type = "com.epicgames.friends.core.apiobjects.Friend",
                                                                timestamp = DateTime.UtcNow.ToString("o")
                                                            })
                                                        )
                                                    )
                                                );

                                            }

                                            return StatusCode(204);
                                        }
                                        else
                                        {
                                            return StatusCode(403);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return StatusCode(403);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[Friends:Chapter1] -> {ex.Message}");
            }

            return StatusCode(403);

        }

        [HttpDelete("v1/{accountId}/friends/{friendId}")]
        [AuthorizeToken]
        public async Task<ActionResult> RemoveFriendsV1(string accountId, string friendID)
        {
            Response.ContentType = "application/json";
            try
            {
                var tokenPayload = HttpContext.Items["Payload"] as TokenPayload;

                var displayName = tokenPayload?.Dn;
                var accountId1 = tokenPayload?.Sub;
                var clientId = tokenPayload?.Clid;

                if (accountId1 != null)
                {
                    var profileCacheEntry = HttpContext.Items["ProfileData"] as ProfileCacheEntry;
                    if (profileCacheEntry != null && !string.IsNullOrEmpty(profileCacheEntry.AccountId))
                    {
                        if (profileCacheEntry.UserData.banned)
                        {
                            return StatusCode(403);
                        }

                        ProfileCacheEntry friendsprofileCacheEntry = await GrabData.Profile(friendID);
                        if (friendsprofileCacheEntry != null && !string.IsNullOrEmpty(friendsprofileCacheEntry.AccountId))
                        {
                            // basically we use the data account id for people who could call the auth the change the account from the request
                            if (friendsprofileCacheEntry.UserFriends.Accepted.Find(d => d.accountId == accountId1) != null)
                            {
                                await Handlers.PullFromArray<UserFriends>("accountId", friendsprofileCacheEntry.AccountId, "accepted", "accountId", profileCacheEntry.AccountId);
                                await Handlers.PullFromArray<UserFriends>("accountId", profileCacheEntry.AccountId, "accepted", "accountId", friendsprofileCacheEntry.AccountId);
                            }

                            if (profileCacheEntry.UserFriends.Incoming != null)
                            {
                                await XmppFriend.SendMessageToId(new
                                {
                                    Payload = new
                                    {
                                        AccountId = profileCacheEntry.AccountId,
                                        Reason = "DELETED"
                                    },
                                    Type = "com.epicgames.friends.core.apiobjects.FriendRemoval",
                                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                                }, friendsprofileCacheEntry.AccountId);

                                await XmppFriend.SendMessageToId(new
                                {
                                    Payload = new
                                    {
                                        AccountId = friendsprofileCacheEntry.AccountId,
                                        Reason = "DELETED"
                                    },
                                    Type = "com.epicgames.friends.core.apiobjects.FriendRemoval",
                                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                                }, profileCacheEntry.AccountId);
                            }
                            return StatusCode(204);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return StatusCode(403);
        }

    }
}
