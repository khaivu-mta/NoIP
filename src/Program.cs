using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace noip
{
    internal static class Program
    {
        private const string defaultPath = "Settings/appsettings";
        private const string defaultPathChrome = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        /// [0] username
        /// [1] password
        /// [2] hostname ddns
        /// </param>
        /// <example>noip your_username your_password your_host_name.ddns.net</example>
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args == null || args.Length <= 0)
            {
                string[] lines = File.Exists(defaultPath) ? File.ReadAllLines(defaultPath) : throw new NullReferenceException("Không tồn tại tệp tin cài đặt 'appSettings'");

                args = lines.Select(m => m?.Split('=')?[1]).ToArray();
            }

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            //options.BinaryLocation = defaultPathChrome;

            ChromeDriver driver = new ChromeDriver(options);

            Login(driver, args);
            RemoveHost(driver, args);
            AddHost(driver, args);

            driver.Quit();
        }

        private static void Login(ChromeDriver driver, string[] args, int @try = 3)
        {
            try
            {
                if (@try <= 0)
                {
                    Environment.Exit(1);
                }

                // login
                driver.Navigate().GoToUrl("https://www.noip.com/login");
                Thread.Sleep(3000);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                Console.WriteLine("=========================== redirect Login https://www.noip.com/login");
                IWebElement username = wait.Until(m => m.FindElement(By.Id("username")));
                username.Clear();
                username.SendKeys(args[0]);
                Thread.Sleep(1000);
                IWebElement password = wait.Until(m => m.FindElement(By.Id("password")));
                password.Clear();
                password.SendKeys(args[1]);
                Thread.Sleep(1000);
                IWebElement submitButton = wait.Until(m => m.FindElement(By.Id("clogs-captcha-button")));
                submitButton.Click();
                Thread.Sleep(2000);
                Console.WriteLine($"click login button");
                bool status = wait.Until(m => m.FindElement(By.XPath(@"//*[@id=""user-email-container""]/a/div"))?.Text == args[0]);
                if (status)
                {
                    Console.WriteLine("Login success.");
                }
                else
                {
                    Console.WriteLine("Login fail.");
                }

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Login(driver, args, --@try);
        }

        private static void RemoveHost(ChromeDriver driver, string[] args, int @try = 3)
        {
            try
            {
                if (@try <= 0)
                {
                    Environment.Exit(1);
                }

                // remove host
                driver.Navigate().GoToUrl("https://my.noip.com/dynamic-dns");
                Thread.Sleep(3000);
                Console.WriteLine("=========================== redirect RemoveHost https://my.noip.com/dynamic-dns");

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> listDomains = wait.Until(m => m.FindElements(By.CssSelector(@"a.link-info.cursor-pointer")));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> listIp = wait.Until(m => m.FindElements(By.CssSelector(@"td[data-title=""IP / Target""]")));
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> listRemove = wait.Until(m => m.FindElements(By.CssSelector(@".btn-remove")));
                Console.WriteLine($"Found {listRemove.Count} domain.");
                for (int i = 0; i < listRemove.Count; i++)
                {
                    Console.WriteLine($@"Hostname [{listDomains[i]?.Text}], ip [{listIp[i]?.Text}].");
                    if (listDomains[i]?.Text == args[2])
                    {
                        listRemove[i].Click();
                        Thread.Sleep(2000);
                        IWebElement btnComfirm = wait.Until(m => m.FindElement(By.XPath(@"/html/body/div[1]/div/div/div[3]/div[1]/div[2]/div/div/div[1]/div[5]/div/div/div[4]/button[1]")));
                        btnComfirm.Click();
                        Console.WriteLine($"Remove hostname [{args[2]}].");
                    }
                }
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            RemoveHost(driver, args, --@try);
        }

        private static string PublicIP()
        {
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            return client.DownloadString("http://ipinfo.io/ip");
        }

        private static void AddHost(ChromeDriver driver, string[] args, int @try = 3)
        {
            try
            {
                if (@try <= 0)
                {
                    Environment.Exit(1);
                }

                // add host
                Thread.Sleep(3000);
                driver.Navigate().GoToUrl("https://www.noip.com/members/dns/host.php");
                Thread.Sleep(3000);
                Console.WriteLine("=========================== redirect AddHost https://www.noip.com/members/dns/host.php");

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                string addDomain = args[2].Remove(args[2].ToLower().IndexOf(".ddns.net"));
                IWebElement hostnameAdd = wait.Until(m => m.FindElement(By.Id("hostname")));
                hostnameAdd.Clear();
                hostnameAdd.SendKeys(addDomain);
                Console.WriteLine($"Hostname add: [{addDomain}.ddns.net]");

                IWebElement ipAdd = wait.Until(m => m.FindElement(By.Id("ip")));
                string publicIp = PublicIP();
                ipAdd.Clear();
                ipAdd.SendKeys(publicIp);
                Console.WriteLine($"Ip add: {publicIp}");

                IWebElement btnAddHost = wait.Until(m => m.FindElement(By.XPath(@"//*[@id=""tab-content""]/div/div/form/div/div[13]/input[3]")));
                btnAddHost.Click();
                Console.WriteLine($"Clicked add hostname.");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            AddHost(driver, args, --@try);
        }
    }
}
