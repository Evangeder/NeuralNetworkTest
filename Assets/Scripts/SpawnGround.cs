using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnGround : MonoBehaviour
{
    public GameObject GroundBlock;
    Dictionary<int, GameObject> GroundBlocks = new Dictionary<int, GameObject>();

    public int GroundSpawnRadius = 10;

    int LastPosition = int.MinValue;

    void Update()
    {
        if (LastPosition != (int)transform.position.x)
        {
            LastPosition = (int)transform.position.x;
            var rmBlocks = new List<int>();
            foreach (var block in GroundBlocks.Where(x => x.Key < (int)transform.position.x - GroundSpawnRadius || x.Key > (int)transform.position.x + GroundSpawnRadius))
            {
                rmBlocks.Add(block.Key);
                Destroy(block.Value);
            }

            foreach (var block in rmBlocks)
                GroundBlocks.Remove(block);

            for (int i = (int)transform.position.x - GroundSpawnRadius; i < (int)transform.position.x + GroundSpawnRadius; i++)
                if (!GroundBlocks.ContainsKey(i) && i % 2 == 0)
                    GroundBlocks.Add(i, Instantiate(GroundBlock, new Vector3(i, -4, 0), Quaternion.identity));

            rmBlocks.Clear();
        }
    }
}
