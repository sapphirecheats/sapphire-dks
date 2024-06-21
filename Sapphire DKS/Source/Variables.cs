using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Collections.Concurrent;
using System.Numerics;
using System.Collections.Generic;

namespace Sapphire_DKS
{
  public class Variables
  {
    public class vars
    {
      public static Keys activationKey = Keys.None;
      public static Keys alternateKey = Keys.None;
      public static Keys holdKey = Keys.None;
      public static Keys hideKey = Keys.None;

      public static int inputDelay = 25;
      public static int holdDelay = 25;
      public static int inputRandomization = 25;
      public static int holdRandomization = 25;

      public static int inputRandomRange = 25;
      public static int holdRandomRange = 25;

      public static bool dksMode = true;
      public static bool sendKeyPress = false;
      public static bool trueStreamerMode = false;
    }
  }
}
