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
using NINA.Core.Utility.Notification;
using NINA.Image.ImageData;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Settings = DaleGhent.NINA.MoonAngle.Properties.Settings;

namespace DaleGhent.NINA.MoonAngle {
    [Export(typeof(IPluginManifest))]
    public class MoonAngle : PluginBase, INotifyPropertyChanged {
        private readonly IPluginOptionsAccessor pluginSettings;
        private readonly IProfileService profileService;
        private readonly IImageSaveMediator imageSaveMediator;

        private const string sunAngleKeywordName = "SUNANGLE";
        private const string sunAngleKeywordComment = "[deg] Angular separation between {0} and sun";
        private readonly ImagePattern imagePatternSun = new ImagePattern("$$SUNANGLE$$", "Target's angular separation from the sun", "Moon Angle");

        private const string moonAngleKeywordName = "MOONANGL";
        private const string moonAngleKeywordComment = "[deg] Angular separation between {0} and moon";
        private readonly ImagePattern imagePatternMoon = new ImagePattern("$$MOONANGLE$$", "Target's angular separation from the moon", "Moon Angle");

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

            this.imageSaveMediator.BeforeImageSaved += ImageSaveMediator_BeforeImageSaved;
            this.imageSaveMediator.BeforeFinalizeImageSaved += ImageSaveMediator_BeforeFinalizeImageSaved;

            this.pluginSettings = new PluginOptionsAccessor(profileService, Guid.Parse(this.Identifier));
            this.profileService = profileService;

            profileService.ProfileChanged += ProfileService_ProfileChanged;
        }

        public override Task Teardown() {
            profileService.ProfileChanged -= ProfileService_ProfileChanged;
            imageSaveMediator.BeforeImageSaved -= ImageSaveMediator_BeforeImageSaved;
            imageSaveMediator.BeforeFinalizeImageSaved -= ImageSaveMediator_BeforeFinalizeImageSaved;
            return base.Teardown();
        }

        private void ProfileService_ProfileChanged(object sender, EventArgs e) {
            RaiseAllPropertiesChanged();
        }

        private async Task ImageSaveMediator_BeforeImageSaved(object sender, BeforeImageSavedEventArgs e) {
            if (!e.Image.MetaData.Image.ImageType.Contains("DARK") && !e.Image.MetaData.Image.ImageType.Equals("BIAS")) {
                await e.ImagePrepareTask;

                var observerInfo = new ObserverInfo() {
                    Latitude = e.Image.MetaData.Observer.Latitude,
                    Longitude = e.Image.MetaData.Observer.Longitude,
                    Elevation = e.Image.MetaData.Observer.Elevation,
                    Temperature = e.Image.MetaData.WeatherData.Temperature,
                    Humidity = e.Image.MetaData.WeatherData.Humidity,
                    Pressure = e.Image.MetaData.WeatherData.Pressure,
                };

                if (!e.Image.MetaData.Image.ImageType.Equals("SNAPSHOT")) {
                    if (e.Image.MetaData.Target.Coordinates != null && !double.IsNaN(e.Image.MetaData.Target.Coordinates.Dec) && !double.IsNaN(e.Image.MetaData.Target.Coordinates.RADegrees)) {
                        e.Image.MetaData.GenericHeaders.Add(new DoubleMetaDataHeader(sunAngleKeywordName, Utility.CalculateTargetSeparation(e.Image.MetaData.Target.Coordinates, Enums.SepObject.Sun, observerInfo), string.Format(sunAngleKeywordComment, "object")));
                        e.Image.MetaData.GenericHeaders.Add(new DoubleMetaDataHeader(moonAngleKeywordName, Utility.CalculateTargetSeparation(e.Image.MetaData.Target.Coordinates, Enums.SepObject.Moon, observerInfo), string.Format(moonAngleKeywordComment, "object")));
                    }
                } else {
                    if (e.Image.MetaData.Telescope.Coordinates != null && !double.IsNaN(e.Image.MetaData.Telescope.Coordinates.Dec) && !double.IsNaN(e.Image.MetaData.Telescope.Coordinates.RADegrees)) {
                        e.Image.MetaData.GenericHeaders.Add(new DoubleMetaDataHeader(sunAngleKeywordName, Utility.CalculateTargetSeparation(e.Image.MetaData.Telescope.Coordinates, Enums.SepObject.Sun, observerInfo), string.Format(sunAngleKeywordComment, "center")));
                        e.Image.MetaData.GenericHeaders.Add(new DoubleMetaDataHeader(moonAngleKeywordName, Utility.CalculateTargetSeparation(e.Image.MetaData.Telescope.Coordinates, Enums.SepObject.Moon, observerInfo), string.Format(moonAngleKeywordComment, "center")));
                    }
                }
            }
        }

        private Task ImageSaveMediator_BeforeFinalizeImageSaved(object sender, BeforeFinalizeImageSavedEventArgs e) {
            if (!e.Image.RawImageData.MetaData.Image.ImageType.Contains("DARK") && !e.Image.RawImageData.MetaData.Image.ImageType.Equals("BIAS")) {

                var observerInfo = new ObserverInfo() {
                    Latitude = e.Image.RawImageData.MetaData.Observer.Latitude,
                    Longitude = e.Image.RawImageData.MetaData.Observer.Longitude,
                    Elevation = e.Image.RawImageData.MetaData.Observer.Elevation,
                    Temperature = e.Image.RawImageData.MetaData.WeatherData.Temperature,
                    Humidity = e.Image.RawImageData.MetaData.WeatherData.Humidity,
                    Pressure = e.Image.RawImageData.MetaData.WeatherData.Pressure,
                };

                if (!e.Image.RawImageData.MetaData.Image.ImageType.Equals("SNAPSHOT")) {
                    if (e.Image.RawImageData.MetaData.Target.Coordinates != null && !double.IsNaN(e.Image.RawImageData.MetaData.Target.Coordinates.Dec) && !double.IsNaN(e.Image.RawImageData.MetaData.Target.Coordinates.RADegrees)) {
                        var sunSep = Utility.CalculateTargetSeparation(e.Image.RawImageData.MetaData.Target.Coordinates, Enums.SepObject.Sun, observerInfo);
                        var moonSep = Utility.CalculateTargetSeparation(e.Image.RawImageData.MetaData.Target.Coordinates, Enums.SepObject.Moon, observerInfo);

                        e.AddImagePattern(new ImagePattern(imagePatternSun.Key, imagePatternSun.Description, imagePatternSun.Category) { Value = $"{sunSep:0.00}" });
                        e.AddImagePattern(new ImagePattern(imagePatternMoon.Key, imagePatternMoon.Description, imagePatternMoon.Category) { Value = $"{moonSep:0.00}" });
                    }
                } else {
                    if (e.Image.RawImageData.MetaData.Telescope.Coordinates != null && !double.IsNaN(e.Image.RawImageData.MetaData.Telescope.Coordinates.Dec) && !double.IsNaN(e.Image.RawImageData.MetaData.Telescope.Coordinates.RADegrees)) {
                        var sunSep = Utility.CalculateTargetSeparation(e.Image.RawImageData.MetaData.Telescope.Coordinates, Enums.SepObject.Sun, observerInfo);
                        var moonSep = Utility.CalculateTargetSeparation(e.Image.RawImageData.MetaData.Telescope.Coordinates, Enums.SepObject.Moon, observerInfo);

                        e.AddImagePattern(new ImagePattern(imagePatternSun.Key, imagePatternSun.Description, imagePatternSun.Category) { Value = $"{sunSep:0.00}" });
                        e.AddImagePattern(new ImagePattern(imagePatternMoon.Key, imagePatternMoon.Description, imagePatternMoon.Category) { Value = $"{moonSep:0.00}" });
                    }
                }
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