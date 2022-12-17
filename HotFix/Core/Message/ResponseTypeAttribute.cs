using System;

namespace HotFix
{
    public class ResponseTypeAttribute: BaseAttribute
    {
        public string Type { get; }

        public ResponseTypeAttribute(string type)
        {
            this.Type = type;
        }
    }
}