using System;
using System.Collections.Generic;
using System.IO;
using Espionage.Engine.Resources;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Espionage.Engine.Source
{
    public class BSPMapProvider : IMapProvider, ICallbacks
    {
    #if UNITY_EDITOR

        [MenuItem( "Source/Load BSP" )]
        public static void LoadMap()
        {
            using var _ = Debugging.Stopwatch( "Loaded BSP" );

            var path = EditorUtility.OpenFilePanel( "BSP", @"D:\Programs\SteamLibrary\steamapps\common\Half-Life 2\hl2\maps", "bsp" );
            var bsp = new BSP( new FileInfo( path ) );

            var map = new Map( new BSPMapProvider( bsp ) );
            map.Load();
        }

    #endif

        // Id
        public string Identifier => BSP.File.FullName;

        // Outcome
        public Scene? Scene { get; private set; }

        // Loading Meta
        public float Progress => 0;
        public bool IsLoading { get; private set; }

        public BSPMapProvider( BSP bsp )
        {
            BSP = bsp;
            Callback.Register( this );
        }

        //
        // Resource
        //

        public BSP BSP { get; }

        public void Load( Action finished )
        {
            Debugging.Log.Info( $"Loading {BSP.File.Name}, Format {BSP.Reader.Header.Format}, Version {BSP.Reader.Header.Version}" );

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

        // BSP Generator

        public GameObject Generate()
        {
            var geoRoot = new GameObject( "Geometry" );

            // Build Mesh

            var combiner = new List<CombineInstance>();


            for ( var i = 0; i < BSP.Faces.Length; i++ )
            {
                var face = BSP.Faces[i];

                var flags = BSP.TextureInfo[face.TexInfo].Flags;
                if ( flags.HasFlag( BSP.TexInfo.Flag.Nodraw ) || flags.HasFlag( BSP.TexInfo.Flag.Trigger ) || flags.HasFlag( BSP.TexInfo.Flag.Sky ) || flags.HasFlag( BSP.TexInfo.Flag.Hitbox ) )
                    continue;

                var mesh = face.DisplacementInfo != -1 ? MakeDisplacement( face ) : MakeFace( face );

                if ( mesh == null )
                    continue;

                combiner.Add( new CombineInstance() { mesh = mesh, transform = Matrix4x4.identity } );
            }

            {
                // Create Mesh

                var finalMesh = new Mesh()
                {
                    name = $"Map"
                };

                finalMesh.CombineMeshes( combiner.ToArray() );
                finalMesh.Optimize();

                finalMesh.RecalculateBounds();
                finalMesh.RecalculateNormals();

                // Create Object
                var go = new GameObject( $"Map" );
                go.AddComponent<MeshRenderer>();

                var filter = go.AddComponent<MeshFilter>();
                filter.mesh = finalMesh;

                go.transform.parent = geoRoot.transform;
            }

            // Add Cubemaps

            var cubemapRoot = new GameObject( "Cubemaps" );

            foreach ( var item in BSP.Cubemaps )
            {
                var go = new GameObject( "Cubemap" );

                var probe = go.AddComponent<ReflectionProbe>();
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.resolution = item.Size;
                probe.size = Vector3.one * 50;

                go.transform.parent = cubemapRoot.transform;
                go.transform.position = item.Origin * BSP.Scale;
            }

            return geoRoot;
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

                var planeNormal = BSP.Planes[face.PlaneNum].Normal;

                if ( BSP.SurfEdges[face.FirstEdge + i] >= 0 )
                {
                    if ( surfaceVertices.IndexOf( point1 ) < 0 )
                    {
                        surfaceVertices.Add( point1 );
                        normals.Add( planeNormal );
                    }

                    originalVertices.Add( point1 );

                    if ( surfaceVertices.IndexOf( point2 ) < 0 )
                    {
                        surfaceVertices.Add( point2 );
                        normals.Add( planeNormal );
                    }

                    originalVertices.Add( point2 );
                }
                else
                {
                    if ( surfaceVertices.IndexOf( point2 ) < 0 )
                    {
                        surfaceVertices.Add( point2 );
                        normals.Add( planeNormal );
                    }

                    originalVertices.Add( point2 );

                    if ( surfaceVertices.IndexOf( point1 ) < 0 )
                    {
                        surfaceVertices.Add( point1 );
                        normals.Add( planeNormal );
                    }

                    originalVertices.Add( point1 );
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
            // Create UVs
            //

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


            //
            // Finish
            // 

            var mesh = new Mesh
            {
                vertices = surfaceVertices.ToArray(),
                triangles = tris.ToArray(),
                normals = normals.ToArray(),
                uv = uvPoints,
                uv2 = uv2Points
            };

            mesh.Optimize();
            mesh.RecalculateTangents();

            return mesh;
        }

        public Mesh MakeDisplacement( BSP.Face face ) => null;
    }
}