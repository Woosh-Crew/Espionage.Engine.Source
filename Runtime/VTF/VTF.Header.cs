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

                Version = new uint[2]
                {
                    reader.ReadUInt32(),
                    reader.ReadUInt32()
                };

                Size = reader.ReadUInt32();

                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();

                Flags = (Flags) reader.ReadUInt32();

                Frames = reader.ReadUInt16();
                FirstFrame = reader.ReadUInt16();

                Padding0 = new[]
                {
                    reader.ReadSByte(),
                    reader.ReadSByte(),
                    reader.ReadSByte(),
                    reader.ReadSByte()
                };

                Reflectivity = new[]
                {
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                };

                Padding1 = new[]
                {
                    reader.ReadSByte(),
                    reader.ReadSByte(),
                    reader.ReadSByte(),
                    reader.ReadSByte()
                };

                BumpmapScale = reader.ReadSingle();
                HighResImageFormat = reader.ReadUInt32();
                MipmapCount = reader.ReadSByte();
                LowResImageFormat = reader.ReadUInt32();
                LowResImageWidth = reader.ReadSByte();
                LowResImageHeight = reader.ReadSByte();

                // Version Specific
                Depth = default;
                Padding2 = default;
                NumResources = default;
                Padding3 = default;

                if ( IsVersion( 7, 2 ) )
                    Depth = reader.ReadUInt16();

                if ( IsVersion( 7, 3 ) )
                {
                    Padding2 = new sbyte[3]
                    {
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte()
                    };

                    NumResources = reader.ReadUInt32();

                    Padding2 = new sbyte[8]
                    {
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte()
                    };
                }
            }

            public bool IsVersion( int major, int minor ) => Version[0] >= major && Version[1] >= minor;

            public readonly byte[] Indent;
            public readonly uint[] Version;
            public readonly uint Size;

            public readonly ushort Width;
            public readonly ushort Height;

            public readonly Flags Flags;

            public readonly ushort Frames;
            public readonly ushort FirstFrame;

            public readonly sbyte[] Padding0;
            public readonly float[] Reflectivity;
            public readonly sbyte[] Padding1;

            public readonly float BumpmapScale;
            public readonly uint HighResImageFormat;
            public readonly sbyte MipmapCount;
            public readonly uint LowResImageFormat;
            public readonly sbyte LowResImageWidth;
            public readonly sbyte LowResImageHeight;

            // 7.2+ Readonly 
            public readonly ushort Depth;

            // 7.3+ Readonly 
            public readonly sbyte[] Padding2;
            public readonly uint NumResources;

            public readonly sbyte[] Padding3;

            public readonly struct Entry
            {
                public Entry( BinaryReader reader )
                {
                    Tag = new sbyte[3]
                    {
                        reader.ReadSByte(),
                        reader.ReadSByte(),
                        reader.ReadSByte()
                    };

                    Flags = reader.ReadSByte();
                    Offset = reader.ReadUInt32();
                }

                public readonly sbyte[] Tag;
                public readonly sbyte Flags;
                public readonly uint Offset;
            }
        }
    }
}