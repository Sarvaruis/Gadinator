using System;
using System.Drawing;
using System.Drawing.Imaging;
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
    [Route("api/Projects")]
    public class ProjectsController : Controller
    {
        private readonly ProjektTestowySerwerContext _context;

        public ProjectsController(ProjektTestowySerwerContext context)
        {
            _context = context;
        }

        // GET: api/Projects/Current            - returns currently choosen project
        // GET: api/Projects/Unload             - unloads the project from session data
        // GET: api/Projects/GetAll             - returns all projects in database
        // GET: api/Projects/GetFromCurrentUser - returns all projects of logged in user
        // GET: api/Projects/GetBackgroundData  - returns current choosen project background base64 data
        // GET: api/Projects/GetByUser/{id}     - returns all projects of specific user (by id)
        // GET: api/Projects/Get/{id}           - returns specific project (by id)
        [HttpGet]
        [HttpGet("{command}")]
        [HttpGet("{command}/{id}")]
        public async Task<IActionResult> GetProjects([FromRoute] string command, int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "current": return await GetCurrentProject();
                case "unload": return UnloadCurrentProject();
                case "get": return await GetProjectById(id);
                case "getall": return GetAllProjects();
                case "getbyuser": return await GetProjectsByUserId(id);
                case "getfromcurrentuser": return await GetCurrentUsersProjects();
                case "getbackgrounddata": return await GetCurrentUsersBackgroundData();
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> GetCurrentProject()
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

            return Ok(project);
        }

        public IActionResult UnloadCurrentProject()
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

            HttpContext.Session.Remove("choosenProjectId");
            return Ok("Successfully unloaded project.");
        }

        public async Task<IActionResult> GetProjectById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }

        public IActionResult GetAllProjects()
        {
            return Ok(_context.Projects);
        }

        public async Task<IActionResult> GetProjectsByUserId(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var projects = await _context.Projects.Where(m => m.UserId == id).ToListAsync();

            if (projects == null || projects.Count() == 0)
            {
                return NotFound();
            }
            return Ok(projects);
        }

        public async Task<IActionResult> GetCurrentUsersProjects()
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

            var projects = await _context.Projects.Where(m => m.UserId == userId).ToListAsync();
            if (projects == null || projects.Count() == 0)
            {
                return NotFound();
            }
            return Ok(projects);   
        }

        public async Task<IActionResult> GetCurrentUsersBackgroundData()
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
                return NotFound("Project not found in database.");
            }
            if (project.BackgroundFilePath == "" || project.BackgroundFilePath == null)
            {
                return NotFound("No background specified.");
            }
            if (!System.IO.File.Exists("Backgrounds\\" + userId + "_" + project.BackgroundFilePath))
            {
                return NotFound("Background file not found.");
            }
            string base64Data = Convert.ToBase64String(System.IO.File.ReadAllBytes(@"Backgrounds\\" +
                userId + "_" + project.BackgroundFilePath));

            return Ok(base64Data);
        }

        // POST: api/Projects                - add project to currently logged or specific user
        // ex.: {"Name": "test", "GridWidth": 10, "GridHeight": 10}
        // ex.: {"Name": "test", "GridWidth": 10, "GridHeight": 10, "UserId": 1}
        // POST: api/Projects/Load           - load specific users project
        // ex.: {"ProjectId": 1}
        [HttpPost]
        [HttpPost("{command}")]
        public async Task<IActionResult> PostProjects([FromRoute] string command)
        { 
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

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "load": return await LoadProject(jObject);
                case null: return await AddProject(jObject);
                default: return BadRequest();
            }      
        }

        public async Task<IActionResult> LoadProject(JObject jObject)
        {
            if (jObject["ProjectId"] == null) return BadRequest();

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

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == jObject["ProjectId"].ToString().ToNullableInt());

            if (project == null || project.UserId != userId)
            {
                return NotFound("Project not found");
            }

            HttpContext.Session.SetInt32("choosenProjectId", project.Id);
            return Ok("Project loaded successfully.");
        }

        public async Task<IActionResult> AddProject(JObject jObject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (jObject["Name"] == null || jObject["GridWidth"] == null || jObject["GridHeight"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            int? userId;
            if (jObject["UserId"] != null)
            {
                userId = jObject["UserId"].ToString().ToNullableInt();
                if (userId == null)
                {
                    return BadRequest("Wrong json user id input.");
                }
                var searchUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (searchUser == null) return BadRequest("User doesn't exists.");
            }
            else
            {
                /* sessions variable check */
                string access = HttpContext.Session.GetString("access");
                if (access == null || !access.Equals("1"))
                {
                    return BadRequest("User needs to be logged in.");
                }
                userId = HttpContext.Session.GetInt32("loggedUserId");
                if (userId == null)
                {
                    return BadRequest("No user in session found.");
                }
                var currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
            }

            if (jObject["Name"].ToString().Length > 100) return BadRequest("Too long project name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Project name cannot be empty.");

            int? gridWidth = jObject["GridWidth"].ToString().ToNullableInt();
            if (gridWidth == null)
            {
                return BadRequest("Wrong json grid width input.");
            }
            else if (gridWidth > 100)
            {
                return BadRequest("Grid width is too large.");
            }
            else if (gridWidth < 3)
            {
                return BadRequest("Grid width is too small.");
            }

            int? gridHeight = jObject["GridHeight"].ToString().ToNullableInt();
            if (gridHeight == null)
            {
                return BadRequest("Wrong json grid height input.");
            }
            else if (gridHeight > 100)
            {
                return BadRequest("Grid height is too large.");
            }
            else if (gridHeight < 3)
            {
                return BadRequest("Grid height is too small.");
            }

            Projects project = new Projects
            {
                Name = jObject["Name"].ToString(),
                GridWidth = (int)gridWidth,
                GridHeight = (int)gridHeight,
                UserId = (int)userId
            };

            _context.Projects.Add(project);
            
            await _context.SaveChangesAsync();
            return Ok("Project created successfully.");
        }

        // PUT: api/Projects/ChangeBackground  - changes background image or creates if there is none to current choosen 
        // or specific project when user is logged in or specific project and specific user (foreign key has to be matched)
        // when Name is empty, it deletes the background
        // ex.: {"Name": "test", "ImgData": "base64 encoded data"}
        // ex.: {"ProjectId": 1, "Name": "test", "ImgData": "base64 encoded data"}
        // ex.: {"UserId": 1, "ProjectId": 1, "Name": "test", "ImgData": "base64 encoded data"}
        // PUT: api/Projects  - changes currently choosen or specific project
        // ex.: {"Name": "test", "GridWidth": 10, "GridHeight": 10}
        // ex.: {"ProjectId": 1, "Name": "test", "GridWidth": 10, "GridHeight": 10, "UserId": 1}
        [HttpPut]
        [HttpPut("{command}")]
        public async Task<IActionResult> PutProjects([FromRoute] string command)
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

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "changebackground": return await ChangeBackground(jObject);
                case null: return await ModifyProject(jObject);
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> ChangeBackground(JObject jObject)
        {
            if (jObject["Name"] == null || jObject["ImgData"] == null) return BadRequest();

            int? userId = null;
            if (jObject["UserId"] != null)
            {
                userId = jObject["UserId"].ToString().ToNullableInt();
                if (userId == null)
                {
                    return BadRequest("Wrong user json input.");
                }
                var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (user == null)
                {
                    return NotFound("User not found in database.");
                }
            }
            else
            {
                string access = HttpContext.Session.GetString("access");
                if (access == null || !access.Equals("1"))
                {
                    return BadRequest("User needs to be logged in.");
                }

                userId = HttpContext.Session.GetInt32("loggedUserId");
                if (userId == null)
                {
                    return BadRequest("No user in session found.");
                }
            }

            int? projectId = null;
            Projects project = null;
            if (jObject["ProjectId"] != null)
            {
                projectId = jObject["ProjectId"].ToString().ToNullableInt();
                if (projectId == null)
                {
                    return BadRequest("Wrong project json input.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null || project.UserId != userId)
                {
                    return NotFound("Project not found in database.");
                }
            }
            else
            {
                projectId = HttpContext.Session.GetInt32("choosenProjectId");
                if (projectId == null)
                {
                    return BadRequest("No project in session found.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null || project.UserId != userId)
                {
                    return NotFound("Project not found in database.");
                }
            }

            if (project.BackgroundFilePath != null && System.IO.File.Exists("Backgrounds\\" + userId + "_" + project.BackgroundFilePath))
            {
                System.IO.File.Delete("Backgrounds\\" + userId + "_" + project.BackgroundFilePath);
            }

            if (jObject["Name"].ToString() == "")
            {
                project.BackgroundFilePath = null;

                _context.Entry(project).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                    return Ok("Background data deleted successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Database concurrency exception occured.");
                }
            }

            string extensions = jObject["Name"].ToString().ToLower();
            if (extensions.Contains(".jpg") || extensions.Contains(".jpeg") || extensions.Contains(".png") || extensions.Contains(".bmp"))
            {
                project.BackgroundFilePath = jObject["Name"].ToString();
                var img = Image.FromStream(new MemoryStream(Convert.FromBase64String(jObject["ImgData"].ToString())));
                ImageFormat format = ImageFormat.Bmp;
                if (extensions.Contains(".jpg") || extensions.Contains(".jpeg")) format = ImageFormat.Jpeg;
                else if (extensions.Contains(".png")) format = ImageFormat.Png;

                img.Save("Backgrounds\\" + userId + "_" + project.BackgroundFilePath, format);

                _context.Entry(project).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                    return Ok("Background data changed successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Database concurrency exception occured.");
                }
            }
            else
            {
                return BadRequest("Image format not supported.");
            }
        }

        public async Task<IActionResult> ModifyProject(JObject jObject)
        {
            if (jObject["Name"] == null || jObject["GridWidth"] == null || jObject["GridHeight"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            Projects project;
            int? userId, projectId;
            if (jObject["UserId"] != null && jObject["ProjectId"] != null)
            {
                userId = jObject["UserId"].ToString().ToNullableInt();
                if (userId == null)
                {
                    return BadRequest("Wrong json user id input.");
                }
                var searchUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (searchUser == null) return BadRequest("User doesn't exists.");

                projectId = jObject["ProjectId"].ToString().ToNullableInt();
                if (projectId == null)
                {
                    return BadRequest("No project in session found.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null)
                {
                    return NotFound("Project not found in database");
                }
            }
            else
            {
                /* sessions variable check */
                string access = HttpContext.Session.GetString("access");
                if (access == null || !access.Equals("1"))
                {
                    return BadRequest("User needs to be logged in.");
                }
                userId = HttpContext.Session.GetInt32("loggedUserId");
                if (userId == null)
                {
                    return BadRequest("No user in session found.");
                }
                var currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
                projectId = HttpContext.Session.GetInt32("choosenProjectId");
                if (projectId == null)
                {
                    return BadRequest("No project in session found.");
                }
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null)
                {
                    return NotFound("Project not found in database");
                }
            }

            if (jObject["Name"].ToString().Length > 100) return BadRequest("Too long project name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Project name cannot be empty.");

            int? gridWidth = jObject["GridWidth"].ToString().ToNullableInt();
            if (gridWidth == null)
            {
                return BadRequest("Wrong json grid width input.");
            }
            else if (gridWidth > 100)
            {
                return BadRequest("Grid width is too large.");
            }
            else if (gridWidth < 3)
            {
                return BadRequest("Grid width is too small.");
            }

            int? gridHeight = jObject["GridHeight"].ToString().ToNullableInt();
            if (gridHeight == null)
            {
                return BadRequest("Wrong json grid height input.");
            }
            else if (gridHeight > 100)
            {
                return BadRequest("Grid height is too large.");
            }
            else if (gridHeight < 3)
            {
                return BadRequest("Grid height is too small.");
            }

            project.Name = jObject["Name"].ToString();
            project.GridWidth = (int)gridWidth;
            project.GridHeight = (int)gridHeight;
            if (jObject["UserId"] != null)
            {
                project.UserId = (int)userId;
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Project data changed successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }

        // DELETE: api/Projects      - deletes current choosen project
        // DELETE: api/Projects/{id} - deletes specific project
        [HttpDelete]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjects([FromRoute] int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Projects project;
            if (id == null)
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

                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectsId);
                if (project == null)
                {
                    return NotFound();
                }
            }
            else
            {
                project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);
                if (project == null)
                {
                    return NotFound("Project not found in database.");
                }
            }

            if (project.BackgroundFilePath != null && System.IO.File.Exists("Backgrounds\\" + project.UserId + "_" + project.BackgroundFilePath))
            {
                System.IO.File.Delete("Backgrounds\\" + project.UserId + "_" + project.BackgroundFilePath);
            }
            _context.Projects.Remove(project); 
            await _context.SaveChangesAsync();
            if (id == null) HttpContext.Session.Remove("choosenProjectId");
            return Ok("Project deleted successfully.");
        }

        private bool ProjectsExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}