using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var data = new List<string>() { "A", "B", "C", "D" };

        var tree = new MerkleTree(data.Select(a => Encoding.UTF8.GetBytes(a)).ToList());

        tree.Print();
    }
}

public enum NodeType
{
    Left, Right, Root
}

public class MerkleNode : IEquatable<MerkleNode>
{
    public string HexHash => string.Join("", Hash.Select(v => $"{v:x2}"));

    public byte[] Hash { get; set; }
    public MerkleNode? Parent { get; set; }
    public MerkleNode? Left { get; set; }
    public MerkleNode? Right { get; set; }
    public NodeType Type { get; set; }

    public MerkleNode(byte[] hash, MerkleNode? parent, MerkleNode? left, MerkleNode? right, NodeType type = default)
    {
        Hash = hash;
        Parent = parent;
        Left = left;
        Right = right;
        Type = type;
    }

    public bool Equals(MerkleNode? other) =>
        other is not null &&
        HexHash == other.HexHash &&
        Parent?.HexHash == other.Parent?.HexHash &&
        Left?.HexHash == other.Left?.HexHash &&
        Right?.HexHash == other.Right?.HexHash;

    public override bool Equals(object? obj) => obj is not null && obj is MerkleNode node && Equals(node);

    public override int GetHashCode() => Hash.Aggregate(default(int), HashCode.Combine);

    public static bool operator ==(MerkleNode? l, MerkleNode? r) => l is not null && l.Equals(r);

    public static bool operator !=(MerkleNode? l, MerkleNode? r) => !(l == r);
}

class MerkleTree
{
    public  readonly MerkleNode Root;

    private readonly List<MerkleNode> _leaves;
    private readonly Func<byte[], byte[]> _hasher;

    public MerkleTree(List<byte[]> data, Func<byte[], byte[]>? hasher = null)
    {
        _leaves = new List<MerkleNode>();

        _hasher ??= SHA256.HashData;

        foreach (var item in data)
        {
            var leaf = new MerkleNode(_hasher(item), null, null, null);
            _leaves.Add(leaf);
        }

        var nodes = _leaves.ToList();

        var start = 0;
        var end = nodes.Count - 1;
        var mid = (start + end) / 2;

        var left = Build(nodes, start, mid);
        var right = Build(nodes, mid + 1, end);

        Root = new MerkleNode(_hasher(left.Hash.Concat(right.Hash).ToArray()), null, left, right, NodeType.Root);
        left.Parent = Root;
        left.Type = NodeType.Left;

        right.Parent = Root;
        right.Type = NodeType.Right;
    }

    private MerkleNode Build(List<MerkleNode> nodes, int start, int end)
    {
        if (start - end == 0)
            return new MerkleNode(nodes[start].Hash, null, null, null, NodeType.Left);

        if (start - end == 1)
            return new MerkleNode(_hasher(nodes[start].Hash.Concat(nodes[end].Hash).ToArray()), null, nodes[start], nodes[end]);

        var mid = (start + end) / 2;

        var left = Build(nodes, start, mid);
        var right = Build(nodes, mid + 1, end);

        var parent = new MerkleNode(_hasher(left.Hash.Concat(right.Hash).ToArray()), null, left, right);
        left.Parent = parent;
        left.Type = NodeType.Left;

        right.Parent = parent;
        right.Type = NodeType.Right;

        return parent;
    }

    public void Print()
    {
        var stack = new Stack<MerkleNode>();
        stack.Push(Root);

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

            Console.WriteLine(node.HexHash);

            if (node.Left is not null)
                stack.Push(node.Left);

            if (node.Right is not null)
                stack.Push(node.Right);
        }
    }
}