﻿using System.Web;
using System.Web.Mvc;

namespace Homework_5_26
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}