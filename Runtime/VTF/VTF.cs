using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Espionage.Engine.Source
{
    /// <summary>Deserialized VTF Data</summary>
    public partial class VTF
    {
        public FileInfo File { get; }
        public VTFReader Reader { get; }

        public VTF( FileInfo file )
        {
            File = file;

            // Open Streams for reading the header
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var reader = new BinaryReader( fileStream );

            Reader = new VTFReader( reader );
        }

        ~VTF() { Reader.Dispose(); }
    }
}