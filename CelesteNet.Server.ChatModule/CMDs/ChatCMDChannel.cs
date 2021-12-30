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
    public class ChatCMDJoin : ChatCMDChannel {

        public override string Info => $"Alias for {Chat.Settings.CommandPrefix}{Chat.Commands.Get<ChatCMDChannel>().ID}";

    }

    public class ChatCMDChannel : ChatCMD {
        public override string Info => "Switch to a different channel.";
        public override string Help =>
$@"Switch to a different channel.
To list all public channels, {Chat.Settings.CommandPrefix}{ID}
To create / join a public channel, {Chat.Settings.CommandPrefix}{ID} channel
To create / join a private channel, {Chat.Settings.CommandPrefix}{ID} {Channels.PrefixPrivate}channel
To go back to the default channel, {Chat.Settings.CommandPrefix}{ID} {Chat.Server.Channels.Default.Name}";

        public const int pageSize = 8;


        public override void Init(ChatModule chat) {
            Chat = chat;

            ArgParser parser = new(chat, this);
            parser.AddParameter(new ParamChannelName(chat));
            ArgParsers.Add(parser);

            parser = new(chat, this);
            parser.AddParameter(new ParamChannelPage(chat));
            ArgParsers.Add(parser);
        }

        public override void Run(ChatCMDEnv env, List<ChatCMDArg> args) {
            CelesteNetPlayerSession? session = env.Session;
            if (session == null)
                throw new ArgumentNullException($"Called {Chat.Settings.CommandPrefix}{ID} without player Session.");

            if (args.Count == 0)
                throw new ArgumentException($"Called {Chat.Settings.CommandPrefix}{ID} with no Argument?");

            if (args[0].Type == ChatCMDArgType.ChannelPage) {
                int page = args[0].Int;
                ListSnapshot<Channel>? channels = args[0].ChannelList;

                if (channels == null)
                    throw new Exception($"Server failed to find channels to list.");

                int pages = (int) Math.Ceiling(channels.Count / (float) pageSize);
                StringBuilder builder = new();

                if (page == 0 && session != null)
                    builder
                        .Append("You're in ")
                        .Append(session.Channel.Name)
                        .AppendLine();

                for (int i = page * pageSize; i < (page + 1) * pageSize && i < channels.Count; i++) {
                    Channel? c = channels[i];
                    builder
                        .Append(c.PublicName)
                        .Append(" - ")
                        .Append(c.Players.Count)
                        .Append(" players")
                        .AppendLine();
                }

                builder
                    .Append("Page ")
                    .Append(page + 1)
                    .Append("/")
                    .Append(pages);

                env.Send(builder.ToString().Trim());

            } else if (args[0].Type == ChatCMDArgType.ChannelName) {
                if (session == null)
                    throw new Exception("Only Players can switch channels. Are you the server?");

                Tuple<Channel, Channel> tuple = env.Server.Channels.Move(session, args[0].String);
                if (tuple.Item1 == tuple.Item2) {
                    env.Send($"Already in {tuple.Item2.Name}");
                } else {
                    env.Send($"Moved to {tuple.Item2.Name}");
                }
            }

        }

    }
}
