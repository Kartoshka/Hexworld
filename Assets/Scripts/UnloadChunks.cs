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
			if(!((c.pos.x <= (currentPos.x+unloadPastDistance) && c.pos.x >=(currentPos.x-unloadPastDistance)) && (c.pos.y <= (currentPos.y+unloadPastDistance) && c.pos.y >=(currentPos.x-unloadPastDistance)))){
				cManager.DestroyChunk (c.pos);
			}
		}
	}

	public override void OnChunkManagerUpdate (ChunkManager cManager)
	{

	}
		

}
