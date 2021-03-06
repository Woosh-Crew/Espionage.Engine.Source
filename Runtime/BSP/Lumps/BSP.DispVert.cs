using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct DispVert : ILump
        {
            public Vector Vector; // Vector field defining displacement volume.
            public float Distance; // Displacement distances.
            public float Alpha; // "per vertex" alpha values.

            public void Read( BinaryReader reader )
            {
                var pos = new Vector();
                pos.Read( reader );
                Vector = pos;

                Distance = reader.ReadSingle();
                Alpha = reader.ReadSingle();
            }
        }
    }
}