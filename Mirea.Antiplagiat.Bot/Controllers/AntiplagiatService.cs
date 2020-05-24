using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Extentions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirea.Antiplagiat.Bot.Models
{
    public class AntiplagiatService : IAntiplagiatService
    {
        public delegate void DocumentCheckedEvent(object sender, string documentPath, string resultPath);
        public event DocumentCheckedEvent OnDocumentChecked;

        private readonly Credentials credentials;
        private readonly IWebDriver driver;
        private readonly ILogger<AntiplagiatService> logger;

        private int maxIdChecking = 0;
        private Queue<string> documentsQueue;
        private List<string> checkeding;

        public AntiplagiatService(ILogger<AntiplagiatService> logger, Credentials credentials)
        {
            this.logger = logger;
            this.credentials = credentials;
            this.documentsQueue = new Queue<string>();
            this.checkeding = new List<string>
            {
                "7020ef8f"
            };
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", Folders.Repots());
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            driver = new ChromeDriver(service, options);
        }

        public Task Run()
        {
            return Task.Run(async () =>
            {

                logger.LogInformation(AppData.Strings.AuthorizeAntiplagiat);
                driver.Url = "https://users.antiplagiat.ru/cabinet";
                driver.Navigate();

                IWebElement loginButton = driver.FindElement(By.ClassName("enter")).FindElements(By.TagName("a")).FirstOrDefault();
                if (loginButton != null)
                {
                    loginButton.Click();
                    IWebElement emailTextbox = driver.FindElement(By.ClassName("email")).FindElement(By.TagName("input"));
                    IWebElement passwordTextBox = driver.FindElement(By.ClassName("passwd")).FindElement(By.TagName("input"));
                    IWebElement enterButton = driver.FindElement(By.Id("login-button"));

                    emailTextbox.SendKeys(credentials.Login);
                    passwordTextBox.SendKeys(credentials.Password);
                    enterButton.Click();
                }
                logger.LogInformation(AppData.Strings.AuthorizeAntiplagiatSuccess);

                while (true)
                {
                    if (documentsQueue.Any() && maxIdChecking < 10)
                    {
                        string path = documentsQueue.Dequeue();
                        await UploadDocument(path);
                    }
                    if (checkeding.Any())
                    {
                        await CheckStatus();
                    }
                    else
                    {
                        maxIdChecking = 0;
                    }
                    await Task.Delay(250);
                }
            });
        }

        private async Task CheckStatus()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("scroll-area")));

            IWebElement scrollPanel = driver.FindElement(By.ClassName("scroll-area"));

            var webElements = scrollPanel.FindElements(By.TagName("tr"));

            for (int i = 0; i < webElements.Count; i++)
            {
                var data = webElements[i].FindElements(By.TagName("td"));
                string text = $"{data[1].Text}  ".Split(' ')[1];
                var dockPath = checkeding.Where(d => d.Contains(text)).FirstOrDefault();
                if (dockPath != null)
                {
                    maxIdChecking = i;
                    IWebElement checkResult = webElements[i].FindElements(By.ClassName("report-link")).FirstOrDefault();
                    if (checkResult != null)
                    {
                        string reportPath = await DownloadReport(webElements[i]);
                        checkeding.Remove(dockPath);
                        OnDocumentChecked?.Invoke(this, dockPath, reportPath);
                        maxIdChecking = 10;
                        driver.Navigate().GoToUrl("https://users.antiplagiat.ru/cabinet");
                        break;
                    }
                }
            }
        }

        private async Task<string> DownloadReport(IWebElement checkResult)
        {
            checkResult.FindElement(By.ClassName("report-link")).Click();

            IWebElement exportSpoiler = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]"));
            exportSpoiler.Click();
            IWebElement exportNavigation = driver.FindElement(By.Id("report-export"));
            exportNavigation.Click();

            while (driver.WindowHandles.Count < 2)
                await Task.Delay(250);

            driver.SwitchTo().Window(driver.WindowHandles[1]);

            IWebElement export = driver.FindElements(By.ClassName("export-make")).FirstOrDefault();
            if (export != null)
                export.Click();



            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(600));
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("export-download")));
            IWebElement downloadButton = driver.FindElement(By.ClassName("export-download"));

            DirectoryInfo d = new DirectoryInfo(Folders.Repots());//Assuming Test is your Folder
            var names = d.GetFiles().Select(d => d.FullName).ToHashSet();

            downloadButton.Click();

            string[] files = new string[0];
            do
            {
                await Task.Delay(5000);
                files = d.GetFiles().Select(d => d.FullName).Except(names).ToArray();
            }
            while (!files.Any() && files.Any(w => w.EndsWith("crdownload")));

            driver.SwitchTo().Window(driver.WindowHandles[1]).Close();
            driver.SwitchTo().Window(driver.WindowHandles[0]);

            return d.GetFiles().Select(d => d.FullName).Except(names).Single();
        }

        private async Task UploadDocument(string path)
        {
            checkeding.Add(path);
            IWebElement fileUpload = driver.FindElement(By.Id("fileupload"));
            fileUpload.SendKeys(path);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("file-upload-btn")));

            IWebElement continueButton = driver.FindElement(By.Id("file-upload-btn"));
            continueButton.Click();

            await Task.Delay(1000);

            driver.Navigate().Refresh();
        }

        public void EnqueueDocument(string path)
        {
            documentsQueue.Enqueue(path);
        }
    }
}
