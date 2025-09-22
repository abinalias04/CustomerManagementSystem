using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Entity.Models
{
    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }

        public ICollection<District> Districts { get; set; } = new List<District>();
    }

    public class District
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }

        public int StateId { get; set; }
        public State State { get; set; }

        public ICollection<Pincode> Pincodes { get; set; } = new List<Pincode>();
    }

    public class Pincode
    {
        public int PincodeId { get; set; }

        [Column("Pincode")] // Map to existing DB column if necessary
        public string PincodeValue { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; }

        public ICollection<PostOffice> PostOffices { get; set; } = new List<PostOffice>();
    }

    public class PostOffice
    {
        public int PostOfficeId { get; set; }
        public string PostOfficeName { get; set; }

        public int PincodeId { get; set; }
        public Pincode Pincode { get; set; }
    }
}
