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
            
            // Get only the ParentId to start traversal
            var messageInfo = _context.PrivateMessages.AsNoTracking()
                .Where(p => p.Id == messageId)
                .Select(p => new { p.Id, p.ParentId })
                .FirstOrDefault();

            if (messageInfo == null)
            {
                return thread;
            }

            // Find the root message ID by traversing up
            int rootId = messageInfo.Id;
            var parentId = messageInfo.ParentId;
            
            while (parentId != null)
            {
                var parent = _context.PrivateMessages.AsNoTracking()
                    .Where(p => p.Id == parentId.Value)
                    .Select(p => new { p.Id, p.ParentId })
                    .FirstOrDefault();
                    
                if (parent == null)
                {
                    break; // Should not happen in consistent data
                }
                
                rootId = parent.Id;
                parentId = parent.ParentId;
            }

            // Collect all messages in the thread by querying level by level
            var allMessages = new List<PrivateMessage>();
            var currentLevelIds = new List<int> { rootId };

            while (currentLevelIds.Any())
            {
                // Fetch all messages at the current level
                var currentLevelMessages = _context.PrivateMessages.AsNoTracking()
                    .Where(m => currentLevelIds.Contains(m.Id))
                    .ToList();
                
                allMessages.AddRange(currentLevelMessages);
                
                // Get IDs of all children for the next level
                var currentLevelMessageIds = currentLevelMessages.Select(cm => cm.Id).ToList();
                currentLevelIds = _context.PrivateMessages.AsNoTracking()
                    .Where(m => m.ParentId.HasValue && currentLevelMessageIds.Contains(m.ParentId.Value))
                    .Select(m => m.Id)
                    .ToList();
            }

            return allMessages.OrderBy(m => m.SentDate).ToList();
        }

        public int GetUnreadCount(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Count(pm => pm.ToId == userId && !pm.IsRead);
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

            // Nullify ParentId on child messages to avoid FK Restrict violation
            var children = _context.PrivateMessages.Where(m => m.ParentId == id).ToList();
            foreach (var child in children)
            {
                child.ParentId = null;
            }

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

            // Nullify ParentId on child messages to avoid FK Restrict violation
            var children = _context.PrivateMessages.Where(m => m.ParentId == id).ToList();
            foreach (var child in children)
            {
                child.ParentId = null;
            }

            _context.PrivateMessages.Remove(pm);
            _context.SaveChanges();
            return true;
        }

        public int CountByUserId(string userId)
        {
            return _context.PrivateMessages
                .AsNoTracking()
                .Count(pm => pm.FromId == userId || pm.ToId == userId);
        }

        public void DeleteByUserId(string userId)
        {
            var messages = _context.PrivateMessages
                .Where(pm => pm.FromId == userId || pm.ToId == userId)
                .ToList();

            if (!messages.Any()) return;

            var messageIds = messages.Select(m => m.Id).ToHashSet();

            // Nullify ParentId on any messages that reference messages being deleted
            var childrenToUpdate = _context.PrivateMessages
                .Where(m => m.ParentId.HasValue && messageIds.Contains(m.ParentId.Value))
                .ToList();
            foreach (var child in childrenToUpdate)
            {
                child.ParentId = null;
            }

            _context.PrivateMessages.RemoveRange(messages);
            _context.SaveChanges();
        }

        public void ReassignByUserId(string userId, string newUserId)
        {
            var messages = _context.PrivateMessages
                .Where(pm => pm.FromId == userId || pm.ToId == userId)
                .ToList();

            foreach (var pm in messages)
            {
                if (pm.FromId == userId) pm.FromId = newUserId;
                if (pm.ToId == userId) pm.ToId = newUserId;
            }

            _context.SaveChanges();
        }

        public void Report(int id)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm != null && !pm.Reported)
            {
                pm.Reported = true;
                _context.SaveChanges();
            }
        }

        public void RemoveReport(int id)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm != null && pm.Reported)
            {
                pm.Reported = false;
                _context.SaveChanges();
            }
        }

        public List<PrivateMessage> GetReported(int page, int pageSize, out int totalCount)
        {
            var query = _context.PrivateMessages
                .AsNoTracking()
                .Where(pm => pm.Reported)
                .OrderByDescending(pm => pm.SentDate);

            totalCount = query.Count();

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
