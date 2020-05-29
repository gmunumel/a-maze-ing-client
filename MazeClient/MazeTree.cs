using System;
using System.Collections.Generic;

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

        public Node Insert(Node root, string[] tiles, bool?[] visited)
        {
            if (root == null)
            {
                root = new Node();
                root.Parent = null;

                string tileName = string.Empty;
                if (tiles.Length >= 1 && !string.IsNullOrEmpty(tiles[0]))
                    tileName = tiles[0];
                if (tiles.Length >= 2 && !string.IsNullOrEmpty(tiles[1]))
                    tileName = tiles[1];
                if (tiles.Length >= 3 && !string.IsNullOrEmpty(tiles[2]))
                    tileName = tiles[2];
                if (tiles.Length == 4 && !string.IsNullOrEmpty(tiles[3]))
                    tileName = tiles[3];
                root.Tile = tileName;

                bool visit = false;
                if (visited.Length >= 1 && visited[0] != null)
                    visit = visited[0].Value;
                if (visited.Length >= 2 && visited[1] != null)
                    visit = visited[1].Value;
                if (visited.Length >= 3 && visited[2] != null)
                    visit = visited[2].Value;
                if (visited.Length == 4 && visited[3] != null)
                    visit = visited[3].Value;
                root.Visited = visit;
            }
            else
            {
                if (tiles.Length >= 1 && !string.IsNullOrEmpty(tiles[0]) &&
                    visited.Length >= 1 && visited[0] != null)
                {
                    root.Right = Insert(root.Right, new string[] { tiles[0] }, new bool?[] { visited[0] });
                    root.Right.Parent = root;

                }

                if (tiles.Length >= 2 && !string.IsNullOrEmpty(tiles[1]) &&
                    visited.Length >= 2 && visited[1] != null)

                {
                    root.Up = Insert(root.Up, new string[] { null, tiles[1] }, new bool?[] { null, visited[1] });
                    root.Up.Parent = root;
                }

                if (tiles.Length >= 3 && !string.IsNullOrEmpty(tiles[2]) &&
                    visited.Length >= 3 && visited[2] != null)
                {
                    root.Left = Insert(root.Left, new string[] { null, null, tiles[2] },
                        new bool?[] { null, null, visited[2] });

                    root.Left.Parent = root;
                }

                if (tiles.Length == 4 && !string.IsNullOrEmpty(tiles[3]) &&
                    visited.Length == 4 && visited[3] != null)
                {
                    root.Down = Insert(root.Down, new string[] { null, null, null, tiles[3] },
                        new bool?[] { null, null, null, visited[3] });

                    root.Down.Parent = root;
                }
            }

            return root;
        }

        public void Traverse(Node root)
        {
            if (root == null)
            {
                return;
            }

            Console.WriteLine(string.Format("{0} - {1}", root.Tile, root.Visited));

            if (root.Right != null)
                Console.WriteLine("Right");

            Traverse(root.Right);

            if (root.Up != null)
                Console.WriteLine("Up");

            Traverse(root.Up);

            if (root.Left != null)
                Console.WriteLine("Left");

            Traverse(root.Left);

            if (root.Down != null)
                Console.WriteLine("Down");

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

