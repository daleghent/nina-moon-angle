#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.Astrometry.Interfaces;
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
using System.Windows.Media;

namespace DaleGhent.NINA.MoonAngle.MoonAngleCondition {

    [ExportMetadata("Name", "Moon angle")]
    [ExportMetadata("Description", "Loops until the angular separation between the target and the moon satisfies the defined parameters")]
    [ExportMetadata("Icon", "MoonAngle_SVG")]
    [ExportMetadata("Category", "Loop Condition")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class MoonAngleCondition : SequenceCondition, IValidatable {
        private double separationLimit = 120;
        private ComparisonOperatorEnum comparisonOperator = ComparisonOperatorEnum.LESS_THAN_OR_EQUAL;
        private bool lorentzian = false;
        private int lorentzianWidth = 14;

        private double actualSeparation = 0;
        private double lorentzianSeparationLimit = 0;
        private double effectiveSeparationLimit = 0;

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

            ActualSeparation = Utility.CalculateTargetSeparation(TargetInfo.Coordinates, Enums.SepObject.Moon, GetObserverInfo());
            UpdateLorentizanFactors();

            if (!Check(null, null)) {
                if (this.Parent != null) {
                    if (ItemUtility.IsInRootContainer(Parent) && this.Parent.Status == SequenceEntityStatus.RUNNING) {
                        Logger.Info($"Moon and target angular separation is outside the prescribed condition ({effectiveSeparationLimit:0.00} {Utility.PrintComparator(ComparisonOperator)} {SeparationLimit:0.00}, Lorentzian = {Lorentzian}) - Interrupting current Instruction Set");
                        await this.Parent.Interrupt();
                    }
                }
            }
        }

        /// <summary>
        /// The user-supplied separation limit, in degrees
        /// </summary>
        [JsonProperty]
        public double SeparationLimit {
            get => separationLimit;
            set {
                separationLimit = value < 0d ? 0 : value > 180d ? 180 : Math.Round(value, 2);

                UpdateLorentizanFactors();
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

        /// <summary>
        /// Whether to modify the user-supplied separation limit using the Lorentzian Moon Avoidance algorithm
        /// </summary>
        [JsonProperty]
        public bool Lorentzian {
            get => lorentzian;
            set {
                lorentzian = value;

                UpdateLorentizanFactors();
                Check(null, null);

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The width (in days) parameter supplied to the Lorentzian Moon Avoidance algorithm
        /// </summary>
        [JsonProperty]
        public int LorentzianWidth {
            get => lorentzianWidth;
            set {
                if (value != lorentzianWidth) {
                    lorentzianWidth = value < 0 ? 0 : value > 15 ? 15 : value;

                    UpdateLorentizanFactors();
                    Check(null, null);

                    RaisePropertyChanged();
                }
            }
        }

        public ComparisonOperatorEnum[] ComparisonOperators => Enum.GetValues(typeof(ComparisonOperatorEnum))
            .Cast<ComparisonOperatorEnum>()
            .ToArray();

        private Color limitStatusColor = Colors.Transparent;

        public Color LimitStatusColor {
            get => limitStatusColor;
            private set {
                if (!limitStatusColor.Equals(value)) {
                    limitStatusColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            if (TargetInfo == null) {
                Logger.Error("No target is defined. Ending loop.");
                return true;
            }

            bool result;
            var currentSeparation = Math.Round(ActualSeparation, 2);
            var effectiveSeparationLimit = Lorentzian ? LorentzianSeparationLimit : SeparationLimit;

            Logger.Trace($"Parameters: {currentSeparation:0.00} {Utility.PrintComparator(ComparisonOperator)}  {effectiveSeparationLimit:0.00} (Lorentzian = {lorentzian})");

            switch (ComparisonOperator) {
                case ComparisonOperatorEnum.LESS_THAN:
                    result = currentSeparation < effectiveSeparationLimit;
                    break;

                case ComparisonOperatorEnum.LESS_THAN_OR_EQUAL:
                    result = currentSeparation <= effectiveSeparationLimit;
                    break;

                case ComparisonOperatorEnum.EQUALS:
                    result = currentSeparation == effectiveSeparationLimit;
                    break;

                case ComparisonOperatorEnum.GREATER_THAN_OR_EQUAL:
                    result = currentSeparation >= effectiveSeparationLimit;
                    break;

                case ComparisonOperatorEnum.GREATER_THAN:
                    result = currentSeparation > effectiveSeparationLimit;
                    break;

                case ComparisonOperatorEnum.NOT_EQUAL:
                    result = currentSeparation != effectiveSeparationLimit;
                    break;

                default:
                    return false;
            }

            LimitStatusColor = result ? Colors.DarkRed : Colors.Transparent;

            return !result;
        }

        /// <summary>
        /// The true angular distance between the target and the moon
        /// </summary>
        public double ActualSeparation {
            get => actualSeparation;
            private set {
                actualSeparation = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The separation limit as modified by the Lorentizan Moon Avoidance algorithm
        /// </summary>
        public double LorentzianSeparationLimit {
            get => lorentzianSeparationLimit;
            set {
                lorentzianSeparationLimit = value;
                RaisePropertyChanged();
            }
        }

        private IDeepSkyObject TargetInfo { get; set; } = null;

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

        private ObserverInfo GetObserverInfo() {
            var observerInfo = new ObserverInfo() {
                Latitude = profileService.ActiveProfile.AstrometrySettings.Latitude,
                Longitude = profileService.ActiveProfile.AstrometrySettings.Longitude,
                Elevation = profileService.ActiveProfile.AstrometrySettings.Elevation,
            };

            var weatherData = weatherDataMediator.GetInfo();

            if (weatherData?.Connected == true) {
                if (!double.IsNaN(weatherData.Pressure)) {
                    observerInfo.Pressure = weatherData.Pressure;
                }

                if (!double.IsNaN(weatherData.Temperature)) {
                    observerInfo.Temperature = weatherData.Temperature;
                }
            }
            return observerInfo;
        }

        private static double CalculateLorentzianSeparation(double distance, int width) {
            // Moon-Avoidance Lorentzian formulated by the Berkeley Automated Imaging Telescope (BAIT) team
            // Formula borrowed from ACP http://bobdenny.com/ar/RefDocs/HelpFiles/ACPScheduler81Help/Constraints.htm

            const double LUNAR_MONTH = 29.53058770576;

            var lAge = (AstroUtil.GetJulianDate(DateTime.UtcNow) - 2451550.1) / LUNAR_MONTH;
            var moonAgePct = lAge - Math.Floor(lAge);

            if (moonAgePct < 0) {
                moonAgePct += 1;
            }

            var moonAgeDays = moonAgePct * LUNAR_MONTH;
            var separation = distance / (1d + Math.Pow((0.5 - (moonAgeDays / LUNAR_MONTH)) / (width / LUNAR_MONTH), 2));

            Logger.Trace($"Moon age (%): {moonAgePct * 100:N2}, Moon age (days): {moonAgeDays:N4}, Width: {width}, Original separation: {distance:N2}, Lorentzian-adjusted: {separation:N2}");

            return separation;
        }

        private void UpdateLorentizanFactors() {
            if (Lorentzian) {
                LorentzianSeparationLimit = CalculateLorentzianSeparation(separationLimit, lorentzianWidth);
            }
        }

        public MoonAngleCondition(MoonAngleCondition copyMe) : this(copyMe.profileService, copyMe.weatherDataMediator) {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new MoonAngleCondition(this) {
                SeparationLimit = SeparationLimit,
                ComparisonOperator = ComparisonOperator,
                Lorentzian = Lorentzian,
                LorentzianWidth = LorentzianWidth,
            };
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            RunWatchdogIfInsideSequenceRoot();
        }

        public override void AfterParentChanged() {
            TargetInfo = Utility.FindDsoInfo(this.Parent);
            RunWatchdogIfInsideSequenceRoot();
        }

        public override string ToString() {
            return $"Condition: {nameof(MoonAngleCondition)}: {ActualSeparation:0.00} {Utility.PrintComparator(ComparisonOperator)} {effectiveSeparationLimit:0.00}, Lorentzian = {lorentzian}";
        }
    }
}