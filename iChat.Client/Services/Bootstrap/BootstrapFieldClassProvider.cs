using Microsoft.AspNetCore.Components.Forms;

namespace iChat.Client.Services.Bootstrap
{
    public class BootstrapFieldClassProvider : FieldCssClassProvider
    {
        public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
        {
            var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
            var isModified = editContext.IsModified(fieldIdentifier);

            if (isValid && isModified)
            {
                return "form-control is-valid";
            }
            else if (!isValid && isModified)
            {
                return "form-control is-invalid";
            }

            return "form-control";
        }
    }

}
