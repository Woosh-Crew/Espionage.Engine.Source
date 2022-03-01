using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Espionage.Engine.Source
{
    /// <summary>Deserialized BSP Data</summary>
    [Title( "BSP File" ), Group( "Files" ), File( Extension = "bsp" )]
    public partial class BSP : IFile
    {
        public const float Scale = 0.01905f;

        public Library ClassInfo { get; }

        public BSP() { ClassInfo = Library.Register( this ); }
        ~BSP() { Library.Unregister( this ); }

        //
        // File
        //

        public FileInfo File { get; set; }
        public BSPReader Reader { get; private set; }

        public void Load( FileStream fileStream )
        {
            // Open Streams for reading the header
            using var binaryReader = new BinaryReader( fileStream );
            Reader = new BSPReader( binaryReader );

            Entities = Reader.Read( 0, ReadEntities );

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
            Models = Reader.Read<Model>( 14, 48 );

            DisplacementInfo = Reader.Read<DispInfo>( 26, 176 );
            DisplacementVerts = Reader.Read<DispVert>( 33, 20 );

            Cubemaps = Reader.Read<Cubemap>( 42, 16 );

            TexdataStringTable = Reader.Read( 44, 4, e => e.ReadInt32() );

            Reader.Dispose();
        }

        //
        // Lumps
        //

        public Plane[] Planes; // LUMP 1
        public TexData[] TextureData; // LUMP 2
        public Vector[] Vertices; // LUMP 3
        public Vis[] Visibility; // LUMP 4
        public Node[] Nodes; // LUMP 5
        public TexInfo[] TextureInfo; // LUMP 6
        public Face[] Faces; // LUMP 7
        public Lighting[] LightingInfo; // LUMP 8
        public Leaf[] Leafs; // LUMP 10
        public Edge[] Edges; // LUMP 12
        public int[] SurfEdges; // LUMP 13
        public Model[] Models; // LUMP 14
        public DispInfo[] DisplacementInfo; // LUMP 26

        public DispVert[] DisplacementVerts; // LUMP 33

        public Cubemap[] Cubemaps; // LUMP 42

        public string[] TexdataStringData; // LUMP 43
        public int[] TexdataStringTable; // LUMP 44

        //
        // Entities
        //

        public Entity[] Entities; // LUMP 0

        private Entity[] ReadEntities( BinaryReader reader, Header.Lump lump )
        {
            var raw = reader.ReadBytes( lump.Length );
            var table = Encoding.UTF8.GetString( raw ).ToCharArray();

            var insideScope = false;
            var entities = new List<Entity>();
            var builder = new StringBuilder();

            for ( var i = 0; i < table.Length; i++ )
            {
                var character = table[i];

                // Start of Entity Scope
                if ( character == '{' && !insideScope )
                {
                    // Skip the \n
                    i++;
                    insideScope = true;

                    continue;
                }

                // End of Entity Scope
                if ( character == '}' && insideScope )
                {
                    insideScope = false;

                    // Jake: For some dumb ass reason I have to trim the end
                    // because theres a \n ? makes no sense...
                    entities.Add( new Entity( builder.ToString().TrimEnd( '\n' ) ) );
                    builder = new StringBuilder();

                    i++;

                    continue;
                }

                builder.Append( table[i] );
            }

            return entities.ToArray();
        }

        public interface IPointEntity
        {
            void OnRead( Entity ent ) { }
        }

        public interface IBrushEntity
        {
            void OnRead( Entity ent, GameObject model ) { }
        }
    }
}