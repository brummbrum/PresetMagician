using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using PresetMagician.Core.Data;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class CharacteristicViewModel : ViewModelBase
    {
        private ModelBackup _modelBackup;
        private readonly CharacteristicsService _characteristicsService;

        public CharacteristicViewModel(Characteristic characteristic, CharacteristicsService characteristicsService)
        {
            DeferValidationUntilFirstSaveCall = false;

            _modelBackup = characteristic.CreateBackup();
            _characteristicsService = characteristicsService;

            Characteristic = characteristic;
            RedirectTargets = characteristicsService.GetRedirectTargets(characteristic);
            CharacteristicsRedirectingToThis = characteristicsService.GetRedirectSources(characteristic);
            AllowRedirect = CharacteristicsRedirectingToThis.Count == 0;
        }

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (IsRedirect && RedirectCharacteristic == null)
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(RedirectCharacteristic),
                    "You need to specify a characteristic to redirect to"));
            }

            if (IsIgnored && IsRedirect)
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(RedirectCharacteristic),
                    "You cannot ignore and redirect a characteristic at the same time"));
            }

            if (_characteristicsService.HasCharacteristic(Characteristic))
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(CharacteristicName),
                    "Another characteristic with the same name already exists"));
            }

            base.ValidateFields(validationResults);
        }

        protected override async Task<bool> CancelAsync()
        {
            Characteristic.RestoreBackup(_modelBackup);
            return await base.CancelAsync();
        }


        [Model] public Characteristic Characteristic { get; set; }

        [ViewModelToModel("Characteristic")]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9/&+\-\s]+$", ErrorMessage =
            "Only alphanumeric characters, numbers, &, +, /, - and spaces are allowed.")]
        public string CharacteristicName { get; set; }


        [ViewModelToModel("Characteristic")] public bool IsRedirect { get; set; }
        [ViewModelToModel("Characteristic")] public bool IsIgnored { get; set; }

        [ViewModelToModel("Characteristic")] public Characteristic RedirectCharacteristic { get; set; }

        public List<Characteristic> RedirectTargets { get; set; }
        public List<Characteristic> CharacteristicsRedirectingToThis { get; set; }
        public new string Title { get; set; }
        public bool AllowRedirect { get; set; }
    }
}