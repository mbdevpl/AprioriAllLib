using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
{
	/// <summary>
	/// Simple radix tree implementation.
	/// </summary>
	public class RadixTree
	{
		private int width;

		//private int depth;

		private Node root;

		public RadixTree(int width/*, int depth*/)
		{
			this.width = width;
			//this.depth = depth;
			root = null;
		}

		/// <summary>
		/// Tries to add a new list to the tree.
		/// </summary>
		/// <param name="list"></param>
		/// <returns>true if a given element was not already in the tree</returns>
		public bool TryAdd(List<int> list)
		{
			if (root == null)
				root = new Node(width);
			Node currentNode = root;
			foreach (int i in list)
			{
				if (currentNode.Get(i) == null)
					currentNode.Set(i, new Node(width));
				currentNode = currentNode.Get(i);
			}
			return currentNode.SetEnd();
		}

		/// <summary>
		/// Checks if a given element can be added to the tree.
		/// </summary>
		/// <param name="list"></param>
		/// <returns>true if a given element is not in the tree</returns>
		public bool Check(List<int> list)
		{
			if (root == null)
				//root = new Node(width);
				return true;
			Node currentNode = root;
			foreach (int i in list)
			{
				if (currentNode.Get(i) == null)
					//currentNode.Set(i, new Node(width));
					return true;
				currentNode = currentNode.Get(i);
			}
			return currentNode.CheckEnd();
		}
	}

	class Node
	{
		private bool end;

		private List<Node> nodes;

		public Node(int width)
		{
			end = false;
			nodes = new List<Node>();
			for (int i = 0; i < width; ++i)
				nodes.Add(null);
		}

		public void Set(int n, Node node)
		{
			nodes[n] = node;
		}

		public Node Get(int n)
		{
			return nodes[n];
		}

		public bool CheckEnd()
		{
			if (end)
				return false;
			return true;
		}

		public bool SetEnd()
		{
			if (end)
				return false;
			end = true;
			return true;
		}

	}
}
