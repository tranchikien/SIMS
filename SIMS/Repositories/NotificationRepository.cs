using SIMS.Data;
using SIMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SIMSDbContext _context;

        public NotificationRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public void Add(Notification notification)
        {
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public IEnumerable<Notification> GetByRecipient(int? recipientId, string recipientRole)
        {
            var query = _context.Notifications
                .Where(n => n.RecipientRole == recipientRole);

            if (recipientId.HasValue)
            {
                query = query.Where(n => n.RecipientId == null || n.RecipientId == recipientId.Value);
            }
            else
            {
                query = query.Where(n => n.RecipientId == null);
            }

            return query.OrderByDescending(n => n.CreatedAt).ToList();
        }

        public IEnumerable<Notification> GetUnreadByRecipient(int? recipientId, string recipientRole)
        {
            var query = _context.Notifications
                .Where(n => n.RecipientRole == recipientRole && !n.IsRead);

            if (recipientId.HasValue)
            {
                query = query.Where(n => n.RecipientId == null || n.RecipientId == recipientId.Value);
            }
            else
            {
                query = query.Where(n => n.RecipientId == null);
            }

            return query.OrderByDescending(n => n.CreatedAt).ToList();
        }

        public void MarkAsRead(int notificationId)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }
        }

        public void MarkAllAsRead(int? recipientId, string recipientRole)
        {
            var query = _context.Notifications
                .Where(n => n.RecipientRole == recipientRole && !n.IsRead);

            if (recipientId.HasValue)
            {
                query = query.Where(n => n.RecipientId == null || n.RecipientId == recipientId.Value);
            }
            else
            {
                query = query.Where(n => n.RecipientId == null);
            }

            foreach (var notification in query)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();
        }

        public Notification? GetById(int id)
        {
            return _context.Notifications.FirstOrDefault(n => n.Id == id);
        }
    }
}

