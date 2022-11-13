namespace Celeste.Mod.CelesteNet.DataTypes {
    public class DataReady : DataType<DataReady> {

        public static new readonly string DataID = "ready";

        public override DataFlags DataFlags => DataFlags.Small;

    }
}