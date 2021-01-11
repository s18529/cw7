using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zad3.Helpers;
using Zad3.Models;

namespace Zad3.Services
{
    public interface IStudentDbService
    {
        public MyHelper Promote(Study study);
        public MyHelper AddStudent(Student student);
    }
}
