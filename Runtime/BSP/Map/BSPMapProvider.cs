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

        private GameObject _root;

        public void Load( Action finished )
        {
            Debugging.Log.Info( $"Loading {BSP.File.Name}, Format {BSP.Reader.Header.Format}, Version {BSP.Reader.Header.Version}" );

            // Start
            IsLoading = true;

            // Create Scene
            Scene = SceneManager.CreateScene( Path.GetFileName( BSP.File.Name ) );
            SceneManager.SetActiveScene( Scene.Value );

            // Generate BSP
            _root = Generate();

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
            var root = new GameObject( "Geometry" );

            var combiner = new CombineInstance[BSP.Faces.Length];

            for ( var i = 0; i < BSP.Faces.Length; i++ )
            {
                var face = BSP.Faces[i];

                var mesh = MakeFace( face );
                if ( mesh == null )
                    continue;

                combiner[i] = new CombineInstance() { mesh = mesh, transform = Matrix4x4.identity };
            }

            var filter = root.AddComponent<MeshFilter>();

            var finalMesh = new Mesh();
            finalMesh.Clear();

            finalMesh.CombineMeshes( combiner );
            finalMesh.RecalculateBounds();
            finalMesh.RecalculateNormals();

            filter.mesh = finalMesh;

            root.AddComponent<MeshRenderer>();

            return root;
        }

        public Mesh MakeFace( BSP.Face face )
        {
            var flags = BSP.TextureInfo[face.TexInfo].Flags;
            if ( flags.HasFlag( BSP.TexInfo.Flag.Nodraw ) || flags.HasFlag( BSP.TexInfo.Flag.Trigger ) || flags.HasFlag( BSP.TexInfo.Flag.Sky ) || flags.HasFlag( BSP.TexInfo.Flag.Hitbox ) )
                return null;

            var surfaceVertices = new List<Vector3>();
            var originalVertices = new List<Vector3>();
            var normals = new List<Vector3>();

            for ( var i = 0; i < face.NumEdges; i++ )
            {
                var currentEdge = BSP.Edges[Mathf.Abs( BSP.SurfEdges[face.FirstEdge + i] )].VertexIndices;

                Vector3 point1 = BSP.Vertices[currentEdge[0]];
                Vector3 point2 = BSP.Vertices[currentEdge[1]];

                var planeNormal = BSP.Planes[face.PlaneNum].Normal;

                // point1 = new Vector3( point1.x, point1.y, point1.z );
                // point2 = new Vector3( point2.x, point2.y, point2.z );

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

            /*
            #region Apply Displacement
            if (face.dispinfo > -1)
            {
                ddispinfo_t disp = bspParser.dispInfo[face.dispinfo];
                int power = Mathf.RoundToInt(Mathf.Pow(2, disp.power));

                List<Vector3> dispVertices = new List<Vector3>();
                Vector3 startingPosition = surfaceVertices[0];
                Vector3 topCorner = surfaceVertices[1], topRightCorner = surfaceVertices[2], rightCorner = surfaceVertices[3];

                #region Setting Orientation
                Vector3 dispStartingVertex = disp.startPosition;
                if (Vector3.Distance(dispStartingVertex, topCorner) < 0.01f)
                {
                    Vector3 tempCorner = startingPosition;

                    startingPosition = topCorner;
                    topCorner = topRightCorner;
                    topRightCorner = rightCorner;
                    rightCorner = tempCorner;
                }
                else if (Vector3.Distance(dispStartingVertex, rightCorner) < 0.01f)
                {
                    Vector3 tempCorner = startingPosition;

                    startingPosition = rightCorner;
                    rightCorner = topRightCorner;
                    topRightCorner = topCorner;
                    topCorner = tempCorner;
                }
                else if (Vector3.Distance(dispStartingVertex, topRightCorner) < 0.01f)
                {
                    Vector3 tempCorner = startingPosition;

                    startingPosition = topRightCorner;
                    topRightCorner = tempCorner;
                    tempCorner = rightCorner;
                    rightCorner = topCorner;
                    topCorner = tempCorner;
                }
                #endregion

                int orderNum = 0;
                #region Method 13 (The one and only two)
                Vector3 leftSide = (topCorner - startingPosition), rightSide = (topRightCorner - rightCorner);
                float leftSideLineSegmentationDistance = leftSide.magnitude / power, rightSideLineSegmentationDistance = rightSide.magnitude / power;
                for (int line = 0; line < (power + 1); line++)
                {
                    for (int point = 0; point < (power + 1); point++)
                    {
                        Vector3 leftPoint = (leftSide.normalized * line * leftSideLineSegmentationDistance) + startingPosition;
                        Vector3 rightPoint = (rightSide.normalized * line * rightSideLineSegmentationDistance) + rightCorner;
                        Vector3 currentLine = rightPoint - leftPoint;
                        Vector3 pointDirection = currentLine.normalized;
                        float pointSideSegmentationDistance = currentLine.magnitude / power;

                        Vector3 pointA = leftPoint + (pointDirection * pointSideSegmentationDistance * point);

                        Vector3 dispDirectionA = bspParser.dispVerts[disp.DispVertStart + orderNum].vec;
                        dispVertices.Add(pointA + (dispDirectionA * bspParser.dispVerts[disp.DispVertStart + orderNum].dist));
                        orderNum++;
                    }
                }
                #endregion

                surfaceVertices = dispVertices;
            }
            #endregion
*/

            #region Triangulate

            var triangleIndices = new List<int>();

            if ( face.DisplacementInfo > -1 )
            {
                // ddispinfo_t disp = bspParser.dispInfo[face.dispinfo];
                // int power = Mathf.RoundToInt(Mathf.Pow(2, disp.power));
                //
                // #region Method 12 Triangulation
                // for (int row = 0; row < power; row++)
                // {
                //     for (int col = 0; col < power; col++)
                //     {
                //         int currentLine = row * (power + 1);
                //         int nextLineStart = (row + 1) * (power + 1);
                //
                //         triangleIndices.Add(currentLine + col);
                //         triangleIndices.Add(currentLine + col + 1);
                //         triangleIndices.Add(nextLineStart + col);
                //
                //         triangleIndices.Add(currentLine + col + 1);
                //         triangleIndices.Add(nextLineStart + col + 1);
                //         triangleIndices.Add(nextLineStart + col);
                //     }
                // }
                // #endregion
            }
            else
            {
                for ( var i = 0; i < originalVertices.Count / 2; i++ )
                {
                    var firstOrigIndex = i * 2;
                    var secondOrigIndex = i * 2 + 1;
                    var thirdOrigIndex = 0;

                    var firstIndex = surfaceVertices.IndexOf( originalVertices[firstOrigIndex] );
                    var secondIndex = surfaceVertices.IndexOf( originalVertices[secondOrigIndex] );
                    var thirdIndex = surfaceVertices.IndexOf( originalVertices[thirdOrigIndex] );

                    triangleIndices.Add( firstIndex );
                    triangleIndices.Add( secondIndex );
                    triangleIndices.Add( thirdIndex );
                }
            }

            #endregion

            #region Map UV Points

            Vector3 s = Vector3.zero, t = Vector3.zero;
            float xOffset = 0;
            float yOffset = 0;

            s = new Vector3( BSP.TextureInfo[face.TexInfo].TextureVecs[0][0], BSP.TextureInfo[face.TexInfo].TextureVecs[0][1], BSP.TextureInfo[face.TexInfo].TextureVecs[0][2] );
            t = new Vector3( BSP.TextureInfo[face.TexInfo].TextureVecs[1][0], BSP.TextureInfo[face.TexInfo].TextureVecs[1][1], BSP.TextureInfo[face.TexInfo].TextureVecs[1][2] );
            xOffset = BSP.TextureInfo[face.TexInfo].TextureVecs[0][3];
            yOffset = BSP.TextureInfo[face.TexInfo].TextureVecs[1][3];

            var uvPoints = new Vector2[surfaceVertices.Count];
            var textureWidth = 0;
            var textureHeight = 0;

            textureWidth = BSP.TextureData[BSP.TextureInfo[face.TexInfo].TexData].Width;
            textureHeight = BSP.TextureData[BSP.TextureInfo[face.TexInfo].TexData].Height;

            for ( var i = 0; i < uvPoints.Length; i++ )
                uvPoints[i] = new Vector2( (
                        Vector3.Dot( surfaceVertices[i], s ) + xOffset ) / textureWidth,
                    ( textureHeight - ( Vector3.Dot( surfaceVertices[i], t ) + yOffset ) ) / textureHeight
                ) / BSP.Scale;

            #endregion

            #region Organize Mesh Data

            var mesh = new Mesh
            {
                vertices = surfaceVertices.ToArray(),
                triangles = triangleIndices.ToArray(),
                normals = normals.ToArray(),
                uv = uvPoints
            };

            #endregion

            mesh.Optimize();
            mesh.RecalculateTangents();

            return mesh;
        }
    }
}