using System.IO;
using System.Text;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public readonly struct Header
        {
            public string Format => Encoding.UTF8.GetString( Indent );

            internal Header( BinaryReader reader )
            {
                Indent = reader.ReadBytes( 4 );
                Version = reader.ReadInt32();

                // Read Lumps
                Lumps = new Lump[64];
                for ( var i = 0; i < 64; i++ )
                {
                    var lump = Lumps[i];

                    lump.Offset = reader.ReadInt32();
                    lump.Length = reader.ReadInt32();
                    lump.Version = reader.ReadInt32();
                    lump.Indent = reader.ReadBytes( 4 );

                    Lumps[i] = lump;
                }

                Revision = reader.ReadInt32();
            }

            public readonly byte[] Indent;
            public readonly Lump[] Lumps;
            public readonly int Version;
            public readonly int Revision;

            public struct Lump
            {
                public int Offset;
                public int Length;
                public int Version;
                public byte[] Indent;
            }
        }
    }
}