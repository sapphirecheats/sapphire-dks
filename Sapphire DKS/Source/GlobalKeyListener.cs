using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyListener
{
  public class GlobalKeyListener
  {
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern short GetKeyState(int keyCode);

    private Dictionary<Keys, Action> keyBindings;
    private Dictionary<Keys, bool> keyStates;

    public GlobalKeyListener()
    {
      keyBindings = new Dictionary<Keys, Action>();
      keyStates = new Dictionary<Keys, bool>();
    }

    public void BindKey(Keys key, Action function)
    {
      keyBindings[key] = function;
      keyStates[key] = false;
    }

    public void RemoveKeyBinding(Keys key)
    {
      keyBindings.Remove(key);
    }

    private void MonitorKeys()
    {
      foreach (var key in keyBindings.Keys)
      {
        if ((GetAsyncKeyState(key) & 0x8000) != 0 && !keyStates[key])
        {
          Task.Run(() => keyBindings[key]());
          keyStates[key] = true;
        }
        else if ((GetAsyncKeyState(key) & 0x8000) == 0)
          keyStates[key] = false;
      }
    }

    public async Task ListenForKey()
    {
      await Task.Run(() => 
      {
        while (true)
        {
          Thread.Sleep(1);
          MonitorKeys();
        }
      });
    }

    public static List<Keys> keyList = new List<Keys>();

    public Keys? GetKeyPressed()
    {
      if (keyList.Count > 0)
        return null;

      var excludedKeys = new HashSet<Keys> { Keys.LButton, Keys.RButton, Keys.MButton };

      var values = Enum.GetValues(typeof(Keys))
                       .Cast<Keys>()
                       .Where(k => !excludedKeys.Contains(k));

      foreach (Keys key in values)
        keyList.Add(key);

      Keys bind = Keys.None;

      while (bind == Keys.None)
        foreach (Keys key in keyList)
          if (IsKeyPressed(key))
            bind = key;

      return bind;
    }

    static KeyStates GetKeyState(Keys key)
    {
      KeyStates state = KeyStates.None;

      short retVal = GetKeyState((int)key);

      if ((retVal & 0x8000) == 0x8000)
        state |= KeyStates.Down;

      if ((retVal & 1) == 1)
        state |= KeyStates.Toggled;

      return state;
    }

    [Flags]
    private enum KeyStates
    {
      None = 0, Down = 1, Toggled = 2
    }

    public bool IsKeyPressed(Keys key)
    {
      return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
    }
  }
}
