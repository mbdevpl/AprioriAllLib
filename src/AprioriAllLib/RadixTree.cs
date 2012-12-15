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
		/// <summary>
		/// Maximal width of the tree. Large values greatly increase memory use in case of large trees.
		/// </summary>
		private int width;

		//private int depth;

		/// <summary>
		/// Root of the tree, this node is a starting point in all insertions/searches.
		/// </summary>
		private RadixTreeNode root;

		/// <summary>
		/// Constructs a new radix tree with a specified maximal width.
		/// </summary>
		/// <param name="width"></param>
		public RadixTree(int width/*, int depth*/)
		{
			this.width = width;
			//this.depth = depth;
			root = null;
		}

		public bool TryAdd(List<int> list)
		{
			return TryAdd(list, -1, false);
		}

		public bool TryAdd(List<int> list, int omittedIndex)
		{
			return TryAdd(list, omittedIndex, false);
		}

		/// <summary>
		/// Tries to add a new list to the tree.
		/// </summary>
		/// <param name="list">list of numbers that will be added to radix tree</param>
		/// <param name="omittedIndex">element of the list at this index is treated as non-existent</param>
		/// <param name="storeOmittedAtLeaf">omitted element is stored in the leaf node of the tree</param>
		/// <returns>true if a given element was not already in the tree</returns>
		public bool TryAdd(List<int> list, int omittedIndex, bool storeOmittedAtLeaf)
		{
			if (root == null)
				root = new RadixTreeNode(width);
			RadixTreeNode currentNode = root;
			var listEnum = list.GetEnumerator();
			for (int n = 0; n < list.Count; ++n)
			{
				listEnum.MoveNext();
				if (n == omittedIndex)
					continue;
				int i = listEnum.Current;
				if (currentNode.Get(i) == null)
					currentNode.Set(i, new RadixTreeNode(width));
				currentNode = currentNode.Get(i);
			}

			if(storeOmittedAtLeaf)
				currentNode.StoreValue(list[omittedIndex]);

			return currentNode.SetEnd();
		}

		public bool Check(List<int> list)
		{
			return Check(list, -1);
		}

		/// <summary>
		/// Checks if a given element can be added to the tree.
		/// </summary>
		/// <param name="list">list of numbers that will be compared with the contents of radix tree</param>
		/// <param name="ommitedIndex">element of the list at this index is treated as non-existent</param>
		/// <returns>true if a given element is not in the tree</returns>
		public bool Check(List<int> list, int omittedIndex)
		{
			if (root == null)
				return true;
			RadixTreeNode currentNode = root;
			var listEnum = list.GetEnumerator();
			for (int n = 0; n < list.Count; ++n)
			{
				listEnum.MoveNext();
				if (n == omittedIndex)
					continue;
				int i = listEnum.Current;
				if (currentNode.Get(i) == null)
					return true;
				currentNode = currentNode.Get(i);
			}
			return currentNode.CheckEnd();
		}

		internal RadixTreeNode GetNode(List<int> list, int omittedIndex)
		{
			if (root == null)
				return null;
			RadixTreeNode currentNode = root;
			var listEnum = list.GetEnumerator();
			for (int n = 0; n < list.Count; ++n)
			{
				listEnum.MoveNext();
				if (n == omittedIndex)
					continue;
				int i = listEnum.Current;
				if (currentNode.Get(i) == null)
					return null;
				currentNode = currentNode.Get(i);
			}
			return currentNode;
		}

	}
}
