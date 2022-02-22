# Moon angle

## 1.1.0.0 - 2022-02-22
* Migrated plugin to use new NINA `ObserverInfo` object with standardized defaults for temperature and air pressure
* Migrated plugin to use public NINA.Astrometry.SOFA cartesian angular separation (iauSeps) calculator instead of a local implementation of it
* Added some extra checks around temperature and air pressure weather data to prevent `ObserverInfo` defaults from potentially being replaced with `double.NaN`
* Minimum supported NINA version is now 2.0 beta 48

## 1.0.0.0 - 2022-01-17
* Initial Release
* Includes **Moon Angle** loop condition
