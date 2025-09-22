using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var data = new List<string>() { "A", "B", "C", "D", "E" };
        var tree = new MerkleTree(data);

        tree.Print();
    }
}

public enum NodeType
{
    Left, Right, Root
}

public class MerkleNode : IEquatable<MerkleNode>
{
    public string Hash { get; set; }
    public MerkleNode? Parent { get; set; }
    public MerkleNode? Left { get; set; }
    public MerkleNode? Right { get; set; }
    public NodeType Type { get; set; }

    public MerkleNode(string hash, MerkleNode? parent, MerkleNode? left, MerkleNode? right, NodeType type = default)
    {
        Hash = hash;
        Parent = parent;
        Left = left;
        Right = right;
        Type = type;
    }

    public bool Equals(MerkleNode? other) =>
        other is not null &&
        Hash == other.Hash &&
        Left?.Hash == other.Left?.Hash &&
        Right?.Hash == other.Right?.Hash &&
        Parent?.Hash == other.Parent?.Hash;

    public override bool Equals(object? obj) => obj is not null && obj is MerkleNode node && Equals(node);

    public override int GetHashCode() => Hash.GetHashCode();

    public static bool operator ==(MerkleNode? l, MerkleNode? r) => l is not null && l.Equals(r);

    public static bool operator !=(MerkleNode? l, MerkleNode? r) => !(l == r);
}

class MerkleTree
{
    private readonly MerkleNode _root;
    private readonly List<MerkleNode> _leaves;

    public MerkleTree(List<string> data, Func<string, string>? hasher = null)
    {
        _leaves = new List<MerkleNode>();

        hasher ??= Hash;

        foreach (var item in data)
        {
            var leaf = new MerkleNode(hasher(item), null, null, null);
            _leaves.Add(leaf);
        }

        var nodes = _leaves.ToList();

        var start = 0;
        var end = nodes.Count - 1;
        var mid = (start + end) / 2;

        var left = Build(nodes, start, mid, hasher);
        var right = Build(nodes, mid + 1, end, hasher);

        _root = new MerkleNode(hasher(left.Hash + right.Hash), null, left, right, NodeType.Root);
        left.Parent = _root;
        left.Type = NodeType.Left;

        right.Parent = _root;
        right.Type = NodeType.Right;
    }

    private MerkleNode Build(List<MerkleNode> nodes, int start, int end, Func<string, string> hasher)
    {
        if (start - end == 0)
            return new MerkleNode(nodes[start].Hash, null, null, null, NodeType.Left);

        if (start - end == 1)
            return new MerkleNode(hasher(nodes[start].Hash + nodes[end].Hash), null, nodes[start], nodes[end]);

        var mid = (start + end) / 2;

        var left = Build(nodes, start, mid, hasher);
        var right = Build(nodes, mid + 1, end, hasher);

        var parent = new MerkleNode(hasher(left.Hash + right.Hash), null, left, right);
        left.Parent = parent;
        left.Type = NodeType.Left;

        right.Parent = parent;
        right.Type = NodeType.Right;

        return parent;
    }

    public void Print()
    {
        var stack = new Stack<MerkleNode>();
        stack.Push(_root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            var level = 0;

            var tmp = node;
            var fills = new List<string>();
            while (tmp?.Parent is not null)
            {
                tmp = tmp.Parent;

                if (tmp is not null && tmp.Type != NodeType.Root)
                    if (tmp.Type == NodeType.Right)
                        fills.Add("|  ");
                    else
                        fills.Add("   ");

                level++;
            }

            Console.Write(string.Join(string.Empty, fills.Reverse<string>()));

            if (node.Parent is not null)
                if (node.Parent.Left == node)
                    Console.Write("L__");
                else
                    Console.Write("|--");

            Console.WriteLine(node.Hash);

            if (node.Left is not null)
                stack.Push(node.Left);

            if (node.Right is not null)
                stack.Push(node.Right);
        }
    }

    private string Hash(string data) => data;
}