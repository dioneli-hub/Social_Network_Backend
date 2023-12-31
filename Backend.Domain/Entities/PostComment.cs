﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain
{
    public class PostComment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int AuthorId { get; set; }
        public int PostId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Post Post { get; set; }
        public User Author { get; set; }


    }
}
