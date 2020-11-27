using System;
using static Interop.User32;

namespace System.Windows.Forms
{
    public enum MessageBoxButtons
    {
        /// <summary>
        ///  Specifies that the message box contains an OK button. This field is
        ///  constant.
        /// </summary>
        OK = (int)MB.OK,

        /// <summary>
        ///  Specifies that the message box contains OK and Cancel buttons. This
        ///  field is constant.
        /// </summary>
        OKCancel = (int)MB.OKCANCEL,

        /// <summary>
        ///  Specifies that the message box contains Abort, Retry, and Ignore
        ///  buttons.
        ///  This field is constant.
        /// </summary>
        AbortRetryIgnore = (int)MB.ABORTRETRYIGNORE,

        /// <summary>
        ///  Specifies that the message box contains Yes, No, and Cancel buttons.
        ///  This field is constant.
        /// </summary>
        YesNoCancel = (int)MB.YESNOCANCEL,

        /// <summary>
        ///  Specifies that the
        ///  message box contains Yes and No buttons. This field is
        ///  constant.
        /// </summary>
        YesNo = (int)MB.YESNO,

        /// <summary>
        ///  Specifies that the message box contains Retry and Cancel buttons.
        ///  This field is constant.
        /// </summary>
        RetryCancel = (int)MB.RETRYCANCEL
    }
}