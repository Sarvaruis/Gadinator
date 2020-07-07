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
    [Route("api/Categories")]
    public class CategoriesController : Controller
    {
        private readonly ProjektTestowySerwerContext _context;

        public CategoriesController(ProjektTestowySerwerContext context)
        {
            _context = context;
        }

        // GET: api/Categories/GetAll   - returns all categories in database
        // GET: api/Categories/Get/{id} - returns specific category by id
        [HttpGet]
        [HttpGet("{command}")]
        [HttpGet("{command}/{id}")]
        public async Task<IActionResult> GetCategories([FromRoute] string command, int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "get": return await GetCategoriesById(id);
                case "getall": return GetAllCategories();
                default: return BadRequest("Unknown command.");
            }
        }

        public async Task<IActionResult> GetCategoriesById(int? id)
        {
            if (id == null) return BadRequest("Wrong id input.");
            var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        public IActionResult GetAllCategories()
        {
            return Ok(_context.Categories);
        }

        // POST: api/Categories - adds category to database
        // ex.: {"Name": "test"}
        [HttpPost]
        public async Task<IActionResult> PostCategories()
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

            if (jObject["Name"] == null) return BadRequest("Wrong name input.");
            var searchCategory = await _context.Categories.SingleOrDefaultAsync(m => m.Name == jObject["Name"].ToString());
            if (searchCategory != null) return BadRequest("Category already exists.");
            if (jObject["Name"].ToString().Length > 50) return BadRequest("Too long name.");
            if (jObject["Name"].ToString().Length == 0) return BadRequest("Name cannot be empty.");

            Categories category = new Categories
            {
                Name = jObject["Name"].ToString()
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok("Category created successfully.");
        }

        // PUT api/Categories - adds category to database
        // ex.: {"CategoryId":1, "Name": "test"}
        [HttpPut]
        public async Task<IActionResult> PutCategories()
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

            if (jObject["Name"] == null || jObject["CategoryId"] == null) return BadRequest("Wrong json input.");

            int? categoryId = jObject["CategoryId"].ToString().ToNullableInt();
            if (categoryId == null)
            {
                return BadRequest("Wrong category Id json input.");
            }
            Categories categoryToEdit = await _context.Categories.SingleOrDefaultAsync(m => m.Id == categoryId);
            if (categoryToEdit == null)
            {
                return NotFound("Category not found in the database");
            }

            if (jObject["Name"].ToString() != categoryToEdit.Name)
            {
                var searchCategory = await _context.Categories.SingleOrDefaultAsync(m => m.Name == jObject["Name"].ToString());
                if (searchCategory != null) return BadRequest("Category already exists.");
                if (jObject["Name"].ToString().Length > 50) return BadRequest("Too long name.");
                if (jObject["Name"].ToString().Length == 0) return BadRequest("Name cannot be empty.");

                categoryToEdit.Name = jObject["Name"].ToString();

                _context.Entry(categoryToEdit).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return Ok("Category data changed successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }

            return Ok("Nothing changed.");
        }

        // DELETE: api/Categories/{id} - removes specific category by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategories([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories.SingleOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound("Category not found in the database.");
            }

            var objects = await _context.Objects.Where(m => m.CategoryId == category.Id).ToListAsync();
            foreach (var obj in objects)
            {
                if (obj.ImagePath != null && System.IO.File.Exists("ObjectImages\\" + obj.Id + "_" + obj.ImagePath))
                {
                    System.IO.File.Delete("ObjectImages\\" + obj.Id + "_" + obj.ImagePath);
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok("Category and it's objects have been successfully removed.");
        }

        private bool CategoriesExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}