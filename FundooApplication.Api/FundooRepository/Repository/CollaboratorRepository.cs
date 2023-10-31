using FundooModel.Notes;
using FundooRepository.Context;
using FundooRepository.IRepository;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NlogImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundooRepository.Repository
{
    public class CollaboratorRepository : ICollaboratorRepository
    {
        public readonly UserDbContext context;
        public readonly IDistributedCache distributedCache;
        NlogOperation nlog = new NlogOperation();
        public CollaboratorRepository(UserDbContext context, IDistributedCache distributedCache)
        {
            this.context = context;
            this.distributedCache = distributedCache;
        }

        public Task<int> AddCollaborator(Collaborator collaborator)
        {
            this.context.Collaborator.Add(collaborator);
            var result = this.context.SaveChangesAsync();
            nlog.LogInfo("Collaborator added");
            return result;
        }

        public bool DeleteCollab(int noteId, int receiverId)
        {
            var result = this.context.Collaborator.Where(x => x.NoteId == noteId && x.ReceiverUserId == receiverId).FirstOrDefault();
            if (result != null)
            {



                this.context.Collaborator.Remove(result);
                var deleteResult = this.context.SaveChanges();
                if (deleteResult == 1)
                {
                    nlog.LogInfo("Collaborator deleted added");
                    return true;
                }
            }
            nlog.LogWarn("Collaborator not found");
            return false;
        }

        public IEnumerable<Collaborator> GetAllCollabNotes(int userId)
        {
            var result = this.context.Collaborator.Where(x => x.SenderUserId == userId || x.ReceiverUserId == userId).AsEnumerable();
            if (result != null)
            {
                nlog.LogInfo("Got all notes with Collabrator added");
                this.PutListToCache(userId);
                return result;
            }
            var data = this.GetListFromCache("Collablist");
            nlog.LogWarn("Collaborator with notes not found");
            return null;          
        }

        public IEnumerable<NotesCollab> GetAllNotesColllab(int userId)
        {
            List<NotesCollab> collab = new List<NotesCollab>();
            var result = this.context.Notes.Join(this.context.Collaborator.Where(X=> X.SenderUserId ==  userId),
                Note => Note.NoteId,
                Collaborator => Collaborator.NoteId,
                (Note, Collaborator) => new NotesCollab
                {
                    NoteId = Note.NoteId,
                    Title = Note.Title,
                    Description = Note.Description,
                    Image = Note.Image,
                    Colour = Note.Color,
                    Reminder = Note.Remainder,
                    IsArchive = Note.IsArchive,
                    IsPin = Note.IsPin,
                    IsTrash = Note.IsTrash,
                    CreatedDate = Note.CreatedDate,
                    CollabId = Collaborator.CollabId,
                    SenderUserId =Collaborator.SenderUserId,
                    ReceiverUserId = Collaborator.ReceiverUserId                    
                });
            foreach (var data in result)
            {
                if (data.SenderUserId == userId || data.ReceiverUserId == userId)
                {
                    collab.Add(data);
                }
            }
            nlog.LogInfo("Dispalyed All Collab Notes Successfully");
            return result;
        }
        public void PutListToCache(int userid)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
            var enlist = this.context.Collaborator.Where(x => x.SenderUserId == userid || x.ReceiverUserId == userid);
            var jsonstring = JsonConvert.SerializeObject(enlist);
            distributedCache.SetString("Collablist", jsonstring, options);
        }
        public List<Note> GetListFromCache(string key)
        {
            var CacheString = this.distributedCache.GetString(key);
            return JsonConvert.DeserializeObject<IEnumerable<Note>>(CacheString).ToList();
        }

    }
}
