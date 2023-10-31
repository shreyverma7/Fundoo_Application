﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FundooModel.Notes;
using FundooRepository.Context;
using FundooRepository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NlogImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace FundooRepo.Repository
{
    public class NotesRepository : INotesRepository
    {
        public readonly UserDbContext context;
        NlogOperation nlog = new NlogOperation();
        private readonly IDistributedCache distributedCache;
        public NotesRepository(UserDbContext context, IDistributedCache distributedCache)
        {
            this.context = context;
            this.distributedCache = distributedCache;
        }
        public Task<int> AddNotes(Note note)
        {
            this.context.Notes.Add(note);
            var result = this.context.SaveChangesAsync();
            nlog.LogInfo("Added Note successfully");
            return result;
        }
        public Note EditNotes(Note note)
        {
            var data = this.context.Notes.Where(x => x.NoteId == note.NoteId).FirstOrDefault();
            if (data != null)
            {
                data.Id = note.Id;
                //    data.EmailId = note.EmailId;
                data.Remainder = note.Remainder;
                data.Title = note.Title;
                data.Color = note.Color;
                data.ModifiedDate = note.ModifiedDate;
                data.CreatedDate = note.CreatedDate;
                data.Description = note.Description;
                data.Image = note.Image;
                data.IsArchive = note.IsArchive;
                data.IsPin = note.IsPin;
                data.IsTrash = note.IsTrash;
                this.context.Notes.Update(data);
                this.context.SaveChangesAsync();
                nlog.LogInfo("Edited successfully");
                return note;
            }
            nlog.LogWarn("Note id or Note Emial did not found!");
            return null;
        }
        public IEnumerable<Note> GetAllNotes(int id)
        {
            
            var result = this.context.Notes.Where(x => x.Id == id).AsEnumerable();
            if (result != null)
            {
                //PUSH THE data to  cache
                this.PutListToCache(id);
                return result;
            }
            var data = this.GetListFromCache("noteList");
            return null;
        }
        public Note GetNoteById(int userId, int noteId)
        {
            var data = this.GetListFromCache("noteList");
            if (data != null)
            {
                nlog.LogInfo("[cache] GetNotes by id successfull");
                return data.Where(x => x.Id == userId && x.NoteId == noteId).FirstOrDefault();
            }
            else
            {
                nlog.LogInfo("GetNotes by id successfull");
                var result = this.context.Notes.Where(x => x.Id == userId && x.NoteId == noteId).FirstOrDefault();
                return result;
            }
        }
        public bool DeleteNote(int noteid, int userId)
        {
            var result = this.context.Notes.Where(x => x.NoteId == noteid && x.Id == userId).FirstOrDefault();
            if (result != null)
            {
                result.IsTrash = true;
                this.context.Notes.Update(result);
                var deleteResult = this.context.SaveChanges();
                if (deleteResult == 1)
                {
                    nlog.LogInfo("Added in trash successfully");
                    return true;
                }
                nlog.LogWarn("Adding to trash unsuccessful");
                return false;
            }
            nlog.LogWarn("Empty value sent");
            return false;
        }
        public IEnumerable<Note> GetThrashedTask(int userId)
        {
            var data = this.GetListFromCache("noteList");
            if (data != null)
            {
                nlog.LogInfo("[cache] GetThrashedTask by id successfull");
                return data.Where(x => x.Id == userId && x.IsTrash == true).AsEnumerable();
            }
            else
            {
                nlog.LogInfo("GetThrashedTask by id successfull");
                var result = this.context.Notes.Where(x => x.Id == userId && x.IsTrash == true).AsEnumerable();
                return result;
            }
        }
        public bool TrashNote(int userId)
        {
            var result = this.context.Notes.Where(x => x.Id == userId && x.IsTrash == true).ToList();
            foreach (var data in result)
            {
                nlog.LogInfo("Task deleted successfully");
                this.context.Notes.Remove(data);
            }
            var deleteResult = this.context.SaveChanges();
            if (deleteResult == 0)
            {
                nlog.LogWarn(" No task to Delete");
                return false;
            }
            nlog.LogWarn(" No task to Delete");
            return false;
        }
        public Note PinNote(int noteId, int userId)
        {
            var result = this.context.Notes.Where(x => x.NoteId == noteId && x.Id == userId).FirstOrDefault();
            if (result != null)
            {
                result.IsPin = true;
                this.context.Notes.Update(result);
                this.context.SaveChangesAsync();
                nlog.LogInfo("Task pinned successfully");
                return result;
            }
            nlog.LogWarn("No Pinned note found");
            return null;
        }
        public IEnumerable<Note> GetArcheived(int userId)
        {
           
            var data = this.GetListFromCache("noteList");
            if (data != null)
            {
                nlog.LogInfo("[cache] GetArcheived by id successfull");
                return data.Where(x => x.Id == userId && x.IsArchive == true).AsEnumerable();
            }
            else
            {
                nlog.LogInfo("GetArcheived by id successfull");
                var result = this.context.Notes.Where(x => x.Id == userId && x.IsArchive == true).AsEnumerable();
                return result;
            }
            
        }
        public Note ArcheiveNote(int noteId, int userId)
        {
            var result = this.context.Notes.Where(x => x.NoteId == noteId && x.Id == userId).FirstOrDefault();
            if (result != null)
            {
                result.IsArchive = true;
                this.context.Notes.Update(result);
                this.context.SaveChangesAsync();
                nlog.LogInfo("Task added archeive successfully");
                return result;
            }
            nlog.LogWarn("Cannot add archeive Notes");
            return null;
        }
        public IEnumerable<Note> GetPinnedTask(int userId)
        {
           
            var data = this.GetListFromCache("noteList");
            if (data != null)
            {
                nlog.LogInfo("[cache] GetPinnedTask by id successfull");
                return data.Where(x => x.Id == userId && x.IsPin == true).AsEnumerable();
            }
            else
            {
                nlog.LogInfo("GetPinnedTask by id successfull");
                var result = this.context.Notes.Where(x => x.Id == userId && x.IsPin == true).AsEnumerable();
                return result;
            }
          
        }
        public bool RestoreNotes(int noteId, int userId)
        {
            var result = this.context.Notes.Where(x => x.NoteId == noteId && x.Id == userId).FirstOrDefault();
            if (result != null)
            {
                result.IsTrash = false;
                this.context.Notes.Update(result);
                var restoreResult = this.context.SaveChanges();
                if (restoreResult != 0)
                {
                    nlog.LogInfo("Restored Notes successful");
                    return true;
                }
            }
            nlog.LogWarn("Restored not found ");
            return false;
        }

        public string Image(IFormFile file, int noteId)

        {

            try

            {

                if (file == null)

                {

                    return null;

                }

                var stream = file.OpenReadStream();

                var name = file.FileName;

                Account account = new Account("dqtdpxlam", "933235679953766", "D_dTx2_TlgeLDvLIw5VSyeI7Fdg");

                Cloudinary cloudinary = new Cloudinary(account);

                var uploadParams = new ImageUploadParams()

                {

                    File = new FileDescription(name, stream)

                };

                ImageUploadResult uploadResult = cloudinary.Upload(uploadParams);

                cloudinary.Api.UrlImgUp.BuildUrl(String.Format("{0}.{1}", uploadResult.PublicId, uploadResult.Format));

                var data = this.context.Notes.Where(t => t.NoteId == noteId).FirstOrDefault();

                string Image = uploadResult.Url.ToString();
                data.Image = Image.ToString();

                var result = this.context.SaveChanges();

                nlog.LogInfo("Image Added Successfully");

                return Image;

            }

            catch (Exception ex)

            {

                nlog.LogWarn(ex.Message);

                return ex.Message;

            }

        }
        public void PutListToCache(int userid)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
            var enlist = this.context.Notes.Where(x => x.Id == userid);
            var jsonstring = JsonConvert.SerializeObject(enlist);
            distributedCache.SetString("noteList", jsonstring, options);
        }
        public List<Note> GetListFromCache(string key)
        {
            var CacheString = this.distributedCache.GetString(key);
            return JsonConvert.DeserializeObject<IEnumerable<Note>>(CacheString).ToList();
        }
    }
}
