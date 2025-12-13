using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        IEnumerable<Notification> GetByRecipient(int? recipientId, string recipientRole);
        IEnumerable<Notification> GetUnreadByRecipient(int? recipientId, string recipientRole);
        void MarkAsRead(int notificationId);
        void MarkAllAsRead(int? recipientId, string recipientRole);
        Notification? GetById(int id);
    }
}

