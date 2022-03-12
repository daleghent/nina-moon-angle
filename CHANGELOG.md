# Moon angle

## 1.2.0.0 - 2022-03-12
* Implemented optional dynamic adjustment of the specified separation limit based on the **Lorentzian Moon Avoidance** algorithm. Activating this option will cause the Moon Angle condition to assume that the specified limit is the desired separation during a full moon. It will then reduce the limit distance as the moon phase gets closer to a new moon, with the separation distance during a new moon being reduced to half of the the specified separation limit. See the plugin Description for more information. Many thanks to Simon Kapadia for providing this improvement.

## 1.1.0.0 - 2022-02-22
* Migrated plugin to use new NINA `ObserverInfo` object with standardized defaults for temperature and air pressure
* Migrated plugin to use public NINA.Astrometry.SOFA cartesian angular separation (iauSeps) calculator instead of a local implementation of it
* Added some extra checks around temperature and air pressure weather data to prevent `ObserverInfo` defaults from potentially being replaced with `double.NaN`
* Minimum supported NINA version is now 2.0 beta 48

## 1.0.0.0 - 2022-01-17
* Initial Release
* Includes **Moon Angle** loop condition
