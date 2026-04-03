using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace beat_win;

public partial class FormSettingsMenu : Form
{
    public FormSettingsMenu()
    {
        InitializeComponent();
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        ConfigFile.Instance.RaylibRenderer = comboBox1.SelectedIndex == 1;
        ConfigFile.Save();
    }
}
