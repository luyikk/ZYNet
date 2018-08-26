using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Frame
{
    public enum ErrorTag
    {
        None=0,
        Disconnect=-1,
        TimeOut =-101,
        ReturnTypeErr=-201,
        CallErr=-205,
        SetErr=-206,
        FodyInstallErr=-207,
    }
}
