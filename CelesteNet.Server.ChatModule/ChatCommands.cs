using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.CelesteNet.DataTypes;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CelesteNet.Server.Chat {
    public class ChatCommands : IDisposable {

        public readonly List<ChatCMD> All = new();
        public readonly Dictionary<string, ChatCMD> ByID = new();
        public readonly Dictionary<Type, ChatCMD> ByType = new();

        public ChatCommands(ChatModule chat) {
            foreach (Type type in CelesteNetUtils.GetTypes()) {
                if (!typeof(ChatCMD).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                ChatCMD? cmd = (ChatCMD?) Activator.CreateInstance(type);
                if (cmd == null)
                    throw new Exception($"Cannot create instance of CMD {type.FullName}");
                Logger.Log(LogLevel.VVV, "chatcmds", $"Found command: {cmd.ID.ToLowerInvariant()} ({type.FullName})");
                All.Add(cmd);
                ByID[cmd.ID.ToLowerInvariant()] = cmd;
                ByType[type] = cmd;
            }

            foreach (ChatCMD cmd in All)
                cmd.Init(chat);

            All = All.OrderBy(cmd => cmd.HelpOrder).ToList();
        }

        public void Dispose() {
            foreach (ChatCMD cmd in All)
                cmd.Dispose();
        }

        public ChatCMD? Get(string id)
            => ByID.TryGetValue(id, out ChatCMD? cmd) ? cmd : null;

        public T? Get<T>(string id) where T : ChatCMD
            => ByID.TryGetValue(id, out ChatCMD? cmd) ? (T) cmd : null;

        public T Get<T>() where T : ChatCMD
            => ByType.TryGetValue(typeof(T), out ChatCMD? cmd) ? (T) cmd : throw new Exception($"Invalid CMD type {typeof(T).FullName}");

    }

    public abstract class ChatCMD : IDisposable {

        public static readonly char[] NameDelimiters = {
            ' ', '\n'
        };

#pragma warning disable CS8618 // Set manually after construction.
        public ChatModule Chat;
#pragma warning restore CS8618

        public List<ArgParser> ArgParsers = new();

        public virtual string ID => GetType().Name.Substring(7).ToLowerInvariant();

        public abstract string Info { get; }
        public virtual string Help => Info;
        public virtual int HelpOrder => 0;

        public virtual void Init(ChatModule chat) {
            Chat = chat;
        }

        public virtual void Dispose() {
        }

        public virtual void ParseAndRun(ChatCMDEnv env) {
            Exception? caught = null;

            foreach (ArgParser parser in ArgParsers) {
                try {
                    ParseAndRun(env, parser);
                    return;
                } catch(Exception e) {
                    Logger.Log(LogLevel.DEV, "ChatCMD", $"ParseAndRun exception caught: {e.Message}");
                    caught = e;
                }
            }

            throw caught ?? new Exception("How did we get here");
        }

        public virtual void ParseAndRun(ChatCMDEnv env, ArgParser parser) {
            string raw = env.FullText.Substring(Chat.Settings.CommandPrefix.Length + ID.Length);

            List<ChatCMDArg> args = parser.Parse(raw, env);

            Run(env, args);
        }

        public virtual void Run(ChatCMDEnv env, List<ChatCMDArg> args) {
        }

    }

    public class ChatCMDArg {

        public string RawText = "";
        public string String = "";
        public int Index;

        public ChatCMDArgType Type;

        public int Int;
        public long Long;
        public ulong ULong;
        public float Float;

        public int IntRangeFrom;
        public int IntRangeTo;
        public int IntRangeMin => Math.Min(IntRangeFrom, IntRangeTo);
        public int IntRangeMax => Math.Max(IntRangeFrom, IntRangeTo);

        public CelesteNetPlayerSession? Session;
        public ListSnapshot<Channel>? ChannelList;
        public Channel? Channel;

        public ChatCMDArg(int val) {
            Int = val;
            Type = ChatCMDArgType.Int;
        }
        public ChatCMDArg(int start, int end) {
            Int = start;
            IntRangeFrom = start;
            IntRangeTo = end;
            Type = ChatCMDArgType.IntRange;
        }

        public ChatCMDArg(float val) {
            Float = val;
            Type = ChatCMDArgType.Float;
        }

        public ChatCMDArg(long val) {
            Long = val;
            Type = ChatCMDArgType.Long;
        }

        public ChatCMDArg(ulong val) {
            ULong = val;
            Type = ChatCMDArgType.ULong;
        }

        public ChatCMDArg(int val, ListSnapshot<Channel> channels) {
            Int = val;
            Type = ChatCMDArgType.ChannelPage;
            ChannelList = channels;
        }

        public ChatCMDArg(string name, Channel? channel) {
            String = name;
            Type = ChatCMDArgType.ChannelName;
            Channel = channel;
        }

        public ChatCMDArg(CelesteNetPlayerSession? val) {
            Session = val;
            Type = ChatCMDArgType.PlayerSession;
        }

        public ChatCMDArg(string val) {
            String = val;
            Type = ChatCMDArgType.String;
        }

        public string Rest => RawText.Substring(Index);

        public override string ToString() => String;

        public static implicit operator string(ChatCMDArg arg) => arg.String;

    }

    public enum ChatCMDArgType {
        String,

        Int,
        IntRange,

        Long,
        ULong,

        Float,

        PlayerSession,
        ChannelName,
        ChannelPage
    }

    public class ChatCMDEnv {

        public readonly ChatModule Chat;
        public readonly DataChat Msg;

        public ChatCMD? Cmd;

        public ChatCMDEnv(ChatModule chat, DataChat msg) {
            Chat = chat;
            Msg = msg;
        }

        public uint PlayerID => Msg.Player?.ID ?? uint.MaxValue;

        public CelesteNetServer Server => Chat.Server ?? throw new Exception("Not ready.");

        public CelesteNetPlayerSession? Session {
            get {
                if (Msg.Player == null)
                    return null;
                if (!Chat.Server.PlayersByID.TryGetValue(PlayerID, out CelesteNetPlayerSession? session))
                    return null;
                return session;
            }
        }

        public DataPlayerInfo? Player => Session?.PlayerInfo;

        public DataPlayerState? State => Chat.Server.Data.TryGetBoundRef(Player, out DataPlayerState? state) ? state : null;

        public string FullText => Msg.Text;
        public string Text => Cmd == null ? Msg.Text : Msg.Text.Substring(Chat.Settings.CommandPrefix.Length + Cmd.ID.Length);

        public DataChat? Send(string text, string? tag = null, Color? color = null) => Chat.SendTo(Session, text, tag, color ?? Chat.Settings.ColorCommandReply);

        public DataChat? Error(Exception e) {
            string cmdName = Cmd?.ID ?? "?";

            if (e.GetType() == typeof(Exception)) {
                Logger.Log(LogLevel.VVV, "chatcmd", $"Command {cmdName} failed:\n{e}");
                return Send($"Command {cmdName} failed: {e.Message}", color: Chat.Settings.ColorError);
            }

            Logger.Log(LogLevel.ERR, "chatcmd", $"Command {cmdName} failed:\n{e}");
            return Send($"Command {cmdName} failed due to an internal error.", color: Chat.Settings.ColorError);
        }

    }

}
