// ReSharper disable StringLiteralTypo

using UnityEngine;

namespace Espionage.Engine.Source
{
    [Library( "func_brush" ), Title( "Func Brush" ), Group( "Entities" ), Spawnable]
    public class FuncBrush : Behaviour, BSP.IBrushEntity
    {
        public void OnRead( BSP.Entity ent, GameObject model )
        {
            // Set Parent
            model.transform.parent = transform;

            model.transform.localPosition = BSP.Vector.Parse( ent.KeyValues["origin"] );
        }
    }
}