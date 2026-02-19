using backend.Dtos.Users;
using backend.Dtos.Vehicles;
using backend.Models;

namespace backend.Dtos.Assignments
{
    public class AssignedToDto
    {
        public bool IsAssigned { get; set; }

        public UserDto? AssignedDriver { get; set; }
        public VehiclesDto? AssignedVehicle { get; set; }

        public List<UserDto>? FreeDrivers { get; set; }
        public List<VehiclesDto>? FreeVehicles { get; set; }
    }
}
