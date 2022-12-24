#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Astrometry.Interfaces;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Sequencer.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaleGhent.NINA.MoonAngle {

    internal static class Utility {

        public static double CalculateMoonSeparation(Coordinates coordinates, ObserverInfo observerInfo) {
            var date = DateTime.UtcNow;
            var jd = AstroUtil.GetJulianDate(date);
            var moonPosition = AstroUtil.GetMoonPosition(date, jd, observerInfo);

            Logger.Trace($"Moon RA: {AstroUtil.HoursToHMS(moonPosition.RA)}, Dec: {AstroUtil.DegreesToDMS(moonPosition.Dec)}");

            var moonRaRadians = AstroUtil.ToRadians(AstroUtil.HoursToDegrees(moonPosition.RA));
            var moonDecRadians = AstroUtil.ToRadians(moonPosition.Dec);

            _ = coordinates.Transform(Epoch.JNOW);
            var targetRaRadians = AstroUtil.ToRadians(coordinates.RADegrees);
            var targetDecRadians = AstroUtil.ToRadians(coordinates.Dec);

            var theta = SOFA.Seps(moonRaRadians, moonDecRadians, targetRaRadians, targetDecRadians);

            var thetaDegrees = AstroUtil.ToDegree(theta);
            Logger.Trace($"Moon angle: {thetaDegrees:0.00}");

            return thetaDegrees;
        }

        public static double CalculateSunSeparation(Coordinates coordinates, ObserverInfo observerInfo) {
            var date = DateTime.UtcNow;
            var jd = AstroUtil.GetJulianDate(date);
            var sunPosition = AstroUtil.GetSunPosition(date, jd, observerInfo);

            Logger.Trace($"Sun RA: {AstroUtil.HoursToHMS(sunPosition.RA)}, Dec: {AstroUtil.DegreesToDMS(sunPosition.Dec)}");

            var sunRaRadians = AstroUtil.ToRadians(AstroUtil.HoursToDegrees(sunPosition.RA));
            var sunDecRadians = AstroUtil.ToRadians(sunPosition.Dec);

            _ = coordinates.Transform(Epoch.JNOW);
            var targetRaRadians = AstroUtil.ToRadians(coordinates.RADegrees);
            var targetDecRadians = AstroUtil.ToRadians(coordinates.Dec);

            var theta = SOFA.Seps(sunRaRadians, sunDecRadians, targetRaRadians, targetDecRadians);

            var thetaDegrees = AstroUtil.ToDegree(theta);
            Logger.Debug($"Sun angle: {thetaDegrees:0.00}");

            return thetaDegrees;
        }

        public static IDeepSkyObject FindDsoInfo(ISequenceContainer container) {
            IDeepSkyObject target = null;
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