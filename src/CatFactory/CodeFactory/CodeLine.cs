﻿using System;

namespace CatFactory.CodeFactory
{
    public class CodeLine : Line, ILine
    {
        public CodeLine()
            : base()
        {
        }

        public CodeLine(Int32 indent, String content, params String[] values)
            : base(indent, content, values)
        {
        }

        public CodeLine(String content, params String[] values)
            : base(content, values)
        {
        }

        public override String ToString()
            => Content;
    }
}
