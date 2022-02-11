using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Espionage.Engine.Source
{
    public class BSP
    {
        public Header Head { get; }
        public FileInfo File { get; }

        public BSP( FileInfo info )
        {
            File = info;

            // Open Streams for reading the header
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var reader = new BinaryReader( fileStream );

            Head = new Header( reader );

            // Temp Shit
            var entityLump = Head.Lumps[0];

            reader.BaseStream.Seek( entityLump.Offset, SeekOrigin.Begin );

            var textDump = reader.ReadBytes( entityLump.Length );
            var dump = Encoding.UTF8.GetString( textDump );

            Debugging.Log.Info( dump );
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
                Version = reader.ReadInt32( );

                // Read Lumps
                Lumps = new Lump[64];
                for ( var i = 0; i < 64; i++ )
                {
                    var lump = Lumps[i];

                    lump.Offset = reader.ReadInt32( );
                    lump.Length = reader.ReadInt32( );
                    lump.Version = reader.ReadInt32( );
                    lump.Indent = reader.ReadBytes( 4 );

                    Lumps[i] = lump;
                }

                Revision = reader.ReadInt32( );
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
        // Lumps
        //

        public List<Vector3> Vertices = new( );
    }
}