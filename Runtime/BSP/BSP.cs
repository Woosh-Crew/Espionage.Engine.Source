using System.IO;
using System.Text;

namespace Espionage.Engine.Source
{
    public class BSP
    {
        public Header Head { get; }
        public FileInfo File { get; }

        public BSP( FileInfo info )
        {
            File = info;

            // Open Streams
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var reader = new BinaryReader( fileStream );

            Head = new Header( reader );
        }

        //
        // Header
        //

        public readonly struct Header
        {
            public string Format => Encoding.UTF8.GetString( Indent );

            public Header( BinaryReader reader )
            {
                Indent = reader.ReadBytes( 4 );
                Version = reader.ReadInt16( );

                // Read Lumps
                Lumps = new Lump[64];
                for ( var i = 0; i < 64; i++ )
                {
                    var lump = Lumps[i];

                    lump.Offset = reader.ReadInt16( );
                    lump.Length = reader.ReadInt16( );
                    lump.Version = reader.ReadInt16( );
                    lump.Indent = reader.ReadBytes( 4 );

                    Lumps[i] = lump;
                }

                Revision = reader.ReadInt16( );
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

        // 
        // Lump
        //
    }
}