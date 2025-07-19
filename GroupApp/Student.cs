using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupApp
{
    public class Student
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }

        // Override ToString to display the student's full name in the ListBox
        public override string ToString()
        {
            return FullName; // This will display the student's full name in the ListBox
        }
    }
}
