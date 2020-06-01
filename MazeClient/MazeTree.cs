using System;
using System.Collections.Generic;
using System.Linq;
using MazeClient.Enum;

public class Node
{
	public TileEnum Tile;
	public MoveEnum ParentMove;
	public bool Visited;
	public Node Parent;
	public Node Right;
	public Node Up;
	public Node Left;
	public Node Down;

	public Node()
	{
	}

	public Node(TileEnum tile, MoveEnum parentMove, bool visited, Node parent)
	{
		Tile = tile;
		ParentMove = parentMove;
		Visited = visited;
		Parent = parent;
	}
}

public class MazeTree
{
	private Node Root;
	private Node Current;
	private readonly MazeTree Tree;

	public MazeTree()
	{
	}

	public Node GetRoot()
	{
		return Root;
	}

	public Node GetCurrent()
	{
		return Current;
	}

	public MazeTree GetTree()
	{
		return Tree;
	}

	public void Add(Node root, MoveEnum direction, TileEnum tile, bool visited)
	{
		if (root == null)
		{
			Root = new Node(tile, MoveEnum.NONE, visited, null);
			Current = Root;
		}
		else
		{
			root.Visited = true;
			if (direction.CompareTo("Right") == 0)
			{
				root.Right = new Node(tile, direction, visited, root);
			}

			if (direction.CompareTo("Up") == 0)
			{
				root.Up = new Node(tile, direction, visited, root);
			}

			if (direction.CompareTo("Left") == 0)
			{
				root.Left = new Node(tile, direction, visited, root);
			}

			if (direction.CompareTo("Down") == 0)
			{
				root.Down = new Node(tile, direction, visited, root);
			}

			Current = root;
		}
	}

	public void Traverse(Node root)
	{
		if (root == null)
		{
			return;
		}

		Console.WriteLine(string.Format("{0} - {1}", root.Tile, root.ParentMove));

		//if (root.Right != null)
		//Console.WriteLine("Right");
		Traverse(root.Right);

		//if (root.Up != null)
		//Console.WriteLine("Up");
		Traverse(root.Up);

		//if (root.Left != null)
		//Console.WriteLine("Left");
		Traverse(root.Left);

		//if (root.Down != null)
		//Console.WriteLine("Down");
		Traverse(root.Down);
	}

	public void TraserveInverse(Node node, ref List<string> path)
	{
		if (node == null)
		{
			return;
		}

		path.Add(node.Tile.ToString());
		if (!string.IsNullOrEmpty(node.ParentMove.ToString()))
			path.Add(node.ParentMove.ToString());

		//Console.WriteLine(string.Format("{0} - {1}", node.Tile, node.Visited));

		TraserveInverse(node.Parent, ref path);
	}

	public void GetPaths(Node node, List<string> path, ref List<List<string>> pathAcc)
	{
		if (node == null)
		{
			return;
		}

		if (!string.IsNullOrEmpty(node.ParentMove.ToString()))
			path.Add(node.ParentMove.ToString());
		path.Add(node.Tile.ToString());

		if (node.Right == null && node.Up == null &&
			node.Left == null && node.Down == null)
		{
			pathAcc.Add(path.ToList());
		}
		else
		{
			GetPaths(node.Right, new List<string>(path), ref pathAcc);
			GetPaths(node.Up, new List<string>(path), ref pathAcc);
			GetPaths(node.Left, new List<string>(path), ref pathAcc);
			GetPaths(node.Down, new List<string>(path), ref pathAcc);
		}
	}

	public List<string> FindShortestPath(Node current, string tile, List<List<string>> pathAcc)
	{
		List<string> result = new List<string>();

		List<string> currentPath = new List<string>();
		TraserveInverse(current, ref currentPath);

		currentPath.Reverse();

		if (currentPath.Count > 3)
		{
			pathAcc.RemoveAll(x => !x.Contains(tile));

			Dictionary<string, int> counts = GetPathsCounts(pathAcc, currentPath, tile);

			List<KeyValuePair<string, int>> listSorted = counts.ToList();
			listSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

			List<string> path = listSorted.First().Key.Split(',').ToList();
			//string currentPathStr = String.Join(",", currentPath);

			//Console.WriteLine("path: " + string.Join(",", path));
			//Console.WriteLine("currentPath: " + string.Join(",", currentPath));

			result = RemoveSimilar(currentPath, path, tile);
		}

		return result;
	}

	public void PrintPaths(List<List<string>> paths)
	{
		foreach (List<string> path in paths)
			Console.WriteLine(string.Join(",", path));
	}

	private List<string> RemoveSimilar(List<string> currentPath, List<string> path, string tile)
	{
		List<string> result = new List<string>();

		char[] splitCurrentPath = string.Join(",", currentPath).ToCharArray();
		char[] splitPath = string.Join(",", path).ToCharArray();

		string currentPathResult = string.Empty;
		string pathResult = string.Empty;

		if (string.Compare(splitCurrentPath.ToString(), splitPath.ToString()) == 0)
		{
			currentPath.Reverse();
			splitCurrentPath = string.Join(",", currentPath).ToCharArray();
			for (int i = 0; i < splitCurrentPath.Length; i++)
			{
				currentPathResult += splitCurrentPath[i];
				if (splitCurrentPath[i] == Convert.ToChar(tile))
					break;
			}
		}
		else
		{
			int index = 0;
			//Console.WriteLine("splitCurrentPath[index]: " + splitCurrentPath[index] + " splitPath[index]: " + splitPath[index]);
			while (index < splitCurrentPath.Length
					&& index < splitPath.Length
					&& splitCurrentPath[index] == splitPath[index])
			{
				index++;
			}

			if (index < splitCurrentPath.Length)
			{
				currentPath.Reverse();
				splitCurrentPath = string.Join(",", currentPath).ToCharArray();
				for (int i = 0; i < splitCurrentPath.Length - index; i++)
				{
					currentPathResult += splitCurrentPath[i];
				}
			}

			//Console.WriteLine("index: " + index + " splitPath.Length: " + splitPath.Length);
			if (index < splitPath.Length)
			{
				pathResult += splitPath[index - 2];
				pathResult += splitPath[index - 1];
				for (int i = index; i < splitPath.Length; i++)
				{
					pathResult += splitPath[i];
					if (splitPath[i] == Convert.ToChar(tile))
						break;
				}
			}
		}

		if (!string.IsNullOrEmpty(currentPathResult))
			result.Add(currentPathResult);

		if (!string.IsNullOrEmpty(pathResult))
			result.Add(pathResult);

		return result;
	}

	private Dictionary<string, int> GetPathsCounts(List<List<string>> pathAcc, List<string> currentPath, string tile)
	{
		Dictionary<string, int> result = new Dictionary<string, int>();

		foreach (List<string> pathSmallAcc in pathAcc)
		{
			int count = 0, index = 0;
			bool found = false;

			int currentPathLevel = currentPath.Count == 3 ? 1 : (currentPath.Count - 1) / 2;

			for (int i = 0; i < pathSmallAcc.Count - 2 && i < currentPath.Count - 2; i += 2)
			{
				string smallPath = string.Format("{0}{1}{2}", pathSmallAcc[i], pathSmallAcc[i + 1], pathSmallAcc[i + 2]);
				string smallCurrentPath = string.Format("{0}{1}{2}", currentPath[i], currentPath[i + 1], currentPath[i + 2]);

				//Console.WriteLine(smallPath);
				//Console.WriteLine(smallCurrentPath);

				if (string.Compare(smallPath, smallCurrentPath) != 0)
				{
					count++;
				}
				else
				{
					currentPathLevel--;
				}

				if (string.Compare(pathSmallAcc[i + 2], tile) == 0)
				{
					found = true;
					break;
				}

				index = i;
			}

			if (!found)
			{
				for (int i = index; i < pathSmallAcc.Count - 2; i += 2)
				{
					if (string.Compare(pathSmallAcc[i + 2], tile) == 0)
					{
						break;
					}
					count++;
				}
			}

			result.Add(string.Join(",", pathSmallAcc), count + currentPathLevel);
		}

		return result;
	}
}

//public static void Main()
//{
//	Node root = null;
//	Node current = null;
//	Tree bst = new Tree();

//	Console.WriteLine("Root");

//	bst.Add(null, null, "S", true);

//	root = bst.GetRoot();
//	Console.WriteLine(root.Tile);

	/*
	Example 1
	bst.Add(root, "Down", "E", false);
	bst.Add(root, "Up", "C", false);
	bst.Add(root, "Right", "x", false);
		
	Node current1 = bst.GetCurrent().Right;
	current = bst.GetCurrent().Down;
		
	bst.Add(current, "Down", "x", false);
		
	current = bst.GetCurrent().Down;
		
	bst.Add(current, "Down", "x", false);
		
	current = bst.GetCurrent().Down;
		
	bst.Add(current, "Down", "x", false);
		
	current = bst.GetCurrent().Down;
		
	bst.Add(current, "Down", "C", false);
		
	current = current1;
		
	Console.WriteLine(current.Tile);
		
	bst.Add(current, "Up", "C", false);
	bst.Add(current, "Right", "x", false);
		
	current = bst.GetCurrent().Right;
		
	//Console.WriteLine(current.Tile);
		
	bst.Add(current, "Down", "C", false);
	bst.Add(current, "Left", "o", false);
	bst.Add(current, "Right", "x", false);
		
	current = bst.GetCurrent().Right;
		
	Console.WriteLine("Paths");
		
	List<string> path = new List<string>();
	List<List<string>> pathAcc = new List<List<string>>();
	bst.GetPaths(root, path, ref pathAcc);
	bst.PrintPaths(pathAcc);	
		
	List<string> shtPath = bst.FindShortestPath(current, "C", pathAcc);
		
	Console.WriteLine("sth path: " + String.Join(",", shtPath));
	*/

	//Example 2
	/*
	bst.Add(root, "Right", "o", false);

	current = bst.GetCurrent().Right;

	bst.Add(current, "Right", "C", false);

	current = bst.GetCurrent().Right;

	bst.Add(current, "Right", "E", false);

	current = bst.GetCurrent().Right;

	List<string> path = new List<string>();
	List<List<string>> pathAcc = new List<List<string>>();
	bst.GetPaths(root, path, ref pathAcc);
	bst.PrintPaths(pathAcc);

	List<string> shtPath = bst.FindShortestPath(current, "C", pathAcc);

	Console.WriteLine("sth path: " + String.Join(",", shtPath));
	*/
//}