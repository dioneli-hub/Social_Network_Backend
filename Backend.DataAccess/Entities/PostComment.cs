using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DataAccess.Entities
{
    public class PostComment
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public int PostId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Post Post { get; set; }

        //in the future author Id and author????

    }
}
