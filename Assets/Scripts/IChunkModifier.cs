using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbChunkModifier: MonoBehaviour  {

    public abstract void OnChunkManagerStart(ChunkManager cManager);
    public abstract void OnMoveChunks(ChunkManager cManager);
    public abstract void OnChunkManagerUpdate(ChunkManager cManager);
}
