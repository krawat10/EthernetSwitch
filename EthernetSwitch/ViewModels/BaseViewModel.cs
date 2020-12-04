using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace EthernetSwitch.Models
{
    public class BaseViewModel
    {
        public string Error { get; set; }
        public bool Success { get; set; }

        internal bool Validate(ModelStateDictionary modelState)
        {
            if(!modelState.IsValid)
            {
                Error = string.Join("\n", modelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
            }

            return modelState.IsValid;
        }


        internal void AddError(string message)
        {
            Error = string.Join("\n", Error, message);
        }
    }
}