namespace UnistayWeb.BLL.ViewModels
{
    public class RoleSelectionDto
    {
        public string Role { get; set; } = string.Empty;
    }

    public class SeekerProfileDto
    {
        public DateTime DateOfBirth { get; set; }
        public string Preferences { get; set; } = string.Empty;
    }

    public class LandlordVerifyDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
    }

    public class OnboardingStatusDto
    {
        public bool IsOnboardingComplete { get; set; }
        public int OnboardingStep { get; set; }
    }

    public class OnboardingResultDto
    {
        public string Message { get; set; } = string.Empty;
        public int NextStep { get; set; }
        public string? Analysis { get; set; }
    }
}
