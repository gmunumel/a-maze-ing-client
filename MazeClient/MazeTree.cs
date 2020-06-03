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

	public Node GetParent()
	{
		return Current.Parent;
	}

	public void SetCurrent(Node current)
    {
		Current = current;
    }

	public void SetVisited(bool visited)
    {
		Current.Visited = visited;
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
			if (direction.CompareTo(MoveEnum.Right) == 0)
			{
				root.Right = new Node(tile, direction, visited, root);
			}

			if (direction.CompareTo(MoveEnum.Up) == 0)
			{
				root.Up = new Node(tile, direction, visited, root);
			}

			if (direction.CompareTo(MoveEnum.Left) == 0)
			{
				root.Left = new Node(tile, direction, visited, root);
			}

			if (direction.CompareTo(MoveEnum.Down) == 0)
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

	public List<string> FindShortestPath(Node root, Node current, string tile)
	{
		List<string> result = new List<string>();

		List<string> currentPath = new List<string>();
		TraserveInverse(current, ref currentPath);

		currentPath.Reverse();

		if (currentPath.Count > 3)
		{
			List<string> path = new List<string>();
			List<List<string>> pathAcc = new List<List<string>>();
			GetPaths(root, path, ref pathAcc);
			PrintPaths(pathAcc);

			pathAcc.RemoveAll(x => !x.Contains(tile));

			Dictionary<string, int> counts = GetPathsCounts(pathAcc, currentPath, tile);

			List<KeyValuePair<string, int>> listSorted = counts.ToList();
			listSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

			path = listSorted.First().Key.Split(',').ToList();
			//string currentPathStr = String.Join(",", currentPath);

			//Console.WriteLine("path: " + string.Join(",", path));
			//Console.WriteLine("currentPath: " + string.Join(",", currentPath));

			result = RemoveSimilar(currentPath, path, tile);

			List<string> r = string.Join(",", result).Split(',').ToList();

			r.RemoveAll(x => string.Compare(x, TileEnum.S.ToString()) == 0
					|| string.Compare(x, TileEnum.C.ToString()) == 0
					|| string.Compare(x, TileEnum.E.ToString()) == 0
					|| string.Compare(x, TileEnum.o.ToString()) == 0
					|| string.Compare(x, TileEnum.x.ToString()) == 0);

			result = r;
		}

		return result;
	}

	public void Move(string move)
    {
		if (string.Compare(move, MoveEnum.Right.ToString()) == 0)
		{
			Current = Current.Right;
		}

		if (string.Compare(move, MoveEnum.Up.ToString()) == 0)
		{
			Current = Current.Up;
		}

		if (string.Compare(move, MoveEnum.Left.ToString()) == 0)
		{
			Current = Current.Left;
		}

		if (string.Compare(move, MoveEnum.Down.ToString()) == 0)
		{
			Current = Current.Down;
		}

		if (string.Compare(move, MoveEnum.Parent.ToString()) == 0)
        {
			Current = Current.Parent;
        }
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

		//Console.WriteLine("string.Join currentPath): " + string.Join(",", currentPath));
		//Console.WriteLine("string.Join splitPath): " + string.Join(",", path));

		if (string.Compare(string.Join(",", currentPath), string.Join(",", path)) == 0)
		{
			currentPath.Reverse();
			splitCurrentPath = string.Join(",", currentPath).ToCharArray();
			for (int i = 0; i < splitCurrentPath.Length; i++)
			{
				currentPathResult += splitCurrentPath[i];
				if (splitCurrentPath[i] == Convert.ToChar(tile))
					break;
			}

			currentPathResult = ConvertToParent(currentPathResult);
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

				currentPathResult = ConvertToParent(currentPathResult);
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

	private string ConvertToParent(string path)
	{
		string result = string.Empty;
		string[] joinedString = path.Split(',');

		foreach (string sValue in joinedString)
		{
			string toAdd = sValue;

			if (string.Compare(sValue, MoveEnum.Right.ToString()) == 0
			   || string.Compare(sValue, MoveEnum.Up.ToString()) == 0
			   || string.Compare(sValue, MoveEnum.Left.ToString()) == 0
			   || string.Compare(sValue, MoveEnum.Down.ToString()) == 0)
				toAdd = MoveEnum.Parent.ToString();

			result += toAdd;
			result += ",";
		}

		if (string.Compare(result.Substring(result.Length - 1, 1), ",") == 0)
			result = result.Remove(result.Length - 1, 1);

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
		
	List<string> shtPath = bst.FindShortestPath(root, current, "C");
		
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

	List<string> shtPath = bst.FindShortestPath(root, current, "C");

	Console.WriteLine("sth path: " + String.Join(",", shtPath));
	*/
//}