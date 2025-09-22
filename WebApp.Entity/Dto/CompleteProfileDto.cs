namespace WebApp.Entity.Dto
{
    public class CompleteProfileDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public int StateId { get; set; }
        public int DistrictId { get; set; }
        public string Pincode { get; set; }   
        public int PostOfficeId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

    //public class StateDto { public int Id { get; set; } public string Name { get; set; } }
    //public class DistrictDto { public int Id { get; set; } public string Name { get; set; } }

    public class PincodeDetailsDto
    {
        public int PincodeId { get; set; }
        public string PincodeValue { get; set; } = string.Empty;

        public int StateId { get; set; }
        public string StateName { get; set; } = string.Empty;

        public int DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;

        public List<PostOfficeDto> PostOffices { get; set; } = new();
    }

    public class PostOfficeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }


    public class ProfileDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string StateName { get; set; }
        public string DistrictName { get; set; }
        public string Pincode { get; set; }
        public string PostOfficeName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int PostOfficeId { get; set; }
    }
  

}
