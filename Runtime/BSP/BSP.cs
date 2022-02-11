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

            Planes = Read( reader, Head.Lumps[1], 20, e => new Plane( e ) );
            TexDatas = Read( reader, Head.Lumps[2], 32, e => new TexData( e ) );
            Vertices = Read( reader, Head.Lumps[3], 12, e => e.ReadVec3( ) );
            Cubemaps = Read( reader, Head.Lumps[42], 16, e => new Cubemap( e ) );
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

        // Lumps

        private static T[] Read<T>( BinaryReader reader, Header.Lump lump, int size, Func<BinaryReader, T> item )
        {
            reader.BaseStream.Seek( lump.Offset, SeekOrigin.Begin );

            var final = new T[lump.Length / size];
            for ( var i = 0; i < lump.Length / size; i++ )
                final[i] = item.Invoke( reader );

            return final;
        }

        // Entities // LUMP 0
        public readonly Plane[] Planes; // LUMP 1
        public readonly TexData[] TexDatas; // LUMP 2
        public readonly Vector3[] Vertices; // LUMP 3
        public readonly Cubemap[] Cubemaps; // LUMP 42

        //
        // Structs
        //

        public readonly struct Plane
        {
            public Plane( BinaryReader reader )
            {
                Normal = reader.ReadVec3( );
                Distance = reader.ReadSingle( );
                Type = reader.ReadInt32( );
            }

            public readonly Vector3 Normal;
            public readonly float Distance; // From Origin
            public readonly int Type; // Axis Identifier
        }

        public readonly struct TexData
        {
            public TexData( BinaryReader reader )
            {
                Reflectivity = reader.ReadVec3( );
                NameID = reader.ReadInt32( );

                Width = reader.ReadInt32( );
                Height = reader.ReadInt32( );

                ViewWidth = reader.ReadInt32( );
                ViewHeight = reader.ReadInt32( );
            }

            public readonly Vector3 Reflectivity;
            public readonly int NameID;
            public readonly int Width, Height;
            public readonly int ViewWidth, ViewHeight; // Tf are these for?
        }

        public readonly struct Cubemap
        {
            public Cubemap( BinaryReader reader )
            {
                Origin = reader.ReadVec3( );
                Size = reader.ReadInt32( );
            }

            public readonly Vector3 Origin;
            public readonly int Size;
        }
    }
}