using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjektTestowySerwer.Helpers;
using ProjektTestowySerwer.Models;

namespace ProjektTestowySerwer.Controllers
{
    [Produces("application/json")]
    [Route("api/Areas")]
    public class AreasController : Controller
    {
        private readonly ProjektTestowySerwerContext _context;

        public AreasController(ProjektTestowySerwerContext context)
        {
            _context = context;
        }

        // GET: api/Areas/GetAll                 - returns all areas in database
        // GET: api/Areas/GetFromCurrentProject  - returns all areas of current project
        // GET: api/Areas/GetByProject/{id}      - returns all areas of specific project
        // GET: api/Areas/Get/{id}               - returns specific area (by id)
        [HttpGet]
        [HttpGet("{command}")]
        [HttpGet("{command}/{id}")]
        public async Task<IActionResult> GetAreas([FromRoute] string command, int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "get": return await GetAreaById(id);
                case "getall": return GetAllAreas();
                case "getbyproject": return await GetAreasByProjectId(id);
                case "getfromcurrentproject": return await GetCurrentProjectAreas();
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> GetCurrentProjectAreas()
        {
            string access = HttpContext.Session.GetString("access");
            if (access == null || !access.Equals("1"))
            {
                return BadRequest("User needs to be logged in.");
            }

            int? userId = HttpContext.Session.GetInt32("loggedUserId");
            if (userId == null)
            {
                return BadRequest("No user in session found.");
            }

            int? projectsId = HttpContext.Session.GetInt32("choosenProjectId");
            if (projectsId == null)
            {
                return BadRequest("No project in session found.");
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectsId);
            if (project == null)
            {
                return NotFound();
            }
            var areas = await _context.Areas.Where(m => m.ProjectId == project.Id).ToListAsync();

            return Ok(areas);
        }

        public async Task<IActionResult> GetAreaById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var area = await _context.Areas.SingleOrDefaultAsync(m => m.Id == id);

            if (area == null)
            {
                return NotFound();
            }
            return Ok(area);
        }

        public IActionResult GetAllAreas()
        {
            return Ok(_context.Areas);
        }

        public async Task<IActionResult> GetAreasByProjectId(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var areas = await _context.Areas.Where(m => m.ProjectId == id).ToListAsync();

            if (areas == null || areas.Count() == 0)
            {
                return NotFound();
            }
            return Ok(areas);
        }

        // POST: api/Areas - add area to currently choosen or specific project
        // ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10}
        // ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1}
        // ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ProjectId": 1}
        // ex.: {"Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1, "ProjectId": 1}
        [HttpPost]
        public async Task<IActionResult> PostAreas()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            JObject jObject;
            using (var reader = new StreamReader(Request.Body))
            {
                string plainText = reader.ReadToEnd();
                try
                {
                    jObject = JObject.Parse(plainText);
                }
                catch (JsonReaderException)
                {
                    return BadRequest();
                }
            }

            if (jObject["Name"] == null || jObject["X"] == null || jObject["Y"] == null ||
                jObject["Width"] == null || jObject["Height"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            Projects project;
            if (jObject["ProjectId"] != null)
            {
                int? projectId = jObject["ProjectId"].ToString().ToNullableInt();
                if (projectId == null)
                {
                    return BadRequest("Wrong json project id input.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null) return BadRequest("Project doesn't exists.");
            }
            else
            {
                /* sessions variable check */
                string access = HttpContext.Session.GetString("access");
                if (access == null || !access.Equals("1"))
                {
                    return BadRequest("User needs to be logged in.");
                }
                var userId = HttpContext.Session.GetInt32("loggedUserId");
                if (userId == null)
                {
                    return BadRequest("No user in session found.");
                }
                var currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
                int? projectId = HttpContext.Session.GetInt32("choosenProjectId");
                if (projectId == null)
                {
                    return BadRequest("No project in session found.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null) return BadRequest("Project doesn't exists.");
            }

            if (jObject["Name"].ToString().Length > 50) return BadRequest("Too long area name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Area name cannot be empty.");

            int? width = jObject["Width"].ToString().ToNullableInt();
            if (width == null)
            {
                return BadRequest("Wrong json width input.");
            }
            if (width < 1)
            {
                return BadRequest("Width is too small.");
            }

            int? height = jObject["Height"].ToString().ToNullableInt();
            if (height == null)
            {
                return BadRequest("Wrong json height input.");
            }
            if (height < 1)
            {
                return BadRequest("Height is too small.");
            }

            int? x = jObject["X"].ToString().ToNullableInt();
            if (x == null)
            {
                return BadRequest("Wrong json x position input.");
            }
            if (x < 0)
            {
                return BadRequest("X cannot be negative.");
            }

            int? y = jObject["Y"].ToString().ToNullableInt();
            if (y == null)
            {
                return BadRequest("Wrong json y position input.");
            }
            if (y < 0)
            {
                return BadRequest("Y cannot be negative.");
            }

            if ((x + width) > project.GridWidth)
            {
                return BadRequest("X + Width cannot be above the grids width.");
            }
            if ((y + height) > project.GridHeight)
            {
                return BadRequest("Y + Height cannot be above the grids height.");
            }

            Areas area = new Areas
            {
                Name = jObject["Name"].ToString(),
                X = (int)x,
                Y = (int)y,
                Width = (int)width,
                Height = (int)height,
                ProjectId = project.Id
            };

            if (jObject["ParentAreaId"] != null)
            {
                int? parentAreaId = jObject["ParentAreaId"].ToString().ToNullableInt();
                if (parentAreaId == null)
                {
                    return BadRequest("Wrong json area parent input.");
                }
                Areas parentArea = await _context.Areas.SingleOrDefaultAsync(m => m.Id == parentAreaId);
                if (parentArea == null)
                {
                    return NotFound("Area not found in database.");
                }
                area.ParentAreaId = parentArea.Id;
            }

            _context.Areas.Add(area);

            await _context.SaveChangesAsync();
            return Ok("Area has been created successfully.");
        }

        // PUT: api/Areas  - changes specific area
        // ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10}
        // ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1}
        // ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ProjectId": 1}
        // ex.: {"AreaId": 1, "Name": "test", "X": 0, "Y": 0, "Width": 10, "Height": 10, "ParentAreaId": 1, "ProjectId": 1}
        [HttpPut]
        public async Task<IActionResult> PutAreas()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            JObject jObject;
            using (var reader = new StreamReader(Request.Body))
            {
                string plainText = reader.ReadToEnd();
                try
                {
                    jObject = JObject.Parse(plainText);
                }
                catch (JsonReaderException)
                {
                    return BadRequest();
                }
            }

            if (jObject["Name"] == null || jObject["X"] == null || jObject["Y"] == null ||
                jObject["Width"] == null || jObject["Height"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            Projects project;
            if (jObject["ProjectId"] != null)
            {
                int? projectId = jObject["UserId"].ToString().ToNullableInt();
                if (projectId == null)
                {
                    return BadRequest("Wrong json project id input.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null) return BadRequest("Project doesn't exists.");
            }
            else
            {
                /* sessions variable check */
                string access = HttpContext.Session.GetString("access");
                if (access == null || !access.Equals("1"))
                {
                    return BadRequest("User needs to be logged in.");
                }
                var userId = HttpContext.Session.GetInt32("loggedUserId");
                if (userId == null)
                {
                    return BadRequest("No user in session found.");
                }
                var currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
                int? projectId = HttpContext.Session.GetInt32("choosenProjectId");
                if (projectId == null)
                {
                    return BadRequest("No project in session found.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null) return BadRequest("Project doesn't exists.");
            }

            if (jObject["Name"].ToString().Length > 50) return BadRequest("Too long area name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Area name cannot be empty.");

            int? width = jObject["Width"].ToString().ToNullableInt();
            if (width == null)
            {
                return BadRequest("Wrong json width input.");
            }
            if (width < 1)
            {
                return BadRequest("Width is too small.");
            }

            int? height = jObject["Height"].ToString().ToNullableInt();
            if (height == null)
            {
                return BadRequest("Wrong json height input.");
            }
            if (height < 1)
            {
                return BadRequest("Height is too small.");
            }

            int? x = jObject["X"].ToString().ToNullableInt();
            if (x == null)
            {
                return BadRequest("Wrong json x position input.");
            }
            if (x < 0)
            {
                return BadRequest("X cannot be negative.");
            }

            int? y = jObject["Y"].ToString().ToNullableInt();
            if (y == null)
            {
                return BadRequest("Wrong json y position input.");
            }
            if (y < 0)
            {
                return BadRequest("Y cannot be negative.");
            }

            if ((x + width) > project.GridWidth)
            {
                return BadRequest("X + Width cannot be above the grids width.");
            }
            if ((y + height) > project.GridHeight)
            {
                return BadRequest("Y + Height cannot be above the grids height.");
            }
            

            int? editedAreaId = jObject["AreaId"].ToString().ToNullableInt();
            if (editedAreaId == null)
            {
                return BadRequest("Wrong json AreaId input.");
            }
            Areas editedArea = await _context.Areas.SingleOrDefaultAsync(m => m.Id == editedAreaId);
            if (editedArea == null)
            {
                return BadRequest("Area to edit is not found in database.");
            }

            if (jObject["ParentAreaId"] != null)
            {
                int? parentAreaId = jObject["ParentAreaId"].ToString().ToNullableInt();
                if (parentAreaId == null)
                {
                    return BadRequest("Wrong json area parent input.");
                }
                Areas parentArea = await _context.Areas.SingleOrDefaultAsync(m => m.Id == parentAreaId);
                if (parentArea == null)
                {
                    return NotFound("Area not found in database.");
                }
                if (editedArea.ParentAreaId != parentArea.Id)
                {
                    editedArea.ParentAreaId = parentArea.Id;
                }
                else
                {
                    return NotFound("Parent area and edited area cannot be the same object.");
                }
            }

            editedArea.Name = jObject["Name"].ToString();
            editedArea.X = (int)x;
            editedArea.Y = (int)y;
            editedArea.Width = (int)width;
            editedArea.Height = (int)height;
            editedArea.ProjectId = project.Id;

            _context.Entry(editedArea).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Area data changed successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }


        // DELETE: api/Areas/{id} - deletes specific area
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAreas([FromRoute] int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Areas area;
            if (id == null)
            {
                return BadRequest("Wrong id route input.");
            }

            area = await _context.Areas.SingleOrDefaultAsync(m => m.Id == id);
            if (area == null)
            {
                return NotFound("Area not found in database.");
            }

            _context.Areas.Remove(area);
            
            await _context.SaveChangesAsync();
            return Ok("Area deleted successfully.");
        }

        private bool AreasExists(int id)
        {
            return _context.Areas.Any(e => e.Id == id);
        }
    }
}