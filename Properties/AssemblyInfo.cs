using System.Reflection;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin
[assembly: Guid("036af399-91b0-4a29-a7d3-44af0bfde13e")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.5.0.0")]
[assembly: AssemblyFileVersion("1.5.0.0")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("Moon Angle")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Instructions that consider the angular separation between the target object and the moon")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Dale Ghent")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("Moon Angle")]
[assembly: AssemblyCopyright("Copyright © 2022 Dale Ghent")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.1.2001")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/daleghent/nina-moon-angle")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage - omit if not applicaple
//[assembly: AssemblyMetadata("Homepage", "https://daleghent.com/moon-angle")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "moon,luna,lune,maan,mond,місяць,księżyc,tungl,ukwezi,月亮,月,φεγγάρι,Луна")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/daleghent/nina-moon-angle/blob/main/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "https://daleghent.github.io/nina-plugins/assets/images/moonangle-logo.png")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"Moon Angle provides a loop condition that allows you to start or end loops based on the target's angular distance from the Moon.

This loop condition appears under the **Loop Condition** category of instructions.

# FITS keywords and file name patterns #

This plugin adds:
* `SUNANGLE` and `MOONANGL` FITS keywords to all non-DARK and non-BIAS type exposures. The definitions for these keywords may be found in the HEASARC [Dictionary of Commonly Used FITS Keywords](https://heasarc.gsfc.nasa.gov/docs/fcg/common_dict.html).
* `$$SUNANGLE$$` and `$$MOONANGLE$$` file name patterns.

For exposures taken during a sequence, the angular sparation that is recorded in the FITS keywords and file patterns will be measured from the object of interest. If the exposure is taken outside of a sequence, such as manual exposures made from the Imaging window, the angular separation will be measured from the pointing coordinates reported by the mount.

# Lorentzian Moon Avoidance #

The **Lorentzian** option modifies the calculated target-moon separation angle based on the Lorentzian Moon Avoidance (LMA) algorithm, forumlated by the [Berkely Automated Imaging Telescope](https://w.astro.berkeley.edu/bait/) team.
When enabled, N.I.N.A. will assume that the specified separation distance assumes a full moon at its most luminous phase. The LMA algorithm will use _current_ phase of the lunar cycle to reduce the specified separaration angle.
The result is that the condition will allow imaging to take place closer to the moon the less full, and thus less bright, it is.

The separation is reduced such that by **width** days before (or after) the full moon the specified separation distance will be halved. The default width of 14 days is a reasonable starting point.
The calculation uses [the LMA formula](http://bobdenny.com/ar/RefDocs/HelpFiles/ACPScheduler81Help/Constraints.htm) provided, with thanks, by Bob Denny of [DC3 Dreams](http://dc3.com/).

Many thanks to Simon Kapadia for this implementation.

# Getting help #

Help for this plugin may be found in the **#plugin-discussions** channel on the NINA project [Discord chat server](https://discord.gg/nighttime-imaging) or by filing an issue report at this plugin's [Github repository](https://github.com/daleghent/nina-moon-angle/issues).")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]