using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zad3.Models;

namespace Zad3.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> students;
        public MockDbService()
        {
            students = new List<Student>
            {
                  new Student { IdStudent = 1, FirstName = "Jan", LastName = "Kowalski" },
                  new Student { IdStudent = 2, FirstName = "Anna", LastName = "Malkowski" },
                  new Student { IdStudent = 3, FirstName = "Andzrej", LastName = "Adrzejwicz" }
            };
            
        }
        public IEnumerable<Student> GetStudents()
        {

            return students;
        }
    }
}
