using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class OffsetAnimatedTile : AnimatedTile
{
    public override bool GetTileAnimationData(
        Vector3Int position,
        ITilemap tilemap,
        ref TileAnimationData data)
    {
        bool ok = base.GetTileAnimationData(
            position,
            tilemap,
            ref data);

        if (ok)
        {
            data.animationStartTime =
                position.x * 0.1f;
        }

        return ok;
    }
}