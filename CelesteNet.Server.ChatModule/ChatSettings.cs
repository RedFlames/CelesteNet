﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.CelesteNet.Server.Chat {
    public class ChatSettings : CelesteNetServerModuleSettings {

        public int MaxChatTextLength { get; set; } = 256;

        public string CommandPrefix { get; set; } = "/";

        public bool GreetPlayers { get; set; } = true;
        public bool LogEmotes { get; set; } = false;

        public int SpamCount { get; set; } = 3;
        public int SpamCountMax { get; set; } = 4;
        public double SpamTimeout { get; set; } = 4;
        public double SpamTimeoutAdd { get; set; } = 5;

        public List<string> FilterDrop {  get; set; } = new List<string>();
        public List<string> FilterKick { get; set; } = new List<string>();
        public List<string> FilterWarnOnce { get; set; } = new List<string>();
        public FilterHandling FilterPlayerNames { get; set; } = FilterHandling.Drop | FilterHandling.Kick;
        public FilterHandling FilterChannelNames { get; set; } = FilterHandling.Drop | FilterHandling.Kick;
        public bool FilterOnlyGlobalAndMainChat { get; set; } = true;
        public bool FilterPrivateChannelNames { get; set; } = false;

        public Color ColorBroadcast { get; set; } = Calc.HexToColor("#00adee");
        public Color ColorServer { get; set; } = Calc.HexToColor("#9e24f5");
        public Color ColorError { get; set; } = Calc.HexToColor("#c71585");
        public Color ColorCommand { get; set; } = Calc.HexToColor("#2e31f1");
        public Color ColorCommandReply { get; set; } = Calc.HexToColor("#e39dcc");
        public Color ColorWhisper { get; set; } = Calc.HexToColor("#888888");
        public Color ColorLogEmote { get; set; } = Calc.HexToColor("#bbbb88");

        public string MessageGreeting { get; set; } = "Welcome {player}, to <insert server name here>!";
        public string MessageMOTD { get; set; } =
@"Don't cheat. Have fun!
Press T to talk.
Send /help for a list of all commands.";
        public string MessageLeave { get; set; } = "Cya, {player}!";
        public string MessageKick { get; set; } = "{player} did an oopsie!";
        public string MessageDefaultKickReason { get; set; } = "Kicked";
        public string MessageBan { get; set; } = "{player} won't come back.";
        public string MessageSpam { get; set; } = "Stop spamming.";
        public string MessageWarnOnce { get; set; } = "Your message may be in violation of CNet rules. Send it again if you're sure it's acceptable, and it will go through. May be subjected to review.";
    }

    [Flags]
    public enum FilterHandling {
        None = 0,
        WarnOnce = 1,
        Drop = 2,
        Kick = 4,
    }
}
