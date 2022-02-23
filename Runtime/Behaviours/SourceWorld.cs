// ReSharper disable StringLiteralTypo

namespace Espionage.Engine.Source
{
    [Library( "worldspawn" ), Spawnable]
    public class SourceWorld : World, BSP.IPointEntity
    {
        [Property( Name = "coldword" )]
        public bool IsCold { get; set; }

        [Property( Name = "skyname" )]
        public string SkyName { get; set; }
    }
}