using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Edge : ILump
        {
            public ushort[] VertexIndices;

            public void Read( BinaryReader reader )
            {
                VertexIndices = new ushort[2];
                VertexIndices[0] = reader.ReadUInt16();
                VertexIndices[1] = reader.ReadUInt16();
            }
        }
    }
}