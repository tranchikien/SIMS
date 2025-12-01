namespace SIMS.Models
{
    public class StudentGradeDisplay
    {
        public string CourseName { get; set; } = string.Empty;
<<<<<<< HEAD
        public decimal? FinalScore { get; set; }
=======
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? AssignmentScore { get; set; }
>>>>>>> ee194c07c122bf48106af85d3475a24fce023e6c
        public decimal? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
        public string? Comment { get; set; }
    }
}

