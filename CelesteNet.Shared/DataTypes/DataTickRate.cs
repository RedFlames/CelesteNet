namespace Celeste.Mod.CelesteNet.DataTypes {
    public class DataTickRate : DataType<DataTickRate> {

        public static new readonly string DataID = "tickRate";

        public float TickRate;

        public override DataFlags DataFlags => DataFlags.Small;

        protected override void Read(CelesteNetBinaryReader reader) {
            TickRate = reader.ReadSingle();
        }

        protected override void Write(CelesteNetBinaryWriter writer) {
            writer.Write(TickRate);
        }

    }
}