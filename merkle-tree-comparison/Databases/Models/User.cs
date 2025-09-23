namespace merkle_tree_comparison.Models;

public class User
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public byte[] RootHash { get; set; } = [];

    public List<UserFile> UserFiles { get; set; }
}
