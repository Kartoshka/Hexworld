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

	#region AbChunkModifier methods
    public override void OnChunkManagerStart(ChunkManager cManager)
    {
        run = true;
        awaitingInstantiation = new Queue<ChunkManager.Chunk>();
        ChunkManager.Chunk c = cManager.getNewChunkData(cManager.findCurrentChunk());
		cManager.instantiateChunk(c);

        this.verifySurroundings(cManager,2);

		//How many threads concurrently run and generate chunk data
		this.StartCoroutineAsync (continuousGenThread (cManager));
		//this.StartCoroutineAsync (continuousGenThread (cManager));

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

	public void OnApplicationQuit()
	{
		run = false;
		StopCoroutine (traversal);
	}

	#endregion

	/// <summary>
	/// Traverses the list of chunks awaiting instantiation and tries to generate as many of them as it can while staying within 60fps
	/// </summary>
	/// <returns>The list.</returns>
	/// <param name="cManager">C manager.</param>
    private IEnumerator TraverseList(ChunkManager cManager)
    {
		int startFrame = Environment.TickCount;
        if (awaitingInstantiation == null)
        {
            yield return null;
        }
        else
        {
			int timeTaken = 0;
            while (awaitingInstantiation.Count > 0)
            {
				
                ChunkManager.Chunk c = awaitingInstantiation.Dequeue();
                //cManager.instantiateChunk(c.pos, c.size, 512, c.blockTypes);
				cManager.instantiateChunk(c);
				int instantiationTime = (Environment.TickCount - startFrame);
				startFrame = Environment.TickCount;
				timeTaken += instantiationTime;
				if (timeTaken > 16.67) {
					break;
				} else if ((timeTaken + instantiationTime) > 16.67) {
					break;
				}
            }
        }
        traversal = null;
        yield return null;
    }

	/// <summary>
	/// Verifies chunks around player to see if any need to be loaded
	/// </summary>
	/// <param name="cManager">C manager.</param>
	/// <param name="radius">Radius.</param>
    private void verifySurroundings(ChunkManager cManager,int radius)
    {
        Vector2 currentChunk = cManager.findCurrentChunk();

        int startX = (int)currentChunk.x;
        int startZ = (int)currentChunk.y;

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
			if (chunks[c][2] < radius+0.5 && !cManager.chunkIsLoaded (new Vector2 (startX + chunks [c] [0], startZ + chunks [c] [1])) && !cManager.chunkIsGenerating (new Vector2 (startX + chunks [c] [0], startZ + chunks [c] [1]))) {
				requests.Enqueue (new Vector2 (startX + chunks [c] [0], startZ + chunks [c] [1]));
			}
		}

    }
		

	#region Asynchrounous method body
    private bool run = true;
	private Queue<Vector2> requests = new Queue<Vector2>();
    private object requestLock = new object();


	/// <summary>
	/// Continuously runs and checks whether there are any requests waiting to be parsed
	/// </summary>
	/// <returns>The gen thread.</returns>
	/// <param name="cManager">C manager.</param>
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

	#endregion


}
