// ReSharper disable StringLiteralTypo

using UnityEngine;
using UnityEngine.Rendering;

namespace Espionage.Engine.Source
{
    [Library( "func_brush" ), Title( "Func Brush" ), Group( "Entities" ), Spawnable]
    public class FuncBrush : Behaviour, BSP.IBrushEntity
    {
        [Property( "disablereceiveshadows" )]
        public bool DisableReceiveShadows { get; set; }

        [Property( "disableshadows" )]
        public bool DisableCastingShadows { get; set; }

        public void OnRead( BSP.Entity ent, GameObject model )
        {
            // Set Parent
            model.transform.parent = transform;
            model.transform.localPosition = BSP.Vector.Parse( ent.KeyValues["origin"] );

            var renderers = GetComponentsInChildren<MeshRenderer>();

            foreach ( var meshRenderer in renderers )
            {
                meshRenderer.receiveShadows = !DisableReceiveShadows;
                meshRenderer.shadowCastingMode = DisableCastingShadows ? ShadowCastingMode.Off : meshRenderer.shadowCastingMode;
            }
        }
    }
}