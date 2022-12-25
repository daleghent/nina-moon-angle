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

        public static double CalculateTargetSeparation(Coordinates targetCoordinates, Enums.SepObject toObject, ObserverInfo observerInfo) {
            var date = DateTime.UtcNow;
            var jd = AstroUtil.GetJulianDate(date);

            var sepObjectPosition = new NOVAS.SkyPosition();

            switch (toObject) {
                case Enums.SepObject.Moon:
                    sepObjectPosition = AstroUtil.GetMoonPosition(date, jd, observerInfo);
                    break;

                case Enums.SepObject.Sun:
                    sepObjectPosition = AstroUtil.GetSunPosition(date, jd, observerInfo);
                    break;
            }

            Logger.Trace($"{toObject} RA: {AstroUtil.HoursToHMS(sepObjectPosition.RA)}, Dec: {AstroUtil.DegreesToDMS(sepObjectPosition.Dec)}");

            var sepObjectRaRadians = AstroUtil.ToRadians(AstroUtil.HoursToDegrees(sepObjectPosition.RA));
            var sepObjectDecRadians = AstroUtil.ToRadians(sepObjectPosition.Dec);

            _ = targetCoordinates.Transform(Epoch.JNOW);
            var targetRaRadians = AstroUtil.ToRadians(targetCoordinates.RADegrees);
            var targetDecRadians = AstroUtil.ToRadians(targetCoordinates.Dec);

            var theta = SOFA.Seps(sepObjectRaRadians, sepObjectDecRadians, targetRaRadians, targetDecRadians);

            var thetaDegrees = AstroUtil.ToDegree(theta);
            Logger.Trace($"{toObject} angle: {thetaDegrees:0.00}");

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