using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Messages { get; set; }

        public ValidationResult()
        {
            Messages = new List<string>();
        }

        public ValidationResult(bool isValid)
        {
            Messages = new List<string>();
            IsValid = isValid;
        }
    }
}
