#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Sequencer.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaleGhent.NINA.MoonAngle.Utility {

    internal static class Utility {

        public static NOVAS.SkyPosition GetMoonPosition(DateTime date, double jd, ObserverInfo observerInfo) {
            var deltaT = AstroUtil.DeltaT(date);

            var onSurface = new NOVAS.OnSurface() {
                Latitude = observerInfo.Latitude,
                Longitude = observerInfo.Longitude,
                Height = observerInfo.Elevation,
                Pressure = observerInfo.Pressure,
                Temperature = observerInfo.Temperature,
            };

            var obs = new NOVAS.Observer() {
                OnSurf = onSurface,
                Where = (short)NOVAS.ObserverLocation.EarthSurface
            };

            var moon = new NOVAS.CelestialObject() {
                Name = "Moon",
                Number = (short)NOVAS.Body.Moon,
                Star = new NOVAS.CatalogueEntry(),
                Type = (short)NOVAS.ObjectType.MajorPlanetSunOrMoon
            };

            var moonPosition = new NOVAS.SkyPosition();

            var jdTt = jd + AstroUtil.SecondsToDays(deltaT);
            _ = NOVAS.Place(jdTt, moon, obs, deltaT, NOVAS.CoordinateSystem.EquinoxOfDate, NOVAS.Accuracy.Full, ref moonPosition);

            Logger.Debug($"Moon RA: {AstroUtil.HoursToHMS(moonPosition.RA)}, Dec: {AstroUtil.DegreesToDMS(moonPosition.Dec)}");

            return moonPosition;
        }

        public static DeepSkyObject FindDsoInfo(ISequenceContainer container) {
            DeepSkyObject target = null;
            ISequenceContainer acontainer = container;

            while (acontainer != null) {
                if (acontainer is IDeepSkyObjectContainer dsoContainer) {
                    target = dsoContainer.Target.DeepSkyObject;
                    break;
                }

                acontainer = acontainer.Parent;
            }

            return target;
        }

        public static string PrintComparator(ComparisonOperatorEnum comparator) {
            switch (comparator) {
                case ComparisonOperatorEnum.LESS_THAN:
                    return "<";

                case ComparisonOperatorEnum.LESS_THAN_OR_EQUAL:
                    return "<=";

                case ComparisonOperatorEnum.EQUALS:
                    return "=";

                case ComparisonOperatorEnum.GREATER_THAN_OR_EQUAL:
                    return ">=";

                case ComparisonOperatorEnum.GREATER_THAN:
                    return ">";

                case ComparisonOperatorEnum.NOT_EQUAL:
                    return "!=";

                default:
                    return string.Empty;
            }
        }
    }
}