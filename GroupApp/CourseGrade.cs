using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupApp
{
    public class CourseGrade
    {
        public int CourseId { get; set; }  // New property
        public string CourseName { get; set; } = string.Empty;
        public double CourseWork { get; set; }
        public double Quiz { get; set; }
        public double Discussion { get; set; }
        public double TotalPercentage { get; set; }
        public string PointEquivalent { get; set; } = string.Empty;
        public int ActivityCount { get; set; }  // New property

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(CourseName) &&
                   !string.IsNullOrEmpty(PointEquivalent) &&
                   TotalPercentage >= 0 && TotalPercentage <= 100;
        }
    }
}
