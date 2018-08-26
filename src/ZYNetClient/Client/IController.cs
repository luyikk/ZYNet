using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Client
{
    public interface IController
    {
        /// <summary>
        /// auto set CClient
        /// </summary>
        CloudClient CClient { get;  set; }
        /// <summary>
        /// auto set async
        /// </summary>
        AsyncCalls Async { get; set; }

        /// <summary>
        /// if is Async Call,this.Async is implementation,or  CClient is implementation
        /// </summary>
        bool IsAsync { get; set; }


    }
}
