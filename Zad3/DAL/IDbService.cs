using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zad3.Models;

namespace Zad3.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}
