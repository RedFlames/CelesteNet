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

    /*public class ChatArguments {

        public readonly List<Type> All = new();

        public ChatArguments() {
            foreach (Type type in CelesteNetUtils.GetTypes()) {
                if (!typeof(ArgType).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                Logger.Log(LogLevel.VVV, "chatargs", $"Found arg parser: ({type.FullName})");
                All.Add(type);
            }

            All = All.OrderBy(t => t.GetProperty("ParseOrder").GetConstantValue()).ToList();
        }

    }*/

    public class ArgParser {
        public readonly ChatModule Chat;
        public readonly ChatCMD Cmd;

        public bool AllOptional => Parameters.TrueForAll((Param a) => { return a.isOptional; });
        public int NeededParamCount => Parameters.Count(p => !p.isOptional);
        public bool NoParse => Parameters.Count == 0;
        public int HelpOrder = 0;

        // Only accepts one ArgType parser per position,
        // so if multiple ArgTypes apply you will need multiple ArgParsers.
        // Otherwise e.g. modeling a command that takes either
        // 2 Ints or 2 Strings but not an Int + String would be kinda hard?
        public List<Param> Parameters;

        public char[] Delimiters;

        public ArgParser(ChatModule chat, ChatCMD cmd, char[]? delimiters = null) {
            Chat = chat;
            Cmd = cmd;
            Parameters = new();
            Delimiters = delimiters ?? new char[] { ' ' };
        }

        public void AddParameter(Param p) {
            Param prev;
            if (!p.isOptional && Parameters.Count > 0 && (prev = Parameters.Last()).isOptional)
                throw new Exception($"Parameter {p.Help} of {Cmd.ID} must be Optional after {prev.Help} (flags = {prev.Flags})");
            Parameters.Add(p);
        }

        public List<ChatCMDArg> Parse(string raw, ChatCMDEnv env) {
            List<ChatCMDArg> values = new();

            raw = raw.Trim();

            if ((raw.IsNullOrEmpty() && AllOptional))
                return values;

            if (NoParse) {
                values.Add(new ChatCMDArg(raw));
                return values;
            }

            Logger.Log(LogLevel.DEV, "argparse", $"Parsing '{raw}'...");

            int from, to = -1, p;

            for(p = 0; p < Parameters.Count; p++) {
                Param param = Parameters[p];

                if ((from = to + 1) >= raw.Length) {
                    break;
                }

                if ((to = raw.IndexOfAny(Delimiters, from)) < 0 
                    || (param is ParamString && p == Parameters.Count - 1)) {
                    to = raw.Length;
                }

                int argIndex = from;
                int argLength = to - from;

                string rawValue = raw.Substring(argIndex, argLength).Trim();

                Logger.Log(LogLevel.DEV, $"argparse{p}", $"Looking for param {p} {param.Placeholder} at {argIndex}+{argLength}.");
                Logger.Log(LogLevel.DEV, $"argparse{p}", $"Substring is '{rawValue}'.");

                // detect spaced out ranges
                if (param is ParamIntRange && raw[to] == ' ') {
                    int lookahead_idx = to + 1;
                    while (lookahead_idx < raw.Length && raw[lookahead_idx] == ' ')
                        lookahead_idx++;

                    if(raw[lookahead_idx] == '-' && raw[++lookahead_idx] == ' ') {
                        while (lookahead_idx < raw.Length && raw[lookahead_idx] == ' ')
                            lookahead_idx++;
                        if ((to = raw.IndexOf(' ', lookahead_idx)) < 0) {
                            to = raw.Length;
                        }
                        rawValue = raw.Substring(argIndex, to - from - 1).Replace(" ", "");
                    }
                }

                if(param.TryParse(rawValue, env, out ChatCMDArg? arg) && arg != null) {
                    Logger.Log(LogLevel.DEV, $"argparse{p}", $"{param.GetType()}.TryParse returned {arg.GetType().FullName} {arg.RawText} {arg.String}.");
                    values.Add(arg);
                } else {
                    Logger.Log(LogLevel.DEV, $"argparse{p}", $"{param.GetType()}.TryParse failed or returened null.");
                    break;
                }
            }

            if(raw.IsNullOrEmpty() && Parameters.Count > 0 && !Parameters[0].isOptional) {
                if (Parameters[0].TryParse(raw, env, out ChatCMDArg? arg) && arg != null) {
                    values.Add(arg);
                    p = 1;
                }
            }

            if (p < Parameters.Count && !Parameters[p].isOptional)
                throw new Exception($"Necessary parameter {Parameters[p].Placeholder} not found.");

            return values;
        }

        public string ToExample() => string.Join(" ", Parameters.Select(p => p.ExampleValue));

        public override string ToString() => string.Join(" ", Parameters.Select(p => p.Placeholder));
    }

    public abstract class Param {


        public readonly ChatModule Chat;

        public virtual string Help => "";
        protected virtual string PlaceholderName => "?";
        public virtual string Placeholder => (Flags.HasFlag(ParamFlags.Optional) ? "[" : "<") + PlaceholderName + (Flags.HasFlag(ParamFlags.Optional) ? "]" : ">");
        public virtual string ExampleValue => "?";

        public readonly ParamFlags Flags = ParamFlags.None;
        public bool isOptional => Flags.HasFlag(ParamFlags.Optional);
        public Action<string, ChatCMDEnv, ChatCMDArg>? Validate;

        public Param(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None) {
            Chat = chat;
            Validate = validate;
            Flags = flags;
        }

        public virtual bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {
            arg = new ChatCMDArg(raw);
            Validate?.Invoke(raw, env, arg);
            return true;
        }

        public override string ToString() => Help;

        public static implicit operator string(Param arg) => arg.Help;

    }

    [Flags]
    public enum ParamFlags {
        None,
        Optional,
        NonPositive,
        NonNegative,
        NonZero
    }

}
