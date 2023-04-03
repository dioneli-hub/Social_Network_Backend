using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DataAccess.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<UserFollower> UserFollowers { get; set; }
        public ICollection<UserFollower> UserFollowsTo { get; set; }
    }
}
