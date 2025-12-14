using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVParking.Data
{
    public class AppParameter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ParamKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ParamValue { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ParamDescription { get; set; }

        [MaxLength(50)]
        public string? ParamType { get; set; } // e.g., "Setting", "FeatureFlag", "Enum", "Config"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}