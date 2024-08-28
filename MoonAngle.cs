#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Image.ImageData;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Settings = DaleGhent.NINA.MoonAngle.Properties.Settings;

namespace DaleGhent.NINA.MoonAngle {

    [Export(typeof(IPluginManifest))]
    public class MoonAngle : PluginBase, INotifyPropertyChanged {
        private readonly IProfileService profileService;
        private readonly IImageSaveMediator imageSaveMediator;

        private readonly ImagePattern imagePatternSun = new("$$SUNANGLE$$", "Target's angular separation from the sun", "Moon Angle") { Value = "146.83" };
        private readonly ImagePattern imagePatternMoon = new("$$MOONANGLE$$", "Target's angular separation from the moon", "Moon Angle") { Value = "34.19" };

        [ImportingConstructor]
        public MoonAngle(IProfileService profileService, IOptionsVM options, IImageSaveMediator imageSaveMediator) {
            if (Settings.Default.UpgradeSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            this.imageSaveMediator = imageSaveMediator;

            options.AddImagePattern(imagePatternSun);
            options.AddImagePattern(imagePatternMoon);

            this.imageSaveMediator.BeforeFinalizeImageSaved += ImageSaveMediator_BeforeFinalizeImageSaved;
            this.profileService = profileService;

            profileService.ProfileChanged += ProfileService_ProfileChanged;
        }

        public override Task Teardown() {
            profileService.ProfileChanged -= ProfileService_ProfileChanged;
            imageSaveMediator.BeforeFinalizeImageSaved -= ImageSaveMediator_BeforeFinalizeImageSaved;
            return base.Teardown();
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            RaiseAllPropertiesChanged();
        }

        private Task ImageSaveMediator_BeforeFinalizeImageSaved(object sender, BeforeFinalizeImageSavedEventArgs e) {
            var imageType = e.Image.RawImageData.MetaData.Image.ImageType;

            // Don't do anything for darks or biases
            if (imageType.Contains("DARK") || imageType.Equals("BIAS")) {
                return Task.CompletedTask;
            }

            // Don't do anything if the observer's location is not set
            if (double.IsNaN(e.Image.RawImageData.MetaData.Observer.Latitude) ||
                double.IsNaN(e.Image.RawImageData.MetaData.Observer.Longitude)) {
                return Task.CompletedTask;
            }

            Coordinates targetCoords;
            var targetName = "object";

            if (imageType.Equals("SNAPSHOT") && e.Image.RawImageData.MetaData.Telescope.Coordinates != null) {
                targetCoords = e.Image.RawImageData.MetaData.Telescope.Coordinates;
            } else if (e.Image.RawImageData.MetaData.Target.Coordinates != null) {
                targetCoords = e.Image.RawImageData.MetaData.Target.Coordinates;

                if (!string.IsNullOrEmpty(e.Image.RawImageData.MetaData.Target.Name)) {
                    targetName = e.Image.RawImageData.MetaData.Target.Name;
                }
            } else {
                // Exit if we cannot determine the target's coordinates or where the telescope is pointing
                return Task.CompletedTask;
            }

            var observerInfo = new ObserverInfo() {
                Latitude = e.Image.RawImageData.MetaData.Observer.Latitude,
                Longitude = e.Image.RawImageData.MetaData.Observer.Longitude,
                Elevation = e.Image.RawImageData.MetaData.Observer.Elevation,
                Temperature = e.Image.RawImageData.MetaData.WeatherData.Temperature,
                Humidity = e.Image.RawImageData.MetaData.WeatherData.Humidity,
                Pressure = e.Image.RawImageData.MetaData.WeatherData.Pressure,
            };

            if (!double.IsNaN(targetCoords.Dec) && !double.IsNaN(targetCoords.RADegrees)) {
                const string keywordComment = "[deg] Angular sep. between {0} and {1}";

                var sunSep = Utility.CalculateTargetSeparation(targetCoords, Enums.SepObject.Sun, observerInfo);
                var moonSep = Utility.CalculateTargetSeparation(targetCoords, Enums.SepObject.Moon, observerInfo);

                e.Image.RawImageData.MetaData.GenericHeaders.Add(new DoubleMetaDataHeader("SUNANGLE", sunSep, string.Format(keywordComment, targetName, "sun")));
                e.Image.RawImageData.MetaData.GenericHeaders.Add(new DoubleMetaDataHeader("MOONANGL", moonSep, string.Format(keywordComment, targetName, "moon")));

                e.AddImagePattern(new ImagePattern(imagePatternSun.Key, imagePatternSun.Description, imagePatternSun.Category) { Value = $"{sunSep:0.00}" });
                e.AddImagePattern(new ImagePattern(imagePatternMoon.Key, imagePatternMoon.Description, imagePatternMoon.Category) { Value = $"{moonSep:0.00}" });
            }

            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaiseAllPropertiesChanged() {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}