using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class TileView : MonoBehaviour
{
    public void SetData(Tile tile) {
        transform.localPosition = new Vector3(tile.coord.x + 0.5f, tile.coord.y + 0.5f);
    }
}
