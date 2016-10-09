using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnloadChunks : AbChunkModifier {

	public int unloadPastDistance = 1;

	public override void OnChunkManagerStart (ChunkManager cManager)
	{

	}

	public override void OnMoveChunks (ChunkManager cManager)
	{
		Vector2 currentPos = cManager.findCurrentChunk ();
		ChunkManager.Chunk[] loadedChunks =cManager.getLoadedChunks ();

		foreach (ChunkManager.Chunk c in loadedChunks)
		{
			float distance = Mathf.Sqrt (Mathf.Pow(c.pos.x-currentPos.x,2)+Mathf.Pow(c.pos.y-currentPos.y,2));
			if (distance > unloadPastDistance) {
				cManager.DestroyChunk (c.pos);
			}
		}

		Resources.UnloadUnusedAssets ();
	}

	public override void OnChunkManagerUpdate (ChunkManager cManager)
	{

	}
		

}
