﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class Build : Attribute
    {

    }

    public interface IFodyCall
    {
        object Call(int cmd, Type returnType, object[] args);
    }
}
