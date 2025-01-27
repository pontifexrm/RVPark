using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RVParking.Data
{
    public class Bkg_Semaphore
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Sema4Id { get; set; }
        public int Sema4Flag { get; set; } = 0;

        public void IncrementSema4Id()
        {
            Sema4Flag++;
        }

        public void DecrementSema4Id()
        {
            Sema4Flag--;
        }
    }
}
