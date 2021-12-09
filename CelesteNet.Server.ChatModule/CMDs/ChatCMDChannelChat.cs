using Celeste.Mod.CelesteNet.DataTypes;
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
    public class ChatCMDCC : ChatCMDChannelChat {

        public override string Info => $"Alias for {Chat.Settings.CommandPrefix}{Chat.Commands.Get<ChatCMDChannelChat>().ID}";

    }

    public class ChatCMDChannelChat : ChatCMD {

        public override string Info => "Send a whisper to everyone in the channel or toggle auto channel chat.";

        public override string Help =>
$@"Send a whisper to everyone in the channel or toggle auto channel chat.
To send a message in the channel, {Chat.Settings.CommandPrefix}{ID} message here
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

            if (args.Count == 0 || args[0].Type != ChatCMDArgType.String || args[0].String.IsNullOrEmpty()) {
                if (env.Server.UserData.GetKey(session.UID).IsNullOrEmpty())
                    throw new Exception("You must be registered to enable / disable auto channel chat mode!");

                ChatModule.UserChatSettings settings = env.Server.UserData.Load<ChatModule.UserChatSettings>(session.UID);
                settings.AutoChannelChat = !settings.AutoChannelChat;
                env.Server.UserData.Save(session.UID, settings);
                env.Send($"{(settings.AutoChannelChat ? "Enabled" : "Disabled")} auto channel chat.\nFilter out global chat via the mod options.");
                return;
            }

            DataPlayerInfo? player = env.Player;
            Channel channel = session.Channel;

            using ListSnapshot<CelesteNetPlayerSession> others = channel.Players.Where(p => p != session).ToSnapshot(channel.Lock);
            DataChat? msg = Chat.PrepareAndLog(null, new DataChat {
                Player = player,
#pragma warning disable CS8619 // LINQ is dumb.
                Targets = others.Select(p => p.PlayerInfo).Where(p => p != null).ToArray(),
#pragma warning restore CS8619
                Tag = $"channel {channel.Name}",
                Text = args[0].String,
                Color = Chat.Settings.ColorWhisper
            });

            if (msg == null)
                return;

            if (player != null) {
                env.Msg.Tag = $"channel {channel.Name}";
                env.Msg.Text = args[0].String;
                env.Msg.Color = Chat.Settings.ColorWhisper;
                Chat.ForceSend(env.Msg);
            }

            // FIXME: ForceSend doesn't already do this..?
            if ((msg.Targets?.Length ?? 0) == 0)
                return;

            DataInternalBlob blob = new(env.Server.Data, msg);
            foreach (CelesteNetPlayerSession other in others)
                other.Con.Send(blob);
        }

    }
}
