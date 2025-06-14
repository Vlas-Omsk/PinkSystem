﻿using PinkSystem.Net;

namespace PinkSystem.IO.Content
{
    public sealed class FormUrlEncodedContentReader : StringContentReader
    {
        public FormUrlEncodedContentReader(QueryData data) : base(data.ToString(), "application/x-www-form-urlencoded")
        {
        }
    }
}
