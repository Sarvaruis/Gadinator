using System;
using System.Collections.Generic;
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
    [Route("api/Objects")]
    public class ObjectsController : Controller
    {
        private readonly ProjektTestowySerwerContext _context;

        public ObjectsController(ProjektTestowySerwerContext context)
        {
            _context = context;
        }


        // GET: api/Objects/GetAll             - returns all objects in database
        // GET: api/Objects/Get/{id}           - returns specific object (by id)
        // GET: api/Objects/GetImage/{id}      - returns image (base64 encoded data) from specific object (by id)
        // GET: api/Objects/GetByCategory/{id} - returns all objects by specific category (by id)
        [HttpGet]
        [HttpGet("{command}")]
        [HttpGet("{command}/{id}")]
        public async Task<IActionResult> GetObjects([FromRoute] string command, int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "get": return await GetObjectsById(id);
                case "getimage": return await GetObjectImageById(id);
                case "getall": return GetAllObjects();
                case "getbycategory": return await GetObjectsByCategoryId(id);
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> GetObjectsById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == id);

            if (obj == null)
            {
                return NotFound("Object not found in database.");
            }
            return Ok(obj);
        }

        public async Task<IActionResult> GetObjectImageById(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }

            var obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == id);
            if (obj == null)
            {
                return NotFound("Object not found in database.");
            }
    
            if (obj.ImagePath == "" || obj.ImagePath == null)
            {
                return NotFound("No image specified.");
            }
            if (!System.IO.File.Exists("ObjectImages\\" + obj.Id + "_" + obj.ImagePath))
            {
                return NotFound("Image file not found.");
            }
            string base64Data = Convert.ToBase64String(System.IO.File.ReadAllBytes(@"ObjectImages\\" +
                obj.Id + "_" + obj.ImagePath));

            return Ok(base64Data);
        }

        public IActionResult GetAllObjects()
        {
            return Ok(_context.Objects);
        }

        public async Task<IActionResult> GetObjectsByCategoryId(int? id)
        {
            if (id == null)
            {
                return BadRequest("Wrong id input.");
            }
            var objects = await _context.Objects.Where(m => m.CategoryId == id).ToListAsync();

            if (objects == null || objects.Count() == 0)
            {
                return NotFound("Objects not found in database.");
            }
            return Ok(objects);
        }

        // POST: api/Objects - add object to specific category
        // ex.: {"Name": "test", "Width": 3, "Height": 3, "CategoryId", 1}
        [HttpPost]
        public async Task<IActionResult> PostObjects()
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

            if (jObject["Name"] == null || jObject["Width"] == null ||
                jObject["Height"] == null || jObject["CategoryId"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            int? categoryId = jObject["CategoryId"].ToString().ToNullableInt();
            if (categoryId == null)
            {
                return BadRequest("Wrong json category id input.");
            }
            var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == categoryId);
            if (category == null) return BadRequest("Category doesn't exists.");

            if (jObject["Name"].ToString().Length > 100) return BadRequest("Too long object name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Object name cannot be empty.");

            int? width = jObject["Width"].ToString().ToNullableInt();
            if (width == null)
            {
                return BadRequest("Wrong json width input.");
            }
            else if (width > 100)
            {
                return BadRequest("Width is too large.");
            }
            else if (width < 1)
            {
                return BadRequest("Width is too small.");
            }

            int? height = jObject["Height"].ToString().ToNullableInt();
            if (height == null)
            {
                return BadRequest("Wrong json height input.");
            }
            else if (height > 100)
            {
                return BadRequest("Height is too large.");
            }
            else if (height < 1)
            {
                return BadRequest("Height is too small.");
            }

            Objects obj = new Objects
            {
                Name = jObject["Name"].ToString(),
                Width = (int)width,
                Height = (int)height,
                CategoryId = category.Id
            };

            _context.Objects.Add(obj);

            await _context.SaveChangesAsync();
            return Ok("Objects has been created successfully.");
        }

        // PUT: api/Objects             - modifies object of specific category
        // ex.: {"ObjectId": 1, "Name": "test", "Width": 3, "Height": 3, "CategoryId", 1}
        // PUT: api/Objects/ChangeImage - changes object image or creates if there is none
        // ex.: {"ObjectId": 1, "Name": "test.jpg", "ImgData": "base64 encoded data"}
        // ex.: {"ObjectId": 1, "Name": "", "ImgData": ""}
        [HttpPut]
        [HttpPut("{command}")]
        public async Task<IActionResult> PutObjects([FromRoute] string command)
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
                case "changeimage": return await ChangeImage(jObject);
                case null: return await ModifyObject(jObject);
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> ModifyObject(JObject jObject)
        {
            if (jObject["ObjectId"] == null || jObject["Name"] == null || jObject["Width"] == null ||
                jObject["Height"] == null || jObject["CategoryId"] == null)
            {
                return BadRequest("Wrong body input format.");
            }

            int? objId = jObject["ObjectId"].ToString().ToNullableInt();
            if (objId == null)
            {
                return BadRequest("Wrong json object id input.");
            }
            var obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objId);
            if (obj == null) return BadRequest("Object doesn't exists.");

            int? categoryId = jObject["CategoryId"].ToString().ToNullableInt();
            if (categoryId == null)
            {
                return BadRequest("Wrong json category id input.");
            }
            var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == categoryId);
            if (category == null) return BadRequest("Category doesn't exists.");
            obj.CategoryId = category.Id;

            if (jObject["Name"].ToString().Length > 100) return BadRequest("Too long object name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Object name cannot be empty.");
            obj.Name = jObject["Name"].ToString();

            int? width = jObject["Width"].ToString().ToNullableInt();
            if (width == null)
            {
                return BadRequest("Wrong json width input.");
            }
            else if (width > 100)
            {
                return BadRequest("Width is too large.");
            }
            else if (width < 1)
            {
                return BadRequest("Width is too small.");
            }
            obj.Width = (int)width;

            int? height = jObject["Height"].ToString().ToNullableInt();
            if (height == null)
            {
                return BadRequest("Wrong json height input.");
            }
            else if (height > 100)
            {
                return BadRequest("Height is too large.");
            }
            else if (height < 1)
            {
                return BadRequest("Height is too small.");
            }
            obj.Height = (int)height;

            _context.Entry(obj).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Object data changed successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> ChangeImage(JObject jObject)
        {
            if (jObject["Name"] == null || jObject["ImgData"] == null || jObject["ObjectId"] == null) return BadRequest("Bad json input.");

            int? objId = jObject["ObjectId"].ToString().ToNullableInt();
            if (objId == null)
            {
                return BadRequest("Wrong object id json input.");
            }
            var obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == objId);
            if (obj == null)
            {
                return NotFound("Object not found in database.");
            }

            if (obj.ImagePath != null && System.IO.File.Exists("ObjectImages\\" + obj.Id + "_" + obj.ImagePath))
            {
                System.IO.File.Delete("ObjectImages\\" + obj.Id + "_" + obj.ImagePath);
            }

            if (jObject["Name"].ToString() == "")
            {
                obj.ImagePath = null;

                _context.Entry(obj).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                    return Ok("Object image data deleted successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest("Database concurrency exception occured.");
                }
            }

            string extensions = jObject["Name"].ToString().ToLower();
            if (extensions.Contains(".jpg") || extensions.Contains(".jpeg") || extensions.Contains(".png") || extensions.Contains(".bmp"))
            {
                obj.ImagePath = jObject["Name"].ToString();
                var img = Image.FromStream(new MemoryStream(Convert.FromBase64String(jObject["ImgData"].ToString())));
                ImageFormat format = ImageFormat.Bmp;
                if (extensions.Contains(".jpg") || extensions.Contains(".jpeg")) format = ImageFormat.Jpeg;
                else if (extensions.Contains(".png")) format = ImageFormat.Png;

                img.Save("ObjectImages\\" + obj.Id + "_" + obj.ImagePath, format);

                _context.Entry(obj).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                    return Ok("Object image data changed successfully.");
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

        // DELETE: api/Objects/{id} - removes specific object by id and instances of this object
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObjects([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var obj = await _context.Objects.SingleOrDefaultAsync(m => m.Id == id);
            if (obj == null)
            {
                return NotFound("Object not found in the database.");
            }
            
            if (obj.ImagePath != null && System.IO.File.Exists("ObjectImages\\" + obj.Id + "_" + obj.ImagePath))
            {
                System.IO.File.Delete("ObjectImages\\" + obj.Id + "_" + obj.ImagePath);
            }

            _context.Objects.Remove(obj);
            await _context.SaveChangesAsync();

            return Ok("Object has been removed.");
        }

        private bool ObjectsExists(int id)
        {
            return _context.Objects.Any(e => e.Id == id);
        }
    }
}