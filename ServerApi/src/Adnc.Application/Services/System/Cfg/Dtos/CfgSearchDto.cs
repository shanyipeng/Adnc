﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Adnc.Application.Dtos
{
	/// <summary>
	/// 系统配置
	/// </summary>
	public class CfgSearchDto:BaseSearchDto
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string CfgName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string CfgValue { get; set; }
    }
}
