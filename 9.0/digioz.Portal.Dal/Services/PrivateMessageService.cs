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

            // Find the root message ID by traversing up
            int rootId = messageId;
            var parentId = current.ParentId;
            
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

            // Now collect all messages in the thread starting from the root
            // We'll query iteratively by parent IDs to avoid loading all messages
            var messagesToProcess = new Queue<int>();
            messagesToProcess.Enqueue(rootId);
            var processedIds = new HashSet<int>();

            while (messagesToProcess.Count > 0)
            {
                var currentId = messagesToProcess.Dequeue();
                
                if (processedIds.Contains(currentId))
                {
                    continue;
                }
                
                processedIds.Add(currentId);

                // Get the current message and its direct children
                var currentMessage = _context.PrivateMessages.AsNoTracking()
                    .FirstOrDefault(m => m.Id == currentId);
                    
                if (currentMessage != null)
                {
                    thread.Add(currentMessage);
                    
                    // Find all direct children of this message
                    var childIds = _context.PrivateMessages.AsNoTracking()
                        .Where(m => m.ParentId == currentId)
                        .Select(m => m.Id)
                        .ToList();
                    
                    foreach (var childId in childIds)
                    {
                        messagesToProcess.Enqueue(childId);
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

        public void Delete(int id, string userId)
        {
            var pm = _context.PrivateMessages.Find(id);
            if (pm == null) return;
            if (pm.FromId != userId && pm.ToId != userId) return; // not authorized
            _context.PrivateMessages.Remove(pm);
            _context.SaveChanges();
        }
    }
}
