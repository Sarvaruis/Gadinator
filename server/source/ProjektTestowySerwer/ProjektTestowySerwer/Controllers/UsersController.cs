using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjektTestowySerwer.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ProjektTestowySerwer.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly ProjektTestowySerwerContext _context;

        public UsersController(ProjektTestowySerwerContext context)
        {
            _context = context;
        }

        // GET: api/Users/Logout   - logs out current logged user
        // GET: api/Users/Current  - returns current logged in user
        // GET: api/Users/GetAll   - returns all users in database
        // GET: api/Users/Get/{id} - returns specific user by id
        [HttpGet]
        [HttpGet("{command}")]
        [HttpGet("{command}/{id}")]
        public async Task<IActionResult> GetUsers([FromRoute] string command, int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cmd = command != null ? command.ToLower() : null;
            switch (cmd)
            {
                case "current": return await GetLoggedInUser();
                case "logout": return LogOut();
                case "get": return await GetUserById(id);
                case "getall": return GetAllUsers();
                default: return BadRequest("Unknown command.");
            }   
        }

        public async Task<IActionResult> GetLoggedInUser()
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

            var users = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);

            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }

        public IActionResult LogOut()
        {
            string access = HttpContext.Session.GetString("access");
            if (access == null || !access.Equals("1"))
            {
                return BadRequest("User needs to be logged in.");
            }

            HttpContext.Session.Remove("loggedUserId");
            HttpContext.Session.Remove("choosenProjectId");
            HttpContext.Session.SetString("access", "0");
            return Ok("Successfully logged out.");
        }

        public async Task<IActionResult> GetUserById(int? id)
        {
            if (id == null) return BadRequest("Wrong id input.");
            var users = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);

            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
        }

        public IActionResult GetAllUsers()
        {
            return Ok(_context.Users);
        }

        // POST: api/Users       - adds user to database - login & password in json format in body input
        // POST: api/Users/Login - logs user in - login & password in json format in body input
        // ex.: {"Login": "test", "Password": "pass"}
        [HttpPost]
        [HttpPost("{command}")]
        public async Task<IActionResult> PostUsers([FromRoute] string command)
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
            
            string cmd = command != null ? command.ToLower() : null ;
            switch (cmd)
            {
                case "login": return await LogIn(jObject);
                case null: return await AddUser(jObject);
                default: return BadRequest();
            }
        }

        public async Task<IActionResult> AddUser(JObject jObject)
        {       
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (jObject["Login"] == null || jObject["Password"] == null) return BadRequest("Wrong input.");
            var searchUser = await _context.Users.SingleOrDefaultAsync(m => m.Login == jObject["Login"].ToString());
            if (searchUser != null) return BadRequest("User already exists.");
            if (jObject["Login"] == null || jObject["Password"] == null) return BadRequest("Wrong input.");
            if (jObject["Login"].ToString().Length > 50) return BadRequest("Too long login.");
            if (jObject["Password"].ToString().Length > 50) return BadRequest("Too long password.");
            if (jObject["Login"].ToString().Length == 0) return BadRequest("Login can not be empty.");
            if (jObject["Password"].ToString().Length == 0) return BadRequest("Password can not be empty.");

            Users user = new Users
            {
                Login = jObject["Login"].ToString(),
                Password = jObject["Password"].ToString()
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User created successfully.");
        }

        public async Task<IActionResult> LogIn(JObject jObject)
        {
            if (jObject["Login"] == null || jObject["Password"] == null) return BadRequest();

            var user = await _context.Users.SingleOrDefaultAsync(m => m.Login == jObject["Login"].ToString());
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Password.Equals(jObject["Password"].ToString()))
            {
                HttpContext.Session.SetString("access", "1");
                HttpContext.Session.SetInt32("loggedUserId", user.Id);
                return Ok("Logged in successfully.");
            }
            else
            {
                return BadRequest("Wrong password.");
            }
        }

        // PUT: api/Users      - edits currently logged in user - login & password in json format in body input
        // PUT: api/Users/{id} - edits specific user by his id  - login & password in json format in body input
        // ex.: {"Login": "test", "Password": "pass"}
        [HttpPut]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers([FromRoute] int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Users currentUser;
            if (id == null)
            { 
                /* sessions variable check */
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
                currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
            }
            else
            {
                currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
            }
            /* body variable check */
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
                    return BadRequest("Wrong Json body input.");
                }
            }
            if (jObject["Login"] == null || jObject["Password"] == null) return BadRequest("Wrong input.");

            if (jObject["Login"].ToString() != currentUser.Login)
            {
                var searchUser = await _context.Users.SingleOrDefaultAsync(m => m.Login == jObject["Login"].ToString());
                if (searchUser != null) return BadRequest("Login already in use.");
                if (jObject["Login"].ToString().Length > 50) return BadRequest("Too long login.");
                if (jObject["Login"].ToString().Length == 0) return BadRequest("Login can not be empty.");

                currentUser.Login = jObject["Login"].ToString();
            }

            if (jObject["Password"].ToString() != currentUser.Password)
            {
                if (jObject["Password"].ToString().Length > 50) return BadRequest("Too long Password.");
                if (jObject["Password"].ToString().Length == 0) return BadRequest("Password can not be empty.");

                currentUser.Password = jObject["Password"].ToString();
            }

            _context.Entry(currentUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("User data changed successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

        }

        // DELETE: api/Users      - deletes currently logged in user
        // DELETE: api/Users/{id} - deletes specific user by id
        [HttpDelete]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsers([FromRoute] int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Users currentUser;
            if (id == null)
            {
                /* sessions variable check */
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
                currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == userId);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
            }
            else
            {
                currentUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
                if (currentUser == null)
                {
                    return NotFound("User not found");
                }
            }
            
            var userProjects = await _context.Projects.Where(m => m.UserId == currentUser.Id).ToListAsync();
            foreach (var project in userProjects)
            {
                if (project.BackgroundFilePath != null && System.IO.File.Exists("Backgrounds\\" + project.UserId + "_" + project.BackgroundFilePath))
                {
                    System.IO.File.Delete("Backgrounds\\" + project.UserId + "_" + project.BackgroundFilePath);
                }
            }
            
            _context.Users.Remove(currentUser);
            await _context.SaveChangesAsync();
            if (id == null)
            {
                HttpContext.Session.Remove("loggedUserId");
                HttpContext.Session.SetString("access", "0");
            }
            return Ok("User successfully deleted.");
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}