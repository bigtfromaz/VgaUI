using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelInterface
{
    public class MemberData
    {
        public decimal ID { get; set; } = 0;
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int GHIN_ID { get; set; } = 0;
        public decimal? GHIN_Index { get; set; } = 0;
        public bool HasGHIN_Index { get; set; } = false;
    }
}
