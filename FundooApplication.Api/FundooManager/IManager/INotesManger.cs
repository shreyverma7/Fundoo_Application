﻿using FundooModel.Notes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace FundooManager.IManager
{
    public interface INotesManger
    {
        public Task<int> AddNotes(Note note);
        public Note EditNotes(Note note);
        public IEnumerable<Note> GetAllNotes(int userId);
        public bool DeleteNote(int noteid, int userId);
        public IEnumerable<Note> GetArcheived(int userId);
        public IEnumerable<Note> GetPinnedTask(int userId);
        public IEnumerable<Note> GetThrashedTask(int userId);
        public bool TrashNote(int userId);
        public Note ArcheiveNote(int noteId, int userId);
        public Note PinNote(int noteId, int userId);

        public bool RestoreNotes(int noteId, int userId);
        public string Image(IFormFile file, int noteId);
        public Note GetNoteById(int userId, int noteId);
        public IEnumerable<Note> RemainderById(int userId);
        public Task<int> CreateCopyNote(int userId, int noteId)
    }
}
