using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Espionage.Engine.Source
{
    /// <summary>Deserialized VTF Data</summary>
    public class VTF
    {
        public Header Head { get; }
        public FileInfo File { get; }

        public VTF( FileInfo file )
        {
            File = file;

            // Open Streams for reading the header
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var reader = new BinaryReader( fileStream );

            Head = new Header( reader );
        }

        //
        // Structs
        //

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
            }

            public readonly byte[] Indent;
            public readonly uint[] Version;
            public readonly uint Size;

            public readonly ushort Width;
            public readonly ushort Height;

            public readonly Flags Flags;
        }

        public readonly struct Entry
        {
            public Entry( BinaryReader reader ) { }
        }

        //
        // Enums
        //

        public enum Format
        {
            None = -1,
            RGBA8888 = 0,
            ABGR8888,
            RGB888,
            BGR888,
            RGB565,
            I8,
            IA88,
            P8,
            A8,
            RGB888_BLUESCREEN,
            BGR888_BLUESCREEN,
            ARGB8888,
            BGRA8888,
            DXT1,
            DXT3,
            DXT5,
            BGRX8888,
            BGR565,
            BGRX5551,
            BGRA4444,
            DXT1_ONEBITALPHA,
            BGRA5551,
            UV88,
            UVWQ8888,
            RGBA16161616F,
            RGBA16161616,
            UVLX8888
        }

        [Flags]
        public enum Flags : uint
        {
            POINTSAMPLE = 0x00000001,
            TRILINEAR = 0x00000002,
            CLAMPS = 0x00000004,
            CLAMPT = 0x00000008,
            ANISOTROPIC = 0x00000010,
            HINT_DXT5 = 0x00000020,
            PWL_CORRECTED = 0x00000040,
            NORMAL = 0x00000080,
            NOMIP = 0x00000100,
            NOLOD = 0x00000200,
            ALL_MIPS = 0x00000400,
            PROCEDURAL = 0x00000800,
            ONEBITALPHA = 0x00001000,
            EIGHTBITALPHA = 0x00002000,
            ENVMAP = 0x00004000,
            RENDERTARGET = 0x00008000,
            DEPTHRENDERTARGET = 0x00010000,
            NODEBUGOVERRIDE = 0x00020000,
            SINGLECOPY = 0x00040000,
            PRE_SRGB = 0x00080000,

            UNUSED_00100000 = 0x00100000,
            UNUSED_00200000 = 0x00200000,
            UNUSED_00400000 = 0x00400000,

            NODEPTHBUFFER = 0x00800000,

            UNUSED_01000000 = 0x01000000,

            CLAMPU = 0x02000000,
            VERTEXTEXTURE = 0x04000000,
            SSBUMP = 0x08000000,

            UNUSED_10000000 = 0x10000000,

            BORDER = 0x20000000,

            UNUSED_40000000 = 0x40000000,
            UNUSED_80000000 = 0x80000000
        }
    }
}