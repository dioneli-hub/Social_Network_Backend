using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset LikedAt { get; set; }

        public Post Post { get; set; }
        public User User { get; set; }

    }
}
