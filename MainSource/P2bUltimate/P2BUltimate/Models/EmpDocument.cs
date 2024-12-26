using P2b.Global;
using System;
using System.ComponentModel.DataAnnotations;

namespace P2BUltimate.Models
{
    public class EmpDocument
    {
        public EmpDocument() { }
        [Key]
        public int Id { get; set; }
        public LookupValue Type { get; set; }
        public String Path { get; set; }
        public String EmpCode { get; set; }
    }
}

