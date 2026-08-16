using System.ComponentModel.DataAnnotations;

namespace IntercityTransportManagementSystem.Enums
{
    public enum MessageType
    {
        [Display(Name = "Съобщение")]
        StandardMessage = 1,

        [Display(Name = "Сигнализиране на проблем")]
        ProblemReport = 2
    }
}
