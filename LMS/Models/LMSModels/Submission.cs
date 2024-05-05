using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public uint Score { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime Time { get; set; }
        public uint AssignId { get; set; }
        public string StudentId { get; set; } = null!;
        public uint SubmissionId { get; set; }

        public virtual Assignment Assign { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
