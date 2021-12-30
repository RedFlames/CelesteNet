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
    public class ParamChannelName : Param {

        public override string Help => $"Name of {(MustExist ? "an existing" : "an existing or new")} channel";
        public override string PlaceholderName { get; set; } = "channel";
        public override string ExampleValue => "main";

        public bool MustExist = false;

        public ParamChannelName(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None,
                              bool mustExist = false) : base(chat, validate, flags) {
            MustExist = mustExist;
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {

            CelesteNetPlayerSession? self = env.Session;

            if (MustExist) {
                if (env.Server.Channels.ByName.TryGetValue(raw.Trim(), out Channel? channel)) {
                    arg = new ChatCMDArg(raw, channel);
                } else {
                    throw new Exception($"Channel {raw} not found.");
                }
            } else {
                if (int.TryParse(raw, out int _))
                    throw new Exception("Invalid channel name.");
                arg = new ChatCMDArg(raw, null);
            }

            Validate?.Invoke(raw, env, arg);
            return true;
        }
    }
}