using System;
using System.Collections.Generic;
using UnityEngine;

public class KDTree
{
    private CentroidNode rootNode;
    private CentroidNode currentNearest;
    private float currentMinDistance;

    private bool mustNotEqual = true;

   
    public CentroidNode GetRootNode()
    {
        return this.rootNode;
    }

    public void SetRootNode(CentroidNode rootNode)
    {
        this.rootNode = rootNode;
    }

    public void Insert(CentroidNode node)
    {
        CentroidNode currentNode = this.rootNode;



        if (currentNode == null)
        {
            // Set root node if root node is null.
            this.rootNode = node;
        }

        else 
        {

            // Checks less than or more than x-coordinate of current node on first level of tree.
            // Then checks less than or more than y-coodinate of next current node on next level of tree.
            // After that checks less than or more than z-coordinate of following current node on 3rd level of tree.
            // On 4th level, check x-coordinate. Then 5th coordinate check y-coordinate, and so on.
            // Reference: https://www.youtube.com/watch?app=desktop&v=UfQxABgmppM

            // Using modulus of currentLevel:
            // - Gives the index for x at levels 0 (root), 3, 6, etc.
            // - Gives the index for y at levels 1, 4, 7, etc.
            // - Gives the index for z at levels 2, 5, 8, etc.
     
            InsertTraverse(node, currentNode, 0);

        }
    }
    public void InsertTraverse(CentroidNode nodeToInsert, CentroidNode currentNode, int currentLevel) {

        if (currentNode == null)
        {
            return;
        }

        if (nodeToInsert.position[currentLevel % 3] < currentNode.position[currentLevel % 3])
        {
            if (currentNode.leftChild != null)
            {
                InsertTraverse(nodeToInsert, currentNode.leftChild, currentLevel + 1);
            }
            else
            {
                currentNode.leftChild = nodeToInsert;
            }
        }
        else
        {
            if (currentNode.rightChild != null)
            {
                InsertTraverse(nodeToInsert, currentNode.rightChild, currentLevel + 1);
            }
            else
            {
                currentNode.rightChild = nodeToInsert;
            }

        }

        return;
    }

    public CentroidNode StartSearch(Vector3 point, bool mustNotEqualInput = false)
    {
        mustNotEqual = mustNotEqualInput;

        this.currentMinDistance = 10000000;
        this.currentNearest = null;

        Search(this.rootNode, point, 0);

        return this.currentNearest;
    }

    public void Search(CentroidNode currentNode, Vector3 point, int currentLevel)
    {
        if(currentNode == null)
        {
            // This means that the end of the tree is reached.
            return;
        }

        if(Vector3.Magnitude(currentNode.position - point) < this.currentMinDistance && 
            ( (!mustNotEqual) || (mustNotEqual && currentNode.position != point)))
        {
            this.currentMinDistance = Vector3.Magnitude(currentNode.position - point);
            this.currentNearest = currentNode;
        }

        int nextLevel = currentLevel + 1;
        float currentAxisSqDistance = (float) Math.Pow(point[currentLevel % 3] - currentNode.position[currentLevel % 3], 2);

        if (point[currentLevel % 3] < currentNode.position[currentLevel % 3])
        {
            Search(currentNode.leftChild, point, nextLevel);

            if (currentAxisSqDistance < this.currentMinDistance * this.currentMinDistance)
            {
                // If distance between current closest point and the input point is larger than the x, y or z difference between the input point and the currentNode
                Search(currentNode.rightChild, point, nextLevel);
            }

            return;
        }
        else
        {
            Search(currentNode.rightChild, point, nextLevel);

            if (currentAxisSqDistance < this.currentMinDistance * this.currentMinDistance)
            {
                // If distance between current closest point and the input point is larger than the x, y or z difference between the input point and the currentNode
                Search(currentNode.leftChild, point, nextLevel);
            }

            return;
        }
    }
}


