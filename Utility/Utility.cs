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
                Height = observerInfo.Height,
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

        // Math and methodology borrowed from SOFA
        public static double iauSeps(double al, double ap, double bl, double bp) {
            double[] ac = new double[3];
            double[] bc = new double[3];

            // Spherical to Cartesian.
            iauS2c(al, ap, ref ac);
            iauS2c(bl, bp, ref bc);

            // Angle between the vectors.
            return iauSepp(ac, bc);
        }

        public static double iauSepp(double[] a, double[] b) {
            double[] axb = new double[3];
            double ss;
            double cs;

            // Sine of angle between the vectors, multiplied by the two moduli.
            iauPxp(a, b, ref axb);
            ss = iauPm(axb);

            // Cosine of the angle, multiplied by the two moduli.
            cs = iauPdp(a, b);

            // The angle
            return ((ss != 0d) || (cs != 0d)) ? Math.Atan2(ss, cs) : 0d;
        }

        public static void iauPxp(double[] a, double[] b, ref double[] axb) {
            double xa;
            double ya;
            double za;
            double xb;
            double yb;
            double zb;

            xa = a[0];
            ya = a[1];
            za = a[2];
            xb = b[0];
            yb = b[1];
            zb = b[2];

            axb[0] = (ya * zb) - (za * yb);
            axb[1] = (za * xb) - (xa * zb);
            axb[2] = (xa * yb) - (ya * xb);
        }

        public static double iauPm(double[] p) {
            return Math.Sqrt((p[0] * p[0]) + (p[1] * p[1]) + (p[2] * p[2]));
        }

        public static double iauPdp(double[] a, double[] b) {
            return (a[0] * b[0]) + (a[1] * b[1]) + (a[2] * b[2]);
        }

        public static void iauS2c(double theta, double phi, ref double[] c) {
            double cp;

            cp = Math.Cos(phi);
            c[0] = Math.Cos(theta) * cp;
            c[1] = Math.Sin(theta) * cp;
            c[2] = Math.Sin(phi);
        }
    }
}