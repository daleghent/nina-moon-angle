#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

namespace DaleGhent.NINA.MoonAngle.Utility {

    internal class ObserverInfo {
        public double Latitude { get; set; } = 0d;
        public double Longitude { get; set; } = 0d;
        public double Height { get; set; } = 0d;
        public double Pressure { get; set; } = 1000d;
        public double Temperature { get; set; } = 0d;
    }
}