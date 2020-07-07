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
    [Route("api/Instances")]
    public class InstancesController : Controller
    {
        private readonly ProjektTestowySerwerContext _context;

        public InstancesController(ProjektTestowySerwerContext context)
        {
            _context = context;
        }

        // GET: api/Instances/GetAll         - returns all instances in database
        // GET: api/Instances/GetByProject - returns all instances of project - current or specific
        // ex.: {"CategoryId": 1}
        // ex.: {"ObjectTypeId": 1}
        // ex.: {"ProjectId": 1}
        // ex.: {"ProjectId": 1, "CategoryId": 1}
        // ex.: {"ProjectId": 1, "ObjectTypeId": 1}
        // GET: api/Instances/GetByArea - returns all instances of specific area
        // ex.: {"AreaId": 1}
        // ex.: {"AreaId": 1, "CategoryId": 1}
        // ex.: {"AreaId": 1, "ObjectTypeId": 1}
        // GET: api/Instances/Get/{id}       - returns specific instance (by id)
        [HttpGet]
        [HttpGet("{command}")]
        [HttpGet("{command}/{id}")]
        public async Task<IActionResult> GetAreas([FromRoute] string command, int? id)
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
                case "get": return await GetInstanceById(id);
                case "getall": return GetAllInstances();
                case "getbyproject": return await GetInstancesByProject(jObject);
                case "getbyarea": return await GetInstancesByArea(jObject);
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> GetInstanceById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var instance = await _context.Instances.SingleOrDefaultAsync(m => m.Id == id);

            if (instance == null)
            {
                return NotFound();
            }
            return Ok(instance);
        }

        public IActionResult GetAllInstances()
        {
            return Ok(_context.Instances);
        }

        public async Task<IActionResult> GetInstancesByProject(JObject jObject)
        {
            if (jObject["CategoryId"] == null && jObject["ObjectTypeId"] == null && jObject["ProjectId"] == null)
            {
                return BadRequest("Wrong body input format.");
            }
            if (jObject["CategoryId"] != null && jObject["ObjectTypeId"] != null)
            {
                return BadRequest("You cannot search by Category and Object type at the same time.");
            }

            int? projectId;
            if (jObject["ProjectId"] != null)
            {
                projectId = jObject["ProjectId"].ToString().ToNullableInt();
                if (projectId == null)
                {
                    return BadRequest("Bad project id body input.");
                }
                var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == projectId);
                if (project == null)
                {
                    return NotFound("Project not found in database.");
                }
            }
            else
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
            }

            List<Instances> instances = new List<Instances>();
            if (jObject["CategoryId"] != null)
            {
                int? categoryId = jObject["CategoryId"].ToString().ToNullableInt();
                if (categoryId == null)
                {
                    return BadRequest("Bad category id body input.");
                }
                var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == categoryId);
                if (category == null)
                {
                    return NotFound("Category not found in database.");
                }

                var objects = await _context.Objects.Where(m => m.CategoryId == categoryId).ToListAsync();
                foreach (var obj in objects)
                {
                    var partialInstances = await _context.Instances.Where(m => m.ObjectId == obj.Id).ToListAsync();
                    instances.AddRange(partialInstances);
                }
            }
            else if (jObject["ObjectTypeId"] != null)
            {
                int? objectId = jObject["ObjectTypeId"].ToString().ToNullableInt();
                if (objectId == null)
                {
                    return BadRequest("Bad object id body input.");
                }
                var objectType = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objectId);
                if (objectType == null)
                {
                    return NotFound("Object not found in database.");
                }

                var obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objectId);
                instances = await _context.Instances.Where(m => m.ObjectId == obj.Id).ToListAsync();
            }

            if (instances.Count() == 0)
            {
                return NotFound("Instances not found in the database.");
            }
            return Ok(instances);
        }

        public async Task<IActionResult> GetInstancesByArea(JObject jObject)
        {
            if (jObject["CategoryId"] == null && jObject["ObjectTypeId"] == null && jObject["AreaId"] == null)
            {
                return BadRequest("Wrong body input format.");
            }
            if (jObject["CategoryId"] != null && jObject["ObjectTypeId"] != null)
            {
                return BadRequest("You cannot search by Category and Object type at the same time.");
            }

            if (jObject["AreaId"] != null)
            {
                int? areaId = jObject["AreaId"].ToString().ToNullableInt();
                if (areaId == null)
                {
                    return BadRequest("Bad area id body input.");
                }
                var area = await _context.Projects.SingleOrDefaultAsync(m => m.Id == areaId);
                if (area == null)
                {
                    return NotFound("Area not found in database.");
                }

                List<Instances> instances = new List<Instances>();
                List<Instances> returningInstances = new List<Instances>();
                instances = await _context.Instances.Where(m => m.AreaId == areaId).ToListAsync();

                if (jObject["CategoryId"] != null)
                {
                    int? categoryId = jObject["CategoryId"].ToString().ToNullableInt();
                    if (categoryId == null)
                    {
                        return BadRequest("Bad category id body input.");
                    }
                    var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == categoryId);
                    if (category == null)
                    {
                        return NotFound("Category not found in database.");
                    }

                    var objects = await _context.Objects.Where(m => m.CategoryId == categoryId).ToListAsync();                  
                    foreach (var obj in objects)
                    {
                        returningInstances.AddRange(instances.Where(m => m.ObjectId == obj.Id).ToList());
                    }
                }
                else if (jObject["ObjectTypeId"] != null)
                {
                    int? objectId = jObject["ObjectTypeId"].ToString().ToNullableInt();
                    if (objectId == null)
                    {
                        return BadRequest("Bad object id body input.");
                    }
                    var objectType = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objectId);
                    if (objectType == null)
                    {
                        return NotFound("Object not found in database.");
                    }

                    returningInstances = instances.Where(m => m.ObjectId == objectId).ToList();
                }

                if (returningInstances.Count() == 0)
                {
                    return NotFound("Instances not found in the database.");
                }
                return Ok(returningInstances);
            }
            else
            {
                return BadRequest("Wrong area id json input format.");
            }
        }

        // POST: api/Instances - add instance to specific area
        // ex.: {"X": 0, "Y": 0, "ObjectId": 1, "AreaId": 1}
        [HttpPost]
        public async Task<IActionResult> PostInstances()
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

            if (jObject["X"] == null || jObject["Y"] == null ||
                jObject["ObjectId"] == null || jObject["AreaId"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            int? areaId = jObject["AreaId"].ToString().ToNullableInt();
            if (areaId == null)
            {
                return BadRequest("Wrong area id parse.");
            }
            Areas area = await _context.Areas.SingleOrDefaultAsync(m => m.Id == areaId);
            if (area == null)
            {
                return NotFound("Area is not found in the database.");
            }

            int? objectId = jObject["ObjectId"].ToString().ToNullableInt();
            if (objectId == null)
            {
                return BadRequest("Wrong object id parse.");
            }
            Objects obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objectId);
            if (obj == null)
            {
                return NotFound("Object is not found in the database.");
            }

            int? x = jObject["X"].ToString().ToNullableInt();
            if (x == null)
            {
                return BadRequest("Wrong json x input parse.");
            }
            if (x < 0)
            {
                return BadRequest("X cannot be negative.");
            }
            else if (x + obj.Width > area.Width)
            {
                return BadRequest("X + object width cannot be higher than area width.");
            }

            int? y = jObject["Y"].ToString().ToNullableInt();
            if (y == null)
            {
                return BadRequest("Wrong json y input parse.");
            }
            if (y < 0)
            {
                return BadRequest("Y cannot be negative.");
            }
            else if (y + obj.Height > area.Height)
            {
                return BadRequest("Y + object height cannot be higher than area width.");
            }

            Instances instance = new Instances
            {
                X = (int)x,
                Y = (int)y,
                ObjectId = obj.Id,
                AreaId = area.Id
            };

            _context.Instances.Add(instance);

            await _context.SaveChangesAsync();
            return Ok("Instance has been created successfully.");
        }

        // PUT: api/Instances - modifies specific instance
        // ex.: {"InstanceId": 1, "X": 0, "Y": 0, "ObjectId": 1, "AreaId": 1}
        [HttpPut]
        public async Task<IActionResult> PutInstances()
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

            if (jObject["X"] == null || jObject["Y"] == null ||
                jObject["ObjectId"] == null || jObject["AreaId"] == null
                || jObject["InstanceId"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            int? instanceId = jObject["InstanceId"].ToString().ToNullableInt();
            if (instanceId == null)
            {
                return BadRequest("Wrong instance id parse.");
            }
            Instances instanceToChange = await _context.Instances.SingleOrDefaultAsync(m => m.Id == instanceId);
            if (instanceToChange == null)
            {
                return NotFound("Instance not found in the database.");
            }

            int? areaId = jObject["AreaId"].ToString().ToNullableInt();
            if (areaId == null)
            {
                return BadRequest("Wrong area id parse.");
            }
            Areas area = await _context.Areas.SingleOrDefaultAsync(m => m.Id == areaId);
            if (area == null)
            {
                return NotFound("Area is not found in the database.");
            }
            instanceToChange.AreaId = area.Id;
            
            int? objectId = jObject["ObjectId"].ToString().ToNullableInt();
            if (objectId == null)
            {
                return BadRequest("Wrong object id parse.");
            }
            Objects obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objectId);
            if (obj == null)
            {
                return NotFound("Object is not found in the database.");
            }
            instanceToChange.ObjectId = obj.Id;
            
            int? x = jObject["X"].ToString().ToNullableInt();
            if (x == null)
            {
                return BadRequest("Wrong json x input parse.");
            }
            if (x < 0)
            {
                return BadRequest("X cannot be negative.");
            }
            else if (x + obj.Width > area.Width)
            {
                return BadRequest("X + object width cannot be higher than area width.");
            }
            instanceToChange.X = (int)x;
            
            int? y = jObject["Y"].ToString().ToNullableInt();
            if (y == null)
            {
                return BadRequest("Wrong json y input parse.");
            }
            if (y < 0)
            {
                return BadRequest("Y cannot be negative.");
            }
            else if (y + obj.Height > area.Height)
            {
                return BadRequest("Y + object height cannot be higher than area width.");
            }
            instanceToChange.Y = (int)y;

            _context.Entry(instanceToChange).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Instance has been modified successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Database concurrency exception has occured.");
            }  
        }

        // DELETE: api/Instances/{id} - deletes specific instance by Id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstances([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var instances = await _context.Instances.SingleOrDefaultAsync(m => m.Id == id);
            if (instances == null)
            {
                return NotFound("Instance not found in the database.");
            }

            _context.Instances.Remove(instances);
            await _context.SaveChangesAsync();

            return Ok(instances);
        }

        private bool InstancesExists(int id)
        {
            return _context.Instances.Any(e => e.Id == id);
        }
    }
}