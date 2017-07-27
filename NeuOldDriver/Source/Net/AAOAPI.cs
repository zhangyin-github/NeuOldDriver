﻿using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.Web.Http;

using NeuOldDriver.Global;
using NeuOldDriver.Extensions;

using HtmlAgilityPack;

namespace NeuOldDriver.Net {

    public static class AAOAPI {

        public static readonly IDictionary<string, string> addresses = new Dictionary<string, string>() {
            {"个人信息", "/ACTIONFINDSTUDENTINFO.APPPROCESS"},
            {"全院教师课表", "/ACTIONQUERYTEACHERSCHEDULEBYPUBLIC.APPPROCESS"},
            {"全院班级课表", "/ACTIONQUERYCLASSSCHEDULE.APPPROCESS"},
            {"全院专业课表", "/ACTIONQUERYMAJORSCHEDULE.APPPROCESS"},
            {"全院教室课表", "/ACTIONQUERYCLASSROOMSCHEDULE.APPPROCESS"},
            {"学生课程表", "/ACTIONQUERYSTUDENTSCHEDULEBYSELF.APPPROCESS?m=1"},
            {"学生成绩查询", "/ACTIONQUERYSTUDENTSCORE.APPPROCESS"},
            {"毕业成绩单", "/ACTIONQUERYGRADUATESCHOOLREPORTBYSELF.APPPROCESS"},
            {"学分查询", "/ACTIONQUERYSTUDENTTASKSCORE.APPPROCESS"},
            {"考试日程查询", "/ACTIONQUERYEXAMTIMETABLEBYSTUDENT.APPPROCESS?mode=2"},
            {"教室占用查询", "/ACTIONQUERYCLASSROOMUSEBYWEEKDAYSECTION.APPPROCESS"},
            {"空闲教室查询", "/ACTIONQUERYCLASSROOMNOUSE.APPPROCESS"},
            {"网络报名结果查询", "/ACTIONQUERYBMRESULT.APPPROCESS"},
            {"教师考评", "/ACTIONJSATTENDAPPRAISE_001.APPPROCESS"},
            {"学籍注册查询", "/ACTIONQUERYBASESTUDENTINFO.APPPROCESS"},
            {"学业预警", "/ACTIONQUERYBASESTUDENTINFO.APPPROCESS?mode=3"},
        };

        /// <summary>
        /// Extract captcha image source url from requested html
        /// </summary>
        /// <returns>source url of image</returns>
        public static async Task<string> CaptchaImage() {
            var content = await WebUtils.NetworkRequest(Constants.AAO_API_BASE, "", null, async (response) => {
                return await response.Content.ReadAsStringAsync();
            }, HttpMethod.Get);
            if (content == null)
                return " ";
            var document = new HtmlDocument();
            document.LoadHtml(content);
            // this node is the target element, should be a <img>
            var img = document.GetElementbyId("Agnomen").ParentNode.Element("img");
            return String.Format("{0}/{1}", Constants.AAO_API_BASE, img.Attributes["src"].Value);
        }

        /// <summary>
        /// Try to login into aao.neu.edu.cn
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <param name="captcha">captcha</param>
        /// <returns>reason of login failure, empty if success</returns>
        public static async Task<string> Login(string username, string password, string captcha) {
            var url = Constants.AAO_API_BASE + "/ACTIONLOGON.APPPROCESS?mode=";
            var sb = new StringBuilder();
            sb.Append("WebUserNO=").Append(WebUtils.UrlEncode(username))
              .Append("&Password=").Append(WebUtils.UrlEncode(password))
              .Append("&Agnomen=").Append(captcha)
              .Append("&submit7=%B5%C7%C2%BC");

            return await WebUtils.NetworkRequest(url, sb.ToString(), (request) => {
                request.Headers.Add("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
                request.Headers.Add("Referer", Constants.AAO_API_BASE);
            }, async (response) => { 
                var regex = @"<script language=""JavaScript"">\s*alert\(""([^""]*)""\);\s*</script>";
                var match = Regex.Match(await response.Content.ReadAsStringAsync(), regex);
                return match.Success ? match.Groups[1].Value : "";
            }, HttpMethod.Post);
        }

        public static async Task<bool> Logout(string username, string password) {
            return await Task.Run(() => true);
        }

        public static async Task<string> RequestInfomation(string infoname) {
            string address;
            if (!addresses.TryGetValue(infoname, out address))
                return null;
            var url = String.Format("{0}{1}", Constants.AAO_API_BASE, address);
            return await WebUtils.NetworkRequest(url, "", null, async (response) => {
                return await response.Content.ReadAsStringAsync();
            }, HttpMethod.Get);
        }
    }
}
