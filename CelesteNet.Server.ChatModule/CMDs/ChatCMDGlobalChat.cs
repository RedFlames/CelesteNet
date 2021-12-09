using Celeste.Mod.CelesteNet.DataTypes;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteNet.Server.Chat {
    public class ChatCMDGC : ChatCMDGlobalChat {

        public override string Info => $"Alias for {Chat.Settings.CommandPrefix}{Chat.Commands.Get<ChatCMDGlobalChat>().ID}";

    }

    public class ChatCMDGlobalChat : ChatCMD {

        //public override string Args => "<text>";

        public override string Info => "Send a message to everyone in the server.";

        public override string Help =>
$@"Send a message to everyone in the server.
To send a message, {Chat.Settings.CommandPrefix}{ID} message here
To enable / disable auto channel chat mode, {Chat.Settings.CommandPrefix}{ID}";

        public override void Init(ChatModule chat) {
            Chat = chat;

            ArgParser parser = new(chat, this);
            parser.AddParameter(new ParamString(chat, null, ParamFlags.Optional));
            ArgParsers.Add(parser);
        }

        public override void Run(ChatCMDEnv env, List<ChatCMDArg> args) {
            CelesteNetPlayerSession? session = env.Session;
            if (session == null)
                return;

            if (args.Count == 0 || string.IsNullOrEmpty(args[0].String)) {
                Chat.Commands.Get<ChatCMDChannelChat>().Run(env, args);
                return;
            }

            DataPlayerInfo? player = env.Player;
            if (player == null)
                return;

            DataChat? msg = Chat.PrepareAndLog(null, new DataChat {
                Player = player,
                Text = args[0].String
            });

            if (msg == null)
                return;

            env.Msg.Text = args[0].String;
            env.Msg.Tag = "";
            env.Msg.Color = Color.White;
            env.Msg.Target = null;
            Chat.ForceSend(env.Msg);
        }

    }
}
