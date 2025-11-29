using SIMS.Models;

namespace SIMS.Services
{
    public interface IGradeService
    {
        void SaveGrades(GradeViewModel model, int facultyId);
        string CalculateLetterGrade(decimal score);
    }
}

