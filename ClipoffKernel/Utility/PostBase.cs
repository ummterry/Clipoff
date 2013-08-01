using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clipoff.Utility
{
    abstract class PostBase
    {
        protected string filePath, message;
        public PostBase() { }
        public PostBase(string filePath, string message)
        {
            this.filePath = filePath;
            this.message = message;
        }
        public abstract void Post();
    }
}
