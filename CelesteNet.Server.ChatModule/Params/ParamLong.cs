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
    public class ParamLong : Param {
        public override string Help => "A long (integer) value";
        protected override string PlaceholderName => "long";
        public override string ExampleValue => Max.ToString();
        public long Min, Max;

        public ParamLong(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None,
                              long min = long.MinValue, long max = long.MaxValue) : base(chat, validate, flags) {
            Min = Math.Max(min, Flags.HasFlag(ParamFlags.NonNegative) ? Flags.HasFlag(ParamFlags.NonZero) ? 1 : 0 : long.MinValue);
            Max = Math.Min(max, Flags.HasFlag(ParamFlags.NonPositive) ? Flags.HasFlag(ParamFlags.NonZero) ? -1 : 0 : long.MaxValue);
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {

            if (long.TryParse(raw, out long result) && result >= Min && result <= Max) {
                arg = new ChatCMDArg(result);
                Validate?.Invoke(raw, env, arg);
                return true;
            }

            arg = null;
            return false;
        }
    }
}