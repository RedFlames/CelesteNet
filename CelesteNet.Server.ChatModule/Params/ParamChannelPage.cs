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
    public class ParamChannelPage : Param {

        public override string Help => "Page number of channel list";
        protected override string PlaceholderName => "page";
        public override string ExampleValue => "1";

        public ParamChannelPage(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None) : base(chat, validate, flags) {
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {

            CelesteNetPlayerSession? self = env.Session;
            Channels channels = env.Server.Channels;

            bool parseSuccess = int.TryParse(raw, out int page);
            if (parseSuccess || string.IsNullOrWhiteSpace(raw)) {

                if (channels.All.Count == 0) {
                    // NB: No point in getting ChatCMDChannel.ID since that's not static - you could
                    // call Chat.Commands.Get("channel").ID or Chat.Commands.Get<ChatCMDChannel>().ID
                    // but that's just a longer way of spelling it out :)
                    throw new Exception($"No channels. See {Chat.Settings.CommandPrefix}channel on how to create one."); 
                }

                if(parseSuccess)
                    page--;

                int pages = (int) Math.Ceiling(channels.All.Count / (float) ChatCMDChannel.pageSize);
                if (page < 0 || pages <= page)
                    throw new Exception("Page out of range.");

                arg = new ChatCMDArg(page, channels.All.ToSnapshot());
                Validate?.Invoke(raw, env, arg);
                return true;
            }

            throw new Exception("Invalid page number.");
        }
    }
}