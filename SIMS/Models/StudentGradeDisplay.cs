namespace SIMS.Models
{
    public class StudentGradeDisplay
    {
        public string CourseName { get; set; } = string.Empty;
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
        public string? Comment { get; set; }
    }
}

