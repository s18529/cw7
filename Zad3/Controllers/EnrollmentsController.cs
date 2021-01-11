using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zad3.DTO;
using Zad3.Helpers;
using Zad3.Models;
using Zad3.Services;

namespace Zad3.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService service;
        private string connectionString = "Data Source=db-mssql;Initial Catalog=s18529;Integrated Security=True";
        private IConfiguration Configuration;
        public EnrollmentsController(IStudentDbService dbService, IConfiguration _configuration)
        {
            service = dbService;
            Configuration = _configuration;
        }
        [HttpPost("reToken/{reToken}")]
        public IActionResult reToken(string reToken)
        {
            var refreshToken = Guid.NewGuid();
            JwtSecurityToken token = null;
            using(var client = new SqlConnection(connectionString))
                using(var command = new SqlCommand())
            {
                command.Connection = client;
                client.Open();
                command.CommandText = "Select IndexNumber, FirstName, LastName from student where reToken = @reToken";
                command.Parameters.AddWithValue("reToken", reToken);
                var dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return BadRequest("Błędny refresh Token");
                }
                var index = dr[0].ToString();
                string id = dr[1].ToString() + " " + dr[2].ToString();
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, dr[0].ToString()),
                    new Claim(ClaimTypes.Name, id),
                    new Claim(ClaimTypes.Role, "employee")
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                token = new JwtSecurityToken
                    (
                        issuer: "s18637",
                        audience: "Student",
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(10),
                        signingCredentials: creds
                    );
                dr.Close();
                command.CommandText = " update Student set reToken = @reToken2 where IndexNumber = @index";
                command.Parameters.AddWithValue("reToken2", refreshToken);
                command.Parameters.AddWithValue("index", index);
                command.ExecuteNonQuery();
            }
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        [HttpPost("login")]
        public IActionResult Login(LoggingRequest request)
        {
            var refreshToken = Guid.NewGuid();
            JwtSecurityToken token = null;
            using (var client = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                client.Open();
                command.CommandText = "Select FirstName, LastName, Password, salt from student where IndexNumber = @login";
                command.Parameters.AddWithValue("login", request.Login);
                var dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return NotFound("nie ma takiego studenta");
                }
                var imie = dr[0].ToString();
                var nazwisko = dr[1].ToString();
                var haslo = dr[2].ToString();
                var salt = dr[3].ToString();
                var valueBytes = KeyDerivation.Pbkdf2(
                    password: request.Password,
                    salt: Encoding.UTF8.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8);
                if (!haslo.Equals(Convert.ToBase64String(valueBytes)))
                {
                    return BadRequest("podano złe hasło");
                }

                string id = imie + " " + nazwisko;
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, request.Login),
                    new Claim(ClaimTypes.Name, id),
                    new Claim(ClaimTypes.Role, "employee")
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                token = new JwtSecurityToken
                    (
                    issuer: "s18529",
                    audience: "Student",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                    );
                dr.Close();
                command.CommandText = "update Student set reToken = @reToken where IndexNumber=@login;";
                command.Parameters.AddWithValue("reToken", refreshToken);
                command.ExecuteNonQuery();
            }
            return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken
                });
        }
        //metoda pomocnicza
        [HttpGet("update")]
        public IActionResult updatePass()
        {
            using (var client = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = client;
                    client.Open();
                    command.CommandText = "Select Password, indexNumber from student;";
                    var dr = command.ExecuteReader();
                    ICollection<string[]> colection = new List<string[]>();
                    while (dr.Read())
                    {
                        byte[] random = new byte[128 / 8];
                        string salt;
                        using (var generator = RandomNumberGenerator.Create())
                        {
                            generator.GetBytes(random);
                            salt = Convert.ToBase64String(random);
                        }
                        var haslo = dr[0].ToString();
                        var index = dr[1].ToString();
                        var valueBytes = KeyDerivation.Pbkdf2(
                            password: haslo,
                            salt: Encoding.UTF8.GetBytes(salt),
                            prf: KeyDerivationPrf.HMACSHA512,
                            iterationCount: 10000,
                            numBytesRequested: 256 / 8);
                        string[] list = new string[3];
                        list[0] = index;
                        list[1] = salt;
                        list[2] = Convert.ToBase64String(valueBytes);
                        colection.Add(list);


                    }
                    dr.Close();
                    foreach (string[] list in colection)
                    {
                        using (var com = new SqlCommand())
                        {
                            com.Connection = client;
                            com.CommandText = "update student set Password = @newVal where indexNumber=@index; update student set salt = @salt where indexNumber=@index;";
                            com.Parameters.AddWithValue("newVal", list[2]);
                            com.Parameters.AddWithValue("index", list[0]);
                            com.Parameters.AddWithValue("salt", list[1]);
                            com.ExecuteNonQuery();
                        }
                    }
                }
            }
            return Ok();
        }
        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult AddStudent(Student student)
        {
            MyHelper helper = service.AddStudent(student);
            if (helper.Value != 0)
            {
                return BadRequest(helper.Message);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Created, helper.enrollment);
            }
        }
        
        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult promote(Study study)
        {
            MyHelper helper = service.Promote(study);
            if (helper.Value != 0)
            {
                return BadRequest(helper.Message);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Created, helper.enrollment);
            }
        }
        
    }
}
