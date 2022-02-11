using System.IO;
using System.Text;
using Espionage.Engine.Resources;

namespace Espionage.Engine.Source
{
    public class BSP
    {
        public Header Head { get; }
        public FileInfo File { get; }

        public BSP( FileInfo info )
        {
            File = info;

            // Get Header
            Head = GrabHeader( );
        }

        //
        // Header
        //

        public struct Header
        {
            public byte[] Indent;
            public string Formant => Encoding.UTF8.GetString( Indent );

            public int Version;
            public int Revision;
        }

        private Header GrabHeader( )
        {
            // Just some testing shit
            using var fileStream = File.Open( FileMode.Open );
            using var reader = new BinaryReader( fileStream );

            var indent = reader.ReadBytes( 4 );
            var version = reader.Read( );

            // Put us after the lumps
            reader.BaseStream.Position = 16 * 64;

            var revision = reader.Read( );

            return new Header( )
            {
                Indent = indent,
                Version = version
            };
        }
    }
}