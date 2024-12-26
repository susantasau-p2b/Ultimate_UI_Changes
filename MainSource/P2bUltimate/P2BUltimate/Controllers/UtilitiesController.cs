using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using System.Text;
using System.IO;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class UtilitiesController : Controller
    {
        public ActionResult generateCaptcha()
        {
            System.Drawing.FontFamily family = new System.Drawing.FontFamily("Arial");
            //CaptchaImage img = new CaptchaImage(150, 50, family);
            CaptchaImage.CaptchaImage img = new CaptchaImage.CaptchaImage(150, 50, family);
            string text = img.CreateRandomText(4) + " " + img.CreateRandomText(3);
            img.SetText(text);
            img.GenerateImage();
            Session["captchaText"] = text;

            img.Image.Save(Server.MapPath("~/images/Captcha/") + this.Session.SessionID.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            img.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //var imgtoconvert = this.Session.SessionID.ToString()+".png";
            byte[] ar = ms.ToArray();
            var converted_image = Convert.ToBase64String(ar);
            return Json(converted_image, JsonRequestBehavior.AllowGet);
        }
    }
}