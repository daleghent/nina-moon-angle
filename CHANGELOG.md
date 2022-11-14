# Moon angle

## 2.1.0.0 - 2022-11-13
* Fixed Lorentzian separation math
* Reduced logging level of one message from Debug to Trace

## 2.0.0.0 - 2022-11-12
* Updated plugin to Microsoft .NET 7 for compatibility with NINA 3.0. The version of Moon Angle that is compatible with NINA 2.x will remain under the 1.x versioning scheme, and Moon Angle 2.x and later is relvant only to NINA 3.x and later.

## 1.5.1.0 - 2022-10-09
* Bugfix: The Lorentzian on/off and Lorentzian Width parameters are now saved in templates

## 1.5.0.0 - 2022-06-19
* `SUNANGLE` and `MOONANGL` FITS keywords to all non-DARK and non-BIAS type exposures. The definitions for these keywords may be found in the HEASARC [Dictionary of Commonly Used FITS Keywords](https://heasarc.gsfc.nasa.gov/docs/fcg/common_dict.html)
* `$$SUNANGLE$$` and `$$MOONANGLE$$` file name patterns
* Minimum supported NINA version is now 2.0.1 (2.0 HF1)

For exposures taken during a sequence, the angular sparation that is recorded in the FITS keywords and file patterns will be measured from the object of interest. If the exposure is taken outside of a sequence, such as manual exposures made from the Imaging window, the angular separation will be measured from the pointing coordinates reported by the mount.

## 1.4.0.0 - 2022-04-22
* Adjusted the logging level of some messages from debug to trace. There is no other functional change.

## 1.3.0.0 - 2022-03-13
* Updated to support changes to DSO containers in NINA 2.0 beta 50
* Minimum supported NINA version is now 2.0 Beta 50

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
