﻿using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Remotely.Shared.Models;
using Remotely.ScreenCast.Core.Enums;
using Remotely.ScreenCast.Core.Input;
using Remotely.ScreenCast.Core.Models;
using Remotely.ScreenCast.Core.Sockets;
using Remotely.ScreenCast.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotely.ScreenCast.Core.Interfaces;

namespace Remotely.ScreenCast.Core
{
    public class Conductor
    {
        public static Conductor Current { get; private set; }
        public IScreenCaster ScreenCaster { get; }

        public Conductor(CasterSocket casterSocket,
            IScreenCaster screenCaster)
        {
            Current = this;
            ScreenCaster = screenCaster;
            CasterSocket = casterSocket;
        }
        public event EventHandler<ScreenCastRequest> ScreenCastRequested;

        public event EventHandler<string> SessionIDChanged;

        public event EventHandler<Viewer> ViewerAdded;

        public event EventHandler<string> ViewerRemoved;
        public Dictionary<string, string> ArgDict { get; set; }
        public CasterSocket CasterSocket { get; private set; }
        public HubConnection Connection { get; private set; }
        public string CurrentDesktopName { get; set; }
        public string DeviceID { get; private set; }
        public string Host { get; private set; }
        public IdleTimer IdleTimer { get; set; }
        public AppMode Mode { get; private set; }
        public string RequesterID { get; private set; }
        public string ServiceID { get; private set; }
        public ConcurrentDictionary<string, Viewer> Viewers { get; } = new ConcurrentDictionary<string, Viewer>();
        public async Task Connect()
        {
            await CasterSocket.Connect(Host);
        }

        public void ProcessArgs(string[] args)
        {
            ArgDict = new Dictionary<string, string>();

            for (var i = 0; i < args.Length; i += 2)
            {
                var key = args?[i];
                if (key != null)
                {
                    key = key.Trim().Replace("-", "").ToLower();
                    var value = args?[i + 1];
                    if (value != null)
                    {
                        ArgDict[key] = args[i + 1].Trim();
                    }
                }

            }

            Mode = (AppMode)Enum.Parse(typeof(AppMode), ArgDict["mode"]);
            Host = ArgDict["host"];

            if (Mode == AppMode.Unattended)
            {
                RequesterID = ArgDict["requester"];
                CurrentDesktopName = ArgDict["desktop"];
                ServiceID = ArgDict["serviceid"];
                DeviceID = ArgDict["deviceid"];
            }
        }

        public void InvokeScreenCastRequested(ScreenCastRequest viewerIdAndRequesterName)
        {
            ScreenCastRequested?.Invoke(null, viewerIdAndRequesterName);
        }
        public void InvokeSessionIDChanged(string sessionID)
        {
            SessionIDChanged?.Invoke(null, sessionID);
        }

        public void InvokeViewerAdded(Viewer viewer)
        {
            ViewerAdded?.Invoke(null, viewer);
        }
        public void InvokeViewerRemoved(string viewerID)
        {
            ViewerRemoved?.Invoke(null, viewerID);
        }
    }
}
