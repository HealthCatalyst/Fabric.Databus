using System;
using System.Collections.Generic;
using System.Text;

namespace Fabric.Shared
{
    public static class VerifyHelper
    {
        public static T Verify<T>(this T obj, string message)
        {
            if (obj == null)
                throw new NullReferenceException(message);

            return obj;
        }
    }
}
