using System;
using System.Collections.Generic;
using Celeste.Mod.CelesteNet.DataTypes;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.CelesteNet.Server.Chat.Cmd {
    public class CmdTP : ChatCmd {

        public override CompletionType Completion => CompletionType.Player;

        public override string Info => "Teleport to another player.";

        private CelesteNetPlayerSession? Other;
        private DataPlayerInfo? OtherPlayer;
        private DataPlayerState? OtherState;

        public override void Init(ChatModule chat) {
            Chat = chat;

            ArgParser parser = new(chat, this);
            parser.AddParameter(new ParamPlayerSession(chat, ValidatePlayerSession));
            ArgParsers.Add(parser);
        }

        private void ValidatePlayerSession(string raw, CmdEnv env, ICmdArg arg) {
            CelesteNetPlayerSession? self = env.Session;
            if (self == null || env.Player == null)
                throw new CommandRunException("Are you trying to TP as the server?");

            if (arg is not CmdArgPlayerSession sessionArg)
                throw new CommandRunException("Invalid username or ID.");

            CelesteNetPlayerSession? other = sessionArg.Session;
            DataPlayerInfo otherPlayer = other?.PlayerInfo ?? throw new CommandRunException("Invalid username or ID.");

            if (!env.Server.UserData.Load<TPSettings>(other.UID).Enabled)
                throw new CommandRunException($"{otherPlayer.FullName} has blocked teleports.");

            if (self.Channel != other.Channel)
                throw new CommandRunException($"{otherPlayer.FullName} is in a different channel.");

            if (!env.Server.Data.TryGetBoundRef(otherPlayer, out DataPlayerState? otherState) ||
                otherState == null ||
                otherState.SID.IsNullOrEmpty())
                throw new CommandRunException($"{otherPlayer.FullName} isn't in-game.");

            Other = other;
            OtherPlayer = otherPlayer;
            OtherState = otherState;
        }

        public override void Run(CmdEnv env, List<ICmdArg>? args) {
            CelesteNetPlayerSession? self = env.Session;

            if (self == null || Other == null || OtherPlayer == null || OtherState == null)
                throw new InvalidOperationException("This shouldn't happen, if ArgTypePlayerSession parsed successfully...");

            DataChat? msg = env.Send($"Teleporting to {OtherPlayer.FullName}");

            Logger.Log(LogLevel.VVV, "teleport", $"Run with '{args?.Count}' arguments: {args}");
            Logger.Log(LogLevel.VVV, "teleport", $"self / other vars: {self} / {Other} / {OtherPlayer} / {OtherState}");

            self.Request<DataSession>(400,
                (con, session) => {
                    Logger.Log(LogLevel.VVV, "teleport", $"self DataSession request success: {session}. Waiting for frame...");
                    self.WaitFor<DataPlayerFrame>(400,
                        (con, frame) => {
                            Logger.Log(LogLevel.VVV, "teleport", $"self WaitFor frame success: {frame}.");
                            return SaveAndTeleport(env, msg, self, Other, OtherPlayer, OtherState, session, frame.Position);
                        },
                        () => {
                            Logger.Log(LogLevel.VVV, "teleport", $"self WaitFor frame unsuccessful...");
                            SaveAndTeleport(env, msg, self, Other, OtherPlayer, OtherState, session, null);
                        }
                    );
                },
                () => {
                    Logger.Log(LogLevel.VVV, "teleport", $"self DataSession request unsuccessful...");
                    SaveAndTeleport(env, msg, self, Other, OtherPlayer, OtherState, null, null);
                }
            );
        }

        private bool SaveAndTeleport(CmdEnv env, DataChat? msg, CelesteNetPlayerSession self, CelesteNetPlayerSession other, DataPlayerInfo otherPlayer, DataPlayerState otherState, DataSession? savedSession, Vector2? savedPos) {
            new DynamicData(self).Set("tpHistory", new TPHistoryEntry {
                State = env.State,
                Session = savedSession,
                Position = savedPos
            });

            Logger.Log(LogLevel.VVV, "teleport", $"Updated tpHistory: {env.State} / {savedSession} / {savedPos}. Requesting other session...");

            other.Request<DataSession>(400,
                (con, session) => {
                    Logger.Log(LogLevel.VVV, "teleport", $"other DataSession request success: {session}. Waiting for frame...");
                    other.WaitFor<DataPlayerFrame>(300,
                        (con, frame) => {
                            Logger.Log(LogLevel.VVV, "teleport", $"other WaitFor frame success: {frame}.");
                            return Teleport(env, msg, self, other, otherPlayer, otherState, session, frame.Position);
                        },
                        () => {
                            Logger.Log(LogLevel.VVV, "teleport", $"other WaitFor frame unsuccessful...");
                            Teleport(env, msg, self, other, otherPlayer, otherState, session, null);
                        }
                    );
                },
                () => {
                    Logger.Log(LogLevel.VVV, "teleport", $"other DataSession request unsuccessful...");
                    Teleport(env, msg, self, other, otherPlayer, otherState, null, null);
                }
            );
            return true;
        }

        private bool Teleport(CmdEnv env, DataChat? msg, CelesteNetPlayerSession self, CelesteNetPlayerSession other, DataPlayerInfo otherPlayer, DataPlayerState otherState, DataSession? tpSession, Vector2? tpPos) {
            if (msg != null) {
                self.WaitFor<DataPlayerState>(6000, (con, state) => {
                    Logger.Log(LogLevel.VVV, "teleport", $"self WaitFor State success: {state}.");
                    if (state.SID != otherState.SID ||
                        state.Mode != otherState.Mode ||
                        state.Level != otherState.Level) {
                        msg.Text = $"Teleport to {otherPlayer.FullName} failed(?)";
                        Chat.ForceSend(msg);
                        return false;
                    }

                    msg.Text = $"Teleported to {otherPlayer.FullName}";
                    Chat.ForceSend(msg);
                    return true;

                }, () => {
                    Logger.Log(LogLevel.VVV, "teleport", $"self WaitFor State unsuccessful.");
                    msg.Text = $"Couldn't teleport to {otherPlayer.FullName} - maybe missing map?";
                    Chat.ForceSend(msg);

                    other.Request<DataMapModInfo>(1000, (con, info) => {
                        Logger.Log(LogLevel.VVV, "teleport", $"other MapModInfo request success: {info}.");
                        if (!string.IsNullOrEmpty(info.ModID))
                            self.Con.Send(new DataModRec {
                                ModID = info.ModID,
                                ModName = info.ModName,
                                ModVersion = info.ModVersion
                            });
                    });
                });
            }



            var moveTo = new DataMoveTo {
                SID = otherState.SID,
                Mode = otherState.Mode,
                Level = otherState.Level,
                Session = tpSession,
                Position = tpPos
            };

            Logger.Log(LogLevel.VVV, "teleport", $"self sending MoveTo: {moveTo}.");

            self.Con.Send(moveTo);

            return true;
        }

    }

    public class CmdTPOn : ChatCmd {

        public override string Info => "Allow others to teleport to you.";

        public override void Run(CmdEnv env, List<ICmdArg>? args) {
            if (env.Session == null)
                return;

            if (env.Server.UserData.GetKey(env.Session.UID).IsNullOrEmpty())
                throw new CommandRunException("You must be registered to enable / disable teleports!");

            env.Server.UserData.Save(env.Session.UID, new TPSettings {
                Enabled = true
            });
            env.Send("Others can teleport to you now.");
        }

    }

    public class CmdTPOff : ChatCmd {

        public override string Info => "Prevent others from teleporting to you.";

        public override void Run(CmdEnv env, List<ICmdArg>? args) {
            if (env.Session == null)
                return;

            if (env.Server.UserData.GetKey(env.Session.UID).IsNullOrEmpty())
                throw new CommandRunException("You must be registered to enable / disable teleports!");

            env.Server.UserData.Save(env.Session.UID, new TPSettings {
                Enabled = false
            });
            env.Send("Others can't teleport to you anymore.");
        }

    }

    public class TPSettings {
        public bool Enabled { get; set; } = true;
    }

    public class TPHistoryEntry {
        public DataPlayerState? State;
        public DataSession? Session;
        public Vector2? Position;
    }
}
