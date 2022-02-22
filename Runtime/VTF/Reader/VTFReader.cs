using System;
using System.IO;

namespace Espionage.Engine.Source
{
    public class VTFReader : IDisposable
    {
        public BinaryReader Reader { get; }
        public VTF.Header Header { get; }

        public VTFReader( BinaryReader reader )
        {
            Reader = reader;
            Header = new VTF.Header( reader );
        }

        public void Dispose() => Reader?.Dispose();
    }
}