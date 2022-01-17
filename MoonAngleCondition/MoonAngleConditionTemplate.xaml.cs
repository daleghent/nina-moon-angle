using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;

namespace DaleGhent.NINA.MoonAngle.MoonAngleCondition {

    /// <summary>
    /// Interaction logic for MoonAngleConditionTemplate.xaml
    /// </summary>
    [Export(typeof(ResourceDictionary))]
    public partial class MoonAngleConditionTemplate : ResourceDictionary {

        public MoonAngleConditionTemplate() {
            InitializeComponent();
        }
    }
}