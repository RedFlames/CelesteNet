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
    public class ParamPlayerSession : Param {
        public override string Help => $"An {(IngameOnly ? "in-game" : "online")} player";
        protected override string PlaceholderName => "player";
        public override string ExampleValue => "Madeline";
        public bool IngameOnly = false;

        public ParamPlayerSession(ChatModule chat, Action<string, ChatCMDEnv, ChatCMDArg>? validate = null, ParamFlags flags = ParamFlags.None,
                                    bool ingameOnly = false) : base(chat, validate, flags) {
            IngameOnly = ingameOnly;
        }

        public override bool TryParse(string raw, ChatCMDEnv env, out ChatCMDArg? arg) {
            arg = null;
            CelesteNetPlayerSession? session = null;

            if (uint.TryParse(raw, out uint PID)) {
                Chat.Server.PlayersByID.TryGetValue(PID, out session);
            } else { 
                using (Chat.Server.ConLock.R()) {
                    session = Chat.Server.Sessions.FirstOrDefault(session => string.Equals(session.PlayerInfo?.FullName, raw, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            if (session != null) {
                arg = new ChatCMDArg(session);

                DataPlayerInfo? sessionPlayer = session.PlayerInfo;
                if (!Chat.Server.Data.TryGetBoundRef(session.PlayerInfo, out DataPlayerState? otherState) ||
                otherState == null || otherState.SID.IsNullOrEmpty()) {
                    if (IngameOnly)
                        throw new Exception("Player is not in-game.");
                }

                Validate?.Invoke(raw, env, arg);
                return true;
            }

            throw new Exception($"Player '{raw}' not found.");
        }

    }
}