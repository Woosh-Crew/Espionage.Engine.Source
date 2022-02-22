using System;
using System.IO;
using System.Text;

namespace Espionage.Engine.Source
{
    /// <summary>Deserialized BSP Data</summary>
    public partial class BSP
    {
        public const float Scale = 0.01905f;

        public FileInfo File { get; }
        public BSPReader Reader { get; }

        public BSP( FileInfo info )
        {
            File = info;

            // Open Streams for reading the header
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var binaryReader = new BinaryReader( fileStream );

            Reader = new BSPReader( binaryReader );

            Entities = Reader.Read( 0, ( reader, lump ) =>
            {
                var raw = reader.ReadBytes( lump.Length );
                var table = Encoding.UTF8.GetString( raw );

                // Separate Entities by the { } token
                var ents = table.Split( '{', StringSplitOptions.RemoveEmptyEntries );

                var entities = new Entity[ents.Length];

                for ( var i = 0; i < entities.Length; i++ )
                {
                    ents[i] = ents[i].Trim( '{', '}', '\n' );

                    var entity = new Entity( ents[i] );
                    entities[i] = entity;
                }

                return entities;
            } );

            Planes = Reader.Read<Plane>( 1, 20 );
            TextureData = Reader.Read<TexData>( 2, 32 );
            Vertices = Reader.Read<Vector>( 3, 12 );
            // Visibility = Reader.Read<Vis>( 4, 12 ); -- This gives us a OverflowException? Not sure why
            Nodes = Reader.Read<Node>( 5, 32 );
            TextureInfo = Reader.Read<TexInfo>( 6, 72 );
            Faces = Reader.Read<Face>( 7, 56 );
            LightingInfo = Reader.Read<Lighting>( 8, 5 );

            Leafs = Reader.Read<Leaf>( 10, 56 );

            Edges = Reader.Read<Edge>( 12, 4 );
            SurfEdges = Reader.Read( 13, 4, e => e.ReadInt32() );

            DisplacementInfo = Reader.Read<DispInfo>( 26, 176 );
            DisplacementVerts = Reader.Read<DispVert>( 33, 20 );

            Cubemaps = Reader.Read<Cubemap>( 42, 16 );

            TexdataStringTable = Reader.Read( 44, 4, e => e.ReadInt32() );
        }

        ~BSP()
        {
            // I think we have to do this
            Reader.Dispose();
        }

        public readonly Entity[] Entities; // LUMP 0
        public readonly Plane[] Planes; // LUMP 1
        public readonly TexData[] TextureData; // LUMP 2
        public readonly Vector[] Vertices; // LUMP 3
        public readonly Vis[] Visibility; // LUMP 4
        public readonly Node[] Nodes; // LUMP 5
        public readonly TexInfo[] TextureInfo; // LUMP 6
        public readonly Face[] Faces; // LUMP 7
        public readonly Lighting[] LightingInfo; // LUMP 8
        public readonly Leaf[] Leafs; // LUMP 10
        public readonly Edge[] Edges; // LUMP 12
        public readonly int[] SurfEdges; // LUMP 13
        public readonly DispInfo[] DisplacementInfo; // LUMP 26

        public readonly DispVert[] DisplacementVerts; // LUMP 33

        public readonly Cubemap[] Cubemaps; // LUMP 42

        public readonly int[] TexdataStringTable; // LUMP 44
        public readonly string[] TexdataStringData; // LUMP 43
    }
}