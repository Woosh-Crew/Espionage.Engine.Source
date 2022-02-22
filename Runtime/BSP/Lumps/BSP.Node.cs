using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Node : ILump
        {
            public int PlaneNum;
            public int[] Children;

            public short[] Mins;
            public short[] Maxs;

            public ushort FirstFace;
            public ushort NumFaces;

            public short Area;
            public short Padding;

            public void Read( BinaryReader reader )
            {
                PlaneNum = reader.ReadInt32();

                Children = new int[2];
                Children[0] = reader.ReadInt32();
                Children[1] = reader.ReadInt32();

                Mins = new short[3];
                for ( var i = 0; i < 3; i++ )
                    Mins[i] = reader.ReadInt16();

                Maxs = new short[3];
                for ( var i = 0; i < 3; i++ )
                    Maxs[i] = reader.ReadInt16();

                FirstFace = reader.ReadUInt16();
                NumFaces = reader.ReadUInt16();

                Area = reader.ReadInt16();
                Padding = reader.ReadInt16();
            }
        }
    }
}