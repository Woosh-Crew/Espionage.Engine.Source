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
                Version = reader.Read( );
                Lumps = null;
                Revision = reader.Read( );
            }

            public readonly byte[] Indent;
            public readonly int[] Lumps;
            public readonly int Version;
            public readonly int Revision;
        }

        // 
        // Lump
        //
    }
}