// ReSharper disable StringLiteralTypo

using UnityEngine;

namespace Espionage.Engine.Source
{
    [Library( "worldspawn" ), Group( "Source Entities" ), Spawnable]
    public class SourceWorld : World, BSP.IBrushEntity
    {
        [Property( "coldword" )]
        public bool IsCold { get; set; }

        [Property( "skyname" )]
        public string SkyName { get; set; }

        public void OnRead( BSP.Entity ent, GameObject model ) => model.transform.parent = Transform.parent;
    }
}
