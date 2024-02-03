using Dapper;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace ark.bible.analysis
{
    public class StickManager
    {
        CurrentUser _cu;
        string con_str = "";
        public StickManager(CurrentUser cu)
        {
            _cu = cu;
            con_str = connection_string(_cu.email);
        }
        static Random generator = new Random();
        public Secret VerifyOtp(Secret secret)
        {
            if (secret == null) throw new ArgumentNullException("secret");
            if (secret.secret_id == null) throw new ArgumentNullException("secret_id");
            var sec = new Ark.Sqlite.SqliteManager(con_str).First<Secret>(select_table_secret(secret.secret_id));
            if (sec == null) throw new InvalidDataException("otp");
            if (sec.otp != secret.otp) throw new DataMisalignedException("otp mismatch");
            if (sec.status.HasValue && sec.status.Value) throw new DataMisalignedException("otp expired.");
            new Ark.Sqlite.SqliteManager(con_str).ExecuteQuery(update_table_secret(sec.secret_id, sec.otp, true, sec.ip));
            return sec;
        }
        public Secret SendOtp(Secret secret)
        {
            string r = generator.Next(0, 9999).ToString("D4");
            SendEmail(secret.email, email_otp_template(r));
            var sec = new Secret();
            sec.otp = r;
            sec.status = false;
            UpsertSecret(sec);
            sec.otp = null;
            return sec;
        }
        public void SendEmail(string? to, string htmlString)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse("sticky.notes@immanuel.co");
            //email.From.Add(InternetAddress.Parse("sticky.notes@immanuel.co"));
            email.From.Add(new MailboxAddress("Sticky-Notes (ARK)", "sticky.notes@immanuel.co"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Bcc.Add(MailboxAddress.Parse("raj@immanuel.co"));
            email.Bcc.Add(MailboxAddress.Parse("sticky.notes@immanuel.co"));
            email.Subject = "Stick Notes: OTP";
            var builder = new BodyBuilder();
            builder.HtmlBody = htmlString;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect("mail.immanuel.co", 2525, SecureSocketOptions.Auto);
            smtp.Authenticate("sticky.notes@immanuel.co", "Loo0@$E7");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
        //static void Email(string? to, string htmlString)
        //{
        //    //https://clientarea.mochahost.com/knowledgebase/472/Setting-up-your-e-mail-client-manually-cPanel.html
        //    MailMessage message = new MailMessage();
        //    SmtpClient smtp = new SmtpClient();
        //    message.From = new MailAddress("raj@immanuel.co");
        //    message.Sender = new MailAddress("raj@immanuel.co");
        //    message.To.Add(to);
        //    message.Subject = "Stick Notes: OTP";
        //    message.IsBodyHtml = true; 
        //    message.Body = htmlString;
        //    smtp.Port = 2525;
        //    smtp.Host = "mail.immanuel.co";
        //    smtp.EnableSsl = true;
        //    smtp.UseDefaultCredentials = false;
        //    smtp.Credentials = new NetworkCredential("raj@immanuel.co", "<<TOBEFILLED>>");
        //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //    smtp.Send(message);
        //}
        Secret UpsertSecret(Secret secret)
        {
            if (secret == null) throw new ArgumentNullException("secret");
            new Ark.Sqlite.SqliteManager(con_str).CreateTable(create_table_secrets());
            secret.ip = string.IsNullOrEmpty(secret.ip) ? _cu.ip : secret.ip;
            if (secret.secret_id == null || secret.secret_id <= 0 || new Ark.Sqlite.SqliteManager(con_str).ExecuteSelect<Secret>(select_table_secret(secret.secret_id)) == null)
            {
                secret.secret_id = null;
                secret.secret_id = (long)new Ark.Sqlite.SqliteManager(con_str).ExecuteQuery(insert_table_secret(secret.otp, secret.status, secret.ip));
            }
            else
            {
                new Ark.Sqlite.SqliteManager(con_str).ExecuteQuery(update_table_secret(secret.secret_id, secret.otp, secret.status, secret.ip));
            }
            return secret;
        }
        public Record UpsertSticky(Record record)
        {
            if (record == null) throw new ArgumentNullException("record");
            if (string.IsNullOrEmpty(_cu.email)) throw new ArgumentNullException("_cu.email");
            new Ark.Sqlite.SqliteManager(con_str).CreateTable(create_table_records(_cu.email));
            record.ip = string.IsNullOrEmpty(record.ip) ? _cu.ip : record.ip;
            try
            {
                if (record.record_id == null || record.record_id <= 0 || new Ark.Sqlite.SqliteManager(con_str).ExecuteSelect<Record>(select_table_records(record.record_id)) == null)
                {
                    record.record_id = null;
                    record.record_id = (long)new Ark.Sqlite.SqliteManager(con_str).ExecuteQuery(insert_table_records(_cu.email, record.title, record.key, record.value, record.ip, record.color));
                    record = new Ark.Sqlite.SqliteManager(con_str).First<Record>(select_table_records(record.record_id));
                }
                else
                {
                    new Ark.Sqlite.SqliteManager(con_str).ExecuteQuery(update_table_records(record.record_id, _cu.email, record.title, record.key, record.value, record.ip, record.color));
                    record = new Ark.Sqlite.SqliteManager(con_str).First<Record>(select_table_records(record.record_id));
                }
                return record;
            }
            catch (Exception exp)
            {
                if (++retry_cnt < 3 && ManageColorColumnError(exp)) return UpsertSticky(record);
                throw;
            }
        }
        public Record GetStickyById(int id)
        {
            try
            {
                return new Ark.Sqlite.SqliteManager(con_str).First<Record>(select_table_records(id));
            }
            catch (Exception exp)
            {
                if (exp.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) || ++retry_cnt > 2)
                    return new Record();
                if (ManageColorColumnError(exp)) return GetStickyById(id);
                throw;
            }
        }
        public List<Record> GetStickyRows()
        {
            try
            {
                var rest = new Ark.Sqlite.SqliteManager(con_str).ExecuteSelect<Record>(@$"SELECT title, value, at, color FROM records;").ToList();
                return rest;
            }
            catch (Exception exp)
            {
                if (exp.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) || ++retry_cnt > 2)
                    return new List<Record>();
                if (ManageColorColumnError(exp)) return GetStickyRows();
                throw;
            }
        }
        bool ManageColorColumnError(Exception exp)
        {
            //no such column: color
            //table records has no column named color
            if (exp.Message.Contains("no such column: color", StringComparison.OrdinalIgnoreCase) ||
                exp.Message.Contains("table records has no column named color", StringComparison.OrdinalIgnoreCase))
            {
                string qq = @"alter table records add column color text null;";
                new Ark.Sqlite.SqliteManager(con_str).CreateTable(qq);
                return true;
            }
            return false;
        }
        int retry_cnt = 0;
        public Dictionary<string, List<Record>> GetStickyAll()
        {
            try
            {
                var rest = new Ark.Sqlite.SqliteManager(con_str).ExecuteSelect<Record>(select_table_records(null)).ToList();
                return rest.GroupBy(t => t.at.Value.Date).OrderByDescending(t => t.Key).Select(t => new
                {
                    key = t.Key.Date.ToLongDateString(),
                    records = t.OrderByDescending(tt => tt.at).AsList()
                }).ToDictionary(k => k.key, v => v.records);
            }
            catch (Exception exp)
            {
                if (exp.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) || ++retry_cnt > 2)
                    return new Dictionary<string, List<Record>>();
                if (ManageColorColumnError(exp)) return GetStickyAll();
                throw;
            }
        }
        static Func<string?, string> cleanup = str => Ark.Sqlite.SqliteManager.ReplaceSpecialChar(str ?? "", new Dictionary<string, string?>() { { "'", "''" } });
        Func<string, string> connection_string = (string email) => $"Data Source={email.ToLower().Trim().Replace("@", "_at_the_rate_").Replace(".", "_dot_")}.db";
        Func<string, string> create_table_records = (string email) =>
        @$"CREATE TABLE  IF NOT EXISTS records (
    record_id INTEGER NOT NULL
                      CONSTRAINT PK_records PRIMARY KEY AUTOINCREMENT,
    title     TEXT,
    [key]     TEXT,
    value     TEXT,
    ip        TEXT,
    at        TEXT,
    color     TEXT
);
";
        Func<string> create_table_secrets = () =>
        @$"CREATE TABLE  IF NOT EXISTS secrets (
            secret_id INTEGER NOT NULL
                              CONSTRAINT PK_secrets PRIMARY KEY AUTOINCREMENT,
            otp     TEXT,
            status     BOOLEAN,
            ip        TEXT,
            at        TEXT
        );
        ";

        Func<long?, string> select_table_secret = (long? secret_id) =>
        @$"SELECT secret_id, otp, status, ip, at
        FROM secrets {(secret_id.HasValue && secret_id.Value > 0 ? $"where secret_id = {secret_id}" : "")}";

        Func<string, bool?, string, string> insert_table_secret = (string otp, bool? status, string ip) =>
        @$"INSERT INTO secrets (
                        otp,
                        status,
                        ip,
                        at
                    )
                    VALUES (
                        '{cleanup(otp)}',
                        '{(status.HasValue ? cleanup(status.ToString().ToLowerInvariant()) : null)}',
                        '{cleanup(ip)}',
                        '{DateTime.UtcNow}'
                    );
";

        Func<long?, string, bool?, string, string> update_table_secret = (long? secret_id, string otp, bool? status, string ip) =>
         @$"UPDATE secrets 
            set otp = '{cleanup(otp)}',
                        status = '{(status.HasValue ? cleanup(status.ToString().ToLowerInvariant()) : null)}',
                        ip = '{cleanup(ip)}',
                        at = '{DateTime.UtcNow}'
                    where secret_id = {secret_id};
";

        Func<long?, string?, string?, string?, string?, string?, string?, string> update_table_records = (long? record_id, string? email, string? title, string? key, string? val, string? ip, string? color) =>
        @$"UPDATE records
   SET title = '{cleanup(title)}',
       [key] = '{cleanup(key)}',
       value = '{cleanup(val)}',
       ip = '{cleanup(ip)}',
       color = '{color}',
       at = '{DateTime.UtcNow}'
 WHERE record_id = {record_id};
";
        Func<string?, string?, string?, string?, string?, string?, string> insert_table_records = (string? email, string? title, string? key, string? val, string? ip, string? color) =>
        @$"INSERT INTO records (
                        title,
                        [key],
                        value,
                        ip,
                        at,
                        color
                    )
                    VALUES (
                        '{cleanup(title)}',
                        '{cleanup(key)}',
                        '{cleanup(val)}',
                        '{cleanup(ip)}',
                        '{DateTime.UtcNow}',
                        '{color}'
                    );
";
        Func<long?, string> select_table_records = (long? record_id) =>
        @$"SELECT record_id, title, [key], value, ip, at, color
        FROM records {(record_id.HasValue && record_id.Value > 0 ? $"where record_id = {record_id}" : "")}";

        Func<string, string> email_otp_template = (string otp) => @$"<div style='font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2'>
  <div style='margin:50px auto;width:70%;padding:20px 0'>
    <div style='border-bottom:1px solid #eee'>
      <a href='' style='font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600'>Sticky Notes (ARK)</a>
    </div>
    <p style='font-size:1.1em'>Hi,</p>
    <p>Use the following OTP to complete your login. OTP is valid for 1 Hour</p>
    <h2 style='background: #00466a;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;'>{otp}</h2>
    <p style='font-size:0.9em;'>Regards,<br />Sticky Team :)</p>
    <hr style='border:none;border-top:1px solid #eee' />
    <div style='float:right;padding:8px 0;color:#aaa;font-size:0.8em;line-height:1;font-weight:300'>
      <p>ARK</p>
      <p>Rapid Team</p>
    </div>
  </div>
</div>";

    }
}
