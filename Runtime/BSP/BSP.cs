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

            // Read Vertices
            reader.BaseStream.Seek( Head[3].Offset, SeekOrigin.Begin );

            Vertices = new Vector3[Head[3].Length / 12];
            for ( var i = 0; i < Head[3].Length / 12; i++ )
                Vertices[i] = reader.ReadVec3( );
        }

        //
        // Header
        //

        public readonly struct Header
        {
            public Lump this[ int key ] => Lumps[key];
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

        // Lumps

        public readonly Vector3[] Vertices; // LUMP 3

        //
        // Face
        //

        public readonly struct Face
        {
            public readonly ushort Plane;
            public readonly byte Side;
            public readonly byte OnNode;

            // Edges
            public readonly int FirstEdge;
            public readonly short NumEdges;
        }
    }
}