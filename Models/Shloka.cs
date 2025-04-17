using System.Collections.Generic;

public class Shloka
{
    public int Id { get; set; }
    public string? ChapterName { get; set; }
    public int TotalShlokas { get; set; }
    public int ShlokaNumber { get; set; }
    public string? ShlokaText { get; set; }
    public string? Sanskrit { get; set; }
    public string? Gujrati { get; set; }
    public string? Hindi { get; set; }
    public bool IsLiked { get; set; }
}
