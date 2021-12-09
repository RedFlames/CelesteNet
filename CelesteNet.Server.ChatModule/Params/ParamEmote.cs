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
    public class ParamEmote : Param {

        public override string Help => "";
        protected override string PlaceholderName => "emote";

        public ParamEmote(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None
                          ) : base(chat, validate, flags) {
        }
    }
}