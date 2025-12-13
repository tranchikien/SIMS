using SIMS.Models;
using SIMS.Repositories;
using System;

namespace SIMS.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public void NotifyGradeAdded(int studentId, int courseId, int gradeId, string courseName, string facultyName)
        {
            var notification = new Notification
            {
                NotificationType = "GradeAdded",
                Title = "New Grade Added",
                Message = $"Your grade for {courseName} has been added by {facultyName}.",
                RecipientRole = "Student",
                RecipientId = studentId,
                RelatedStudentId = studentId,
                RelatedCourseId = courseId,
                RelatedGradeId = gradeId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _notificationRepository.Add(notification);
        }

        public void NotifyFacultyAssigned(int studentId, int courseId, int enrollmentId, string facultyName, string courseName)
        {
            var notification = new Notification
            {
                NotificationType = "FacultyAssigned",
                Title = "Faculty Assigned",
                Message = $"You have been assigned to {courseName} by {facultyName}.",
                RecipientRole = "Student",
                RecipientId = studentId,
                RelatedStudentId = studentId,
                RelatedCourseId = courseId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _notificationRepository.Add(notification);
        }
    }
}

