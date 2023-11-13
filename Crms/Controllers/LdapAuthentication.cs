using System;
using System.Text;
using System.Collections;
using System.Web.Security;

using System.Security.Principal;
using System.DirectoryServices;
using System.Web.Mvc;


namespace Crms.Controllers
{
    public class LdapAuthentication
    {
        private string _path;
        private string _filterAttribute;

        /// <summary>
        /// パスワードが無期限に設定されているかのフラグ
        /// </summary>
        /// <history>
        ///  2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加 新規追加
        /// </history>
        static readonly int ADS_UF_DONT_EXPIRE_PASSWD = 0x10000;
        
        /// <summary>
        /// パスワードの有効期限
        /// </summary>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加 新規追加
        /// </history>
        private static TimeSpan _maxPwdAge = new TimeSpan();


        public LdapAuthentication(string path)
        {
            _path = path;
        }

        /// <summary>
        /// AD認証処理
        /// </summary>
        /// <param name="domain">ドメイン名</param>
        /// <param name="username">ユーザーID</param>
        /// <param name="pwd">パスワード</param>
        /// <returns>戻り値(0:正常 -1:ユーザ無し -2:その他エラー 1:有効期限)</returns>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加 戻り値をbool型→int型に変更
        /// </history>
        public int IsAuthenticated(string domain, string username, string pwd)
        {
            string domainAndUsername = domain + @"\" + username;        //ドメインユーザ名

            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

            int ret = 0;                                                //戻り値

            try
            {
                //有効期限の取得
                DirectorySearcher ds = new DirectorySearcher(entry, "(objectClass=domainDNS)"
                    , new string[] { "maxPwdAge" }
                    , SearchScope.Base);

                SearchResult dsresult = ds.FindOne();

                //ドメイン情報を取得できない場合
                if (dsresult == null)
                {
                    ret = -2;
                }
                else
                {
                    long tick = (long)dsresult.Properties["maxPwdAge"][0];

                    if (tick == long.MinValue)
                    {
                        _maxPwdAge = new TimeSpan(0L); // 無期限
                    }
                    else
                    {
                        _maxPwdAge = new TimeSpan(Math.Abs(tick));
                    }
                }

                //ユーザ情報の取得
                //Bind to the native AdsObject to force authentication.
                object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("userAccountControl");      //ユーザーアカウントコントロール
                search.PropertiesToLoad.Add("pwdLastSet");              //最後にパスワードを変更した日
                SearchResult result = search.FindOne();

                //ユーザが無い
                if (null == result)
                {
                    ret = -1;
                }
                else
                {
                    //パスワードの有効期限を取得
                    DateTime expireDate = GetExpiration((int)result.Properties["userAccountControl"][0], (long)result.Properties["pwdLastSet"][0]);

                    TimeSpan span = expireDate - DateTime.Now;
                   
                    //現在時刻が有効期限の７日以内の場合
                    if (span.Days <= 7)
                    {
                        ret = 1;
                    }
                }

                //Update the new path to the user in the directory.
                _path = result.Path;
                _filterAttribute = (string)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return ret;
        }

        /// <summary>
        /// パスワード変更
        /// </summary>
        /// <param name="domain">ドメイン名</param>
        /// <param name="username">ユーザ名</param>
        /// <param name="oldpassword">現在のパスワード</param>
        /// <param name="newpassword">新しいパスワード</param>
        /// <param name="password">パスワード</param>
        /// <returns>/returns>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加
        /// </history>
        public bool ChangePassword(string domain, string username, string oldpwd, string newpwd, ModelStateDictionary modelstate)
        {
            string domainAndUsername = domain + @"\" + username;        //ドメインユーザ名

            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, oldpwd);

            try
            {
                //ユーザ情報の取得
                object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";

                search.PropertiesToLoad.Add("ADsPath");
                SearchResult result = search.FindOne();

                string adsPath = (string)result.Properties["ADsPath"][0];

                DirectoryEntry dEntry = new DirectoryEntry(adsPath);

                dEntry.Invoke("ChangePassword", oldpwd, newpwd);

                dEntry.CommitChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message) && ex.InnerException.Message.Contains("制限の違反が発生しました"))
                {
                    modelstate.AddModelError("", "入力されたパスワードはアカウントポリシーに反しているため変更できません。システム課にアカウントポリシーを確認して下さい");
                }
                else
                {
                    modelstate.AddModelError("", ex.Message);
                }

                return false;
            }

            return true;
        }

        //public bool IsAuthenticated(string domain, string username, string pwd)
        //{
        //    string domainAndUsername = domain + @"\" + username;
        //    DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);


        //    try
        //    {
        //        //Bind to the native AdsObject to force authentication.
        //        object obj = entry.NativeObject;

        //        DirectorySearcher search = new DirectorySearcher(entry);

        //        search.Filter = "(SAMAccountName=" + username + ")";
        //        search.PropertiesToLoad.Add("cn");
        //        SearchResult result = search.FindOne();

        //        if (null == result)
        //        {
        //            return false;
        //        }

        //        //Update the new path to the user in the directory.
        //        _path = result.Path;
        //        _filterAttribute = (string)result.Properties["cn"][0];
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //    return true;
        //}
        
        public string GetGroups()
        {
            DirectorySearcher search = new DirectorySearcher(_path);
            search.Filter = "(cn=" + _filterAttribute + ")";
            search.PropertiesToLoad.Add("memberOf");
            StringBuilder groupNames = new StringBuilder();

            try
            {
                SearchResult result = search.FindOne();
                int propertyCount = result.Properties["memberOf"].Count;
                string dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (string)result.Properties["memberOf"][propertyCounter];
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }
                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();
        }

        /// <summary>
        /// userAccountControlを調べて無期限に設定されているか調べる
        ///  ticksはパスワードを最後に変更したプロパティpwdLastSetの値 
        /// _maxPwdAgeを調べて、起源が設定されていないかも調べ、最後の
        /// パスワードを変更した日時+_maxPwdAgeから失効日を求める。
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加
        /// </history>
        private static DateTime GetExpiration(int userAccountControl, long ticks)
        {
            
            if (Convert.ToBoolean(userAccountControl & ADS_UF_DONT_EXPIRE_PASSWD))
            {
                // パスワードは無期限
                return DateTime.MaxValue;
            }

            if (ticks == 0)
            {
                // ユーザは次回ログイン時にパスアワードを変更する必要がある
                return DateTime.MinValue;
            }
           
            if (_maxPwdAge.Ticks == 0L)
            {
                // パスワードの期限は無期限
                return DateTime.MaxValue;
            }

            DateTime pwdLastSet = DateTime.FromFileTime(ticks);

            return pwdLastSet.Add(_maxPwdAge);
        }
 
    }
}