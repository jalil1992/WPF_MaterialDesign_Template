using Anticaptcha.Api;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using PCKLIB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Cookie = System.Net.Cookie;
using Size = System.Drawing.Size;

namespace WPF_M_Selenium
{
    public class Browser : IBrowser
    {
        public bool m_status_ok = true; // default working status
        System.Timers.Timer m_timer = new System.Timers.Timer();
        public List<Session> m_sess =  new List<Session>();
        public List<DataTable> m_result_set = new List<DataTable>();
        public Session m_cur_sess = new Session();
        public Browser(string ID, List<Session> sess)
        {
            m_ID = ID;
            m_sess = sess;
            m_proxy = null;

            m_timer = new System.Timers.Timer();
            m_timer.Interval = 500;
            m_timer.Elapsed += M_timer_Tick;
            m_timer.Start();

            int _ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int _ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            // calc layout
            int cnt = 1;
            int r, c;
            if (cnt > 10)
            {
                r = (cnt - 1)/ 5 + 1;
                c = 5;
            }
            else if(cnt > 4)
            {
                r = (cnt - 1) / 3 + 1;
                c = 3;
            }
            else
            {
                r = 1;
                c = cnt;
            }
            m_size.Width = _ScreenWidth / c;
            m_size.Height = (_ScreenHeight - 100) / r - 10;

            m_size.Width = Math.Min(1024, m_size.Width);
            m_size.Height = Math.Min(800, m_size.Height);

            m_location.X = 0;
            m_location.Y = 0;

            m_incognito = false;
            m_dis_js = false;
            m_dis_webrtc = false;
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (check_global_finish())
                m_status_ok = false;
            if (m_status_ok == false)
            {
                finish_work("Stopped :\\");
            }
        }

        public void finish_work(string msg, bool ok = false)
        {
            try
            {
                m_timer.Stop();
                App.log_info($" Processing ended : {msg}");
                try
                {
                    quit();
                }
                catch (Exception)
                {

                }

                //App.g_status = ScraperStatus.STOPPED;
                //App.g_main_wnd.refresh_status();
            }
            catch (Exception ex)
            {
                App.log_info($" Error occured. {ex.Message}\n {ex.StackTrace}");
            }
        }

        bool check_global_finish()
        {
            //if (App.g_status == ScraperStatus.STOPPED)
            {
                return true;
            }
            
            return false;
        }

        public async void work_flow()
        {
            try
            {
                if (!await start())
                {
                    finish_work("Chrome starting failed");
                    return;
                }

                // ** TODO
                foreach(var sess in m_sess)
                {
                    m_cur_sess = sess;
                    await run_session(sess);
                }

                finish_work("Finished.", true);
                return;
            }
            catch (Exception ex) {
                App.log_error(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public async Task<bool> need_to_solve_captcha()
        {
            var xpath = "//div[@id='ctl00_ContentPlaceHolder1_as1_pnlReCaptcha']//iframe";
            switch_iframe(xpath);
            xpath = "//div[@id='recaptcha-accessible-status']";
            var status = await get_value(xpath);
            switch_iframe_back();
            return status != "You are verified" && status != "";
        }

        public async Task<bool> run_session(Session sess)
        {
            try
            {
                //App.log_info($"Session {sess.name} started");
                //await navigate("https://www.georgiapublicnotice.com/Search.aspx");



                //// check captcha and click search
                //int trial_cnt = 0;
                //while (true)
                //{
                //    if (await need_to_solve_captcha())
                //    {
                //        // solve captcha
                //        if (!await solve_captcha())
                //        {
                //            App.log_info("Failed to solve captcha. Please check your API key.");
                //            return false;
                //        }
                        
                //        await click_element("//input[contains(@id, 'btnGo')]");
                //        await delay(3);
                //        if (await need_to_solve_captcha())
                //        {
                //            trial_cnt++;
                //            if (trial_cnt == 5)
                //            {
                //                App.log_info("Failed to solve captcha. Please retry later.");
                //                return false;
                //            }
                //            continue;
                //        }
                //    }
                //    App.log_info("Captcha solved.");
                //    break;
                //}

                //// set search conditions
                //// search term
                //var xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_txtSearch']";
                //await enter_text(xpath, sess.search_term);
                
                //// search type
                //switch(sess.search_type)
                //{
                //    case SearchType.ALL_WORDS:
                //        xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_rdoType_0']";
                //        break;
                //    case SearchType.ANY_WORDS:
                //        xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_rdoType_1']";
                //        break;
                //    case SearchType.EXACT_PHRASE:
                //        xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_rdoType_2']";
                //        break;
                //}
                //await click_element(xpath);

                //// exclude term
                //xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_txtExclude']";
                //await enter_text(xpath, sess.exclude_term);
                
                //// county
                //if(sess.search_county.Count > 0)
                //{
                //    xpath = "//div[@id='ctl00_ContentPlaceHolder1_as1_divCounty']/label";
                //    await click_element(xpath);
                //    foreach(var county in sess.search_county)
                //    {
                //        string county_name = Constants.Counties[(int)county];
                //        xpath = $"//ul[@id='ctl00_ContentPlaceHolder1_as1_lstCounty']//label[text()='{county_name}']";
                //        if (await wait_present(xpath, 0.1))
                //        {
                //            await click_element(xpath);
                //            await delay(0.5);
                //        }
                //    }
                //}

                //// city 
                //if (sess.search_city.Count > 0)
                //{
                //    xpath = "//div[@id='ctl00_ContentPlaceHolder1_as1_divCity']/label";
                //    await click_element(xpath);
                //    foreach (var city in sess.search_city)
                //    {
                //        string city_name = Constants.Cities[(int)city];
                //        xpath = $"//ul[@id='ctl00_ContentPlaceHolder1_as1_lstCity']//label[text()='{city_name}']";
                //        if(await wait_present(xpath, 0.1))
                //        {
                //            await click_element(xpath);
                //            await delay(0.5);
                //        }
                //    }
                //}

                //// publication 
                //if (sess.publication.Count > 0)
                //{
                //    xpath = "//div[@id='ctl00_ContentPlaceHolder1_as1_divPublication']/label";
                //    await click_element(xpath);
                //    foreach (var pub in sess.publication)
                //    {
                //        string pub_name = Constants.Publications[(int)pub];
                //        xpath = $"//ul[@id='ctl00_ContentPlaceHolder1_as1_lstPublication']//label[text()='{pub_name}']";
                //        if (await wait_present(xpath, 0.1))
                //        {
                //            await click_element(xpath);
                //            await delay(0.5);
                //        }
                //    }
                //}

                //// date range
                //xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_rbRange']";
                //await click_element(xpath);
                //xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_txtDateFrom']";
                //await set_value(xpath, sess.date_from);
                //xpath = "//input[@id='ctl00_ContentPlaceHolder1_as1_txtDateTo']";
                //await set_value(xpath, sess.date_to);

                //// trigger search
                //await click_element("//input[contains(@id, 'btnGo')]");

                //// start scraping
                //DataTable dt = new DataTable();
                //dt.Columns.Add("Time");
                //foreach (var v in sess.variables)
                //{
                //    while(dt.Columns.Contains(v.name))
                //    {
                //        v.name = v.name + "_";
                //    }
                //    dt.Columns.Add(v.name);
                //}

                //while (true)
                //{
                //    xpath = "//table[@id='ctl00_ContentPlaceHolder1_WSExtendedGridNP1_GridView1']/tbody/tr";
                //    if (!await wait_present(xpath, 5))
                //        break;

                //    var cnt = occurence(xpath);
                //    for(int r = 3; r < cnt; r ++) // skip the first two rows and the last one
                //    {
                //        xpath = $"(//table[@id='ctl00_ContentPlaceHolder1_WSExtendedGridNP1_GridView1']/tbody/tr)[{r}]//input[@class='viewButton']";
                //        await click_element(xpath);
                //        // scrape the info
                //        xpath = "//span[@id='ctl00_ContentPlaceHolder1_PublicNoticeDetailsBody1_PublicNoticeDetails1_lblCity']";
                //        if (!await wait_present(xpath, 5))
                //            continue;
                //        var city = await get_value(xpath);
                //        xpath = "//span[@id='ctl00_ContentPlaceHolder1_PublicNoticeDetailsBody1_PublicNoticeDetails1_lblCounty']";
                //        var county = await get_value(xpath);
                //        xpath = "//span[@id='ctl00_ContentPlaceHolder1_PublicNoticeDetailsBody1_lblPublicationDAte']";
                //        var date_str = await get_value(xpath);
                //        DateTime date;
                //        DateTime.TryParse(date_str, out date);
                //        xpath = "//span[@id='ctl00_ContentPlaceHolder1_PublicNoticeDetailsBody1_lblContentText']";
                //        var content = await get_value(xpath);

                //        // parse the content according to the variables
                //        List<string> values = new List<string>();
                //        values.Add(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                //        foreach (var v in sess.variables)
                //        {
                //            // find the earliest showup of the start mask
                //            int start_pos = -1;
                //            foreach(var mask in v.start_masks)
                //            {
                //                var pos = content.ToUpper().IndexOf(mask.ToUpper());
                //                if (pos >= 0)
                //                {
                //                    if (start_pos < 0 || start_pos > pos + mask.Length)
                //                        start_pos = pos + mask.Length;
                //                }
                //            }


                //            if(start_pos < 0)
                //            {
                //                values.Add("");
                //                continue;
                //            }

                //            // find the ending position
                //            int end_pos = -1;
                //            foreach (var mask in v.end_masks)
                //            {
                //                var pos = content.ToUpper().IndexOf(mask.ToUpper(), start_pos);
                //                if (pos >= 0)
                //                {
                //                    if (end_pos < 0 || end_pos > pos)
                //                        end_pos = pos;
                //                }
                //            }

                //            var value = "";
                //            if (end_pos < 0)
                //            {
                //                value = content.Substring(start_pos).Trim();
                //            }
                //            else
                //            {
                //                value = content.Substring(start_pos, end_pos - start_pos);
                //            }
                //            values.Add(value);
                //        }

                //        // add the values ot the result dt
                //        dt.Rows.Add(values.ToArray());

                //        // save result to txt
                //        var filename = $"{county}-{city}-{date.ToString("dd-MM-yy")}-{DateTime.Now.ToString("HHmmss")}.txt";
                //        sess.save_txt(values, filename);

                //        // notify the UI new result
                //        App.g_main_wnd.set_result(dt);

                //        // back to search result list
                //        browser.Navigate().Back();

                //        //if (dt.Rows.Count > 3 && System.Diagnostics.Debugger.IsAttached)
                //        //    break;
                //    }

                //    // check next page
                //    xpath = "//input[@id='ctl00_ContentPlaceHolder1_WSExtendedGridNP1_GridView1_ctl01_btnNext']";
                //    if (!await wait_present(xpath, 1))
                //        break;

                //    var disabled = await get_value(xpath, "", "disabled");
                //    if (disabled != "" && disabled != null)
                //        break;
                //    await click_element(xpath);
                //}

                //// save result to google sheeet
                //await sess.save_gsheet(dt);
                return true;
            }
            catch(Exception ex)
            {
                App.log_error(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> solve_captcha()
        {
            try
            {
                App.log_info("Solving captcha");
                var xpath = "//div[@id='recaptcha']";
                var sitekey = await get_value(xpath, "", "data-sitekey");
                xpath = "//div[@id='ctl00_ContentPlaceHolder1_as1_pnlReCaptcha']//iframe";
                switch_iframe(xpath);
                xpath = "//div[@id='recaptcha-token']";
                var token = await get_value(xpath, "", "value");
                switch_iframe_back();

                Anticaptcha.Helper.DebugHelper.VerboseMode = true;
                var api = new NoCaptchaProxyless
                {
                    ClientKey = App.g_setting.captcha_key,
                    WebsiteUrl = new Uri(browser.Url),
                    WebsiteKey = sitekey,
                    WebsiteSToken = token
                };

                if (!api.CreateTask())
                {
                    App.log_info("AntiCaptcha API v2 creating task failed. " + api.ErrorMessage);
                    return false;
                }
                
                var js = @"
                    element = document.createElement('input');
                    element.id = 'tri_button';
                    element.type = 'button';
                    element.style.backgroundColor = 'red';
                    element.style.color = 'white';
                    element.style.height = '80px';
                    element.value = 'The bot is solving google captcha! Please wait';
                    xpath = ""//div[@id='recaptcha']"";
                    y = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    y.appendChild(element);
                ";
                browser.ExecuteScript(js);
                if (!api.WaitForResult())
                {
                    App.log_info("Could not solve the captcha.");
                    return false;
                }

                // solved
                js = @"
                    xpath = ""//input[@id='tri_button']"";
                    x = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    xpath = ""//div[@id='recaptcha']"";
                    y = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    y.removeChild(x);
                ";
                browser.ExecuteScript(js);
                string response = api.GetTaskSolution().GRecaptchaResponse;
                xpath = "//textarea[@id='g-recaptcha-response']";
                var elem = browser.FindElementByXPath(xpath);
                browser.ExecuteScript($"arguments[0].value='{response}'", elem);
                //await set_value(xpath, response);
                return true;
            }
            catch(Exception ex)
            {
                App.log_error(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> template()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                App.log_error(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }
    }
}
