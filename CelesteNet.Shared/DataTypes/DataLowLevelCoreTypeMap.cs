using System;

namespace Celeste.Mod.CelesteNet.DataTypes {
    public class DataLowLevelCoreTypeMap : DataType<DataLowLevelCoreTypeMap> {

        // TODO Change this to "coreTypeMap" in the next protocol-breaking version
        public static new readonly string DataID = "slimMap";

        public override DataFlags DataFlags => DataFlags.Small;

        public Type? PacketType = null;
        public int ID;

        protected override void Read(CelesteNetBinaryReader reader) {
            PacketType = Type.GetType(reader.ReadNetString(), false);
            ID = reader.Read7BitEncodedInt();
        }

        protected override void Write(CelesteNetBinaryWriter writer) {
            writer.WriteNetString(PacketType?.FullName);
            writer.Write7BitEncodedInt(ID);
        }

    }
}