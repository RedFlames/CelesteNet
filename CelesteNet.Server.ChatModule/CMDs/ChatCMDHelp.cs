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

    public class ChatCMDH : ChatCMDHelp {

        public override string Info => $"Alias for {Chat.Settings.CommandPrefix}{Chat.Commands.Get<ChatCMDHelp>().ID}";

    }
    public class ChatCMDHelp : ChatCMD {

        //public override string Args => "[page] | [command]";

        public override string Info => "Get help on how to use commands.";
        public override string Help =>
$@"List all commands with {Chat.Settings.CommandPrefix}{ID} [page number]
Show help on a command with {Chat.Settings.CommandPrefix}{ID} <cmd>
(You did exactly that just now to get here, I assume!)";

        public override int HelpOrder => int.MinValue;

        public const int pageSize = 8;

        public override void Init(ChatModule chat) {
            Chat = chat;

            ArgParser parser = new(chat, this);
            parser.AddParameter(new ParamHelpPage(chat, null, ParamFlags.Optional));
            parser.HelpOrder = int.MinValue;
            ArgParsers.Add(parser);

            parser = new(chat, this);
            parser.AddParameter(new ParamString(chat, null, ParamFlags.None, 0, @"[^0-9]+"));
            ArgParsers.Add(parser);
        }

        public override void Run(ChatCMDEnv env, List<ChatCMDArg> args) {
            Logger.Log(LogLevel.DEV, "cmdhelp", $"{GetType()}.Run args# {args?.Count}");
            if (args?.Count > 0) {
                Logger.Log(LogLevel.DEV, "cmdhelp", $"{GetType()}.Run arg0: {args[0].RawText}");
                if (args[0].Type == ChatCMDArgType.Int) {
                    Logger.Log(LogLevel.DEV, "cmdhelp", $"{GetType()}.Run arg0 is int: {args[0].Int}");
                    env.Send(GetCommandPage(args[0].Int));
                    return;
                }

                env.Send(GetCommandSnippet(args[0].String));
                return;
            }

            Logger.Log(LogLevel.DEV, "cmdhelp", $"Sending zero page");
            env.Send(GetCommandPage());
        }

        public string GetCommandPage(int page = 0) {
            string prefix = Chat.Settings.CommandPrefix;
            StringBuilder builder = new();

            int pages = (int) Math.Ceiling(Chat.Commands.All.Count / (float) pageSize);
            if (page < 0 || pages <= page)
                throw new Exception("Page out of range.");

            for (int i = page * pageSize; i < (page + 1) * pageSize && i < Chat.Commands.All.Count; i++) {
                ChatCMD cmd = Chat.Commands.All[i];

                IOrderedEnumerable<ArgParser> parsers = cmd.ArgParsers.OrderBy(ap => ap.HelpOrder);

                if(cmd.ArgParsers.Count > 1 && cmd.ArgParsers.TrueForAll(ap => ap.Parameters.Count == 1)) {
                    StringBuilder alternatives = new();

                    alternatives
                            .Append(prefix)
                            .Append(cmd.ID)
                            .Append(" ");
                    foreach (ArgParser args in parsers) {
                        alternatives
                            .Append(args.ToString())
                            .Append(args == parsers.Last() ? "" : " | ");
                    }
                    builder
                        .Append(alternatives.ToString())
                        .AppendLine();
                } else {
                    foreach (ArgParser args in parsers) {
                        builder
                            .Append(prefix)
                            .Append(cmd.ID)
                            .Append(" ")
                            .Append(args.ToString())
                            .AppendLine();
                    }
                }
            }

            builder
                .Append("Page ")
                .Append(page + 1)
                .Append("/")
                .Append(pages);

            return builder.ToString().Trim();
        }

        public string GetCommandSnippet(string cmdName) {
            ChatCMD? cmd = Chat.Commands.Get(cmdName);
            if (cmd == null)
                throw new Exception($"Command {cmdName} not found.");

            return Help_GetCommandSnippet(cmd);
        }

        public string Help_GetCommandSnippet(ChatCMD cmd) {
            string prefix = Chat.Settings.CommandPrefix;
            StringBuilder builder = new();
            StringBuilder builderSyntax = new();
            StringBuilder builderExamples = new();

            IOrderedEnumerable<ArgParser> parsers = cmd.ArgParsers.OrderBy(ap => ap.HelpOrder);
            foreach (ArgParser args in parsers) {
                builderSyntax
                    .Append(prefix)
                    .Append(cmd.ID)
                    .Append(" ")
                    .Append(args.ToString())
                    .AppendLine();
                if(parsers.Any(ap => ap.NeededParamCount > 0)) {
                    builderExamples
                        .Append(prefix)
                        .Append(cmd.ID)
                        .Append(" ")
                        .Append(args.ToExample())
                        .AppendLine();
                }
            }

            builder
                .Append(builderSyntax.ToString())
                .AppendLine()
                .AppendLine(cmd.Help);

            if(builderExamples.Length > 0) {
                builder
                    .AppendLine()
                    .AppendLine("Examples:")
                    .Append(builderExamples.ToString());
            }

            return builder.ToString().Trim();
        }

    }
}
