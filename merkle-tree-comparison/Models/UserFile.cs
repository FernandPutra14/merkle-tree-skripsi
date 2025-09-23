namespace merkle_tree_comparison.Models;

public class UserFile
{
    public int Id { get; set; }
    public required Uri FilePath { get; set; }
    public required byte[] FileHash { get; set; }
    public required int Version { get; set; }

    public User User { get; set; }
}
