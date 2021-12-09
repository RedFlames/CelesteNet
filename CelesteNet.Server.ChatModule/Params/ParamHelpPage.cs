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
    public class ParamHelpPage : Param {

        public override string Help => "Page number of command list";
        protected override string PlaceholderName => "page";
        public override string ExampleValue => "1";

        public ParamHelpPage(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None) : base(chat, validate, flags) {
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {

            CelesteNetPlayerSession? self = env.Session;
            Channels channels = env.Server.Channels;

            bool parseSuccess = int.TryParse(raw, out int page);
            if (parseSuccess || string.IsNullOrWhiteSpace(raw)) {

                if(parseSuccess)
                    page--;

                Logger.Log(LogLevel.DEV, "paramhelppage", $"{GetType()}.TryParse is int: {page}");

                int pages = (int) Math.Ceiling(Chat.Commands.All.Count / (float) ChatCMDHelp.pageSize);
                if (page < 0 || pages <= page)
                    throw new Exception("Page out of range.");

                arg = new ChatCMDArg(page);
                Validate?.Invoke(raw, env, arg);
                return true;
            }

            throw new Exception("Invalid page number.");
        }
    }
}