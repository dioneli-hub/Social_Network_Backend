using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain
{
    public class ApplicationFile
    {
        public int Id { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
