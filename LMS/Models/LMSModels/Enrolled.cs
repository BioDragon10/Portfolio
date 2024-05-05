using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public string Grade { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public uint ClassId { get; set; }
        public uint EnrolledId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
