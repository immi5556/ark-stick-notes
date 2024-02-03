using Ark.Sqlite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace ark.bible.analysis.Api
{
    [Route("api/v1/sticky")]
    [ApiController]
    public class StickyController : ControllerBase
    {
        CurrentUser _cu;
        IWebHostEnvironment _env;
        public StickyController(CurrentUser cu, IWebHostEnvironment env)
        {
            _cu = cu;
            _env = env;
        }
        [Route("upsert")]
        [HttpPost]
        public Record UpsertSticky(Record recor)
        {
            return new StickManager(_cu).UpsertSticky(recor);
        }
        [Route("otp/send")]
        [HttpPost]
        public Secret OtpSend(Secret secret)
        {
            try
            {
                if (secret == null || string.IsNullOrEmpty(secret.email)) throw new ArgumentNullException("secret.email");
                if (!IsValidEmail(secret.email)) throw new ArgumentNullException("invalid email entered.");
                secret.ip = String.IsNullOrEmpty(secret.ip) ? Request.Cookies["sticky_ip"] ?? "" : secret.ip;
                var cu = new CurrentUser() { email = secret.email.ToLower().Trim().Replace("@", "_at_the_rate_").Replace(".", "_dot_"), ip = secret.ip };
                return new StickManager(cu).SendOtp(secret);
            }
            catch (Exception exp)
            {
                return new Secret() { error = exp.Message };
            }
        }
        [Route("otp/verify")]
        [HttpPost]
        public Secret OtpVerify(Secret secret)
        {
            try
            {
                if (secret == null || string.IsNullOrEmpty(secret.email)) throw new ArgumentNullException("secret.email");
                if (!IsValidEmail(secret.email)) throw new ArgumentNullException("invalid email entered.");
                secret.ip = String.IsNullOrEmpty(secret.ip) ? Request.Cookies["sticky_ip"] ?? "" : secret.ip;
                var cu = new CurrentUser() { email = secret.email.ToLower().Trim().Replace("@", "_at_the_rate_").Replace(".", "_dot_"), ip = secret.ip };
                return new StickManager(cu).VerifyOtp(secret);
            }
            catch (Exception exp)
            {
                return new Secret() { error = exp.Message };
            }
        }
        [Route("download")]
        [HttpGet]
        public FileResult DownloadFile()
        {
            var rows = new StickManager(_cu).GetStickyRows();
            rows.ForEach(t =>
            {
                t.title = t.title.Replace("<div>", "").Replace("</div>", Environment.NewLine).Replace("<br>", Environment.NewLine).Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine);
                t.value = t.value.Replace("<div>", "").Replace("</div>", Environment.NewLine).Replace("<br>", Environment.NewLine).Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine);
            });
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(rows.ToCsv<Record>(new string[] { "key", "ip" }));
            return File(bytes, "application/octet-stream", $"sticky_{DateTime.Now.ToString("yyyyMMddhhmmss")}.csv");
        }
        [Route("audio/upload")]
        [HttpPost]
        public async Task<dynamic> UploadMp3()
        {
            try
            {
                string url = "";
                Console.WriteLine(Request.Form.Files.Count);
                foreach (var v in Request.Form.Files)
                {
                    var fn = $"{DateTime.Now.ToString("yyyyMMddHHmmssFFF")}.mp3";
                    var em = (string.IsNullOrEmpty(_cu.email) ? "NA" : _cu.email);
                    var dir = System.IO.Path.Combine(_env.WebRootPath, "aud", em);
                    url = $"/api/v1/sticky/audio/stream/{em}/{fn}";
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    var fll = System.IO.Path.Combine(dir, fn);
                    using (Stream fileStream = new FileStream(fll, FileMode.Create))
                    {
                        await v.CopyToAsync(fileStream);
                    }
                }
                return new
                {
                    msg = $"Uploaded {Request.Form.Files.Count} count Success.",
                    url = url
                };
            }
            catch (Exception e)
            {
                return new
                {
                    msg = $"error: {e.ToString()}"
                };
            }
        }
        [HttpGet]
        [Route("audio/stream/{em}/{uid}")]
        public IActionResult GetAudStream([FromRoute] string em, [FromRoute] string uid)
        {
            var path = System.IO.Path.Combine(_env.WebRootPath, "aud", em, uid);
            var res = File(System.IO.File.OpenRead(path), "audio/mp3");
            res.EnableRangeProcessing = true;
            return res;
        }
        //public async Task GetStreaming([FromRoute] string em, [FromRoute] string uid)
        //{
        //    var fll = System.IO.Path.Combine(_env.WebRootPath, "aud", em, uid);
        //    this.Response.StatusCode = 200;
        //    //this.Response.Headers.Add(HeaderNames.ContentDisposition, $"attachment; filename=\"{Path.GetFileName(filePath)}\"");
        //    this.Response.Headers.Add(HeaderNames.ContentType, "application/octet-stream");
        //    var inputStream = new FileStream(fll, FileMode.Open, FileAccess.Read);
        //    var outputStream = this.Response.Body;
        //    const int bufferSize = 1 << 10;
        //    var buffer = new byte[bufferSize];
        //    while (true)
        //    {
        //        var bytesRead = await inputStream.ReadAsync(buffer, 0, bufferSize);
        //        if (bytesRead == 0) break;
        //        await outputStream.WriteAsync(buffer, 0, bytesRead);
        //    }
        //    await outputStream.FlushAsync();
        //}
        static bool IsValidEmail(string email)
        {
            var trimmedEmail = (email ?? "").Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(trimmedEmail);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
