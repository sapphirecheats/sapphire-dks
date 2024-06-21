using KeyListener;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static Sapphire_DKS.Variables;

namespace Sapphire_DKS
{
  public class KeyboardHook
  {
    public static readonly GlobalKeyListener _keyListener = new GlobalKeyListener();

    private static IntPtr _hookID = IntPtr.Zero;
    private static readonly LowLevelKeyboardProc _proc = HookCallback;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;

    private const byte VK_KEYDOWN = 0;
    private const byte VK_KEYUP = 2;

    public static uint WDA_NONE = 0x00000000;
    public static uint WDA_MONITOR = 0x00000001;
    public static uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    public static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
      public uint vkCode;
      public uint scanCode;
      public uint flags;
      public uint time;
      public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
      public int type;
      public InputUnion U;
      public static int Size => Marshal.SizeOf(typeof(INPUT));
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
      [FieldOffset(0)]
      public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
      public ushort wVk;
      public ushort wScan;
      public uint dwFlags;
      public uint time;
      public IntPtr dwExtraInfo;
    }

    public static void SetHook() => _hookID = SetHook(_proc);
    public static void UnHook() => UnhookWindowsHookEx(_hookID);

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
      using (var curProcess = Process.GetCurrentProcess())
      using (var curModule = curProcess.MainModule)
      {
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
      }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode < 0)
        return CallNextHookEx(_hookID, nCode, wParam, lParam);

      if (vars.holdKey != Keys.None && !_keyListener.IsKeyPressed(vars.holdKey))
        return CallNextHookEx(_hookID, nCode, wParam, lParam);

      var kbdStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

      // Require main activation key to be pressed before running any other code
      if ((Keys)kbdStruct.vkCode == vars.activationKey)
      {
        // If DKS mode is enabled we do a different set of steps
        if (vars.dksMode)
        {
          // If we detect that the user pressed their activation key with DKS mode, we cancel the down event of that key
          if (wParam == (IntPtr)WM_KEYDOWN && kbdStruct.dwExtraInfo != unchecked((IntPtr)0xCAFEBABE))
            return (IntPtr)1;

          // Once we detect the up event for that key we set a boolean to true which controls the thread
          // We have to put this functionality of click actuation on a separate thread that has no linking functions because
          // sleeping inside of the hook at all would mess up any key press sent during the wait delays.
          if (wParam == (IntPtr)WM_KEYUP && kbdStruct.dwExtraInfo != unchecked((IntPtr)0xCAFEBABE))
            vars.sendKeyPress = true;
        }
        // If DKS mode is not enabled, we wait for the user to lift up their main DKS key & once we detect that, we tell the thread to send an input
        else if (wParam == (IntPtr)WM_KEYUP && kbdStruct.dwExtraInfo != unchecked((IntPtr)0xCAFEBABE) && !vars.sendKeyPress)
        {
          vars.sendKeyPress = true;
        }
      }

      return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    public static void ActuateKey(ushort keyCode, int inputDelay, int holdDelay)
    {
      // Unique identifier to prevent the hook from blocking our arfiticial inputs
      var extraInfo = new UIntPtr(0xCAFEBABE);

      Thread.Sleep(inputDelay);
      keybd_event((byte)keyCode, 0, VK_KEYDOWN, extraInfo);

      Thread.Sleep(holdDelay);
      keybd_event((byte)keyCode, 0, VK_KEYUP, extraInfo);
    }
  }
}
