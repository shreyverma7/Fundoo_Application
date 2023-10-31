using FundooModel.Labels;
using FundooModel.Notes;
using FundooRepository.Context;
using FundooRepository.IRepository;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog.Fluent;
using NlogImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FundooRepository.Repository
{
    public class LabelsRepository : ILabelsRepository
    {
        public readonly UserDbContext context;
        public readonly IDistributedCache distributedCache;
        NlogOperation nlog = new NlogOperation();
        public LabelsRepository(UserDbContext context, IDistributedCache distributedCache)
        {
            this.context = context;
            this.distributedCache = distributedCache;
        }
        public Task<int> AddLabels(label labels)
        {
            this.context.Labels.Add(labels);
            var result = this.context.SaveChangesAsync();
            nlog.LogInfo("Added labels");
            return result;
        }
        public label EditLabel(label labels)
        {

            try
            {
                var result = this.context.Labels.Where(x => x.LabelId == labels.LabelId).FirstOrDefault();
                result.LabelName = labels.LabelName;
                this.context.Labels.Update(result);
                nlog.LogInfo("Edited label successful");
                var data = this.context.SaveChanges();
                if (data != 0)
                    return result;
                return null;
            }
            catch (Exception ex)
            {
                nlog.LogWarn("No labels found");
                return null;
            }
        }
        public IEnumerable<label> GetAllLabels(int userId)
        {
            var result = this.context.Labels.Where(x => x.Id.Equals(userId)).AsEnumerable();
            if (result != null)
            {
                nlog.LogInfo("Got all labels");
                this.PutListToCache(userId);
                return result;
            }
            nlog.LogError("Label on email not found");
            var data = this.GetListFromCache("Labellist");
            return null;
        }

        public bool DeleteLabels(int labelId)
        {
            var result = this.context.Labels.Where(x => x.LabelId == labelId).ToList();
            foreach (var data in result)
            {
                nlog.LogInfo("Deleted label successfully");
                this.context.Labels.Remove(data);
            }
            var deleteResult = this.context.SaveChanges();
            if (deleteResult == 0)
            {
                nlog.LogWarn("labels is null");
                return false;
            }
            nlog.LogWarn("labels not found");
            return false;
        }
        public IEnumerable<label> GetAllLabelNotes(int userId)
        {
            List<label> list = new List<label>();
            var result = this.context.Notes.Where(x => x.Id.Equals(userId)).Join(this.context.Labels, Note => Note.Id, label => label.Id,
      (Note, label) => new label
      {
          //NoteId = Note.Id,
          LabelId = label.Id,
          LabelName = label.LabelName
      });
            foreach (var data in result)
            {
                list.Add(data);
            }
            return list;
        }

        public void PutListToCache(int userid)
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
            var enlist = this.context.Labels.Where(x => x.Id == userid);
            var jsonstring = JsonConvert.SerializeObject(enlist);
            distributedCache.SetString("Labellist", jsonstring, options);
        }
        public List<Note> GetListFromCache(string key)
        {
            var CacheString = this.distributedCache.GetString(key);
            return JsonConvert.DeserializeObject<IEnumerable<Note>>(CacheString).ToList();
        }
    }
}
