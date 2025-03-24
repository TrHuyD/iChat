using iChat.BackEnd.Services.Validators.TextMessageValidators;

namespace iChat.BackEnd.Services.Validators
{
    public class ValidatorsHelper
    {
        public ValidatorsHelper(WebApplicationBuilder builder) 
        {
            builder.Services.AddSingleton<IBadWordLoarder, SimpleBadWordLoader>();
            builder.Services.AddSingleton<TextMessageValidatorService>();
        }
    }
}
