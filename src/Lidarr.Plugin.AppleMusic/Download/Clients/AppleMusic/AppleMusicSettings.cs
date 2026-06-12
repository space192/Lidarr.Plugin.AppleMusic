using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.AppleMusic
{
    public class AppleMusicSettingsValidator : AbstractValidator<AppleMusicSettings>
    {
        public AppleMusicSettingsValidator()
        {
            RuleFor(x => x.ApiBaseUrl).NotEmpty().WithMessage("API Base URL is required");
            RuleFor(x => x.DownloadPath).IsValidPath();
        }
    }

    public class AppleMusicSettings : IProviderConfig
    {
        private static readonly AppleMusicSettingsValidator Validator = new AppleMusicSettingsValidator();

        [FieldDefinition(0, Label = "API Base URL", Type = FieldType.Textbox, HelpText = "Base URL of the Apple Music Python API service (e.g., http://localhost:8000)")]
        public string ApiBaseUrl { get; set; } = "http://localhost:8000";

        [FieldDefinition(1, Label = "Download Path", Type = FieldType.Textbox, HelpText = "Path where downloaded music files will be stored")]
        public string DownloadPath { get; set; } = "";

        [FieldDefinition(2, Label = "Preferred Codec", Type = FieldType.Select, SelectOptions = typeof(AppleMusicCodec), HelpText = "Preferred audio codec for downloads")]
        public int PreferredCodec { get; set; } = (int)AppleMusicCodec.ALAC;

        [FieldDefinition(3, Label = "Output Format", Type = FieldType.Select, SelectOptions = typeof(AppleMusicOutputFormat), HelpText = "Output audio format for downloaded files")]
        public int OutputFormat { get; set; } = (int)AppleMusicOutputFormat.FLAC;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum AppleMusicCodec
    {
        [FieldOption(Label = "AAC 256kbps")]
        AAC = 0,

        [FieldOption(Label = "ALAC Lossless")]
        ALAC = 1,

        [FieldOption(Label = "Dolby Atmos")]
        Atmos = 2
    }

    public enum AppleMusicOutputFormat
    {
        [FieldOption(Label = "FLAC")]
        FLAC = 0,

        [FieldOption(Label = "ALAC (M4A)")]
        M4A = 1,

        [FieldOption(Label = "WAV")]
        WAV = 2,

        [FieldOption(Label = "MP3 (V0)")]
        MP3 = 3
    }
}
