using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Model
{
    [Table("Simple")]
    public class SimpleTable
    {
        [Key]
        public int Id { get; set; }
        public int BusinessKey { get; set; }

        public string Payload { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool Active { get; set; }

    }
}
