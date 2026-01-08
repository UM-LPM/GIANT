using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class ProgramSolutionPartAction
    {
        public long FileID { get; set; } // random value between [111111111111111111,999999999999999999]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "ProgramSolutionPartAction name is required.")]
        public string? Name { get; set; }
        public List<Property>? Properties { get; set; }

        public Position? NodePosition { get; set; }
    }

    public class ProgramSolutionPartActivator
    {
        public long FileID { get; set; } // random value between [111111111111111111,999999999999999999]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "ProgramSolutionPartAction name is required.")]
        public string? Name { get; set; }
        public List<Property>? Properties { get; set; }

        public Position? NodePosition { get; set; }
    }

    public class ProgramSolutionPartActivatorConnection
    {
        public Guid Activator { get; set; }
        public bool IsNegated { get; set; }

    }

    public class ProgramSolutionPartConnection
    {
        public long FileID { get; set; } // random value between [111111111111111111,999999999999999999]

        public string? Name { get; set; }
        public double Weight { get; set; }
        public List<ProgramSolutionPartActivatorConnection>? ActivatorConnections { get; set; }
        public List<Guid>? Actions { get; set; }

        public Position? NodePosition { get; set; }
    }
}
