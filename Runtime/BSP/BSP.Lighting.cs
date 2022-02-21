using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Lighting : ILump
        {
            public byte R, G, B;
            public sbyte Exponent;

            public void Read( BinaryReader reader )
            {
                R = reader.ReadByte();
                G = reader.ReadByte();
                B = reader.ReadByte();

                Exponent = reader.ReadSByte();
            }
        }
    }
}