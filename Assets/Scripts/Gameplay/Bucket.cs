using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bucket : MonoBehaviour {

    public Transform blockStackParent;

    private float blockStackTotalHeight = 0;
    private List<BucketBlock> blocks = new List<BucketBlock>();
    private Vector3 nextBlockRelativePosition = Vector3.zero;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        blockStackParent.transform.position = Vector3.Lerp(blockStackParent.transform.position, transform.position + new Vector3(0, -blockStackTotalHeight, 0), 0.9f * Time.deltaTime / 0.2f);
    }

    public void ConnectWithBlock(FallingBlock block)
    {
        // Remove the falling block component and add the bucket block component
        GameObject blockObj = block.gameObject;
        float blockHeight = block.blockHeight;
        Object.Destroy(block);
        BucketBlock newBlock = blockObj.AddComponent<BucketBlock>();
        newBlock.blockHeight = blockHeight;

        // Add this to the blocks list
        blocks.Add(newBlock);
        newBlock.transform.position = blockStackParent.transform.position + nextBlockRelativePosition + new Vector3(0, 0.5f * blockHeight, 0);
        newBlock.transform.parent = blockStackParent;
        nextBlockRelativePosition = new Vector3(nextBlockRelativePosition.x, nextBlockRelativePosition.y + blockHeight, nextBlockRelativePosition.z);
        blockStackTotalHeight += blockHeight;
    }

    public bool BreakBlock()
    {
        if(blocks.Count > 0)
        {
            BucketBlock lastBlock = blocks[blocks.Count - 1];
            blockStackTotalHeight -= lastBlock.blockHeight;
            nextBlockRelativePosition.y = blockStackTotalHeight;
            GameObject.Destroy(lastBlock.gameObject);
            blocks.RemoveAt(blocks.Count - 1);
            return true;
        }

        return false;
    }
}
