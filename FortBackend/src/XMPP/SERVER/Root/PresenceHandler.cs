﻿
using FortBackend.src.App.SERVER.Send;
using FortBackend.src.App.Utilities;
using FortBackend.src.XMPP.Data;
using FortLibrary;
using FortLibrary.XMPP;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Xml.Linq;

namespace FortBackend.src.App.SERVER.Root
{
    public class PresenceHandler
    {
        public async static void Init(WebSocket webSocket, XDocument xmlDoc, string clientId, DataSaved UserDataSaved)
        {
            try
            {
                string xmlMessage;
                byte[] buffer;

                if (clientId == null)
                {
                    await Client.CloseClient(webSocket);
                    return;
                }
                XNamespace clientNs = "jabber:client";
                XNamespace MucNamespace = "http://jabber.org/protocol/muc#user";

                var Saved_Clients = GlobalData.Clients.FirstOrDefault(e => e.accountId == UserDataSaved.AccountId);
                if (Saved_Clients == null) { await Client.CloseClient(webSocket); return; }

                var TypeAtrribute = xmlDoc.Root?.Attribute("type");
                var Type = TypeAtrribute is null ? "" : TypeAtrribute.Value;
                switch (Type)
                {
                    case "unavailable":
                        Console.WriteLine("UNNNNNNNNNNNNNN");
                        if (string.IsNullOrEmpty(xmlDoc.Root?.Attribute("to")?.Value))
                        {
                            break;
                        }

                        string ToValue = xmlDoc.Root.Attribute("to")!.Value;

                        if (ToValue.EndsWith("@muc.prod.ol.epicgames.com") ||
                        ToValue.Split("/")[0].EndsWith("@muc.prod.ol.epicgames.com"))
                        {
                            var RoomName = ToValue.Split("@")[0];
                            if (GlobalData.Rooms.ContainsKey(RoomName))
                            {
                                var RoomData = GlobalData.Rooms[RoomName];

                                if (RoomData != null)
                                {
                                    var MemberIndex = RoomData.members.FindIndex(m => m.accountId == Saved_Clients.accountId);

                                    if (MemberIndex != -1)
                                    {
                                        RoomData.members.RemoveAt(MemberIndex);
                                        Saved_Clients.Rooms.RemoveAt(Saved_Clients.Rooms.IndexOf(RoomName));
                                    }

                                    XNamespace mucUserNs = XNamespace.Get("http://jabber.org/protocol/muc#user");

                                    XElement featuresElement = new XElement(clientNs + "presence",
                                     new XAttribute("to", Saved_Clients.jid),
                                     new XAttribute("from", $"{RoomName}@muc.prod.ol.epicgames.com/{Uri.EscapeDataString(Saved_Clients.displayName)}:{Saved_Clients.accountId}:{Saved_Clients.resource}"),
                                        new XElement(MucNamespace + "x",
                                            new XElement("item",
                                                new XAttribute("nick", $"{Uri.EscapeDataString(Saved_Clients.displayName)}:{Saved_Clients.accountId}:{Saved_Clients.resource}"),
                                                new XAttribute("jid", Saved_Clients.jid),
                                                new XAttribute("role", "none")
                                            )
                                        ),
                                        new XElement("status", new XAttribute("code", "110")),
                                        new XElement("status", new XAttribute("code", "100")),
                                        new XElement("status", new XAttribute("code", "170"))
                                    );

                                    xmlMessage = featuresElement.ToString();
                                    buffer = Encoding.UTF8.GetBytes(xmlMessage);
                                    await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                            }
                        }

                        break;
                    default:
                       // Console.WriteLine(xmlDoc.Root?.Attribute("type")?.Value);
                        XNamespace mucNamespace = "http://jabber.org/protocol/muc";
                        //XNamespace mucNamespace = "http://jabber.org/protocol/muc#user";
                        XElement MUCX = xmlDoc.Root?.Descendants().FirstOrDefault(i => i.Name == mucNamespace + "x" || i.Name == "x")!;
                        if (MUCX != null)
                        {
                          
                          //  Console.WriteLine("NO FLIPPIMG WAYU");
                            if (string.IsNullOrEmpty(xmlDoc.Root?.Attribute("to")?.Value))
                            {
                                break;
                            }

                            var RoomName = xmlDoc.Root?.Attribute("to")?.Value.Split("@")[0];
                            if (!string.IsNullOrEmpty(RoomName))
                            {
                                //Console.WriteLine("dfsfs");
                                if (!GlobalData.Rooms.ContainsKey(RoomName))
                                {
                                    //   Console.WriteLine("dfsfs");
                                    GlobalData.Rooms[RoomName] = new RoomsData();
                                }
                                // Console.WriteLine("kfopdsfdsdfs");
                                var currentMembers = GlobalData.Rooms[RoomName].members;
                                //  Console.WriteLine("gdsgfds");
                                if (GlobalData.Rooms.ContainsKey(RoomName))
                                {
                                    //Console.WriteLine("fdsfdsfdsf");
                                    foreach (var member in currentMembers)
                                    {
                                        if (member.accountId == Saved_Clients.accountId)
                                        {
                                            return;
                                        }
                                    }
                                }

                                //  Console.WriteLine("fdsfdsfdsfU");
                                currentMembers.Add(new MembersData { accountId = Saved_Clients.accountId });

                              //  UserDataSaved.Rooms.Append(RoomName);
                                Saved_Clients.Rooms.Append(RoomName);
                                GlobalData.Rooms[RoomName].members = currentMembers;
                                //GlobalData.Rooms[RoomName]["Members"] = currentMembers;
                                // Console.WriteLine("MUCX NOT NULL");


                           


                                XElement featuresElement = new XElement(clientNs + "presence",
                                    new XAttribute("to", Saved_Clients.jid),
                                    new XAttribute("from", $"{RoomName}@muc.prod.ol.epicgames.com/{Uri.EscapeDataString(Saved_Clients.displayName)}:{Saved_Clients.accountId}:{Saved_Clients.resource}"),
                                    new XElement(MucNamespace + "x",
                                        new XElement("item",
                                            new XAttribute("nick", $"{Uri.EscapeDataString(Saved_Clients.displayName)}:{Saved_Clients.accountId}:{Saved_Clients.resource}"),
                                            new XAttribute("jid", Saved_Clients.jid),
                                            new XAttribute("role", "participant"),
                                            new XAttribute("affiliation", "none")
                                        )
                                    ),
                                    new XElement("status", new XAttribute("code", "110")),
                                    new XElement("status", new XAttribute("code", "100")),
                                    new XElement("status", new XAttribute("code", "170")),
                                    new XElement("status", new XAttribute("code", "201"))
                                );

                                xmlMessage = featuresElement.ToString();
                                buffer = Encoding.UTF8.GetBytes(xmlMessage);
                                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                                if (GlobalData.Rooms.TryGetValue(RoomName, out RoomsData? RoomData))
                                {
                                    //Console.WriteLine("TEST  " + RoomData);
                                    foreach (var member in RoomData.members)
                                    {
                                        Clients ClientData = GlobalData.Clients.Find(i => i.accountId == member.accountId)!;
                                        if (ClientData == null) continue;

                                        XElement presenceElement = new XElement(clientNs + "presence",
                                            new XAttribute("from", $"{RoomName}@muc.prod.ol.epicgames.com/{Uri.EscapeDataString(ClientData.displayName)}:{ClientData.accountId}:{ClientData.resource}"),
                                            new XAttribute("to", ClientData.jid),
                                            new XElement(MucNamespace + "x",
                                                    new XElement("item",
                                                    new XAttribute("nick", $"{Uri.EscapeDataString(ClientData.displayName)}:{ClientData.accountId}:{ClientData.resource}"),
                                                    new XAttribute("jid", ClientData.jid),
                                                    new XAttribute("role", "participant"),
                                                    new XAttribute("affiliation", "none")
                                                )
                                            )
                                        );

                                        xmlMessage = presenceElement.ToString();
                                        buffer = Encoding.UTF8.GetBytes(xmlMessage);
                                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                                        if (Saved_Clients.accountId == ClientData.accountId) continue;

                                        presenceElement = new XElement(clientNs + "presence",
                                            new XAttribute("from", $"{RoomName}@muc.prod.ol.epicgames.com/{Uri.EscapeDataString(Saved_Clients.displayName)}:{Saved_Clients.accountId}:{Saved_Clients.resource}"),
                                            new XAttribute("to", Saved_Clients.jid),
                                             new XElement(MucNamespace + "x",
                                                new XElement("item",
                                                    new XAttribute("nick", $"{Uri.EscapeDataString(Saved_Clients.displayName)}:{Saved_Clients.accountId}:{Saved_Clients.resource}"),
                                                    new XAttribute("jid", Saved_Clients.jid),
                                                    new XAttribute("role", "participant"),
                                                    new XAttribute("affiliation", "none")
                                                )
                                            )
                                        );

                                        xmlMessage = presenceElement.ToString();
                                        buffer = Encoding.UTF8.GetBytes(xmlMessage);
                                        await ClientData.Game_Client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                                    }
                                }

                                // bool bindElement = xmlDoc.Root.Descendants().Any(i => i.Name == mucNamespace + "muc:x" || i.Name == mucNamespace +"x");
                                //   Console.WriteLine("TEST DEFECT");
                                //Console.WriteLine(xmlDoc.Root?.Descendants().ToLookup());
                                //   Console.WriteLine(bindElement);
                                // if (bindElement == null) return;



                                //GlobalData.Rooms.FirstOrDefault(test => test.Key == "");
                                //  if (hasMucXOrXChild)
                                // {
                                //  if (xmlDoc.Root.Attribute("to") == null)
                                //  {
                                //     return;
                                // }
                                //     Console.WriteLine("YEAH");
                                // }
                            }
                        }
                        break;

                }
                XElement findStatus = xmlDoc.Root?.Descendants().FirstOrDefault(i => i.Name == "status")!;

                if (findStatus == null || string.IsNullOrEmpty(findStatus.Value))
                {
                    return;
                }
                object GrabbedStatus = "";
                try
                {
                    GrabbedStatus = JsonConvert.DeserializeObject(findStatus.Value)!;
                }
                catch (JsonReaderException ex)
                {
                    Logger.Error(ex.Message, "PRESENSE}:");
                    return; // wow
                }

                if (GrabbedStatus != null)
                {
                    string status = findStatus.Value;

                    bool away = false;
                    if(xmlDoc.Root?.Descendants().Any(i => i.Name == "show") == true)
                    {
                        away = true;
                    }
                 

                    Console.WriteLine("TEST + " + findStatus);
                    Console.WriteLine(away);

                    await XmppFriend.UpdatePresenceForFriends(webSocket, status, away, false);
                    await XmppFriend.GrabSomeonesPresence(Saved_Clients.accountId, Saved_Clients.accountId, false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, "Presence:Init");
            }

        }
    }
}
