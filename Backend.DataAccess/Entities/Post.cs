using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DataAccess.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        //public int AuthorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<PostLike> Likes { get; set; }
        public ICollection<PostComment> Comments { get; set; }

        //in the future author Id and author????

    }
}
