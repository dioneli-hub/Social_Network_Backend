using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend.DataAccess;


namespace Backend.Api.Controllers
{
        [ApiController]
        [Route("api/[controller]")]

        public class FileController : ControllerBase
        {
            private readonly DatabaseContext _database;

            public FileController(DatabaseContext database)
            {
                _database = database;
            }

            [HttpGet("{fileId}", Name = nameof(GetFileById))]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public ActionResult<FileResult> GetFileById(int fileId)
            {
                var file = _database.ApplicationFiles.FirstOrDefault(x => x.Id == fileId);
                if (file == null)
                {
                    return NotFound();
                }

                return File(file.Content, file.ContentType, file.FileName);
            }
        }
    }