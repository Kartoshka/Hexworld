using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;

public class LoadChunks : AbChunkModifier
{
    Queue<ChunkManager.Chunk> awaitingInstantiation;
    public int radius;
    private Coroutine traversal;

    public override void OnChunkManagerStart(ChunkManager cManager)
    {
        run = true;
        awaitingInstantiation = new Queue<ChunkManager.Chunk>();
        ChunkManager.Chunk c = cManager.getNewChunkData(cManager.findCurrentChunk());
		cManager.instantiateChunk(c);

        this.verifySurroundings(cManager,2);
        StartCoroutine(TraverseList(cManager));
		this.StartCoroutineAsync (continuousGenThread (cManager));
		this.StartCoroutineAsync (continuousGenThread (cManager));

    }
    public override void OnMoveChunks(ChunkManager cManager)
    {
        this.verifySurroundings(cManager,radius);
    }

    public override void OnChunkManagerUpdate(ChunkManager cManager)
    {
        if (awaitingInstantiation.Count > 0)
        {
            traversal = StartCoroutine(TraverseList(cManager));
        }
    }

    private IEnumerator TraverseList(ChunkManager cManager)
    {
        if (awaitingInstantiation == null)
        {
            yield return null;
        }
        else
        {
            while (awaitingInstantiation.Count > 0)
            {
                ChunkManager.Chunk c = awaitingInstantiation.Dequeue();
                //cManager.instantiateChunk(c.pos, c.size, 512, c.blockTypes);
				cManager.instantiateChunk(c);
            }
        }
        traversal = null;
        yield return null;
    }

    private void verifySurroundings(ChunkManager cManager,int radius)
    {
        Vector2 currentChunk = cManager.findCurrentChunk();

        int startX = (int)currentChunk.x;
        int startZ = (int)currentChunk.y;

        //int squareSize = 2 * radius + 1;

		List<Vector3> chunks = new List<Vector3>();

		for (int i = -radius; i < radius + 1; i++)
		{
			for (int k = -radius; k < radius + 1; k++)
			{
				Vector3 cVec = new Vector3 (i, k, Mathf.Sqrt(Mathf.Pow((float)i, 2) + Mathf.Pow((float)k, 2)));

				if (chunks.Count == 0)
					chunks.Add (cVec);
				else {
					bool added = false;
					for (int d = 0; d < chunks.Count; d++) {
						if (cVec [2] < chunks [d] [2]) {
							chunks.Insert (d, cVec);
							added = true;
							break;
						}
					}
					if (!added)
						chunks.Add (cVec);
				}
			}
		}

		for (int c = 0; c < chunks.Count; c++) {
			if (chunks[c][2] < radius+0.5 && !cManager.chunkIsLoaded (new Vector2 (startX + chunks [c] [0], startZ + chunks [c] [1])) && !cManager.chunkIsGenerating (new Vector2 (startX + chunks [c] [0], startZ + chunks [c] [1])) && cManager.numChunksGenerating () < 2) {
				requests.Enqueue (new Vector2 (startX + chunks [c] [0], startZ + chunks [c] [1]));
			}
		}

		/*

        for (int i = 0; i < squareSize; i++)
        {
            for (int k = 0; k < squareSize; k++)
            {
				if (!cManager.chunkIsLoaded(new Vector2(startX + i, startZ + k)) && !cManager.chunkIsGenerating(new Vector2(startX + i, startZ + k)) && cManager.numChunksGenerating() < 2)
                {
					requests.Enqueue(new Vector2(startX + i, startZ + k));
                    //this.StartCoroutineAsync(chunkGenThread(cManager, new Vector2(startX + i, startZ + k)));
                }
            }
        }
        */
    }

    

    private IEnumerator chunkGenThread(ChunkManager cManager, Vector2 cPos)
    {
        ChunkManager.Chunk c = cManager.getNewChunkData(cPos);
        awaitingInstantiation.Enqueue(c);
        yield return null;
    }

    private bool run = true;
	private Queue<Vector2> requests = new Queue<Vector2>();
    private object requestLock = new object();
	public IEnumerator continuousGenThread(ChunkManager cManager){
		while (run) {
            bool gen = false;
            Vector2 pos = Vector2.zero;
            lock (requestLock)
            {
                if (requests.Count > 0)
                {
                    pos = requests.Dequeue();
                    gen = true;
                }
            }
            if (gen)
            {
                ChunkManager.Chunk c = cManager.getNewChunkData(pos);
                awaitingInstantiation.Enqueue(c);
            }
		}
		yield return null;
	}

    public void OnApplicationQuit()
    {
        run = false;
		StopCoroutine (traversal);
    }

}
