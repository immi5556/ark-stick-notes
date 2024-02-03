using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace ark.bible.analysis.Controllers
{
    public class RecordController : Controller
    {
        CurrentUser _cu;
        public RecordController(CurrentUser cu)
        {
            _cu = cu;
        }
        public IActionResult Index()
        {
            if (_cu == null || string.IsNullOrEmpty(_cu.email)) return Redirect(Url.Content("~/"));
            ViewBag.time_records = new StickManager(_cu).GetStickyAll();
            ViewBag.user_email = string.Join('^', _cu.email.Split('@'));
            ViewBag.base_url = _cu.base_url;
            return View();
        }
        public IActionResult Test1()
        {
            if (_cu == null || string.IsNullOrEmpty(_cu.email))
                _cu = new CurrentUser() { email = "test@tt.co", ip = "1.1.1.1", base_url = _cu?.base_url };
            ViewBag.time_records = new StickManager(_cu).GetStickyAll();
            ViewBag.user_email = string.Join('^', _cu.email.Split('@'));
            ViewBag.base_url = _cu.base_url;
            return View();
        }
        static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        static bool IsValidMobile(string mob)
        {
            var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            //var e164PhoneNumber = "+44 117 496 0123";
            //var nationalPhoneNumber = "2024561111";
            //var smsShortNumber = "83835";

            try
            {
                var phoneNumber = phoneNumberUtil.Parse(mob, null);
                return phoneNumberUtil.IsValidNumber(phoneNumber);
            }
            catch
            {
                return false;
            }
            //phoneNumber = phoneNumberUtil.Parse(nationalPhoneNumber, "US");
            //phoneNumber = phoneNumberUtil.Parse(smsShortNumber, "US");
        }
        
        public IActionResult Landing()
        {
            return View();
        }
        public IActionResult Share([FromQuery] string q)
        {
            if (q != null && q.Length > 7 && q.Split('^').Length > 1)
            {
                var id = q.Split('^')[0];
                var v = string.Concat(id.Skip(4).Reverse().Skip(3).Reverse());
                if (int.TryParse(v, out int v1))
                {
                    //ViewBag.share_record = new StickManager(new CurrentUser() { email = $"{q.Split('^')[1]}@{q.Split('^')[2]}", ip = _cu.ip }).GetStickyById(v1);
                    ViewBag.share_record = new StickManager(new CurrentUser() { email = $"{q.Split('^')[1]}", ip = _cu.ip }).GetStickyById(v1);
                    ViewBag.share_email = q.Split('^')[1].Replace("_at_the_rate_", "@").Replace("_dot_", ".");
                }
            }
            return View();
        }
        public IActionResult Aud1()
        {
            return View();
        }
        public IActionResult AudMp3()
        {
            return View();
        }
    }
}
