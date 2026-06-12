using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.AppleMusic
{
    public class AppleMusicIndexerSettingsValidator : AbstractValidator<AppleMusicIndexerSettings>
    {
        public AppleMusicIndexerSettingsValidator()
        {
            RuleFor(x => x.ApiBaseUrl).NotEmpty().WithMessage("API Base URL is required");
        }
    }

    public class AppleMusicIndexerSettings : IIndexerSettings
    {
        private static readonly AppleMusicIndexerSettingsValidator Validator = new AppleMusicIndexerSettingsValidator();

        [FieldDefinition(0, Label = "API Base URL", Type = FieldType.Textbox, HelpText = "Base URL of the Apple Music Python API service (e.g., http://localhost:8000)")]
        public string ApiBaseUrl { get; set; } = "http://localhost:8000";

        [FieldDefinition(1, Type = FieldType.Number, Label = "Early Download Limit", Unit = "days", HelpText = "Time before release date Lidarr will download from this indexer, empty is no limit", Advanced = true)]
        public int? EarlyReleaseLimit { get; set; }

        // Required by IIndexerSettings but not used since we build URLs dynamically
        public string BaseUrl { get; set; } = "";

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
