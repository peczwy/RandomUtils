using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Model
{
    [Table("SimpleSnapshot")]
    public class SimpleSnapshotTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BusinessKey { get; set; }

        public string Payload { get; set; }


    }
}
