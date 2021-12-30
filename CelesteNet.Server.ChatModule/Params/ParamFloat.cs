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
    public class ParamFloat : Param {

        public override string Help => "A floating-point value";
        public override string PlaceholderName { get; set; } = "float";
        public float Min, Max;

        public ParamFloat(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None,
                              float min = float.MinValue, float max = float.MaxValue) : base(chat, validate, flags) {
            Min = Math.Max(min, Flags.HasFlag(ParamFlags.NonNegative) ? Flags.HasFlag(ParamFlags.NonZero) ? 1 : 0 : float.MinValue);
            Max = Math.Min(max, Flags.HasFlag(ParamFlags.NonPositive) ? Flags.HasFlag(ParamFlags.NonZero) ? -1 : 0 : float.MaxValue);
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {

            if (float.TryParse(raw, out float result) && result >= Min && result <= Max) {
                arg = new ChatCMDArg(result);
                Validate?.Invoke(raw, env, arg);
                return true;
            }

            arg = null;
            return false;
        }
    }
}