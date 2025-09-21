using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var data = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H" };
        var tree = new MerkleTree(data, s => s.GetHashCode().ToString());

        tree.Print();
    }
}

public class MerkleNode : IEquatable<MerkleNode>
{
    public string Hash { get; set; }
    public MerkleNode? Parent { get; set; }
    public MerkleNode? Left { get; set; }
    public MerkleNode? Right { get; set; }

    public MerkleNode(string hash, MerkleNode? parent, MerkleNode? left, MerkleNode? right)
    {
        Hash = hash;
        Parent = parent;
        Left = left;
        Right = right;
    }

    public static readonly MerkleNode Empty = new MerkleNode(string.Empty, null, null, null);

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
        var orderedData = data.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        var nodes = new Queue<MerkleNode>();
        _leaves = new List<MerkleNode>();

        hasher ??= Hash;

        foreach (var item in orderedData)
        {
            var leaf = new MerkleNode(hasher(item), null, null, null);
            _leaves.Add(leaf);
            nodes.Enqueue(leaf);
        }

        while (nodes.Count > 1)
        {
            var left = nodes.Dequeue();
            var right = nodes.Dequeue();

            var parent = new MerkleNode(hasher(left.Hash + right.Hash), null, left, right);

            left.Parent = parent;
            right.Parent = parent;

            nodes.Enqueue(parent);
        }

        _root = nodes.Dequeue();
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
            while (tmp?.Parent is not null)
            {
                tmp = tmp.Parent;
                level++;
            }

            for (int i = 1; i < level; i++)
                Console.Write("|  ");

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

    private string Hash(string data) => Encoding.UTF8.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
}