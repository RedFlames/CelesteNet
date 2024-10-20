using Celeste.Mod.CelesteNet.DataTypes;

namespace Celeste.Mod.CelesteNet.Server {
    public class DummyTCPUDPConnection : CelesteNetConnection {
        public readonly CelesteNetServer Server;

        public int TCPPingMs => 0;
        public int? UDPPingMs => null;

        public override bool IsConnected => true;

        public override bool IsAlive => true;

        public override string ID { get; }
        public override string UID { get; }

        public DummyTCPUDPConnection(CelesteNetServer server, string id, string uid) : base(server.Data) {
            Server = server;
            ID = id;
            UID = uid;
        }

        public override void DisposeSafe() {
            if (!IsAlive)
                return;
            Server.SafeDisposeQueue.Add(this);
        }
        protected override CelesteNetSendQueue? GetQueue(DataType data) {
            return null;
        }

        protected override void SendNoQueue(DataType data) {
            Logger.Log(LogLevel.INF, "dummy", $"Dummy: SendNoQueue {data.GetType()} {data}");
        }
    }
}