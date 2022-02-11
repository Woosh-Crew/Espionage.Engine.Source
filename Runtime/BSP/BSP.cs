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
            using var fileStream = File.Open( FileMode.Open );
            using var reader = new BinaryReader( fileStream );

            Head = new Header( reader );
        }

        //
        // Header
        //

        public readonly struct Header
        {
            public Header( BinaryReader reader )
            {
                Indent = reader.ReadBytes( 4 );
                Version = reader.Read( );

                // Put us after the lumps
                reader.BaseStream.Position = 16 * 64;
                Revision = reader.Read( );
            }

            public readonly byte[] Indent;
            public string Formant => Encoding.UTF8.GetString( Indent );

            public readonly int Version;
            public readonly int Revision;
        }
    }
}