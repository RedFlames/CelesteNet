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
    public class ParamIntRange : Param {
        public override string Help => "An integer range";
        public override string PlaceholderName { get; set; } = "from-to";
        public override string ExampleValue => $"{(Min == int.MinValue? -9999 : Min)}-{(Max == int.MaxValue? 9999 : Max)}";
        public int Min, Max;

        public ParamIntRange(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None,
                              int min = int.MinValue, int max = int.MaxValue) : base(chat, validate, flags) {
            Min = Math.Max(min, Flags.HasFlag(ParamFlags.NonNegative) ? Flags.HasFlag(ParamFlags.NonZero) ? 1 : 0 : int.MinValue);
            Max = Math.Min(max, Flags.HasFlag(ParamFlags.NonPositive) ? Flags.HasFlag(ParamFlags.NonZero) ? -1 : 0 : int.MaxValue);
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {

            string[] split;
            int from, to;
            if ((split = raw.Split('-')).Length == 2) {
                if (int.TryParse(split[0].Trim(), out from) && int.TryParse(split[1].Trim(), out to)) {
                    if (from >= Min && to <= Max) {
                        arg = new ChatCMDArg(from, to);
                        Validate?.Invoke(raw, env, arg);
                        return true;
                    }
                }
            } else if ((split = raw.Split('+')).Length == 2) {
                if (int.TryParse(split[0].Trim(), out from) && int.TryParse(split[1].Trim(), out to)) {
                    if (from >= Min && from + to <= Max) {
                        arg = new ChatCMDArg(from, from + to);
                        Validate?.Invoke(raw, env, arg);
                        return true;
                    }
                }
            }

            arg = null;
            return false;
        }
    }
}