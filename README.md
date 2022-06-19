# Moon Angle

Moon Angle provides a loop condition that allows you to start or end loops based on the target's angular distance from the Moon.

This loop condition appears under the **Loop Condition** category of instructions.

# FITS keywords and file name patterns #

This plugin adds:
* `SUNANGLE` and `MOONANGL` FITS keywords to all non-DARK and non-BIAS type exposures. The definitions for these keywords may be found in the HEASARC [Dictionary of Commonly Used FITS Keywords](https://heasarc.gsfc.nasa.gov/docs/fcg/common_dict.html).
* `$$SUNANGLE$$` and `$$MOONANGLE$$` file name patterns.

For exposures taken during a sequence, the angular sparation that is recorded in the FITS keywords and file patterns will be measured from the object of interest. If the exposure is taken outside of a sequence, such as manual exposures made from the Imaging window, the angular separation will be measured from the pointing coordinates reported by the mount.

# Getting help #

Help for this plugin may be found in the **#plugin-discussions** channel on the NINA project [Discord chat server](https://discord.gg/nighttime-imaging) or by filing an issue report at this plugin's [Github repository](https://github.com/daleghent/nina-moon-angle/issues).
