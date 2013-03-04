﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Json;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Samples.Hubs.DemoHub
{
    public class ChatHub : Hub
    {
        // User Names mapped to User objects
        private static readonly ConcurrentDictionary<string, ChatUser> _users = new ConcurrentDictionary<string, ChatUser>(StringComparer.OrdinalIgnoreCase);

        // User Names mapped to list of rooms
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userRooms = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        // Room names mapped to romm objects
        private static readonly ConcurrentDictionary<string, ChatRoom> _rooms = new ConcurrentDictionary<string, ChatRoom>(StringComparer.OrdinalIgnoreCase);

        private int emailCount = 0, numUsers = 10;

        public void Send(string message)
        {
            // Call the broadcastMessage method to update clients.
            /*foreach (string roomName in _userRooms[Clients.Caller.userName])
            {
                Clients.OthersInGroup(Clients.Caller.roomName).broadcastMessage(String.Format(" {0} : {1}", Clients.Caller.userName, message));
            }*/
            Clients.All.broadcastMessage(message, "abhi");
        }

        public Task AddUser(string userName)
        {
            var user = new ChatUser(userName, Context.ConnectionId);
            _users.TryAdd(userName, user);
            Clients.Caller.userName = user.Name;
            Clients.Caller.userId = user.Id;
            return Clients.All.broadcastMessage(String.Format("New user {0} has joined", user.Name));
        }

        public void UpdateData()
        {
            Clients.Caller.updateEmail(emailCount++ + " new message(s)");
            Clients.Caller.updateUsers(numUsers++ + " people are online");
        }

        public override Task OnConnected()
        {
            Timer _timer;
            _timer = new Timer(_ => UpdateData(), state: null, dueTime: 0, period: 10000);
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            // string groupName = Clients.Caller.groupName;
            return base.OnDisconnected();
            // return Clients.Others.broadcastMessage(String.Format("Client {0} has left the chat room", "left"));
        }

        public override Task OnMethodMissing(string methodName, IJsonValue[] parameters)
        {
            return Task.Factory.StartNew(() => Debug.WriteLine(String.Format("Method {0} is not defined on the Hub", methodName)));        
            // Debug.WriteLine(String.Format("Method {0} is not defined on the Hub", methodName));
            // return Clients.Caller.broadcastMessage(String.Format("Method {0} is not defined on the Hub", methodName));
        }

        public override Task OnMethodExecuted(string methodName, IJsonValue[] parameters)
        {
            return Task.Factory.StartNew(() => Debug.WriteLine(String.Format("Method {0} was executed on the Hub", methodName)));        
            // return Clients.Caller.broadcastMessage1(String.Format("Method {0} was executed on the Hub", methodName));
        }

        [Serializable]
        public class ChatUser
        {
            public string ConnectionId { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }

            public ChatUser()
            {
            }

            public ChatUser(string name, string connectionId)
            {
                Name = name;
                ConnectionId = connectionId;
                Id = Guid.NewGuid().ToString("d");
            }
        }

        public class ChatRoom
        {
            // should User be readonly
            public HashSet<string> Users { get; private set; }
            public List<string> ChatMessages { get; set; }

            public ChatRoom()
            {
                Users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                ChatMessages = new List<string>();
            }
        }
    }
}