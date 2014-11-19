using ETour.Common;
using ETour.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ETour.Common;
namespace ETour.Web.Controllers
{
    public class HomeController : WebController
    {
        //
        // GET: /Home/
        public string Pic_Addresss = ConfigHelper.GetConfigString("Pic_Address");
        public ActionResult Index()
        {
            //return RedirectToAction("Index_Test");
            return View();
        }

        #region -------------登录

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View(new LoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            BLL.SS_User userbll = new BLL.SS_User();
            Model.SS_User usermodel = userbll.GetModel(model.UserName, Common.Utils.Md5_16(model.Password));
            if (usermodel == null)
            {
                return JavaScript(JavaScriptMsg("错误提示：用户名或密码错误，请重试."));
            }
            FormsAuthentication.SetAuthCookie(usermodel.User_Name, true);
            return JavaScript(JavaScriptTypeMsg("登录成功", "User_Center"));
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        #endregion

        #region -------------注册

        public ActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        public ActionResult Register(RegisterModel regmodel)
        {
            string username = regmodel.Mobile;
            if (username == "")
            {
                return JavaScript(JavaScriptMsg("错误：手机号码不能为空."));
            }

            string telcode = regmodel.valCode;
            if (telcode == "")
            {
                return JavaScript(JavaScriptMsg("错误：激活码不能为空."));
            }

            string password = regmodel.Password;

            if (password == "")
            {
                return JavaScript(JavaScriptMsg("错误：密码不能为空."));
            }
            string userip = WebRequests.GetIP();

            #region 检查各项并提示
            //检查是否开启会员功能
            //if (siteConfig.memberstatus == 0)
            //{
            //    context.Response.Write("{\"status\":0, \"msg\":\"对不起，会员功能已关闭，无法注册.\"}");
            //    return;
            //}

            ////检查同一IP注册时隔
            //if (BLL.TP_Register_Log.Instance.ExistsByIP(userip, userConfig.regctrl))
            //{
            //    context.Response.Write("{\"status\":0, \"msg\":\"对不起，同IP在" + userConfig.regctrl + "小时内禁止重复注册.\"}");
            //    return;
            //}

            ////不允许同一手机号码注册不同用户
            //if (userConfig.mobilelogin == 1)
            //{
            //    if (BLL.TP_Register_Log.Instance.ExistsMobile(mobile))
            //    {
            //        context.Response.Write("{\"status\":0, \"msg\":\"对不起，该手机号码已被注册.\"}");
            //        return;
            //    }
            //}
            //检查默认组别是否存在

            #endregion

            BLL.TP_Register_Log Bll = new BLL.TP_Register_Log();
            Model.TP_Register_Log model = new Model.TP_Register_Log();
            //验证激活码
            if (Session[WebKeys.SESSION_SMS_CODE] != null && Session[WebKeys.SESSION_SMS_CODE].ToString() != "")
            {
                if (telcode != Session[WebKeys.SESSION_SMS_CODE].ToString())
                {
                    return JavaScript(JavaScriptMsg("错误：激活码不匹配."));
                }
            }
            else
            {
                model = Bll.GetModel("M", username, telcode);
                if (model == null || telcode != model.User_Pass)
                {
                    return JavaScript(JavaScriptMsg("错误：激活码不匹配."));
                }

            }
            //将填写的信息导入到user表中
            Model.SS_User usermodel = new Model.SS_User();
            usermodel.User_Tag = "W";
            usermodel.Mobile = username;
            usermodel.Password = DESEncrypt.Md516(password);
            usermodel.Create_DateTime = DateTime.Now;
            usermodel.In_Use = 1;
            usermodel.Mail_Tag = 1;
            usermodel.Enabled = 1;
            usermodel.User_Name = "ETour" + Utils.Number(6) + DateTime.Now.Millisecond;
            usermodel.Nick_Name = usermodel.User_Name;
            usermodel.Avatar = "images/portrait_100_100.jpg";
            usermodel.Gender = "N";
            usermodel.Birthday = Convert.ToDateTime("1900-01-01");
            usermodel.Employee = new Model.SS_Employee();
            usermodel.Employee.Enabled = 1;

            int newId = new BLL.SS_User().Add(usermodel);
            if (newId < 1)
            {
                return JavaScript(JavaScriptMsg("系统故障，请联系ETour."));
            }

            FormsAuthentication.SetAuthCookie(usermodel.User_Name, true);

            //防止Session提前过期

            Session[WebKeys.SESSION_USER_INFO] = usermodel;
            Session.Timeout = 45;
            Utils.WriteCookie(WebKeys.COOKIE_USER_NAME_REMEMBER, "ETour", username);
            Utils.WriteCookie(WebKeys.COOKIE_USER_PWD_REMEMBER, "ETour", DESEncrypt.Md516(password));
            Utils.WriteCookie(WebKeys.COOKIE_USER_ID, "ETour", newId.ToString());//用户ID
            return JavaScript("alert('注册成功');location.href='user_center';");
        }


        [HttpPost] //===发送手机注册码OK
        public ActionResult User_Register_Smscode(string _mobile, string _password, string _repassword)
        {
            if (_mobile == "")
            {
                return Json(new { status = 0, msg = "发送失败，请填写手机号码." });
            }
            //检查是否过快
            string cookie = Utils.GetCookie("user_register_sms");
            if (cookie == _mobile)
            {
                return Json(new { status = 0, msg = "刚已发送过短信，请1分钟后再试." });
            }
            string strcode = Utils.Number(6); //随机验证码
            //发送短信
            string username = _mobile;
            string password = _password;
            string userip = WebRequests.GetIP();
            string user_type = "M";
            if (password == "")
            {
                return Json(new { status = 0, msg = "错误：密码不能为空." });
            }
            if (username == "")
            {
                return Json(new { status = 0, msg = "错误：手机号码不能为空." });
            }
            #region 检查各项并提示
            //检查是否开启会员功能
            //不允许同一手机号码注册不同用户
            //if (userConfig.mobilelogin == 1)
            //{
            //    if (BLL.TP_Register_Log.Instance.ExistsMobile(mobile))
            //    {
            //        context.Response.Write("{\"status\":0, \"msg\":\"对不起，该手机号码已被注册.\"}");
            //        return;
            //    }
            //} 
            #endregion
            BLL.TP_Register_Log Bll = new BLL.TP_Register_Log();
            Model.TP_Register_Log model = new Model.TP_Register_Log();
            //保存注册信息
            model.Enabled = 1;
            model.User_Name = _mobile;
            model.MSG_Date = DateTime.Now;
            model.User_Type = user_type;
            model.User_Tag = "W";
            model.User_Pass = strcode;

            int newId = Bll.Add(model);
            if (newId < 1)
            {
                return Json(new { status = 0, msg = "系统故障，请联系网站管理员." });
            }
            Bll.UpdateField("User_Tag='W' and User_Type='M' and User_Name='" + username + "' and ID <>" + newId, "Enabled=0");

            //model = Bll.GetModel(newId);

            //短信内容
            string msgContent = "" + strcode + "(手机注册验证码,请完成验证),如非本人操作,请忽略本短信";

            SMSSend ws = new SMSSend();
            string results = ws.IPYY_SMS_POST(ConfigHelper.GetConfigString("SMSUID"), ConfigHelper.GetConfigString("SMSPWD"), _mobile, msgContent);
            if (results != "Success")
            {
                return Json(new { status = 0, msg = "短信发送失败，请再次检查输入手机号码." });
            }

            //写入SESSION，保存验证码
            Session[WebKeys.SESSION_SMS_CODE] = strcode;
            Utils.WriteCookie("user_register_sms", _mobile, 1); //1分钟内无重复发送
            return Json(new { status = 1, msg = "短信发送成功，请注意查收验证码." });
        }

        #endregion

        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }
            BLL.D_Guides_INFO gbll = new BLL.D_Guides_INFO();
            Model.D_Guides_INFO gmodel = gbll.GetModel(id.Value);
            ViewBag.PIC_Address = Pic_Addresss;
            return View(gmodel);
        }

        public ActionResult GetIndexList()
        {
            BLL.D_Guides_INFO gbll = new BLL.D_Guides_INFO();
            var list = gbll.GetIndexList(5);

            return Json(new
            {
                data = list.Select(x => new { title = x.Title, img_url = Pic_Addresss + x.Img_Url, id = x.Guide_ID })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetIndexListAll()
        {
            BLL.D_Guides_INFO gbll = new BLL.D_Guides_INFO();
            var list = gbll.GetIndexList();

            return Json(new
            {
                data = list.Select(x => new { title = x.Title, img_url = Pic_Addresss + x.Img_Url, id = x.Guide_ID })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContentById(string id)
        {
            BLL.D_Guides_INFO gbll = new BLL.D_Guides_INFO();
            Model.D_Guides_INFO gmodel = gbll.GetModel(int.Parse(id));
            gmodel.Base_Address = Pic_Addresss;
            return Json(new { success = true, data = gmodel }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListByKey(string keywords)
        {
            BLL.D_Guides_INFO gbll = new BLL.D_Guides_INFO();
            var list = gbll.GetListByKey(keywords);

            return Json(new
            {
                data = list.Select(x => new { title = x.Title, id = x.Guide_ID })
            }, JsonRequestBehavior.AllowGet);
        }



        [Authorize]
        public ActionResult User_Center()
        {
            return View(CurrentUser);
        }

        private ETour.Model.SS_User _CurrentUser;
        protected ETour.Model.SS_User CurrentUser
        {
            get
            {
                if (_CurrentUser == null)
                    _CurrentUser = new BLL.SS_User().GetByUserName(User.Identity.Name);
                return _CurrentUser;
            }
        }
    }
}
