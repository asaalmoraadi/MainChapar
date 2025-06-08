using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
namespace MainChapar.Helpers
{
    public class AtLeastOneFileRequiredAttribute: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var files = value as IList<IFormFile>;
            return files != null && files.Any(f => f != null && f.Length > 0);

        }

    }
}
