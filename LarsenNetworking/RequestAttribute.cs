using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarsenNetworking
{
    [AttributeUsage(AttributeTargets.Method)]
    sealed class RequestAttribute : Attribute
    {
        public RequestAttribute()
        {
        }
    }
}
