using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zad3.Models;

namespace Zad3.Helpers
{
    public class MyHelper
    {
        public int Value { get; set; }
        public string Message { get; set; }
        public MyHelper(string message, int value)
        {
            this.Message = message;
            this.Value = value;
        }
        public Enrollment enrollment;
    }
}
