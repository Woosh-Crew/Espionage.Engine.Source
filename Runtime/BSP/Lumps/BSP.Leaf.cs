using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Leaf : ILump
        {
            public int Contents; // OR of all brushes (not needed?)
            public short Cluster; // cluster this leaf is in
            public short Area; // area this leaf is in
            public short Flags; // flags
            public short[] Mins; // for frustum culling
            public short[] Maxs;
            public ushort FirstLeafFace; // index into leaffaces
            public ushort NumLeafFaces;
            public ushort FirstLeafBrush; // index into leafbrushes
            public ushort NumLeafBrushes;
            public short LeafWaterDataID; // -1 for not in water

            public void Read( BinaryReader reader )
            {
                Contents = reader.ReadInt32();
                Cluster = reader.ReadInt16();
                Area = reader.ReadInt16();
                Flags = reader.ReadInt16();

                Mins = new short[3]
                {
                    reader.ReadInt16(),
                    reader.ReadInt16(),
                    reader.ReadInt16()
                };

                Maxs = new short[3]
                {
                    reader.ReadInt16(),
                    reader.ReadInt16(),
                    reader.ReadInt16()
                };

                FirstLeafFace = reader.ReadUInt16();
                NumLeafFaces = reader.ReadUInt16();
                FirstLeafBrush = reader.ReadUInt16();
                NumLeafBrushes = reader.ReadUInt16();
                LeafWaterDataID = reader.ReadInt16();
            }
        }
    }
}