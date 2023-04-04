using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GDL.Src.Utility
{
    public static class MsgBoxHelper
    {
        public enum MessageBoxType : uint
        {
            OK = 0x00000000,
            OKCancel = 0x00000001,
            AbortRetryIgnore = 0x00000002,
            YesNoCancel = 0x00000003,
            YesNo = 0x00000004,
            RetryCancel = 0x00000005,
            CancelTryAgainContinue = 0x00000006,
            IconHand = 0x00000010,
            IconQuestion = 0x00000020,
            IconExclamation = 0x00000030,
            IconAsterisk = 0x00000040,
            UserIcon = 0x00000080,
            IconWarning = IconExclamation,
            IconError = IconHand,
            IconInformation = IconAsterisk,
            IconStop = IconHand
        }

        public enum MessageBoxResult : uint
        {
            OK = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7,
            TryAgain = 10,
            Continue = 11
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public static MessageBoxResult ShowMessage(string message, string caption, MessageBoxType type = MessageBoxType.OK)
        {
            return (MessageBoxResult)MessageBox(IntPtr.Zero, message, caption, (uint)type);
        }
    }
}
