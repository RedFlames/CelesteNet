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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteNet.Server.Chat {
    public class ParamString : Param {
        public override string Help => "A string" + (maxLength > 0 ? $" (max. {maxLength} characters)" : "");
        public override string PlaceholderName { get; set; } = "string";
        public override string ExampleValue => "Text";
        public int maxLength;
        public Regex? Re;
        public bool Truncate;

        public ParamString(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None,
                              int maxlength = 0, string? regex = null, bool truncate = true) : base(chat, validate, flags) {
            maxLength = maxlength;
            if (!regex.IsNullOrEmpty())
                Re = new Regex(regex, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            Truncate = truncate;
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {
            if(maxLength > 0 && raw.Length > maxLength) {
                if(Truncate) {
                    raw = raw.Substring(0, maxLength);
                } else {
                    arg = null;
                    return false;
                }
            }

            if(Re != null) {
                Logger.Log(LogLevel.DEV, "paramstring", $"Testing '{raw}' against regex {Re.ToString()} is {Re.IsMatch(raw)}");
            }

            if (!Re?.IsMatch(raw) ?? false)
                throw new Exception($"Argument '{raw}' is invalid.");

            arg = new ChatCMDArg(raw);
            Validate?.Invoke(raw, env, arg);
            return true;
        }
    }
}