using System.IO;
using System.Text;

namespace Espionage.Engine.Source
{
    public partial class VTF
    {
        public readonly struct Header
        {
            public string Format => Encoding.UTF8.GetString( Indent );

            public Header( BinaryReader reader )
            {
                Indent = reader.ReadBytes( 4 );

                Version = new uint[2];
                Version[0] = reader.ReadUInt32();
                Version[1] = reader.ReadUInt32();

                Size = reader.ReadUInt32();

                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();

                Flags = (Flags) reader.ReadUInt32();

                Frames = reader.ReadUInt16();
                FirstFrame = reader.ReadUInt16();
            }

            public bool IsVersion( int major, int minor ) => Version[0] == major && Version[1] == minor;

            public readonly byte[] Indent;
            public readonly uint[] Version;
            public readonly uint Size;

            public readonly ushort Width;
            public readonly ushort Height;

            public readonly Flags Flags;

            public readonly ushort Frames;
            public readonly ushort FirstFrame;
        }
    }
}