using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DataAccess.Entities
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public DateTimeOffset LikedAt { get; set; }

        public Post Post { get; set; }
        //in the future author Id and author????
    }
}
