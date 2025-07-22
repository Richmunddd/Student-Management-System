using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupApp
{
    public class ActivityGradeModel
    {
        public int ActivityID { get; set; }
        public string? ActivityName { get; set; }
        public int ScoreLimit { get; set; }
        public int Grade { get; set; }
    }
}
