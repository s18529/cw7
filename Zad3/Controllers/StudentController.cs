using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Zad3.DAL;
using Zad3.Models;

namespace Zad3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentController : ControllerBase
    {
        private readonly IDbService db;
        public StudentController(IDbService service)
        {
            db = service;
        }

        [HttpGet]
        public IActionResult GetStudent()
        {
            using (var client = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18529;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                commend.Connection = client;
                commend.CommandText = "select * from student;";
                client.Open();
                var dr = commend.ExecuteReader();
                ICollection<Student> students = new List<Student>();
                while (dr.Read())
                {
                    var student = new Student();
                    student.IndexNumber = dr[0].ToString();
                    student.FirstName = dr[1].ToString();
                    student.LastName = dr[2].ToString();
                    student.Bdate = DateTime.Parse(dr[3].ToString());
                    students.Add(student);
                }

                return Ok(students);

            }
        }
        
        [HttpGet("enrollment/{id}")]
        public IActionResult GetEnrollment(string id)
        {
            using (var client = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18529;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                commend.Connection = client;
                commend.CommandText = "select * from student where IndexNumber=@id;";
                commend.Parameters.AddWithValue("id", id);
                client.Open();
                var dr = commend.ExecuteReader();
                var student = new Student();
                while (dr.Read())
                {
                    student.IndexNumber = dr[0].ToString();
                    student.FirstName = dr[1].ToString();
                    student.LastName = dr[2].ToString();
                    student.Bdate = DateTime.Parse(dr[3].ToString());
                }

                return Ok(student);
            }
        }
        [HttpGet("{id}")]
        public IActionResult GetSudents(int id)
        {
            if (id == 1)
            {
                return Ok("Kowalski");
            }
            else if (id == 2)
            {
                return Ok("Malewski");
            }
            return NotFound("Nie Znaleziono studenta");
        }
        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            //... add to database
            //... generatig index number
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie Zakończone");
        }
        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id)
        {
            return Ok("Aktualizowanie Zakończone");
        }
    }
}
