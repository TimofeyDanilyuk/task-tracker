namespace backend.Models;

public enum BoardRole { Member, Admin }

public class BoardMember
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public BoardRole Role { get; set; } = BoardRole.Member;
}