using System;
using System.Collections.Generic;
using MazeClient.Enum;

namespace MazeClient
{
    public class MazeTree
    {
        public class Node
        {
            public string Tile;
            public bool Visited;
            public Node Parent;
            public Node Right;
            public Node Up;
            public Node Left;
            public Node Down;

        }

        private Node Root;
        private readonly MazeTree Tree;

        public MazeTree()
        {
        }

        public Node GetRoot()
        {
            return Root;
        }

        public MazeTree GetMazeTree()
        {
            return Tree;
        }


        public Node Insert(Node node, string[] tiles, bool?[] visited)
        {
            if (Root == null)
            {
                Root = new Node
                {
                    Parent = null
                };

                string tileName = string.Empty;
                if (tiles.Length >= 1 && !string.IsNullOrEmpty(tiles[0]))
                    tileName = tiles[0];
                if (tiles.Length >= 2 && !string.IsNullOrEmpty(tiles[1]))
                    tileName = tiles[1];
                if (tiles.Length >= 3 && !string.IsNullOrEmpty(tiles[2]))
                    tileName = tiles[2];
                if (tiles.Length == 4 && !string.IsNullOrEmpty(tiles[3]))
                    tileName = tiles[3];
                Root.Tile = tileName;

                bool visit = false;
                if (visited.Length >= 1 && visited[0] != null)
                    visit = visited[0].Value;
                if (visited.Length >= 2 && visited[1] != null)
                    visit = visited[1].Value;
                if (visited.Length >= 3 && visited[2] != null)
                    visit = visited[2].Value;
                if (visited.Length == 4 && visited[3] != null)
                    visit = visited[3].Value;
                Root.Visited = visit;
            }
            else
            {
                if (tiles.Length >= 1 && !string.IsNullOrEmpty(tiles[0]) &&
                    visited.Length >= 1 && visited[0] != null)
                {
                    Root.Right = Insert(node.Right, new string[] { tiles[0] },
                        new bool?[] { visited[0] });
                    Root.Right.Parent = Root;

                }

                if (tiles.Length >= 2 && !string.IsNullOrEmpty(tiles[1]) &&
                    visited.Length >= 2 && visited[1] != null)

                {
                    Root.Up = Insert(node.Up, new string[] { null, tiles[1] },
                        new bool?[] { null, visited[1] });
                    Root.Up.Parent = Root;
                }

                if (tiles.Length >= 3 && !string.IsNullOrEmpty(tiles[2]) &&
                    visited.Length >= 3 && visited[2] != null)
                {
                    Root.Left = Insert(node.Left, new string[] { null, null,
                        tiles[2] },
                        new bool?[] { null, null, visited[2] });

                    Root.Left.Parent = Root;
                }

                if (tiles.Length == 4 && !string.IsNullOrEmpty(tiles[3]) &&
                    visited.Length == 4 && visited[3] != null)
                {
                    Root.Down = Insert(node.Down, new string[] { null, null,
                        null, tiles[3] },
                        new bool?[] { null, null, null, visited[3] });

                    Root.Down.Parent = Root;
                }
            }

            return Root;
        }

        public void Traverse(Node root)
        {
            if (root == null)
            {
                return;
            }

            Console.WriteLine(string.Format("{0} - {1}", root.Tile, root.Visited));

            if (root.Right != null)
                Console.WriteLine(MoveEnum.Right.ToString());

            Traverse(root.Right);

            if (root.Up != null)
                Console.WriteLine(MoveEnum.Up.ToString());

            Traverse(root.Up);

            if (root.Left != null)
                Console.WriteLine(MoveEnum.Left.ToString());

            Traverse(root.Left);

            if (root.Down != null)
                Console.WriteLine(MoveEnum.Down.ToString());

            Traverse(root.Down);
        }

        public void TraserveInverse(Node leaf)
        {
            if (leaf == null)
            {
                return;
            }

            Console.WriteLine(string.Format("{0} - {1}", leaf.Tile, leaf.Visited));

            TraserveInverse(leaf.Parent);
        }

        public void AllNotVisitedNodes(Node leaf, ref List<Node> notVisited)
        {
            if (leaf == null)
            {
                return;
            }

            if (!leaf.Visited)
            {
                notVisited.Add(leaf);
            }

            AllNotVisitedNodes(leaf.Parent, ref notVisited);
        }

        public Node Move(Node root, string move, MoveEnum direction)
        {
            Node result = null;

            if (direction.CompareTo(MoveEnum.Down) == 0)
            {
                if (string.Compare(move, MoveEnum.Right.ToString()) == 0)
                {
                    result = root.Right;
                }
                else if (string.Compare(move, MoveEnum.Up.ToString()) == 0)
                {
                    result = root.Up;
                }
                else if (string.Compare(move, MoveEnum.Left.ToString()) == 0)
                {
                    result = root.Left;
                }
                else 
                {
                    result = root.Down;
                }
            }
            else
            {
                if (root.Parent != null)
                {
                    if (string.Compare(move, MoveEnum.Right.ToString()) == 0)
                    {
                        result = root.Parent.Right;
                    }
                    else if (string.Compare(move, MoveEnum.Up.ToString()) == 0)
                    {
                        result = root.Parent.Up;
                    }
                    else if (string.Compare(move, MoveEnum.Left.ToString()) == 0)
                    {
                        result = root.Parent.Left;
                    }
                    else
                    {
                        result = root.Parent.Down;
                    }
                }
            }

            return result;
        }

        //public static void Main()
        //{
        //    Node root = null;
        //    Tree bst = new Tree();

        //    Console.WriteLine("Root");

        //    root = bst.Insert(root, new string[] { "S" }, new bool?[] { false });
        //    root = bst.Insert(root, new string[] { "x", "o", "C" }, new bool?[] { false, true, false });
        //    root.Right = bst.Insert(root.Right, new string[] { null, "R" }, new bool?[] { null, true });

        //    //Console.WriteLine(root.Tile);
        //    //Console.WriteLine(root.Up.Tile);
        //    //Console.WriteLine(root.Right.Up.Tile);

        //    //bst.Traverse(root);
        //    Console.WriteLine(" ");

        //    bst.TraserveInverse(root.Right.Up);

        //    Console.WriteLine("Hello World");
        //}
    }
}

