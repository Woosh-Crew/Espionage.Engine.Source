using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Espionage.Engine.Resources;

namespace Espionage.Engine.Source
{
    [Library, Title( "BSP Map" ), Group( "Maps" ), File( Extension = "bsp", Serialization = "Binary" )]
    public class BSPMapProvider : IMapProvider
    {
    #if UNITY_EDITOR

        [UnityEditor.MenuItem( "Tools/Espionage.Engine/Source/Load BSP", priority = 0 )]
        private static void LoadBSP()
        {
            var path = UnityEditor.EditorUtility.OpenFilePanel( "Load .bsp File", "", "bsp" );

            if ( string.IsNullOrEmpty( path ) )
            {
                Debugging.Log.Info( "No Map Selected" );
                return;
            }

            var bsp = new BSP( new FileInfo( path ) );
            new Map( new BSPMapProvider( bsp ) ).Load();
        }

    #endif

        // Id
        public string Identifier => BSP.File.FullName;

        // Outcome
        public Scene? Scene { get; private set; }

        // Loading Meta
        public float Progress => 0;
        public bool IsLoading { get; private set; }

        public BSPMapProvider( BSP bsp ) { BSP = bsp; }

        //
        // Resource
        //

        public BSP BSP { get; }

        public void Load( Action finished )
        {
            BSP.Load();

            using var _ = Debugging.Stopwatch( $"Loading {BSP.File.Name}, Format {BSP.Reader.Header.Format}, Version {BSP.Reader.Header.Version}" );

            // Start
            IsLoading = true;

            // Create Scene
            Scene = SceneManager.CreateScene( Path.GetFileName( BSP.File.Name ) );
            SceneManager.SetActiveScene( Scene.Value );

            // Generate BSP
            Generate();

            // Finish
            IsLoading = false;
            finished?.Invoke();
        }

        public void Unload( Action finished )
        {
            IsLoading = true;

            Scene?.Unload();
            Debugging.Log.Info( "Finished Unloading BSP" );

            IsLoading = false;
            finished?.Invoke();
        }

        //
        // BSP Generator
        //

        public void Generate()
        {
            foreach ( var entity in BSP.Entities )
            {
                var className = entity.KeyValues["classname"];
                var spawnedEntity = Library.Database.Create( className );

                // If we're not a valid entity, continue.
                if ( spawnedEntity is null )
                    continue;

                // Fill in Data
                foreach ( var keyValues in entity.KeyValues )
                {
                    var property = spawnedEntity.ClassInfo.Properties[keyValues.Key];

                    if ( property != null )
                    {
                        // This is dumb
                        if ( property.Type == typeof( BSP.Vector ) )
                            property[spawnedEntity] = BSP.Vector.Parse( keyValues.Value );
                        else if ( property.Type == typeof( BSP.Angles ) )
                            property[spawnedEntity] = BSP.Angles.Parse( keyValues.Value );
                        else if ( property.Type == typeof( BSP.Color ) )
                            property[spawnedEntity] = BSP.Color.Parse( keyValues.Value );
                        else if ( property.Type == typeof( bool ) )
                            property[spawnedEntity] = int.Parse( keyValues.Value ) == 1;
                        else
                            property[spawnedEntity] = Convert.ChangeType( keyValues.Value, property.Type );
                    }
                }

                if ( className == "worldspawn" )
                {
                    var obj = MakeModel( BSP.Models[0], true );
                    ( spawnedEntity as BSP.IBrushEntity )?.OnRead( entity, obj );
                    continue;
                }

                // Create Model if it has one
                if ( entity.KeyValues.TryGetValue( "model", out var value ) && value.StartsWith( "*" ) )
                {
                    // Make the Mesh
                    var index = int.Parse( value[1..] );
                    var obj = MakeModel( BSP.Models[index] );

                    obj.name = "Mesh";

                    ( spawnedEntity as BSP.IBrushEntity )?.OnRead( entity, obj );

                    continue;
                }

                ( spawnedEntity as BSP.IPointEntity )?.OnRead( entity );
            }
        }

        public GameObject MakeModel( BSP.Model model, bool hasLightmaps = false )
        {
            var root = new GameObject( "Geometry" );

            // Build Mesh

            var combiner = new Dictionary<int, List<CombineInstance>>();

            for ( var i = 0; i < model.NumFaces; i++ )
            {
                var face = BSP.Faces[model.FirstFace + i];
                var id = BSP.TextureData[BSP.TextureInfo[face.TexInfo].TexData].NameID;

                // This is fucking stupid
                var flags = BSP.TextureInfo[face.TexInfo].Flags;
                if ( flags.HasFlag( BSP.TexInfo.Flag.Trigger ) ||
                     flags.HasFlag( BSP.TexInfo.Flag.Sky ) ||
                     flags.HasFlag( BSP.TexInfo.Flag.Hitbox ) ||
                     flags.HasFlag( BSP.TexInfo.Flag.Hint ) ||
                     flags.HasFlag( BSP.TexInfo.Flag.Sky2D )
                   )
                    continue;

                var mesh = face.DisplacementInfo != -1 ? MakeDisplacement( face ) : MakeFace( face );

                if ( mesh == null )
                    continue;

                if ( !combiner.ContainsKey( id ) )
                    combiner.Add( id, new List<CombineInstance>() );

                combiner[id].Add( new CombineInstance() { mesh = mesh, transform = Matrix4x4.identity } );
            }

            foreach ( var (key, value) in combiner )
            {
                if ( value == null )
                    continue;

                // Create Mesh

                var finalMesh = new Mesh()
                {
                    name = $"Map - {key}"
                };

                finalMesh.CombineMeshes( value.ToArray() );
                finalMesh.Optimize();

                finalMesh.RecalculateBounds();
                finalMesh.RecalculateNormals();

                // Create Object
                var go = new GameObject( $"Map [{key}]" );
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.shadowCastingMode = ShadowCastingMode.TwoSided;

                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = finalMesh;

                var collider = go.AddComponent<MeshCollider>();
                collider.sharedMesh = finalMesh;

                go.transform.parent = root.transform;
                go.isStatic = hasLightmaps;
            }

            return root;
        }

        public Mesh MakeFace( BSP.Face face )
        {
            var surfaceVertices = new List<Vector3>();
            var originalVertices = new List<Vector3>();
            var normals = new List<Vector3>();

            for ( var i = 0; i < face.NumEdges; i++ )
            {
                var currentEdge = BSP.Edges[Mathf.Abs( BSP.SurfEdges[face.FirstEdge + i] )].VertexIndices;

                Vector3 point1 = BSP.Vertices[currentEdge[0]];
                Vector3 point2 = BSP.Vertices[currentEdge[1]];

                var normal = BSP.Planes[face.PlaneNum].Normal;

                if ( BSP.SurfEdges[face.FirstEdge + i] >= 0 )
                {
                    originalVertices.Add( point1 );
                    originalVertices.Add( point2 );

                    if ( !surfaceVertices.Contains( point1 ) )
                    {
                        surfaceVertices.Add( point1 );
                        normals.Add( normal );
                    }

                    if ( !surfaceVertices.Contains( point2 ) )
                    {
                        surfaceVertices.Add( point2 );
                        normals.Add( normal );
                    }
                }
                else
                {
                    originalVertices.Add( point2 );
                    originalVertices.Add( point1 );

                    if ( surfaceVertices.IndexOf( point2 ) < 0 )
                    {
                        surfaceVertices.Add( point2 );
                        normals.Add( normal );
                    }


                    if ( surfaceVertices.IndexOf( point1 ) < 0 )
                    {
                        surfaceVertices.Add( point1 );
                        normals.Add( normal );
                    }
                }
            }

            //
            // Triangulate
            //

            var tris = new List<int>();

            for ( var i = 0; i < originalVertices.Count / 2; i++ )
            {
                var first = surfaceVertices.IndexOf( originalVertices[i * 2] );
                var second = surfaceVertices.IndexOf( originalVertices[i * 2 + 1] );
                var third = surfaceVertices.IndexOf( originalVertices[0] );

                tris.Add( first );
                tris.Add( second );
                tris.Add( third );
            }

            //
            // Finish
            // 

            var (texture, lightmaps) = GenerateUVs( face, surfaceVertices );

            var mesh = new Mesh
            {
                vertices = surfaceVertices.ToArray(),
                triangles = tris.ToArray(),
                normals = normals.ToArray(),
                uv = texture,
                uv2 = lightmaps
            };

            mesh.Optimize();
            mesh.RecalculateTangents();

            return mesh;
        }

        public Mesh MakeDisplacement( BSP.Face face ) => null;

        public (Vector2[] uv1, Vector2[] uv2) GenerateUVs( BSP.Face face, List<Vector3> surfaceVertices )
        {
            var texInfo = BSP.TextureInfo[face.TexInfo];
            var texData = BSP.TextureData[BSP.TextureInfo[face.TexInfo].TexData];

            var texVec = texInfo.TextureVecs;
            var lightmapVec = texInfo.LightmapVecs;

            var uvPoints = new Vector2[surfaceVertices.Count];
            var uv2Points = new Vector2[surfaceVertices.Count];

            var textureWidth = texData.Width * BSP.Scale;
            var textureHeight = texData.Height * BSP.Scale;

            for ( var i = 0; i < uvPoints.Length; i++ )
            {
                var vert = surfaceVertices[i];

                uvPoints[i] = new Vector2(
                    ( texVec[0][0] * vert.x + -texVec[0][2] * vert.y + texVec[0][1] * vert.z + texVec[0][3] ) / textureWidth,
                    ( texVec[1][0] * vert.x + -texVec[1][2] * vert.y + texVec[1][1] * vert.z + texVec[0][3] ) / textureHeight
                );

                uv2Points[i] = new Vector2(
                    lightmapVec[0][0] * vert.x + -lightmapVec[0][2] * vert.y + lightmapVec[0][1] * vert.z + lightmapVec[0][3] - face.LightmapTextureMinsInLuxels[0],
                    lightmapVec[1][0] * vert.x + -lightmapVec[1][2] * vert.y + lightmapVec[1][1] * vert.z + lightmapVec[0][3] - face.LightmapTextureMinsInLuxels[1]
                );
            }

            return ( uvPoints, uv2Points );
        }

        //
        // Lighting & Rendering
        //

        public GameObject MakeCubemap( BSP.Cubemap cubemap )
        {
            var go = new GameObject( "Cubemap" );

            var probe = go.AddComponent<ReflectionProbe>();
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.resolution = cubemap.Size;
            probe.size = Vector3.one * 50;

            go.transform.position = cubemap.Origin * BSP.Scale;

            return go;
        }
    }
}