#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using DaleGhent.NINA.MoonAngle.Utility;
using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Utility;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DaleGhent.NINA.MoonAngle.MoonAngleCondition {

    [ExportMetadata("Name", "Moon angle")]
    [ExportMetadata("Description", "Loops until the angular separation between the target and the moon satisfies the defined parameters")]
    [ExportMetadata("Icon", "MoonAngle_SVG")]
    [ExportMetadata("Category", "Loop Condition")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class MoonAngleCondition : SequenceCondition, IValidatable {
        private double currentSeparation = 0;
        private double separationLimit = 20;
        private ComparisonOperatorEnum comparisonOperator = ComparisonOperatorEnum.LESS_THAN_OR_EQUAL;

        private readonly IProfileService profileService;
        private readonly IWeatherDataMediator weatherDataMediator;

        [ImportingConstructor]
        public MoonAngleCondition(IProfileService profileService, IWeatherDataMediator weatherDataMediator) {
            this.profileService = profileService;
            this.weatherDataMediator = weatherDataMediator;

            ConditionWatchdog = new ConditionWatchdog(InterruptWhenMoonOutsideOfBounds, TimeSpan.FromSeconds(5));
        }

        private async Task InterruptWhenMoonOutsideOfBounds() {
            if (TargetInfo == null) {
                Logger.Error("No target is defined. Ending loop.");
                return;
            }

            CurrentSeparation = CalculateMoonTargetSeparation();
            if (!Check(null, null)) {
                if (this.Parent != null) {
                    if (ItemUtility.IsInRootContainer(Parent) && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                        Logger.Info($"Moon and target angular separation is outside the prescribed condition ({CurrentSeparation:0.00} {Utility.Utility.PrintComparator(ComparisonOperator)} {SeparationLimit:0.00}) - Interrupting current Instruction Set");
                        await this.Parent.Interrupt();
                    }
                }
            }
        }

        [JsonProperty]
        public double SeparationLimit {
            get => separationLimit;
            set {
                separationLimit = value < 0d ? 0 : value > 180d ? 180 : Math.Round(value, 2);
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public ComparisonOperatorEnum ComparisonOperator {
            get => comparisonOperator;
            set {
                comparisonOperator = value;
                RaisePropertyChanged();
            }
        }

        public ComparisonOperatorEnum[] ComparisonOperators => Enum.GetValues(typeof(ComparisonOperatorEnum))
            .Cast<ComparisonOperatorEnum>()
            .ToArray();

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            if (TargetInfo == null) {
                Logger.Error("No target is defined. Ending loop.");
                return true;
            }

            bool result;
            var currentSeparation = Math.Round(CurrentSeparation, 2);
            Logger.Debug($"Parameters: {currentSeparation:0.00} {Utility.Utility.PrintComparator(ComparisonOperator)} {SeparationLimit:0.00}");

            switch (ComparisonOperator) {
                case ComparisonOperatorEnum.LESS_THAN:
                    result = currentSeparation < SeparationLimit;
                    break;

                case ComparisonOperatorEnum.LESS_THAN_OR_EQUAL:
                    result = currentSeparation <= SeparationLimit;
                    break;

                case ComparisonOperatorEnum.EQUALS:
                    result = currentSeparation == SeparationLimit;
                    break;

                case ComparisonOperatorEnum.GREATER_THAN_OR_EQUAL:
                    result = currentSeparation >= SeparationLimit;
                    break;

                case ComparisonOperatorEnum.GREATER_THAN:
                    result = currentSeparation > SeparationLimit;
                    break;

                case ComparisonOperatorEnum.NOT_EQUAL:
                    result = currentSeparation != SeparationLimit;
                    break;

                default:
                    return false;
            }

            return !result;
        }

        public double CurrentSeparation {
            get => currentSeparation;
            private set {
                currentSeparation = value;
                RaisePropertyChanged();
            }
        }

        private DeepSkyObject TargetInfo { get; set; } = null;

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (TargetInfo == null) {
                i.Add("No target is defined");
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged("Issues");
            }

            return i.Count == 0;
        }

        private double CalculateMoonTargetSeparation() {
            var observerInfo = new ObserverInfo() {
                Latitude = profileService.ActiveProfile.AstrometrySettings.Latitude,
                Longitude = profileService.ActiveProfile.AstrometrySettings.Longitude,
                Height = profileService.ActiveProfile.AstrometrySettings.Elevation,
            };

            var weatherData = weatherDataMediator.GetInfo();

            if (weatherData?.Connected == true) {
                observerInfo.Pressure = weatherData.Pressure;
                observerInfo.Temperature = weatherData.Temperature;
            }

            TargetInfo = Utility.Utility.FindDsoInfo(this.Parent);

            var date = DateTime.UtcNow;
            var jd = AstroUtil.GetJulianDate(date);
            var moonPosition = Utility.Utility.GetMoonPosition(date, jd, observerInfo);

            var moonRaRadians = AstroUtil.ToRadians(AstroUtil.HoursToDegrees(moonPosition.RA));
            var moonDecRadians = AstroUtil.ToRadians(moonPosition.Dec);

            TargetInfo.Coordinates.Transform(Epoch.JNOW);
            var targetRaRadians = AstroUtil.ToRadians(TargetInfo.Coordinates.RADegrees);
            var targetDecRadians = AstroUtil.ToRadians(TargetInfo.Coordinates.Dec);

            var theta = Utility.Utility.iauSeps(moonRaRadians, moonDecRadians, targetRaRadians, targetDecRadians);

            var thetaDegrees = AstroUtil.ToDegree(theta);
            Logger.Debug($"Moon angle: {thetaDegrees:0.00}");

            return thetaDegrees;
        }

        public MoonAngleCondition(MoonAngleCondition copyMe) : this(copyMe.profileService, copyMe.weatherDataMediator) {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new MoonAngleCondition(this) {
                SeparationLimit = SeparationLimit,
                ComparisonOperator = ComparisonOperator,
            };
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            RunWatchdogIfInsideSequenceRoot();
        }

        public override void AfterParentChanged() {
            TargetInfo = Utility.Utility.FindDsoInfo(this.Parent);
            RunWatchdogIfInsideSequenceRoot();
        }

        public override string ToString() {
            return $"Condition: {nameof(MoonAngleCondition)}, {CurrentSeparation:0.00} {Utility.Utility.PrintComparator(ComparisonOperator)} {SeparationLimit:0.00}";
        }
    }
}