using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DataAccess.Entities
{
    public class UserFollower
    {
        public int UserId { get; set; }
        public int FollowerId { get; set; }
        public User User { get; set; }
        public User Follower { get; set; }
    }
}
