using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Dal.Services
{
    public class PrivateMessageService : IPrivateMessageService
    {
        private readonly digiozPortalContext _context;

        public PrivateMessageService(digiozPortalContext context)
        {
            _context = context;
        }

        public PrivateMessage Get(int id) => _context.PrivateMessages.Find(id);

        public List<PrivateMessage> GetInbox(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.ToId == userId)
                .OrderByDescending(pm => pm.SentDate)
                .ToList();
        }

        public List<PrivateMessage> GetOutbox(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.FromId == userId && !pm.IsRead)
                .OrderByDescending(pm => pm.SentDate)
                .ToList();
        }

        public List<PrivateMessage> GetSent(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.FromId == userId && pm.IsRead)
                .OrderByDescending(pm => pm.SentDate)
                .ToList();
        }

        public List<PrivateMessage> GetThread(int messageId)
        {
            var thread = new List<PrivateMessage>();
            var current = _context.PrivateMessages.AsNoTracking().FirstOrDefault(p => p.Id == messageId);

            if (current == null)
            {
                return thread;
            }

            // Traverse up to find the root of the thread
            while (current.ParentId != null)
            {
                current = _context.PrivateMessages.AsNoTracking().FirstOrDefault(p => p.Id == current.ParentId);
                if (current == null) return new List<PrivateMessage>(); // Should not happen in consistent data
            }

            // Now 'current' is the root. Collect all descendants.
            var messages = _context.PrivateMessages.AsNoTracking().ToList();
            var messageMap = messages.ToLookup(m => m.ParentId);

            var queue = new Queue<PrivateMessage>();
            queue.Enqueue(current);

            while (queue.Count > 0)
            {
                var msg = queue.Dequeue();
                thread.Add(msg);

                if (messageMap.Contains(msg.Id))
                {
                    foreach (var child in messageMap[msg.Id].OrderBy(m => m.SentDate))
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return thread.OrderBy(m => m.SentDate).ToList();
        }


        public void Add(PrivateMessage message)
        {
            message.SentDate = DateTime.UtcNow;
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();
        }

        public void MarkRead(int id)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm == null) return;
            pm.IsRead = true;
            _context.SaveChanges();
        }

        public void MarkReadIfUnread(int id)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm != null && !pm.IsRead)
            {
                pm.IsRead = true;
                _context.SaveChanges();
            }
        }

        public void Delete(int id, string userId)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm == null) return;
            if (pm.FromId != userId && pm.ToId != userId) return; // not authorized
            _context.PrivateMessages.Remove(pm);
            _context.SaveChanges();
        }

        public bool DeleteIfOwnedByUser(int id, string userId)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm == null || (pm.FromId != userId && pm.ToId != userId))
            {
                return false;
            }
            _context.PrivateMessages.Remove(pm);
            _context.SaveChanges();
            return true;
        }
    }
}
