using Cubes.Core.Email;

namespace Cubes.Core.Web.UIHelpers.Schema.Providers
{
    public class SmtpSettingsProfilesSchemaProvider : SchemaProvider<SmtpSettingsProfiles>
    {
        public override Schema GetSchema()
            => Schema.Create(this.Name, "SMTP Profiles")
                .WithText("name", Validator.Required(), Validator.Pattern(@"^\S+$"))
                .WithText("comments")
                .WithText("host", Validator.Required())
                .WithText("port", Validator.Required(), Validator.Min(25))
                .WithText("timeout", Validator.Required())
                .WithText("sender", Validator.Required())
                .WithCheckbox("useSsl", "Use SSL")
                .WithText("userName")
                .WithPassword("password");
    }
}